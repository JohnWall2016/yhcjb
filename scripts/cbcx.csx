#! "netcoreapp2.1"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"

using YHCJB.HNCJB;
using YHCJB.Util;

void UpdateCbzt(string xlsx = @"D:\Documents\Tencent Files\5672618\FileRecv\立洪25羊牯大道1.xls")
{
    var workBook = ExcelExtension.LoadExcel(xlsx);

    Session.Using(session =>
    {
        var sheet = workBook.GetSheet("Sheet3");
        for (var i = 0; i <= sheet.LastRowNum; i++)
        {
            var name = sheet.Cell(i, 2)?.StringCellValue;
            var idcard = sheet.Cell(i, 6)?.StringCellValue;

            if (idcard == null) continue;

            session.Send(new GrinfoQuery(idcard));
            var pinfo = session.Get<Result<Grinfo>>();

            var jbzt = "";
            if (pinfo.Length > 0)
            {
                jbzt = pinfo[0].JbztCN;
                (sheet.Cell(i, 13) ?? sheet.Row(i).CreateCell(13)).SetValue(jbzt);
            }
            Console.WriteLine($"{name}|{idcard}|{jbzt}");
        }
    });

    workBook.Save(Utils.FileNameAppend(xlsx, ".new"));
    workBook.Close();
}

UpdateCbzt();
