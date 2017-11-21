using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Xcfg.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var connectStrings = ConnectionStrings.Instance;

                foreach (var connectStringsConn in connectStrings.Conns)
                {
                    System.Console.WriteLine(connectStringsConn.Name);
                }
                Thread.Sleep(1000);
            }
           
            System.Console.Read();
        }
    }
}
