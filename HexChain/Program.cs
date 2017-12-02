using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.ServiceModel;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.Collections.ObjectModel;
using System.ServiceModel.Web;

namespace HexChain
{
    class Program
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

        public class LocalInterfaceServer
        {
            string LocalHostEndpoint { get; set; }
            string LocalClientBinding { get; set; }
            ServiceHost svchost;

            public LocalInterfaceServer(string localHostEndpoint, string localClientBinding)
            {
                this.LocalClientBinding = localClientBinding;
                this.LocalHostEndpoint =  localHostEndpoint;
                
            }

            public void Create()
            {
                this.svchost = new ServiceHost(typeof(JinxService));
                try
                {
                    var jx = LocalHostEndpoint;
                    var jxtcp = LocalHostEndpoint.Replace("http", "net.tcp");
                    if (LocalClientBinding != "tcp")
                    {
                        this.svchost.AddServiceEndpoint("IJinxService", 
                            new BasicHttpBinding(), jx);
                    }
                    else
                    {
                        this.svchost.AddServiceEndpoint("IJinxService", 
                            new NetTcpBinding(SecurityMode.None, false), jxtcp);
                    }
                }
                catch (Exception eX)
                {
                    this.svchost = null;
                    throw new ArgumentException("Jinx Host Service can not be started \n\nError Message [" + eX.Message + "]");
                }
                if (this.svchost == null)
                {
                    throw new ArgumentException("Jinx Host Service could not be established");
                }

            }

            public void Open()
            {
                this.svchost.Open();
                Console.WriteLine("Jinx Host Service Listening. {0}.", LocalHostEndpoint);
            }
            public void Close()
            {
               this.svchost.Close();
               this.svchost = null;
               Console.WriteLine("Jinx Host Service stopped.");
            }

            
        }
        public enum BroadcastMode
        {
            Send,
            SendValidBlock,
            Discovery
        }

        public class GlobalInterfaceServer
        {
            WebServiceHost svchost;
            string LocalHostEndpoint { get; set; }
            List<string> peers;

            public GlobalInterfaceServer(string localHostEndpoint, List<string> peers)
            {
                this.LocalHostEndpoint = localHostEndpoint;
                this.peers = peers;
            }

            public void Create()
            {
                this.svchost = new WebServiceHost(typeof(HexChainService), new Uri(LocalHostEndpoint));
                ServiceEndpoint ep = svchost.AddServiceEndpoint(typeof(IHexChainService), new WebHttpBinding(), "");
                ServiceDebugBehavior sdb = svchost.Description.Behaviors.Find<ServiceDebugBehavior>();
                sdb.HttpHelpPageEnabled = false;
            }

            public void Open()
            {
                svchost.Open();
                Console.WriteLine("HexChain WebService is Running. {0}", LocalHostEndpoint);
            }

            public void Close()
            {
                this.svchost.Close();
                Console.WriteLine("HexChain WebService has Stopped. {0}", LocalHostEndpoint);

            }

            public void Discovery()
            {

                Broadcast("searching", BroadcastMode.Discovery);

            }

            public void Broadcast(string message,BroadcastMode mode)
            {
                var msg = Encrypt.EncryptString(message, LicenseKey);
               
                var peers_Unreachable = new List<string>();
                foreach (var peer in this.peers)
                {
                    try
                    {
                        using (var cf = new ChannelFactory<IHexChainService>
                            (new WebHttpBinding(), peer))
                        {
                            cf.Endpoint.Behaviors.Add(new WebHttpBehavior());
                            IHexChainService channel = cf.CreateChannel();
                            switch (mode)
                            {
                                case BroadcastMode.Send:
                                    channel.Send(msg);break;
                                case BroadcastMode.SendValidBlock:
                                    channel.SendValidBlock(msg); break;
                                case BroadcastMode.Discovery:
                                    var id = channel.Discover(msg);
                                    Console.WriteLine("Found : {0} at {1}", id, peer);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Console.WriteLine(ex.Message);
                        if (mode == BroadcastMode.Discovery)
                        {
                            peers_Unreachable.Add(peer);
                        }
                        Console.WriteLine("Not Reachable : {0}", peer);
                    }
                }

                foreach(var peer in peers_Unreachable)
                {
                    this.peers.Remove(peer);
                }
            }
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
                
                //Start the local interface web service (SOAP)
                jinxHost.Create();
                jinxHost.Open();

                //Start the global interface web service (REST)
                hexHost.Create();
                hexHost.Open();

                Console.WriteLine("Press any key to start discovery of peers...");
                Console.ReadLine();
                hexHost.Discovery();


                //create a new hexchain and add to 
                //hexchains concurrent list
                var hexchain = new BlockChain();
                hexchain.difficulty = 2;
                HexChains.Add(hexchain);
                
                //do work handling input from jinx client
                //process any global values coming in
                while (true)
                {
                    //read from the hexchain any global messages
                    string transm;
                    var newTrans = HexChainBuffer.TryDequeue(out transm);
                    if (newTrans)
                    {

                        //send this transaction to all the other nodes
                        //hexHost.Broadcast(transm, BroadcastMode.Send);

                        var trans = JsonConvert.DeserializeObject<Transaction>(transm);

                        //try to mine the block with the transaction
                        //and add it to the blockchain
                        HexChains.First().addBlock(trans);

                        var block = HexChains.First().getLatestBlock();

                        //check if the block added was valid
                        if (HexChains.First().IsChainValid())
                        {
                          
                            //broadcast the block so everyone can mine it
                            var jsonBlock = JsonConvert.SerializeObject(block, Formatting.Indented);
                            
                            //give all the peers a valid block
                            hexHost.Broadcast(jsonBlock, BroadcastMode.SendValidBlock);
                            Console.WriteLine("Valid Block Added :\n\t{0}", block.hash);
                        }
                        else
                        {
                            HexChains.First().chain.RemoveAt(HexChains.First().chain.Count);
                            Console.WriteLine("Invalid Block Rejected :\n\t{0}", block.hash);
                        }
                    }
                }

                hexHost.Close();
                jinxHost.Close();
                
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


