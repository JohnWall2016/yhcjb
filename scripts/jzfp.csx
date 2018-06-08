#! "netcoreapp2.0"
#r "../pkg/NPOI/bin/Debug/netstandard2.0/NPOI.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using YHCJB.Util;
using YHCJB.HNCJB;

void resetTitle(ISheet sheet, string type)
{
    var cells = new List<ICell>
    {
        sheet.Cell(0, 3),
        sheet.Cell(0, 4),
        sheet.Cell(0, 5),
    };

    foreach(var c in cells)
    {
        c.SetCellValue(type + c.StringCellValue);
    }
}

string prepareOutFile(string tmplFile, string type, string ext = ".xls")
{
    var dir = Path.GetDirectoryName(tmplFile);
    var outFile = Path.Combine(dir, type + ".xls");

    if (File.Exists(outFile))
        File.Delete(outFile);

    return outFile;
}

string preparePid(string pid)
{
    if (pid == null) return null;
    pid = pid.Trim();
    if (pid == "") return null;
    //$"|{pid}|".Println();
    if (pid.Length > 18)
        pid = pid.Substring(0, 18);
    return pid;
}

void purifyFpData(string origXls = @"D:\残疾特困\201806原始数据\贫困户信息_20180604.xls",
                  string tmplXls = @"D:\残疾特困\201806清理数据\清理数据模板.xls")
{
    var type = "建档立卡的贫困人口";
    var (begRow, endRow) = (3, 7447);
    
    var inWorkbook = new HSSFWorkbook(new FileStream(origXls, FileMode.Open));
    var inSheet = inWorkbook.GetSheetAt(0);

    var outWorkbook = new HSSFWorkbook(new FileStream(tmplXls, FileMode.Open));
    var outSheet = (HSSFSheet)outWorkbook.GetSheetAt(0);

    resetTitle(outSheet, type);

    for (var curRow = begRow; curRow <= endRow; curRow++)
    {
        var no = curRow - begRow + 1;
        var inRow = inSheet.GetRow(curRow);
        var name = inRow.Cell(5).StringCellValue;
        var pid = preparePid(inRow.Cell(6).StringCellValue);
        var addr = inRow.Cell(2).StringCellValue + "-" + inRow.Cell(3).StringCellValue;

        var msg = $"{no}".AlignRight(6) + name.AlignRight(9) + pid.AlignRight(19);
        msg.Println();

        var outRow = outSheet.GetOrCopyRowFrom(no, 0);
        outRow.Cell(0).SetValue(no);
        outRow.Cell(1).SetValue(name);
        outRow.Cell(2).SetValue(pid);
        outRow.Cell(3).SetValue("是");
        outRow.Cell(4).SetValue(addr);
        outRow.Cell(5).SetValue("");
    }
    
    
    var outFile = prepareOutFile(tmplXls, type);
    using (var stream = new FileStream(outFile, FileMode.CreateNew))
        outWorkbook.Write(stream);
    
    outWorkbook.Close();
    inWorkbook.Close();
}

void purifyTkData(string origXls = @"D:\残疾特困\201806原始数据\民政\6月特困名单.xls",
                  string tmplXls = @"D:\残疾特困\201806清理数据\清理数据模板.xls")
{
    var type = "特困人员";
    var (begRow, endRow) = (1, 958);
    
    var inWorkbook = new HSSFWorkbook(new FileStream(origXls, FileMode.Open));
    var inSheet = inWorkbook.GetSheetAt(0);

    var outWorkbook = new HSSFWorkbook(new FileStream(tmplXls, FileMode.Open));
    var outSheet = (HSSFSheet)outWorkbook.GetSheetAt(0);

    resetTitle(outSheet, type);

    for (var curRow = begRow; curRow <= endRow; curRow++)
    {
        var no = curRow - begRow + 1;
        var inRow = inSheet.GetRow(curRow);
        var name = inRow.Cell(2).StringCellValue;
        var pid = preparePid(inRow.Cell(3).StringCellValue);
        var addr = inRow.Cell(0).StringCellValue + "-" + inRow.Cell(1).StringCellValue;

        var msg = $"{no}".AlignRight(6) + name.AlignRight(9) + pid.AlignRight(19);
        msg.Println();

        var outRow = outSheet.GetOrCopyRowFrom(no, 0);
        outRow.Cell(0).SetValue(no);
        outRow.Cell(1).SetValue(name);
        outRow.Cell(2).SetValue(pid);
        outRow.Cell(3).SetValue("是");
        outRow.Cell(4).SetValue(addr);
        outRow.Cell(5).SetValue("");
    }
    
    
    var outFile = prepareOutFile(tmplXls, type);
    using (var stream = new FileStream(outFile, FileMode.CreateNew))
        outWorkbook.Write(stream);
    
    outWorkbook.Close();
    inWorkbook.Close();
}

