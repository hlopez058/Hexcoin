using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JinxCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Started Cmdline JinxService Interface");

            while (true)
            {
                Console.Write("Command:");
                var message = Console.ReadLine();

                if (message == "") { break; }
                var jinxsvc = new JinxServiceClient("NetTcpBinding_IJinxService");

                var resp = jinxsvc.Send(message);

                var lstblock = jinxsvc.Read();

                Console.WriteLine("Response:" + resp);
                Console.WriteLine("LastBlock:" + lstblock);
            }

            Console.WriteLine("Exited.");
            Console.Read();
        }
    }
}
