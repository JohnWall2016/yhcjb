#! "netcoreapp2.0"
#r "../pkg/NPOI/bin/Debug/netstandard2.0/NPOI.dll"
#r "nuget: SharpZipLib, 1.0.0-alpha2"
#r "../pkg/NPOI.OOXML/bin/Debug/netstandard2.0/NPOI.OOXML.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"

using YHCJB.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using YHCJB.HNCJB;
using System.Text.RegularExpressions;

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

void queryJfqk(string xlsx = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区2012到2016年历年暂停停人员名册表（疑似死亡）.xlsx")
{
    var workbook = ExcelExtension.LoadExcel(xlsx);
    var sheet = workbook.GetSheetAt(0);

    Session.Using(session =>
    {
        for (var i = 2; i <= sheet.LastRowNum; i++)
        {
            var pid = sheet.Cell(i, 2).StringCellValue;
            session.Send(new SncbqkcxjfxxQ { pid = pid });
            var jfxx = session.Get<Result<Sncbqkcxjfxxmx>>();
            var hasJf = "否";
            if (jfxx.datas.Length > 1)
                hasJf = "是";
            $"{pid} {hasJf}".Println();
            sheet.Row(i).CreateCell(10).SetValue(hasJf);
        }
    });

    workbook.Save(Utils.FileNameAppend(xlsx, ".new"));
    workbook.Close();
}

string[] dwPatterns = {
    "湘潭市雨湖区((.*?乡)(.*?社区))",
    "湘潭市雨湖区((.*?乡)(.*?村))",
    "湘潭市雨湖区((.*?乡)(.*?政府机关))",
    "湘潭市雨湖区((.*?街道)办事处(.*?社区))",
    "湘潭市雨湖区((.*?街道)办事处(.*?政府机关))",
    "湘潭市雨湖区((.*?镇)(.*?社区))",
    "湘潭市雨湖区((.*?镇)(.*?居委会))",
    "湘潭市雨湖区((.*?镇)(.*?村))",
    "湘潭市雨湖区((.*?街道)办事处(.*?村))",
    "湘潭市雨湖区((.*?镇)(.*?政府机关))",
};

void dispatchJhmc(string inExcel = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\疑似死亡\雨湖区居保历年疑似死亡暂停人员名册（有缴费记录）.xlsx",
                  string tmplExcel = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\疑似死亡\乡镇街下发模板.xlsx",
                  string outDir = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\疑似死亡\下发乡镇疑似死亡数据",
                  string appendName = "（有缴费记录）")
{
    var dataMap = new Dictionary<string, List<int>>();
    var inWorkbook = ExcelExtension.LoadExcel(inExcel);
    var inSheet = inWorkbook.GetSheetAt(0);
    for (var i = 2; i < inSheet.LastRowNum; i++)
    {
        var region = inSheet.Cell(i, 1).StringCellValue;
        foreach (var pattern in dwPatterns)
        {
            var match = Regex.Match(region, pattern);
            if (match.Length > 0)
            {
                var dw = match.Groups[2].Value;
                if (!dataMap.ContainsKey(dw))
                    dataMap[dw] = new List<int>{i};
                else
                    dataMap[dw].Add(i);
            }
        }
    }
    foreach (var dw in dataMap.Keys)
    {
        Console.WriteLine("{0}: [{1}]", dw, dataMap[dw].JoinToString());
        var outWorkbook = ExcelExtension.LoadExcel(tmplExcel);
        var outSheet = outWorkbook.GetSheetAt(0);

        outSheet.Cell(0, 0).SetValue(Path.GetFileNameWithoutExtension(inExcel));
        outSheet.Cell(1, 0).SetValue("单位名称："+dw);

        var skipRows = 3;
        var index = 1;
        foreach (var i in dataMap[dw])
        {
            var row = outSheet.GetOrCopyRowFrom(skipRows + index - 1, skipRows);
            row.Cell(0).SetValue(index);
            row.Cell(1).SetValue(inSheet.Cell(i, 1).StringCellValue);
            row.Cell(2).SetValue(inSheet.Cell(i, 2).StringCellValue);
            row.Cell(3).SetValue(inSheet.Cell(i, 3).StringCellValue);
            row.Cell(4).SetValue(inSheet.Cell(i, 4).StringCellValue);
            row.Cell(5).SetValue(inSheet.Cell(i, 5).NumericCellValue);
            row.Cell(6).SetValue(inSheet.Cell(i, 6).StringCellValue);
            index += 1;
        }
        var helper = outSheet.GetDataValidationHelper();
        if (dataMap[dw].Count > 1)
            foreach (var dataValidation in outSheet.GetDataValidations())
            {
                var regions = new CellRangeAddressList();
                foreach (var cr in dataValidation.Regions.CellRangeAddresses)
                {
                    regions.AddCellRangeAddress(cr.FirstRow+1, cr.FirstColumn, skipRows + index - 2, cr.LastColumn);
                }
                var vd = helper.CreateValidation(dataValidation.ValidationConstraint, regions);
                vd.ShowPromptBox = true;
                vd.ShowErrorBox = true;
                outSheet.AddValidationData(vd);
            }
        outWorkbook.Save(Path.Combine(outDir, dw+appendName+Path.GetExtension(tmplExcel)));
        outWorkbook.Close();
        //break;
    }

    inWorkbook.Close();
}

void dyztryzz(string xls = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区2012到2016年历年暂停停人员名册表（职保待遇）.xlsx",
              string bz = "职保退休暂停待遇人员", string zzyy = "1407")
{
    var workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(0);

    try
    {
        Session.Using(session =>
        {
            for (var i = 2; i <= 477; i++)
            {
                var pid = sheet.Cell(i, 3).StringCellValue;
                var name = sheet.Cell(i, 4).StringCellValue;
                var reason = sheet.Cell(i, 7).StringCellValue;
                var dykssj = sheet.Cell(i, 13).StringCellValue;

                Console.Write($"{i}:{pid}|{name}|{reason}|{dykssj}|");

                var memo = "";
                var (dbfkje, ktzhje, dkje, yjhje, tkzje) = ("", "", "", "", "");
                if (reason == "职保正常待遇")
                {
                    if (int.TryParse(dykssj, out int izzsj))
                    {
                        izzsj = DateTimeExtension.PrevMonth(izzsj);
                        var zzsj = $"{izzsj / 100:000}-{izzsj % 100:00}";

                        Console.Write($"{zzsj}|");
            
                        session.Send(new DyzzPerInfoQuery(pid));
                        var pinfo = session.Get<Result<DyzzPerInfo>>();
                        //Console.WriteLine("\n" + JsonExtension.ToJson(pinfo));

                        if (pinfo.type == "warn" && pinfo.message != "")
                        {
                            memo = pinfo.message;
                        }
                        else if (pinfo.datas.Length > 0)
                        {
                            var id = pinfo.datas[0].id;
                        
                            DyzzBankInfo bkinfo = null;
                            session.Send(new DyzzBankInfoQuery(id));
                            var binfos = session.Get<Result<DyzzBankInfo>>();
                            //Console.WriteLine("\n" + JsonExtension.ToJson(binfos));
                        
                            if (binfos.datas.Length > 0)
                            {
                                bkinfo = binfos.datas[0];
                            }

                            var apro = new DyzzAccrualPro
                            {
                                zzyy = zzyy,
                                id = id,
                                zzrq = zzsj,
                                sfyszf = "2",
                            };
                            session.Send(apro);
                            var apromxs = session.Get<Result<DyzzAccrualProMx>>();
                            //Console.WriteLine("\n" + JsonExtension.ToJson(apromxs));
                        
                            if (apromxs.datas.Length > 0)
                            {
                                var apromx = apromxs.datas[0];
                            
                                (dbfkje, ktzhje, dkje, yjhje, tkzje) = (apromx.dbfkje, apromx.ktzhje, apromx.dkje, apromx.yjhje, apromx.tkzje);
                                Console.Write($"{dbfkje}|{ktzhje}|{dkje}|{yjhje}|{tkzje}|");
                            
                                if (apromx.tkzje != "0" && bkinfo == null)
                                {
                                    //memo = "退款金额不等于0";
                                    memo = "无可退款账户";
                                }
                                else
                                {
                                    if (bkinfo == null) bkinfo = new DyzzBankInfo();
                                    session.Send(new DyzzPerSave(bkinfo, apro, apromx, bz));
                                    //memo = "TEST";
                                    //Console.WriteLine(new Service(new DyzzPerSave(bkinfo, apro, apromx, bz)));
                                    var result = session.Get<Result>();
                                    if (result.type == "info" && result.message == "")
                                        memo = "待遇终止成功";
                                    else
                                        memo = result.message;
                                }
                            }
                            else
                            {
                                memo = "无法获取终止信息";
                            }
                        }
                        else
                        {
                            memo = "未找到个人信息";
                        }
                    }
                    else
                    {
                        memo = "待遇开始日期格式有误";
                    }
                }
                sheet.Row(i).CreateCell(15).SetValue(dbfkje);
                sheet.Row(i).CreateCell(16).SetValue(ktzhje);
                sheet.Row(i).CreateCell(17).SetValue(dkje);
                sheet.Row(i).CreateCell(18).SetValue(yjhje);
                sheet.Row(i).CreateCell(19).SetValue(tkzje);
                sheet.Row(i).CreateCell(20).SetValue(memo);
                Console.WriteLine(memo);
            }
        });
    }
    finally
    {
        workbook.Save(Utils.FileNameAppend(xls, ".new"));
        workbook.Close();   
    }
}

void unionWjfjl(string fromDir = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\修改后-历年暂停上报表",
                string endwith = "（有缴费记录）",
                string toXlsx = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区居保历年疑似死亡暂停人员街道汇总名册（有缴费记录）.xlsx")
{
    IWorkbook toWorkbook = ExcelExtension.LoadExcel(toXlsx);
    var toSheet = toWorkbook.GetSheetAt(0);
    var count = 1;
    
    foreach(var f in Directory.EnumerateFiles(fromDir))
    {
        var dw = Path.GetFileNameWithoutExtension(f);
        if (!dw.EndsWith(endwith))
            continue;
        dw = dw.Replace(endwith, "");
        Console.WriteLine("合并: {1} - {0}", Path.GetFileName(f), dw);

        var inWorkbook = ExcelExtension.LoadExcel(f);
        var inSheet = inWorkbook.GetSheetAt(0);
        for (var i = 3; i <= inSheet.LastRowNum; i++)
        {
            var pid = inSheet.Cell(i, 2)?.StringCellValue ?? "";
            var name = inSheet.Cell(i, 3)?.StringCellValue ?? "";

            if (name == "" || pid == "") continue;
            
            var toRow = toSheet.GetOrCopyRowFrom(count + 1, 2);
            
            toRow.Cell(0).SetValue(count);
            toRow.Cell(1).SetValue(inSheet.Cell(i, 1).StringCellValue);
            toRow.Cell(2).SetValue(pid);
            toRow.Cell(3).SetValue(name);
            toRow.Cell(4).SetValue(inSheet.Cell(i, 4).StringCellValue);
            toRow.Cell(5).SetValue(inSheet.Cell(i, 5).CellValue());
            toRow.Cell(6).SetValue(inSheet.Cell(i, 6).StringCellValue);
            toRow.Cell(7).SetValue(inSheet.Cell(i, 7)?.StringCellValue ?? "");
            toRow.Cell(8).SetValue(inSheet.Cell(i, 8)?.CellValue() ?? "");
            toRow.Cell(9).SetValue(inSheet.Cell(i, 9)?.CellValue() ?? "");
            toRow.Cell(10).SetValue(dw);
            toRow.Cell(11).SetValue(inSheet.Cell(i, 10)?.CellValue() ?? "");
            
            count += 1;
        }
        inWorkbook.Close();    
    }

    toWorkbook.Save(Utils.FileNameAppend(toXlsx, ".new"));
    toWorkbook.Close();
}

void dyztswry_cs(string xls = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区居保历年疑似死亡暂停人员街道汇总名册（有缴费记录）.xlsx",
              string bz = "有缴费记录已死亡待遇暂停人员", string zzyy = "1401")
{
    var workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(0);

    try
    {
        Session.Using(session =>
        {
            for (var i = 2; i <= sheet.LastRowNum; i++)
            {
                var pid = sheet.Cell(i, 2).StringCellValue;
                var name = sheet.Cell(i, 3).StringCellValue;
                var reason = sheet.Cell(i, 7)?.StringCellValue ?? "";
                var swsj = sheet.Cell(i, 8)?.CellValue() ?? "";

                //Console.WriteLine(swsj);
                
                Console.Write($"{i}:{pid}|{name}|{reason}|{swsj}|");

                var memo = "";
                var (sjztny, dbfkje, ktzhje, dkje, yjhje, tkzje) = ("", "", "", "", "", "");
                if (reason == "经核实已死亡" && swsj.Length >= 6)
                {
                    //if (int.TryParse(swsj, out int izzsj))
                    {
                        //izzsj = DateTimeExtension.PrevMonth(izzsj);
                        //var zzsj = $"{izzsj / 100:000}-{izzsj % 100:00}";
                        
                        //Console.Write($"{zzsj}|");
                        swsj = swsj.Substring(0, 4) + "-" + swsj.Substring(4, 2);
            
                        session.Send(new DyzzPerInfoQuery(pid));
                        var pinfo = session.Get<Result<DyzzPerInfo>>();
                        //Console.WriteLine("\n" + JsonExtension.ToJson(pinfo));

                        if (pinfo.type == "warn" && pinfo.message != "")
                        {
                            memo = pinfo.message;
                        }
                        else if (pinfo.datas.Length > 0)
                        {
                            var id = pinfo.datas[0].id;
                            sjztny = pinfo.datas[0].ztny;

                            Console.Write($"{sjztny}|");
                        
                            DyzzBankInfo bkinfo = null;
                            session.Send(new DyzzBankInfoQuery(id));
                            var binfos = session.Get<Result<DyzzBankInfo>>();
                            //Console.WriteLine("\n" + JsonExtension.ToJson(binfos));
                        
                            if (binfos.datas.Length > 0)
                            {
                                bkinfo = binfos.datas[0];
                            }

                            var apro = new DyzzAccrualPro
                            {
                                zzyy = zzyy,
                                id = id,
                                zzrq = swsj,
                                sfyszf = "2",
                            };
                            session.Send(apro);
                            var apromxs = session.Get<Result<DyzzAccrualProMx>>();
                            //Console.WriteLine("\n" + JsonExtension.ToJson(apromxs));
                        
                            if (apromxs.datas.Length > 0)
                            {
                                var apromx = apromxs.datas[0];
                            
                                (dbfkje, ktzhje, dkje, yjhje, tkzje) = (apromx.dbfkje, apromx.ktzhje, apromx.dkje, apromx.yjhje, apromx.tkzje);
                                Console.Write($"{dbfkje}|{ktzhje}|{dkje}|{yjhje}|{tkzje}|");
                            
                                /*if (apromx.tkzje != "0" && bkinfo == null)
                                {
                                    //memo = "退款金额不等于0";
                                    memo = "无可退款账户";
                                }
                                else
                                {
                                    if (bkinfo == null) bkinfo = new DyzzBankInfo();
                                    session.Send(new DyzzPerSave(bkinfo, apro, apromx, bz));
                                    //memo = "TEST";
                                    //Console.WriteLine(new Service(new DyzzPerSave(bkinfo, apro, apromx, bz)));
                                    var result = session.Get<Result>();
                                    if (result.type == "info" && result.message == "")
                                        memo = "待遇终止成功";
                                    else
                                        memo = result.message;
                                }*/
                            }
                            else
                            {
                                if (apromxs.message != "")
                                    memo = apromxs.message;
                                else
                                    memo = "无法获取终止信息";
                            }
                        }
                        else
                        {
                            memo = "未找到个人信息";
                        }
                    }
                    //else
                    //{
                    //    memo = "待遇开始日期格式有误";
                    //}
                    
                    sheet.Row(i).CreateCell(12).SetValue(sjztny);
                    sheet.Row(i).CreateCell(13).SetValue(dbfkje);
                    sheet.Row(i).CreateCell(14).SetValue(ktzhje);
                    sheet.Row(i).CreateCell(15).SetValue(dkje);
                    sheet.Row(i).CreateCell(16).SetValue(yjhje);
                    sheet.Row(i).CreateCell(17).SetValue(tkzje);
                    sheet.Row(i).CreateCell(18).SetValue(memo);
                }
                Console.WriteLine(memo);
            }
        });
    }
    finally
    {
        workbook.Save(Utils.FileNameAppend(xls, ".new"));
        workbook.Close();   
    }
}


void dyztswry_zz(string xls = @"D:\数据核查\雨湖区2012到2016年历年暂停停人员名册表\雨湖区居保历年疑似死亡暂停人员街道汇总名册（无缴费记录）.xlsx",
              string bz = "无缴费不需稽核的已死亡待遇暂停人员", string zzyy = "1401")
{
    var workbook = ExcelExtension.LoadExcel(xls);
    var sheet = workbook.GetSheetAt(0);

    try
    {
        Session.Using(session =>
        {
            for (var i = 2; i <= sheet.LastRowNum; i++)
            {
                var pid = sheet.Cell(i, 2).StringCellValue;
                var name = sheet.Cell(i, 3).StringCellValue;
                var reason = sheet.Cell(i, 7)?.StringCellValue ?? "";
                var swsj = sheet.Cell(i, 8)?.CellValue() ?? "";
                
                Console.Write($"{i}:{pid}|{name}|{reason}|{swsj}|");

                var memo = "";
                var (sjztny, dbfkje, ktzhje, dkje, yjhje, tkzje) = ("", "", "", "", "", "");
                if (reason == "经核实已死亡" && swsj.Length >= 6)
                {
                    var jhje = sheet.Cell(i, 16).NumericCellValue;
                    var tkje = sheet.Cell(i, 17).NumericCellValue;

                    if (jhje != 0 || tkje != 0) continue;
                        
                    swsj = swsj.Substring(0, 4) + "-" + swsj.Substring(4, 2);
            
                    session.Send(new DyzzPerInfoQuery(pid));
                    var pinfo = session.Get<Result<DyzzPerInfo>>();
                    //Console.WriteLine("\n" + JsonExtension.ToJson(pinfo));

                    if (pinfo.type == "warn" && pinfo.message != "")
                    {
                        memo = pinfo.message;
                    }
                    else if (pinfo.datas.Length > 0)
                    {
                        var id = pinfo.datas[0].id;
                        sjztny = pinfo.datas[0].ztny;

                        Console.Write($"{sjztny}|");
                        
                        DyzzBankInfo bkinfo = null;
                        session.Send(new DyzzBankInfoQuery(id));
                        var binfos = session.Get<Result<DyzzBankInfo>>();
                        //Console.WriteLine("\n" + JsonExtension.ToJson(binfos));
                        
                        if (binfos.datas.Length > 0)
                        {
                            bkinfo = binfos.datas[0];
                        }

                        var apro = new DyzzAccrualPro
                        {
                            zzyy = zzyy,
                            id = id,
                            zzrq = swsj,
                            sfyszf = "2",
                        };
                        session.Send(apro);
                        var apromxs = session.Get<Result<DyzzAccrualProMx>>();
                        //Console.WriteLine("\n" + JsonExtension.ToJson(apromxs));
                        
                        if (apromxs.datas.Length > 0)
                        {
                            var apromx = apromxs.datas[0];
                            
                            (dbfkje, ktzhje, dkje, yjhje, tkzje) = (apromx.dbfkje, apromx.ktzhje, apromx.dkje, apromx.yjhje, apromx.tkzje);
                            Console.Write($"{dbfkje}|{ktzhje}|{dkje}|{yjhje}|{tkzje}|");
                            
                            if (apromx.tkzje != "0")
                            {
                                memo = "退款金额不等于0";
                            }
                            else if (apromx.yjhje != "0")
                            {
                                memo = "应稽核金额不等于0";
                            }
                            else
                            {
                                if (bkinfo == null) bkinfo = new DyzzBankInfo();
                                session.Send(new DyzzPerSave(bkinfo, apro, apromx, bz));
                                //memo = "TEST";
                                //Console.WriteLine(new Service(new DyzzPerSave(bkinfo, apro, apromx, bz)));
                                var result = session.Get<Result>();
                                if (result.type == "info" && result.message == "")
                                    memo = "待遇终止成功";
                                else
                                    memo = result.message;
                            }
                        }
                        else
                        {
                            if (apromxs.message != "")
                                memo = apromxs.message;
                            else
                                memo = "无法获取终止信息";
                        }
                    }
                    else
                    {
                        memo = "未找到个人信息";
                    }
                    
                    sheet.Row(i).CreateCell(12).SetValue(sjztny);
                    sheet.Row(i).CreateCell(13).SetValue(dbfkje);
                    sheet.Row(i).CreateCell(14).SetValue(ktzhje);
                    sheet.Row(i).CreateCell(15).SetValue(dkje);
                    sheet.Row(i).CreateCell(16).SetValue(yjhje);
                    sheet.Row(i).CreateCell(17).SetValue(tkzje);
                    sheet.Row(i).CreateCell(18).SetValue(memo);
                }
                Console.WriteLine(memo);
            }
        });
    }
    finally
    {
        workbook.Save(Utils.FileNameAppend(xls, ".new"));
        workbook.Close();   
    }
}

//unionJhdata();
//updateZtyy();
//queryJfqk();
//dispatchJhmc();
//dyztryzz();
//unionWjfjl();

//dyztswry_cs();

dyztswry_zz();