void purifyDbData(string origXls1 = @"D:\残疾特困\201806原始数据\民政\城市6月名册.xls",
                  string origXls2 = @"D:\残疾特困\201806原始数据\民政\2018年6月雨湖区农村低保名册及报表上报市局本0.xls",
                  string tmplXls = @"D:\残疾特困\201806清理数据\清理数据模板2.xls")
{
    var type = "低保人员";


    var no = 1;
    var outWorkbook = new HSSFWorkbook(new FileStream(tmplXls, FileMode.Open));
    var outSheet = (HSSFSheet)outWorkbook.GetSheetAt(0);
    
    // 1. 处理城市低保
    void processCity()
    {
        var (begRow, endRow) = (1, 7234);
    
        var inWorkbook = new HSSFWorkbook(new FileStream(origXls1, FileMode.Open));
        var inSheet = inWorkbook.GetSheetAt(0);

        var nameids = new ValueTuple<int, int>[]
        {
            (7, 8),
            (9, 10),
            (11, 12),
            (13, 14),
            (15, 16)
        };

        for (int curRow = begRow; curRow <= endRow; curRow++)
        {
            var inRow = inSheet.GetRow(curRow);

            var typ = inRow.Cell(5).StringCellValue.Trim().ToUpper();
            var addr = inRow.Cell(0).StringCellValue + "-" + inRow.Cell(1).StringCellValue;

            foreach(var (nm, id) in nameids)
            {
                var name = inRow.Cell(nm)?.StringCellValue;
                var pid = preparePid(inRow.Cell(id)?.StringCellValue);
                if (name != null && pid != null)
                {
                    var msg = $"{no}".AlignRight(6) + name.AlignRight(9) + pid.AlignRight(19) + typ.AlignRight(3);
                    msg.Println();

                    var outRow = outSheet.GetOrCopyRowFrom(no, 0);
                    outRow.Cell(0).SetValue(no);
                    outRow.Cell(1).SetValue(name);
                    outRow.Cell(2).SetValue(pid);

                    if (typ == "A")
                        outRow.Cell(3).SetValue("是");
                    else if (typ == "B" || typ == "C")
                        outRow.Cell(4).SetValue("是");
                    outRow.Cell(5).SetValue(addr);
                    outRow.Cell(6).SetValue("城市低保"+typ);
                
                    no += 1;
                }
            }
        }
        inWorkbook.Close();
    }
    
    // 2. 处理农村低保
    void processCountry()
    {
        var (begRow, endRow) = (1, 2293);
    
        var inWorkbook = new HSSFWorkbook(new FileStream(origXls2, FileMode.Open));
        var inSheet = inWorkbook.GetSheetAt(0);

        var nameids = new ValueTuple<int, int>[]
        {
            (18, 20),
            (23, 24),
            (27, 28),
            (31, 32),
            (35, 36),
            (39, 40),
            (43, 44)
        };

        for (int curRow = begRow; curRow <= endRow; curRow++)
        {
            var inRow = inSheet.GetRow(curRow);

            var typ = inRow.Cell(10).StringCellValue.Trim().ToUpper();
            var addr = inRow.Cell(0).StringCellValue + "-" + inRow.Cell(1).StringCellValue;

            foreach(var (nm, id) in nameids)
            {
                var name = inRow.Cell(nm)?.StringCellValue;
                var pid = preparePid(inRow.Cell(id)?.StringCellValue);
                if (name != null && pid != null)
                {
                    var msg = $"{no}".AlignRight(6) + name.AlignRight(9) + pid.AlignRight(19) + typ.AlignRight(3);
                    msg.Println();

                    var outRow = outSheet.GetOrCopyRowFrom(no, 0);
                    outRow.Cell(0).SetValue(no);
                    outRow.Cell(1).SetValue(name);
                    outRow.Cell(2).SetValue(pid);

                    if (typ == "A")
                        outRow.Cell(3).SetValue("是");
                    else if (typ == "B" || typ == "C")
                        outRow.Cell(4).SetValue("是");
                    outRow.Cell(5).SetValue(addr);
                    outRow.Cell(6).SetValue("农村低保"+typ);
                
                    no += 1;
                }
            }
        }
        inWorkbook.Close();
    }

    processCity();
    processCountry();
    
    var outFile = prepareOutFile(tmplXls, type);
    using (var stream = new FileStream(outFile, FileMode.CreateNew))
        outWorkbook.Write(stream);
    
    outWorkbook.Close();
}

