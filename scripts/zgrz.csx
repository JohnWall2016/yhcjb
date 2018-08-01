#! "netcoreapp2.1"
#r "../pkg/NPOI/bin/Debug/netstandard2.0/NPOI.dll"
#r "nuget: SharpZipLib, 1.0.0-alpha2"
#r "../pkg/NPOI.OOXML/bin/Debug/netstandard2.0/NPOI.OOXML.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"

using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;
using YHCJB.Util;
using YHCJB.HNCJB;
using System.Collections.Generic;

void splitWrzmc(string inExcel = @"D:\生存认定\2018年\未认证名册\截至目前未做资格认证的正常待遇人员名册20180731.xls",
                string tmplExcel = @"D:\生存认定\2018年\未认证名册\截至目前未做资格认证的正常待遇人员名册模板.xls",
                string outDir = @"D:\生存认定\2018年\未认证名册\未资格认证正常待遇人员名册")
{
    var wb = ExcelExtension.LoadExcel(inExcel);
    var sheet = wb.GetSheetAt(0);

    var map = SystemCode.Xzqh.Xzj.Select(xzj => (xzj, new List<int>())).ToList();

    for (int i = 1; i <= sheet.LastRowNum; i++)
    {
        if (sheet.Cell(i, 6).CellValue() != "1")
            continue;
        
        foreach (var (xzj, rows) in map)
        {
            if (sheet.Cell(i, 0).CellValue().IndexOf(xzj) >= 0)
            {
                rows.Add(i);
                break;
            }
        }
    }

    var total = 0;
    foreach (var (xzj, rows) in map)
    {
        total += rows.Count;
        Console.WriteLine($"{xzj}: {rows.Count}");

        var outwb = ExcelExtension.LoadExcel(tmplExcel);
        var outsheet = outwb.GetSheetAt(0);

        var idx = 1;
        foreach (var row in rows)
        {
            var outrow = outsheet.GetOrCopyRowFrom(idx, 1);
            var xzqh = sheet.Cell(row, 0).CellValue();
            var name = sheet.Cell(row, 2).CellValue();
            var idcard = sheet.Cell(row, 3).CellValue();
            var birthday = sheet.Cell(row, 5).CellValue();
            outrow.Cell(0).SetValue(idx);
            outrow.Cell(1).SetValue(xzqh);
            outrow.Cell(2).SetValue(name);
            outrow.Cell(3).SetValue(idcard);
            outrow.Cell(4).SetValue(birthday);
            idx += 1;
        }
        outwb.Save(Path.Combine(outDir, xzj + ".xls"));
    }
    Console.WriteLine(total);
    
    wb.Close();
}

splitWrzmc();
