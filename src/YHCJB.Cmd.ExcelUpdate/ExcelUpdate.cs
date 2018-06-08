using System;
using System.IO;
using YHCJB.HNCJB;
using YHCJB.Util;
using NPOI.HSSF.UserModel;

namespace YHCJB.Cmd.ExcelUpdate
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Qcsq(args[0]);
        }

        static void Qcsq(string xlsFile)
        {
            var wbook = new HSSFWorkbook(new FileStream(xlsFile, FileMode.Open));
            var sheet = (HSSFSheet)wbook.GetSheetAt(0);
            
            Session.Using((session) =>
            {
                for (var i = 0; i <= sheet.LastRowNum; i++)
                {
                    var pid = sheet.Cell(i, 0).StringCellValue;
                    /*session.Send(new GrinfoQuery(pid));
                    var rs = session.Get<Result<Grinfo>>();*/
                    session.Send(new SncbxxQuery(pid));
                    var rs = session.Get<Result<Sncbxx>>();
                    var txt = "";
                    if (rs.datas.Length > 0)
                        txt += rs.datas[0].name + "个人账户返还结算单等相关资料";
                    else
                        txt = "未查到此人信息";
                    txt.Println();
                    if (sheet.Cell(i, 4) is null)
                        sheet.Row(i).CreateCell(4);
                    sheet.Cell(i, 4).SetCellValue(txt);
                }
            });

            using (var outStream = new FileStream(Utils.FileNameAppend(xlsFile, ".upd"), FileMode.CreateNew))
            {
                wbook.Write(outStream);
            }
            wbook.Close();
        }

        static void Xjzy(string xlsFile)
        {
            var wbook = new HSSFWorkbook(new FileStream(xlsFile, FileMode.Open));
            var sheet = (HSSFSheet)wbook.GetSheetAt(0);
            
            Session.Using((session) =>
            {
                for (var i = 0; i <= sheet.LastRowNum; i++)
                {
                    var pid = sheet.Cell(i, 0).StringCellValue;
                    /*session.Send(new GrinfoQuery(pid));
                    var rs = session.Get<Result<Grinfo>>();*/
                    session.Send(new SncbxxQuery(pid));
                    var rs = session.Get<Result<Sncbxx>>();
                    var txt = "";
                    if (rs.datas.Length > 0)
                    {
                        var type = sheet.Cell(i, 3)?.StringCellValue ?? "0";
                        switch (type)
                        {
                            case "0":
                                txt += rs.datas[0].name + "制度衔接转出相关资料";
                                break;
                            case "1":
                                txt += rs.datas[0].name + "制度衔接转入相关资料";
                                break;
                            case "2":
                                txt += rs.datas[0].name + "居保关系转出相关资料";
                                break;
                            case "3":
                                txt += rs.datas[0].name + "居保关系转入相关资料";
                                break;
                        }
                    }
                    else
                        txt = "未查到此人信息";
                    txt.Println();
                    if (sheet.Cell(i, 4) is null)
                        sheet.Row(i).CreateCell(4);
                    sheet.Cell(i, 4).SetCellValue(txt);
                }
            });

            using (var outStream = new FileStream(Utils.FileNameAppend(xlsFile, ".upd"), FileMode.CreateNew))
            {
                wbook.Write(outStream);
            }
            wbook.Close();
        }
    }
}
