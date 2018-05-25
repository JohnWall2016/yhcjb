using System;
using YHCJB.HNCJB;
using YHCJB.Util;

namespace YHCJB.Cmd.Cbsh
{
    class Program
    {
        static void Main(string[] args)
        {
            Session.Using((session) => {
                    session.Send("cbshQuery", new CbshQuery());
                    var res = Service.FromJson<CbshResult>(session.Get());
                    var saves = new CbshGotoSave();
                    foreach (var r in res.datas)
                    {
                        var remark = (r.birthday < 19510701) ? "不用缴费的到龄人员不自动审核" :
                            (r.type != "011") ? "特殊参保身份人员不自动审核" : "一般参保缴费人员";
                        r.name.AlignRight(10)
                            .AlignRight(r.pid, 20)
                            .AlignRight($"{r.birthday}", 10)
                            .AlignRight(r.type, 5)
                            .AlignRight("", 2)
                            .AlignRight(remark, 0)
                            .Println();
                        if (r.birthday >= 19510701 && r.type == "011")
                            saves.AddRow(r.ToCbshSave());
                    }
                    //Console.WriteLine(new Service("cbshGotoSave", saves));
                    /*session.Send("cbshGotoSave", saves);
                    var result = Service.FromJson<Result>(session.Get());
                    "".Println();
                    (result.type + ":").AlignLeft(10).AlignLeft(result.message, 0).Println();*/
                });
        }
    }
}
