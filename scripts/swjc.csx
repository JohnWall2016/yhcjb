#! "netcoreapp2.1"
#r "../src/YHCJB.Util/bin/Debug/netstandard2.0/YHCJB.Util.dll"
#r "../src/YHCJB.HNCJB/bin/Debug/netstandard2.0/YHCJB.HNCJB.dll"

using YHCJB.HNCJB;
using YHCJB.Util;

Session.Using(session =>
{
    var query = new PausePayInfoQuery("430321195507240523");
    Console.WriteLine(query);
    session.Send(query);
    var info = session.Get<Result<PausePayInfo>>();
    Console.WriteLine(info);
    Console.WriteLine($"{info[0].Idcard}|{info[0].Name}|{info[0].PauseTime}|{info[0].PauseReason}|{info[0].Memo}");
});
