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
        public static ConcurrentBag<BlockChain> _cb = new ConcurrentBag<BlockChain>();
        public static ConcurrentQueue<string> JinxBuffer = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> JinxBufferFlag = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> HexChainBuffer = new ConcurrentQueue<string>();

        static void Main(string[] args)
        {
            try
            {
                var appSettings = System.Configuration.ConfigurationManager.AppSettings;

                var hostNodes = new List<string>();
                appSettings.AllKeys.ToList().ForEach(delegate (string k) { if (k.StartsWith("Host")) { hostNodes.Add(appSettings[k]); } });

                //strat multiple clients for debugging
                var thisHostEndpoint = hostNodes[0];
              
                //if (hostNodes.Count > 0)
                //{
                //    //use default host
                //    foreach (var host in hostNodes)
                //    {
                //        if (host != thisHostEndpoint)
                //            ProgramInstances(host);
                //    }
                //}


                ServiceHost uxSvcHost = new ServiceHost(typeof(JinxService));

                try
                {

                    var jx = thisHostEndpoint.Replace("HexChainService", "JinxService");

                    uxSvcHost.AddServiceEndpoint("IJinxService",new BasicHttpBinding(), jx);

                    //uxSvcHost.AddServiceEndpoint("IJinxService", new NetTcpBinding(SecurityMode.None, false),jx.Replace("http", "net.tcp"));

                    uxSvcHost.Open();

                    Console.WriteLine("Jinx Service started. {0} \nWaiting for Input.", jx);
                }
                catch (Exception eX)
                {
                    uxSvcHost = null;
                    throw new ArgumentException("Host Service can not be started \n\nError Message [" + eX.Message + "]");
                }
                if (uxSvcHost == null)
                {
                    throw new ArgumentException("svchost could not be established");
                }

                
                new Program().Run(thisHostEndpoint);
                
                
                uxSvcHost.Close();
                uxSvcHost = null;
                Console.WriteLine("Jinx Service stopped.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }


            Console.WriteLine("Node has shutdown.");
            Console.Read();
        }

        
        private static void ProgramInstances(string host)
        {
            
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() <= 1)
            {
                
                    Process.Start("HexChain.exe",host);                
            }
        }

        public void Run(string hostEndpoint)
        {
            var PublicKey = "ID[" + Guid.NewGuid() + "]";
            
            WebServiceHost host = new WebServiceHost(typeof(HexChainService), new Uri(hostEndpoint));
            ServiceEndpoint ep = host.AddServiceEndpoint(typeof(IHexChainService), new WebHttpBinding(), "");
            ServiceDebugBehavior sdb = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            sdb.HttpHelpPageEnabled = false;
            host.Open();
            Console.WriteLine("HexChain Service is running at {0}",hostEndpoint);
            
            var jinx = new BlockChain();
            jinx.difficulty = 2;
            _cb.Add(jinx);

            //clients to broadcast data too
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            var peerNodes = new List<string>();
            appSettings.AllKeys.ToList().ForEach(delegate (string k) { if (k.StartsWith("Peer")) { peerNodes.Add(appSettings[k]); } });
            
            while (true)
            {
                //read from the local jinx client buffer
                string msg;
                var newMsg = JinxBuffer.TryDequeue(out msg);
                if (newMsg) {
                    //process the local clients message as a new transaction
                    ProcessMsg(PublicKey, peerNodes, msg);
                }

                //read from the hexchain any global messages
                string trans;
                var newTrans = HexChainBuffer.TryDequeue(out trans);
                if (newTrans)
                {
                    ProcessTransaction(PublicKey, peerNodes, trans);
                }
                

            }

            host.Close();

        }

        private static void ProcessTransaction(string PublicKey, List<string> peerNodes, string msg)
        {

            var trans = JsonConvert.DeserializeObject<Transaction>(msg);
            
            //try to mine the block and add it to the blockchain
            var testChain = _cb.ToArray()[0];

            testChain.addBlock(trans);
            var block = testChain.getLatestBlock();

            //check if the block added was valid
            if (testChain.IsChainValid())
            {

                _cb.ToArray()[0].chain.Add(block);
                //broadcast the block so everyone can mine it
                var jsonBlock = JsonConvert.SerializeObject(block, Formatting.Indented);
                //give all the peers the transaction to mine it
                BroadcastValidBlock(peerNodes, jsonBlock);
            }
        }

        private static void ProcessMsg(string PublicKey, List<string> peerNodes, string msg)
        {

            //start working on new message transaction
            Program.JinxBufferFlag.Enqueue("startflag");

            //build a block for the new transaction
            var trans =
                new Transaction()
                {
                    public_key = PublicKey,
                    data = "val:" + msg
                };


            Program.HexChainBuffer.Enqueue(JsonConvert.SerializeObject(trans));

            //give all the peers the transaction to mine it
            Broadcast(peerNodes, JsonConvert.SerializeObject(trans));

            var flag = "";
            Program.JinxBufferFlag.TryDequeue(out flag);
        }
    

        private static void Broadcast(List<string> peerNodes, string msg)
        {
            foreach (var peer in peerNodes)
            {
                try
                {
                    using (ChannelFactory<IHexChainService> cf =
                      new ChannelFactory<IHexChainService>(
                          new WebHttpBinding(), peer))
                    {
                        cf.Endpoint.Behaviors.Add(new WebHttpBehavior());

                        IHexChainService channel = cf.CreateChannel();

                        //Console.WriteLine("BroadCasting to {0}", peer);
                        //broadcasting a transaction to be mined by all the nodes
                        var s = channel.Send(msg);

                        //Console.WriteLine("Response Output:{0}", s);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\t Not Reachable : {0}", peer);
                    // Console.WriteLine(ex.Message);
                }
            }
        }
        private static void BroadcastValidBlock(List<string> peerNodes, string msg)
        {
            foreach (var peer in peerNodes)
            {
                try
                {
                    using (ChannelFactory<IHexChainService> cf =
                      new ChannelFactory<IHexChainService>(
                          new WebHttpBinding(), peer))
                    {
                        cf.Endpoint.Behaviors.Add(new WebHttpBehavior());

                        IHexChainService channel = cf.CreateChannel();

                        
                        var s = channel.SendValidBlock(msg);

                        //Console.WriteLine("Response Output:{0}", s);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("\t Not Reachable : {0}", peer);
                    // Console.WriteLine(ex.Message);
                }
            }
        }

    }
}


