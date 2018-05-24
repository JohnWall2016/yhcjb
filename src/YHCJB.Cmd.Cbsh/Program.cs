using System;
using YHCJB.HNCJB;

namespace YHCJB.Cmd.Cbsh
{
    class Program
    {
        static void Main(string[] args)
        {
            Session.Using((session) => {
                    Console.WriteLine(session.Login());
                    Console.WriteLine(session.Logout()); 
                });
        }
    }
}
