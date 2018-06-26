#! "netcoreapp2.0"
#r "../pkg/NPOI/bin/Debug/netstandard2.0/NPOI.dll"
#r "nuget: SharpZipLib, 1.0.0-alpha2"
#r "../pkg/NPOI.OOXML/bin/Debug/netstandard2.0/NPOI.OOXML.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"

using YHCJB.Util;
using NPOI.SS.UserModel;

void unionJhdata(string fromDir = @"D:\数据核查\历年疑似死亡名册2018",
               string toXlsx = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\业务系统中疑似死亡待遇暂停人员街道上报死亡时间汇总.xls")
{
    IWorkbook toWorkbook = ExcelExtension.LoadExcel(toXlsx);
    var toSheet = toWorkbook.GetSheetAt(0);
    var count = 1;
    
    foreach(var f in Directory.EnumerateFiles(fromDir))
    {
        var dw = Path.GetFileNameWithoutExtension(f);
        Console.WriteLine("合并: {1} - {0}", Path.GetFileName(f), dw);

        var inWorkbook = ExcelExtension.LoadExcel(f);
        var inSheet = inWorkbook.GetSheetAt(0);
        for (var i = 1; i <= inSheet.LastRowNum; i++)
        {
            var toRow = toSheet.GetOrCopyRowFrom(count, 1);
            toRow.Cell(0).SetValue(count);
            toRow.Cell(1).SetValue(dw);
            toRow.Cell(2).SetValue(inSheet.Cell(i, 0).StringCellValue);
            toRow.Cell(3).SetValue(inSheet.Cell(i, 1).StringCellValue);
            toRow.Cell(4).SetValue(inSheet.Cell(i, 2).StringCellValue);
            toRow.Cell(5).SetValue(inSheet.Cell(i, 3).StringCellValue);
            toRow.Cell(6).SetValue(inSheet.Cell(i, 4).CellValue());
            toRow.Cell(7).SetValue(inSheet.Cell(i, 5).StringCellValue);
            toRow.Cell(8).SetValue(inSheet.Cell(i, 6)?.StringCellValue ?? "");
            toRow.Cell(9).SetValue(inSheet.Cell(i, 7).CellValue());
            
            count += 1;
        }
        inWorkbook.Close();    
    }

    toWorkbook.Save(Utils.FileNameAppend(toXlsx, ".new"));
    toWorkbook.Close();
}

void updateZtyy(string inXls = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区2012到2016年历年暂停停人员名册表.xlsx")
{
    IWorkbook inWorkbook = ExcelExtension.LoadExcel(inXls);
    var inSheet = inWorkbook.GetSheetAt(0);
    
    for (var idx = 2; idx <= inSheet.LastRowNum; idx++)
    {
        var ztyy = inSheet.Cell(idx, 11)?.StringCellValue;
        var zbbd = inSheet.Cell(idx, 13)?.StringCellValue;
        var jdsb = inSheet.Cell(idx, 15)?.StringCellValue;
        
        if (ztyy == null || ztyy == "")
        {
            if (zbbd != null && zbbd != "")
                inSheet.Row(idx).CreateCell(11).SetValue(zbbd);
            else if (jdsb != null && jdsb != "")
                inSheet.Row(idx).CreateCell(11).SetValue(jdsb);
        }
    }
    inWorkbook.Save(Utils.FileNameAppend(inXls, ".new"));
    inWorkbook.Close();
}

//unionJhdata();
updateZtyy();
