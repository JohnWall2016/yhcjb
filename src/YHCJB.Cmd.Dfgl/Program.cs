using System;
using System.Collections;
using YHCJB.HNCJB;
using YHCJB.Util;
using System.Linq;

namespace YHCJB.Cmd.Dfgl
{
    partial class Program
    {
        static void print(string s) => Console.WriteLine(s);
        
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                _usage.Println();
                return;
            }

            if (args[0] == "zcdf")
                ExportZcdf(args.Skip(1));
            else if (args[1] == "zfmx")
                ExportZfmx(args.Skip(1));
            else
                Error("参数有误");
        }

        static void ExportZcdf(params string[] args)
        {
            if (args.Length != 2) Error("参数有误");
            
            (var dflx, var bcny) = (args[0], args[1]);
            
            Session.Using(session => {
                    session.Post("executeDfrymdQuery", new DfrymdQuery(dflx));
                    var res = Service.FromJson<DfrymdResult>(session.Get());
                    foreach (var item in res.datas)
                    {
                        
                    }
                });
        }

        static void ExportZfmx(params string[] args)
        {
            
        }
    }
}
