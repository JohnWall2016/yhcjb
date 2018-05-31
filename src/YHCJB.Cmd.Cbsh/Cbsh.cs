using System;
using YHCJB.HNCJB;
using YHCJB.Util;

namespace YHCJB.Cmd.Cbsh
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Session.Using((session) =>
            {
                session.Send(new CbshQuery());
                var res = session.Get<Result<YHCJB.HNCJB.Cbsh>>();
                var saves = new CbshGotoSave();
                foreach (var r in res.datas)
                {
                    var remark = (r.birthday < 19510701) ? _msg_byjfdl :
                        (r.type != "011") ? _msg_tscb : _msg_ybjf;
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
                session.Send(saves);
                var result = session.Get<Result>();
                "".Println();
                (result.type + ":").AlignLeft(10).AlignLeft(result.message, 0).Println();
            });
        }
    }
}
