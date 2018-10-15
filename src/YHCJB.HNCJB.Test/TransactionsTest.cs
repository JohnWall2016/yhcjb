using System;
using Xunit;
using YHCJB.HNCJB;

namespace YHCJB.HNCJB.Test
{
    public class TransTest
    {
        [Fact]
        public void TestPausePayInfo()
        {
            Session.Using(session =>
            {
                var query = new PausePayInfoQuery("430321195507240523");
                Console.WriteLine(query);
                session.Send(query);
                var info = session.Get<Result<PausePayInfo>>();
                Console.WriteLine(info);
                
                Console.WriteLine($"{info[0].Idcard}|{info[0].Name}|{info[0].PauseTime}|{info[0].PauseReason}|{info[0].Memo}");
            });
        }
    }
}
