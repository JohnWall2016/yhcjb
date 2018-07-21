#! "netcoreapp2.1"
#r "nuget: Microsoft.Extensions.Logging, 2.1.1"
#r "nuget: NLog.Extensions.Logging, 1.1.0"
#load "nuget: Dotnet.Build, 0.3.9"

using static FileUtils;
using System.Threading;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

var logsFolder = Path.Combine(GetScriptFolder(), "..", "logs/cbgl.nlog");
var logger = NLog.LogManager.LoadConfiguration(logsFolder).GetCurrentClassLogger();

/// <summary>
///   参保登记审核
/// </summary>
void Cbdjsh()
{
    logger.Info("Cbdjsh");
    Thread.Sleep(5000);
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
            Cbdjsh();
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
