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
        static void print(string s) => Console.WriteLine(s);
        
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
            /*Session.Using(session => {
                    session.Post("executeDfrymdQuery", new DfrymdQuery("803"));
                    session.Get().Println();
                });*/
        }

        static void ExportZcdf(params string[] args)
        {
            if (args.Length < 1) Error("参数有误");
            var dflx = args[0];

            var wbook = new HSSFWorkbook(new FileStream(_zcdfTmpl, FileMode.Open));
            var sheet = (HSSFSheet)wbook.GetSheetAt(0);

            var date = $"{DateTime.Now:yyyyMMdd}";
            var zbdate = $"制表时间：{DateTime.Now:D}";

            sheet.GetRow(1).GetCell(6).SetCellValue(zbdate);
            
            Session.Using(session => {
                    int begRow = 3, curRow = begRow;
                    session.Post("executeDfrymdQuery", new DfrymdQuery(dflx));
                    var res = Service.FromJson<DfrymdResult>(session.Get());
                    foreach (var item in res.datas)
                    {
                        if (item.id == 0) continue;

                        var row = sheet.GetOrCopyRowFrom(curRow, begRow);
                        row.GetCell(0).SetCellValue(curRow - begRow + 1);
                        row.GetCell(1).SetCellValue(item.region);
                        row.GetCell(2).SetCellValue(item.name);
                        row.GetCell(3).SetCellValue(item.pid);
                        row.GetCell(4).SetCellValue(item.ksny);
                        if (item.dfbz != null)
                            row.GetCell(5).SetCellValue((int)item.dfbz);
                        row.GetCell(6).SetCellValue(item.dflx);
                        row.GetCell(7).SetCellValue(item.JbztCN);
                        if (item.jzny != null)
                            row.GetCell(8).SetCellValue((int)item.jzny);
                        if (item.jzje != null)
                            row.GetCell(9).SetCellValue((int)item.jzje);
                        curRow += 1;
                    }
                    var row2 = sheet.GetOrCopyRowFrom(curRow, begRow);
                    row2.GetCell(2).SetCellValue("共计");
                    row2.GetCell(3).SetCellValue(curRow - begRow);
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
