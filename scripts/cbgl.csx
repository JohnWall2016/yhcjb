#! "netcoreapp2.1"
#r "nuget: Microsoft.Extensions.Logging, 2.1.1"
#r "nuget: NLog.Extensions.Logging, 1.1.0"
#r "nuget: Microsoft.EntityFrameworkCore, 2.1.1"
#r "nuget: Microsoft.EntityFrameworkCore.Relational, 2.1.1"
#r "nuget: Pomelo.EntityFrameworkCore.MySql, 2.1.1"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"
#load "nuget: Dotnet.Build, 0.3.9"
#load "fpdb.csx"

using static FileUtils;
using System.Threading;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using YHCJB.Util;
using YHCJB.HNCJB;

var logsFolder = Path.Combine(GetScriptFolder(), "..", "logs/cbgl.nlog");
var logger = NLog.LogManager.LoadConfiguration(logsFolder).GetCurrentClassLogger();

/// <summary>
///   参保登记审核
/// </summary>
void Cbdjsh(Action<Session, Session, string, string> additionalAction = null)
{
    Session.Using002((session002) =>
    {
        Session.Using007((session007) =>
        {
            session002.Send(new CbshQuery());
            var res = session002.Get<Result<Cbsh>>();
            foreach (var r in res.datas)
            {
                //if (r.pid != "430321198512021767") continue;

                var grinfo = $"{r.name}|{r.pid}|{r.aaf102}";
                if (r.birthday < 19510701)
                {
                    logger.Info($"无需缴费的到龄人员不自动审核｜{grinfo}");
                    break;
                }
                else if (r.type != "011")
                {
                    logger.Info($"非普通参保人员不自动审核｜{grinfo}");
                    break;
                }

                var saves = new CbshGotoSave();
                saves.AddRow(r.ToCbshSave());
                session002.Send(saves);
                var result = session002.Get<Result>();

                logger.Info($"自动审核|{grinfo}|{result.message}");

                if (result.type == "info" && result.vcode == "" && additionalAction != null)
                {
                    additionalAction(session002, session007, r.pid, r.name);
                }
            }
            //additionalAction(session007, "430321198512021767", "王冬阳");
        });
    });
}

/// <summary>
///   修改个人参保身份
/// </summary>
void UpdateCbsf(Session session002, Session session007 string pid, string name)
{
    using (var context = new JZFPContext())
    {
        var grinfo = $"{name}|{pid}";
        var msg = "";

        var tsxx = from tsry in context.TSCBRYs
                   where tsry.Sfzhm == pid
                   select new { tsry.Xm, tsry.Sfzhm, tsry.Rdsf, tsry.Sfbm };
        if (!tsxx.Any())
        {
            msg = "未查询到特殊参保身份信息";
        }
        else
        {
            var grxx = tsxx.First();
            if (grxx.Xm != name)
            {
                msg = $"姓名不一致'{grxx.Xm}'";
            }
            else
            {
                // 查询个人信息
                session007.Send(new InfoByIdcardQuery(pid));
                var pinfos = session007.Get<Result<InfoByIdcard>>();
                if (pinfos.datas.Length == 0)
                {
                    msg = "未查到个人信息";
                }
                else
                {
                    // 是否存在未审核数据 007
                    var pinfo = pinfos.datas[0];
                    session007.Send(new NotAuditInfoQuery(pid));
                    var res = session007.Get<Result>();

                    if (res.message != "")
                    {
                        msg = res.message; // 存在未审核数据
                    }
                    else
                    {
                        // 修改参保身份 007
                        var addInfoChange = new AddInfoChange(pinfo.grbh,
                            pinfo.pid, pinfo.name, pinfo.aaz159);
                        addInfoChange.AddArray(InfoChangeItem.ChangeCbsf(pinfo.cbsf, grxx.Sfbm));
                        session007.Send(addInfoChange);
                        var res = session007.Get<Result>();
                        if (res.type != "info" || res.vcode != "") // 修改身份失败
                        {
                            msg = $"修改身份失败[{res.message}]";
                        }
                        else
                        {
                            // 审核修改信息 002
                            session002.Send(new InfoChangeForAuditQuery(pinfo.pid));
                            var infoChanges = session002.Get<Result<InfoChangeForAudit>>();
                            if (infoChanges.datas.Length == 0)
                            {
                                msg = $"未找到待审核数据";
                            }
                            else
                            {
                                var infoChange = infoChanges.datas[0];
                                var auditInfoChangePass = new AuditInfoChangePass();
                                auditInfoChangePass.AddRow(infoChange.ToAuditInfoChange());
                                session002.Send(auditInfoChangePass);
                                var res = session002.Get<Result>();
                                if (res.type == "data" && res.vcode == "")
                                    msg = $"修改身份为'{grxx.Rdsf}({grxx.Sfbm})'";
                                else
                                    msg = $"审核修改身份失败[{res.message}]";
                            }
                        }
                    }
                }
            }
        }
        logger.Info($"修改个人参保身份|{grinfo}|{msg}");
    }
}

/// <summary>
///   参保登记审核处理循环
/// </summary>
async Task CbdjshLoop(CancellationToken token, TimeSpan delay)
{
    try
    {
        while (true)
        {
            logger.Info("开始批量审核参保登记数据");
            Cbdjsh(UpdateCbsf);
            logger.Info("结束批量审核参保登记数据");
            if (!token.IsCancellationRequested)
            {
                if (delay.TotalMinutes < 1)
                    logger.Info($"延迟{delay.TotalSeconds}秒...");
                else
                    logger.Info($"延迟{delay.TotalMinutes}分钟...");
            }
            await Task.Delay(delay, token);
        }
    }
    catch (OperationCanceledException)
    {
        logger.Info("批量审核退出");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "批量审核出错");
        throw;
    }
}

/// <summary>
///   开始批量审核参保登记数据
/// </summary>
void startCbdjshLoop(TimeSpan delay)
{
    var source = new CancellationTokenSource();

    // Cancel just during delay not bussiness operation.
    Console.CancelKeyPress += (sender, e) =>
    {
        e.Cancel = true;
        source.Cancel();
    };

    var task = CbdjshLoop(source.Token, delay);
    task.GetAwaiter().GetResult(); // block;
}

startCbdjshLoop(TimeSpan.FromSeconds(5));
