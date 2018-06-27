#! "netcoreapp2.0"
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
using System.Text.RegularExpressions;

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

void saveJfsz(string inXls, int begRow = 1, int endRow = 14)
{
    var inWorkbook = new HSSFWorkbook(new FileStream(inXls, FileMode.Open));
    var inSheet = inWorkbook.GetSheetAt(0);

    Session.Using(session =>
    {
        for (var idx = begRow; idx <= endRow; idx++)
        {
            var name = inSheet.Cell(idx, 1).StringCellValue;
            var sflx = inSheet.Cell(idx, 2).StringCellValue;
            var hklx = inSheet.Cell(idx, 3).StringCellValue;
            var jfdc = inSheet.Cell(idx, 4).StringCellValue;
            var ksrq = inSheet.Cell(idx, 5).StringCellValue;
            var jsrq = inSheet.Cell(idx, 6).StringCellValue;

            // 保存征缴规则
            var saveZjgz = new SaveZjgz(name, sflx, hklx, jfdc, ksrq, jsrq);
            Console.WriteLine((string)new Service(saveZjgz));
            session.Send(saveZjgz);
            var res = session.Get<Result>();
            Console.WriteLine(res.message);

            // 获得以上规则的ID
            var zjgzq = new ZjgzQuery
            {
                hklx = hklx,
                sflx = sflx,
                jfdc = jfdc,
                jfnd = ksrq.Substring(0, 4)
            };
            Console.WriteLine((string)new Service(zjgzq));
            session.Send(zjgzq);
            var resmx = session.Get<Result<Zjgzmx>>();
            if (resmx.datas.Length > 0)
            {
                var zjgzmx = resmx.datas[0];
                Console.WriteLine($"{zjgzmx.Id}");

                var gjf = inSheet.Cell(idx, 7).StringCellValue;
                var sbt = inSheet.Cell(idx, 8).StringCellValue;
                var cbt = inSheet.Cell(idx, 9).StringCellValue;
                var xbt = inSheet.Cell(idx, 10).StringCellValue;
                var zdj = inSheet.Cell(idx, 11).StringCellValue;

                var zjgzcs_create = Zjgzcs.Create(zjgzmx.Ksrq, zjgzmx.Jsrq);
                var gjfcs = zjgzcs_create("1", gjf, "");
                var sbtcs = zjgzcs_create("3", sbt, "1");
                var cbtcs = zjgzcs_create("4", cbt, "1");
                var xbtcs = zjgzcs_create("5", xbt, "1");
                var zdjcs = zjgzcs_create("11", zdj, "1");

                var saveZjgzcs = new SaveZjgzcs{ id = zjgzmx.Id };
                saveZjgzcs.AddRow(gjfcs);
                saveZjgzcs.AddRow(sbtcs);
                saveZjgzcs.AddRow(cbtcs);
                saveZjgzcs.AddRow(xbtcs);
                saveZjgzcs.AddRow(zdjcs);

                Console.WriteLine((string)new Service(saveZjgzcs));
                session.Send(saveZjgzcs);
                var res1 = session.Get<Result>();
                Console.WriteLine(res1.message);
            }
        }
    });

    inWorkbook.Close();
}

void unionCjdz(string fromDir = @"D:\残疾特困\201806清理数据\残疾人员地址",
               string toXlsx = @"D:\残疾特困\201806清理数据\残疾人员地址汇总.xlsx")
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
            //Console.WriteLine(inSheet.Cell(i, 1).NumericCellValue);
            toRow.Cell(1).SetValue(inSheet.Cell(i, 1).NumericCellValue);
            toRow.Cell(2).SetValue(inSheet.Cell(i, 2).StringCellValue);
            toRow.Cell(3).SetValue(inSheet.Cell(i, 3).StringCellValue);
                
            toRow.Cell(4).SetValue(inSheet.Cell(i, 7).StringCellValue);
            toRow.Cell(5).SetValue(inSheet.Cell(i, 8).StringCellValue);
            toRow.Cell(6).SetValue(inSheet.Cell(i, 9)?.StringCellValue ?? "");

            toRow.Cell(7).SetValue(dw);

            count += 1;
        }
        inWorkbook.Close();    
    }

    toWorkbook.Save(Utils.FileNameAppend(toXlsx, ".new"));
    toWorkbook.Close();
}

void updateDZ(string inXls = @"D:\残疾特困\201806清理数据\特殊参保人员分类明细含居保参保情况.xls",
               string outXls = @"D:\残疾特困\201806清理数据\特殊参保人员分类明细含居保参保情况.new2.xls")
{
    var inWorkbook = ExcelExtension.LoadExcel(inXls);
    var inSheet = inWorkbook.GetSheetAt(0);

    for (var idx = 2; idx <= inSheet.LastRowNum; idx++)
    {
        // 合并地址
        for(var col = 13; col <= 16; col++)
        {
            var addr = inSheet.Cell(idx, col)?.StringCellValue;
            if (addr != null && addr != "")
            {
                var match = Regex.Match(addr, @"(.*-.*?(村|社区))");
                if (match.Length > 0)
                    addr = match.Groups[1].Value;
                addr.Println();
                inSheet.Row(idx).CreateCell(12).SetValue(addr);
                break;
            }
        }
    }
    
    inWorkbook.Save(outXls);
    inWorkbook.Close();
}

//purifyFpData();
//purifyTkData();
//purifyDbData();
//emergeData();
//updateJbzt();

//saveJfsz(@"D:\残疾特困\参数设置\贫困人口一级.xls");
//saveJfsz(@"D:\残疾特困\参数设置\贫困人口一级（城市）.xls");
//saveJfsz(@"D:\残疾特困\参数设置\低保对象一级（城市）.xls");
//saveJfsz(@"D:\残疾特困\参数设置\低保对象一级（农村）.xls");
//saveJfsz(@"D:\残疾特困\参数设置\低保对象二级（农村）.xls");
//saveJfsz(@"D:\残疾特困\参数设置\低保对象二级（城市）.xls");
//saveJfsz(@"D:\残疾特困\参数设置\普通参保人员（农村）.xls");

//unionCjdz();
//updateDZ();