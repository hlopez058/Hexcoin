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
            var jinxsvc = new JinxServiceClient("BasicHttpBinding_IJinxService");
            Console.WriteLine(
            jinxsvc.Endpoint.Address);

            while (true)
            {
                Console.Write("Command:");
                var message = Console.ReadLine();

                if (message == "") { break; }

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
