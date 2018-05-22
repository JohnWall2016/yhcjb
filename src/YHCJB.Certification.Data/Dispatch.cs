using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;
using YHCJB.Util;
using System.Linq;

namespace YHCJB.Certification.Data
{
    class Dispatch
    {
        static readonly string[] dwPatterns = {
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

        static string DumpList<T>(List<T> list)
        {
            string ret = "[";
            var len = list.Count;
            if (len > 0)
            {
                ret += $"{list[0]}";
            }
            if (len > 1)
            {
                for (var i = 1; i < len; i++)
                {
                    ret += $" ,{list[i]}";
                }
            }
            ret += "]";
            return ret;
        }

        static void DwMapStatics(Dictionary<string, Dictionary<string, CertRowInfo>> map)
        {
            var certedTotle = 0;
            var uncertedTotle = 0;
            var phoneCertedTotle = 0;
            const string hj = "合计";
            const string yrz = "已认证";
            const string wrz = "未认证";
            const string sjrz = "手机认证";
            foreach (var dw in map.Keys)
            {
                var certedTotleDw = 0;
                var uncertedTotleDw = 0;
                var phoneCertedTotleDw = 0;
                $"{dw.AlignLeft(12)}  {yrz.AlignRight(10)} {sjrz.AlignRight(10)} {wrz.AlignRight(10)} {hj.AlignRight(10)}".Println();
                "============================================================".Println();
                foreach(var cs in map[dw].Keys)
                {
                    var certed = map[dw][cs].CertedRows.Count;
                    var uncerted = map[dw][cs].UncertedRows.Count;
                    var phoneCerted = map[dw][cs].PhoneCertedRows.Count;
                    certedTotleDw += certed;
                    uncertedTotleDw += uncerted;
                    phoneCertedTotleDw += phoneCerted;
                    $"{cs.AlignRight(12)}: {certed,10} {phoneCerted, 10} {uncerted,10}".Println();
                }
                $"{hj.AlignRight(12)}: {certedTotleDw,10} {phoneCertedTotleDw,10} {uncertedTotleDw,10} {certedTotleDw+uncertedTotleDw,10}".Println();
                "============================================================".Println();
                certedTotle += certedTotleDw;
                uncertedTotle += uncertedTotleDw;
                phoneCertedTotle += phoneCertedTotleDw;
            }
            $"{hj.AlignRight(12)}: {certedTotle,10} {phoneCertedTotle,10} {uncertedTotle,10} {certedTotle+uncertedTotle,10}".Println();
        }

        static void DwMapDispatch(Dictionary<string, Dictionary<string, CertRowInfo>> map, HSSFSheet inSheet, string outDir, string xlsTmpl)
        {
            if (!Directory.Exists(outDir))
                Directory.CreateDirectory(outDir);
            foreach (var dw in map.Keys/*.Where(k => k == "城正街街道")*/)
            {
                $"导出 {dw}".Println();
                var dwDir = Path.Combine(outDir, dw);
                if (!Directory.Exists(dwDir))
                    Directory.CreateDirectory(dwDir);
                foreach (var cs in map[dw].Keys)
                {
                    $"    {cs}".Println();
                    var outBook = new HSSFWorkbook(new FileStream(xlsTmpl, FileMode.Open));
                    var outSheet = (HSSFSheet)outBook.GetSheetAt(0);

                    var srcRowIdx = 4;
                    var dstRowIdx = srcRowIdx;

                    outSheet.GetRow(1).GetCell(2).SetCellValue(dw+cs);

                    var rowInfo = map[dw][cs];
                    foreach (var idx in rowInfo.UncertedRows)
                    {
                        var inRow = inSheet.GetRow(idx);
                        var name = inRow.GetCell(2).StringCellValue;
                        var id = inRow.GetCell(3).StringCellValue;
                        var sex = (inRow.GetCell(4).StringCellValue == "1") ? "男" : "女";
                        var region = inRow.GetCell(0).StringCellValue;
                        
                        var outRow = outSheet.GetOrCopyRowFrom(dstRowIdx, srcRowIdx);
                        outRow.GetCell(0).SetCellValue(dstRowIdx - srcRowIdx + 1);
                        outRow.GetCell(1).SetCellValue(name);
                        outRow.GetCell(2).SetCellValue(sex);
                        outRow.GetCell(3).SetCellValue(id);
                        outRow.GetCell(4).SetCellValue(region);

                        dstRowIdx ++;
                    }
                    if (dstRowIdx > srcRowIdx) // has data
                    {
                        var outFile = Path.Combine(dwDir, cs + ".xls");
                        if (File.Exists(outFile))
                            File.Delete(outFile);
                        using (var outStream = new FileStream(outFile, FileMode.CreateNew))
                        {
                            outBook.Write(outStream);
                        }
                    }
                    outBook.Close();
                }
            }
        }

        class CertRowInfo
        {
            internal CertRowInfo(string area)
            {
                Area = area;
                CertedRows = new List<int>();
                UncertedRows = new List<int>();
                PhoneCertedRows = new List<int>();
            }
            internal string Area { get; }
            internal List<int> CertedRows { get; }
            internal List<int> UncertedRows { get; }
            internal List<int> PhoneCertedRows { get; }
            internal void AddCertedRow(int row) => CertedRows.Add(row);
            internal void AddUncertedRow(int row) => UncertedRows.Add(row);
            internal void AddPhoneCertedRow(int row) => PhoneCertedRows.Add(row);
        }
        
        static void Main(string[] args)
        {
            var certedBefore = 201809;
            var xlsTmpl = @"D:\生存认定\2018年\2018年度雨湖区城乡居民基本养老保险待遇领取人员资格认证表（表二）.xls";
            
            if (args.Length != 2)
            {
                $"usage: dotnet run [xls] [outdir|-s]".Println();
                return;
            }
            
            (var file, var dir) = (args[0], args[1]);

            var statics = (dir == "-s") ? true : false;
            if (statics)
                $"{file} -> 统计数据".Println();
            else
                $"{file} -> 导出未认证数据到: {dir}".Println();
            
            var wbook = new HSSFWorkbook(new FileStream(file, FileMode.Open));
            var sheet = (HSSFSheet)wbook.GetSheetAt(0);
            var dwMap = new Dictionary<string, Dictionary<string, CertRowInfo>>();
            
            for (var i = 1; i <= sheet.LastRowNum; i++)
            {
                var region = sheet.GetRow(i).GetCell(0).StringCellValue;
                var state = sheet.GetRow(i).GetCell(6).StringCellValue;
                var nextTime = int.Parse(sheet.GetRow(i).GetCell(8).StringCellValue);
                var certedType = sheet.GetRow(i).GetCell(7).StringCellValue;
                if (state == "1")
                {
                    CertRowInfo current = null;
                    foreach (var pat in dwPatterns)
                    {
                        var match = Regex.Match(region, pat);
                        if (match.Length > 0)
                        {
                            var dw = match.Groups[2].Value;
                            var cs = match.Groups[3].Value;
                            //Console.WriteLine($"{dw} - {cs}");
                            
                            if (!dwMap.ContainsKey(dw))
                            {
                                dwMap[dw] = new Dictionary<string, CertRowInfo>();
                            }
                            if (!dwMap[dw].ContainsKey(cs))
                            {
                                current = new CertRowInfo(cs);
                                dwMap[dw][cs] = current;
                            }
                            else
                            {
                                current = dwMap[dw][cs];
                            }
                            break;
                        }
                    }
                    if (current == null)
                    {
                        throw new ApplicationException($"无法找到分组: {region}");
                    }
                    if (nextTime <= certedBefore) // uncertified
                    {
                        current.AddUncertedRow(i);
                    }
                    else // certified
                    {
                        current.AddCertedRow(i);
                        if (certedType == "02") current.AddPhoneCertedRow(i);
                    }
                }
            }
            
            if (statics)
                DwMapStatics(dwMap);
            else
                DwMapDispatch(dwMap, sheet, dir, xlsTmpl);
            wbook.Close();
        }
    }
}
