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

namespace HexChain
{
    class Program
    {
        public static ConcurrentBag<BlockChain> _cb = new ConcurrentBag<BlockChain>();
        public static ConcurrentQueue<string> cq = new ConcurrentQueue<string>();
        public static ConcurrentQueue<string> cqf = new ConcurrentQueue<string>();

        static void Main(string[] args)
        {
            try
            {
                //create a selfhosted app for interfacing with 
                //local UX clients
                ServiceHost svcHost = null;
                try
                {
                    svcHost = new ServiceHost(typeof(JinxService));
                 

                    svcHost.Open();
                }
                catch (Exception eX)
                {
                    svcHost = null;
                    throw new ArgumentException("Jinx Service can not be started \n\nError Message [" + eX.Message + "]");
                }
                if (svcHost == null)
                {
                    throw new ArgumentException("svchost could not be established");
                }


                //strat multiple clients for debugging
                var maxClients = 0;
                if (args.Count() > 0)
                {
                    maxClients = Convert.ToInt16(args[0]);
                }
                if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() <= 1)
                {
                    for (int i = 0; i < maxClients; i++)
                    {
                        Process.Start("HexChain.exe");
                    }
                }

                new Program().Run();
                
                Console.WriteLine("closing svchost");
                svcHost.Close();
                svcHost = null;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.WriteLine("HexChain stopped.");
            Console.Read();
        }

        public void Run()
        {
            var PublicKey = "ID[" + Guid.NewGuid() + "]";

            var peer = new Peer(PublicKey);
            var peerThread = new Thread(peer.Run) { IsBackground = true };
            peerThread.Start();

            //wait for the server to start up.
            Thread.Sleep(1000);

            var jinx = new BlockChain();
            jinx.difficulty = 2;

            _cb.Add(jinx);

            while (true)
            {   
                string msg;
                var newMsg = cq.TryDequeue(out msg);
                if (!newMsg) continue;

                Program.cqf.Enqueue("startflag");

                //build a block for the new transaction
                var trans =
                    new Transaction()
                    {
                        public_key = PublicKey,
                        data = "val:" + msg
                    };

                //we build a block from the transaction 
                //but do not add it to the chain
                var block = _cb.ToArray()[0].buildBlock(trans);

                //broadcast the block so everyone can mine it
                var jsonBlock = JsonConvert.SerializeObject(block,Formatting.Indented);


                //broadcast to all nodes (even this one)
                peer.Channel.BlockBlast(peer.Id, jsonBlock);
                
            }

            peer.Stop();
            peerThread.Join();

        }
        
    }
}