void emergeData(string inXls = @"D:\残疾特困\201806清理数据\特殊缴费人员分类明细表_备份.xls",
               string outXls = @"D:\残疾特困\201806清理数据\特殊缴费人员分类明细表.xls")
{
    
    var inWorkbook = new HSSFWorkbook(new FileStream(inXls, FileMode.Open));
    var inSheet = (HSSFSheet)inWorkbook.GetSheetAt(0);

    var idxSflx = new List<ValueTuple<int, string>>();

    for(var idx = 5; idx <= 10; idx++)
    {
        idxSflx.Add((idx, inSheet.Cell(1, idx).StringCellValue));
    }
    
    for (var idx = 2; idx <= inSheet.LastRowNum; idx++)
    {
        // 合并身份
        foreach(var (col, tname) in idxSflx)
        {
            if (inSheet.Cell(idx, col)?.StringCellValue == "是")
            {
                tname.Println();
                inSheet.Row(idx).CreateCell(4).SetValue(tname);
                break;
            }
        }

        // 合并地址
        for(var col = 12; col <= 15; col++)
        {
            var addr = inSheet.Cell(idx, col)?.StringCellValue;
            if (addr != null && addr != "")
            {
                addr.Println();
                inSheet.Row(idx).CreateCell(11).SetValue(addr);
                break;
            }
        }
    }

    using (var stream = new FileStream(outXls, FileMode.CreateNew))
        inWorkbook.Write(stream);
    
    inWorkbook.Close();
}

void updateJbzt(string inXls = @"D:\残疾特困\201806清理数据\特殊参保人员分类明细_居保参保.xls",
                   string outXls = @"D:\残疾特困\201806清理数据\特殊参保人员分类明细含居保参保情况.xls")
{
    var inWorkbook = new HSSFWorkbook(new FileStream(inXls, FileMode.Open));
    var inSheet = (HSSFSheet)inWorkbook.GetSheetAt(0);
    
    for (var idx = 1; idx <= inSheet.LastRowNum; idx++)
    {
        var cbzt = inSheet.Cell(idx, 17)?.StringCellValue;
        var jfzt = inSheet.Cell(idx, 18)?.StringCellValue;
        var jbzt = "";
        if (cbzt == null || jfzt == null || cbzt == "" || jfzt == "")
            jbzt = "未参加居保";
        else
            jbzt = Grinfo.GetJbztCN(cbzt, jfzt);
        inSheet.Row(idx).CreateCell(16).SetValue(jbzt);
    }

    using (var stream = new FileStream(outXls, FileMode.CreateNew))
        inWorkbook.Write(stream);
    
    inWorkbook.Close();
}

//purifyFpData();
//purifyTkData();
//purifyDbData();
//emergeData();
updateJbzt();
