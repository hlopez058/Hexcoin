using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using System.Linq;

namespace HexChain
{
    partial class Program
    {
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
                                    channel.Send(msg,"false");break;
                                case BroadcastMode.SendValidBlock:
                                    channel.Send(msg,"true"); break;
                                case BroadcastMode.Discovery:
                                    var id = channel.Discover(msg);
                                    Console.WriteLine("Found : {0} at {1}", id, peer);
                                    break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (mode == BroadcastMode.Discovery)
                        {
                            peers_Unreachable.Add(peer);
                        }
                        // Console.WriteLine("Not Reachable : {0} , {1} - {2} - {3}", peer,ex.Message,ex.InnerException,msg);
                        Console.WriteLine("Not Reachable : {0} ", peer);
                    }
                }

                foreach(var peer in peers_Unreachable)
                {
                    this.peers.Remove(peer);
                }
            }

            internal void Listen()
            {

                //Start the global interface web service (REST)
                this.Create();
                this.Open();

                Console.WriteLine("Press any key to start discovery of peers...");
                Console.ReadLine();
                this.Discovery();


                while (this.svchost.State == CommunicationState.Opened)
                {

                    //do blockchain transactions here
                    //do work handling input from jinx client
                    //process any global values coming in
                    while (true)
                    {

                        //send this transaction to all the other nodes
                        string msg;
                        var newMsg = JinxBuffer.TryDequeue(out msg);
                        if (newMsg)
                        {
                            this.Broadcast(msg, BroadcastMode.Send);
                        }

                        //read from the hexchain any global messages
                        string transm;
                        var newTrans = HexChainBuffer.TryDequeue(out transm);
                        if (newTrans)
                        {

                            var trans = JsonConvert.DeserializeObject<Transaction>(transm);

                            if (trans.public_key == PublicID)
                            {
                                //the transaction sent by 
                            }

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
                                this.Broadcast(jsonBlock, BroadcastMode.SendValidBlock);
                                Console.WriteLine("Valid Block Added :\n\t{0}", block.hash);
                            }
                            else
                            {
                                HexChains.First().chain.RemoveAt(HexChains.First().chain.Count);
                                Console.WriteLine("Invalid Block Rejected :\n\t{0}", block.hash);
                            }
                        }
                    }
                }
                this.Close();
            }
        }
        
        
      
    }
}


