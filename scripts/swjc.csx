#! "netcoreapp2.1"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"

using YHCJB.HNCJB;
using YHCJB.Util;

void UpdateIneritanceTable(string xls = @"D:\暂停终止\死亡继承\死亡继承汇总表.new.new.xls")
{
    string PreviousMonth(string yearMonth)
    {
        var yearMon = int.Parse(yearMonth);
        var year = yearMon / 100;
        var month = yearMon % 100;
        month -= 1;
        if (month == 0)
        {
            month = 12;
            year -= 1;
        }
        return string.Format("{0:D4}{1:D2}", year, month);
    }

    string SubstractMonth(string firstYearMonth, string secondYearMonth)
    {
        var first = int.Parse(firstYearMonth);
        var second = int.Parse(secondYearMonth);
        var firstMonths = (first / 100) * 12 + first % 100;
        var secondMonths = (second / 100) * 12 + second % 100;
        return $"{firstMonths - secondMonths}";
    }

    var workbook = ExcelExtension.LoadExcel(xls);
    
    Session.Using(session =>
    {
        var sheet = workbook.GetSheetAt(0);
        for (var idx = 3; idx <= sheet.LastRowNum; idx++)
        {
            var row = sheet.GetRow(idx); 
            var idcard = row.GetCell(1).StringCellValue;

            var query = new GrinfoQuery(idcard);
            //Console.WriteLine(query);
            session.Send(query);
            var pinfo = session.Get<Result<Grinfo>>();
            //Console.WriteLine(pinfo);
            
            if (pinfo.Length <= 0) continue;

            row.GetCell(4).SetValue(pinfo[0].czmc);
            row.GetCell(5).SetValue(pinfo[0].Dwmc);
            row.GetCell(6).SetValue(pinfo[0].JbztCN);

            string deathTime = row.GetCell(3).CellValue();
            session.Send(new PausePayInfoQuery(idcard));
            var info = session.Get<Result<PausePayInfo>>();

            if (info.Length > 0)
            {
                Console.WriteLine($"{info[0].Idcard}|{info[0].Name}|{info[0].PauseTime}|"
                    +$"{SystemCode.Dygl.GetDyztyyCN(info[0].PauseReason)}|{info[0].Memo}");

                row.GetCell(7).SetValue("暂停");
                var pauseTime = info[0].PauseTime;
                row.GetCell(8).SetValue(pauseTime);
                try 
                {
                    //Console.WriteLine(PreviousMonth(pauseTime));
                    var deltaMonths = SubstractMonth(deathTime, PreviousMonth(pauseTime));
                    Console.WriteLine($"{deathTime} - {pauseTime} + 1 = {deltaMonths}");
                    row.GetCell(9).SetValue(deltaMonths); 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法计算时间差, {deathTime} - {pauseTime}: {ex}");
                }
                row.GetCell(10).SetValue(SystemCode.Dygl.GetDyztyyCN(info[0].PauseReason));
                row.GetCell(11).SetValue(info[0].Memo);
            }
        }
    });

    workbook.Save(Utils.FileNameAppend(xls, ".new"));

    workbook.Close();
}

UpdateIneritanceTable();