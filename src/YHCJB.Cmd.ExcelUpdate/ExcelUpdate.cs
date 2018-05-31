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
            var wbook = new HSSFWorkbook(new FileStream(_file1, FileMode.Open));
            var sheet = (HSSFSheet)wbook.GetSheetAt(0);
            
            Session.Using((session) =>
            {
                for (var i = 0; i <= sheet.LastRowNum; i++)
                {
                    var pid = sheet.Cell(i, 0).StringCellValue;
                    session.Send(new GrinfoQuery(pid));
                    var rs = session.Get<Result<Grinfo>>();
                    var txt = "";
                    if (rs.datas.Length > 0)
                        txt += rs.datas[0].name + "制度衔接转出相关资料";
                    else
                        txt = "未查到此人信息";
                    txt.Println();
                    sheet.Cell(i, 3).SetCellValue(txt);
                }
            });

            using (var outStream = new FileStream(Utils.FileNameAppend(_file1, ".upd"), FileMode.CreateNew))
            {
                wbook.Write(outStream);
            }
            wbook.Close();
        }
    }
}
