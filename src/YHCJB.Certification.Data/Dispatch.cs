using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NPOI.HSSF.UserModel;

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

        static void DumpDwMap(Dictionary<string, Dictionary<string, List<int>>> map)
        {
            foreach (var dw in map.Keys)
            {
                foreach(var cs in map[dw].Keys)
                {
                    Console.WriteLine($"{dw}-{cs}");//": {DumpList(map[dw][cs])}");
                }
            }
        }
        
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: dotnet run <xls> <outdir>");
                return;
            }
            (var file, var dir) = (args[0], args[1]);
            Console.WriteLine($"{file} -> {dir}");
            var wbook = new HSSFWorkbook(new FileStream(file, FileMode.Open));
            var sheet = wbook.GetSheetAt(0);
            var dwMap = new Dictionary<string, Dictionary<string, List<int>>>();
            for (var i = 1; i <= sheet.LastRowNum; i++)
            {
                var region = sheet.GetRow(i).GetCell(0).StringCellValue;
                var state = sheet.GetRow(i).GetCell(6).StringCellValue;
                var nextTime = sheet.GetRow(i).GetCell(8).StringCellValue;
                if (state == "1" && nextTime == "201809")
                {
                    var matched = false;
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
                                dwMap[dw] = new Dictionary<string, List<int>>();
                            }
                            if (!dwMap[dw].ContainsKey(cs))
                            {
                                dwMap[dw][cs] = new List<int>(){};
                            } else
                            {
                                dwMap[dw][cs].Add(i);
                            }
                            
                            matched = true;
                            break;
                        }
                    }
                    if (!matched)
                    {
                        throw new ApplicationException($"无法找到分组: {region}");
                    }
                }
            }
            DumpDwMap(dwMap);
            wbook.Close();
        }
    }
}
