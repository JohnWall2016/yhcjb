using System;
using System.IO;
using System.Linq;
using System.Collections;
using YHCJB.HNCJB;
using YHCJB.Util;
using NPOI.HSSF.UserModel;

namespace YHCJB.Cmd.Dfgl
{
    partial class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                _usage.Println();
                return;
            }
            
            if (args[0] == "zcdf")
                ExportZcdf(args.Skip(1).ToArray());
            else if (args[1] == "zfmx")
                ExportZfmx(args.Skip(1).ToArray());
            else
                Error("参数有误");
        }

        static void ExportZcdf(params string[] args)
        {
            if (args.Length < 1) Error("参数有误");
            var dflx = args[0];

            var wbook = new HSSFWorkbook(new FileStream(_zcdfTmpl, FileMode.Open));
            var sheet = (HSSFSheet)wbook.GetSheetAt(0);

            var date = $"{DateTime.Now:yyyyMMdd}";
            var zbdate = $"制表时间：{DateTime.Now:D}";

            sheet.Cell(1, 6).SetValue(zbdate);
            
            Session.Using(session => {
                    int begRow = 3, curRow = begRow;
                    session.Send("executeDfrymdQuery", new DfrymdQuery(dflx));
                    var res = Service.FromJson<DfrymdResult>(session.Get());
                    foreach (var item in res.datas)
                    {
                        if (item.id == 0) continue;

                        var row = sheet.GetOrCopyRowFrom(curRow, begRow);
                        row.Cell(0).SetValue(curRow - begRow + 1);
                        row.Cell(1).SetValue(item.region);
                        row.Cell(2).SetValue(item.name);
                        row.Cell(3).SetValue(item.pid);
                        row.Cell(4).SetValue(item.ksny);
                        if (item.dfbz != null)
                            row.Cell(5).SetValue((int)item.dfbz);
                        row.Cell(6).SetValue(item.dflx);
                        row.Cell(7).SetValue(item.JbztCN);
                        if (item.jzny != null)
                            row.Cell(8).SetValue((int)item.jzny);
                        if (item.jzje != null)
                            row.Cell(9).SetValue((int)item.jzje);
                        curRow += 1;
                    }
                    var row2 = sheet.GetOrCopyRowFrom(curRow, begRow);
                    row2.Cell(2).SetValue("共计");
                    row2.Cell(3).SetValue(curRow - begRow);
                });
            
            var idx = _zcdfTmpl.LastIndexOf('.');
            var saveFile = "";
            if (idx > 0)
            {
                saveFile = _zcdfTmpl.Substring(0, idx);
                saveFile += "(" + DfrymdQuery.GetDflxCN(dflx) + ")";
                saveFile += date + _zcdfTmpl.Substring(idx);
            }
            else
                saveFile = _zcdfTmpl + "(" + DfrymdQuery.GetDflxCN(dflx) + ")";
            
            using (var outStream = new FileStream(saveFile, FileMode.CreateNew))
            {
                wbook.Write(outStream);
            }
            wbook.Close();
        }

        static void ExportZfmx(params string[] args)
        {
            
        }
    }
}
