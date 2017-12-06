using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HexChain
{
    partial class Program
    {
        public static ConcurrentBag<BlockChain> HexChains = new ConcurrentBag<BlockChain>();

        /// <summary>
        /// Tracks local messages send to the hexchain from the jinx client
        /// </summary>
        public static ConcurrentQueue<string> JinxBuffer = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> JinxBufferFlag = new ConcurrentQueue<string>();

        /// <summary>
        /// Tracks global transaction placed on the hexchain
        /// </summary>
        public static ConcurrentQueue<string> HexChainBuffer = new ConcurrentQueue<string>();

        public static string PublicID;
        public static string LicenseKey;

        public enum BroadcastMode
        {
            Send,
            SendValidBlock,
            Discovery
        }

        static void Main(string[] args)
        {
            try
            {
                PublicID = "ID[" + Guid.NewGuid() + "]";
                var appSettings = System.Configuration.ConfigurationManager.AppSettings;
                LicenseKey = appSettings["License"];
                var localHost = appSettings["Host"];
                var peers = new List<string>();
                //get all the peers to broadcast from appsettings keys
                appSettings.AllKeys.ToList().Where(k => k.StartsWith("Peer"))
                    .ToList().ForEach(delegate (string key) { peers.Add(appSettings[key]); });

                //Create servers for local comms, and global comms
                var hexHost = new GlobalInterfaceServer(localHost,peers);
                //use same local host endpoint for jinx host endpoint
                var jinxHost = new LocalInterfaceServer(
                    localHost.Replace("HexChainService", "JinxService"),
                    appSettings["clientBinding"]);

                //create a new thread to launch the local webservice host
                Thread jinxHostThread = new Thread(jinxHost.Listen);
                jinxHostThread.Start();




                //create a new hexchain and add to 
                //hexchains concurrent list
                var hexchain = new BlockChain();
                hexchain.difficulty = 2;
                HexChains.Add(hexchain);

               

                //create a new thread to launch the global webservice host
                Thread hexHostThread = new Thread(hexHost.Listen);
                hexHostThread.Start();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            Console.WriteLine("HexChain Node has shutdown.");
            Console.Read();
        }
        
        
      
    }
}


