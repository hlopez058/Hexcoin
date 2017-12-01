using Newtonsoft.Json;
using System;
using System.ServiceModel;
using System.Threading;

namespace HexChain
{

    //Contract for our network. It says we can 'ping'
    [ServiceContract(CallbackContract = typeof(IPing))]
    public interface IPing
    {
        [OperationContract(IsOneWay = true)]
        void Ping(string sender, string message);


        [OperationContract(IsOneWay = true)]
        void BlockBroadcast(string sender, string message);

        [OperationContract(IsOneWay = true)]
        void BlockBlast(string sender, string message);

    }


    public class PingImplementation : IPing
    {
        public void BlockBlast(string sender, string message)
        {
            var block =JsonConvert.DeserializeObject<Block>(message);

            Program._cb.ToArray()[0].addBlock(block);

            if (Program._cb.ToArray()[0].IsChainValid())
            {
                Console.WriteLine("{0}:{1}", sender,
                    Program._cb.ToArray()[0].getLatestBlock().hash);
            }
            else {
                Console.WriteLine("NOTVALID|{0}:{1}", sender,
                    Program._cb.ToArray()[0].getLatestBlock().hash);
                //Program._cb.ToArray()[0].chain
                //    .RemoveAt(Program._cb.ToArray()[0].chain.Count - 1);
            }

            string flag;
            Program.JinxBufferFlag.TryDequeue(out flag);
        }

        public void Ping(string sender, string message)
        {
            Console.WriteLine("{0} says: {1}", sender, message);
        }

        public void BlockBroadcast(string sender, string message)
        {
            var block = Newtonsoft.Json.JsonConvert
                .DeserializeObject<Block>(message);

            //mine the block with chain on seperate thread
        
            //var miner = new Miner(Program.GetLongestBlockChain(), block);
            //var miningThread = new Thread(miner.miningRun) {
            //    IsBackground = true };
            //miningThread.Start();

            //miningThread.Join();


            //var lastblock = Program.GetLongestBlockChain().getLatestBlock();
           // var jsonLastBlock = Newtonsoft.Json.JsonConvert.SerializeObject(blockChain.getLatestBlock());
           // Console.WriteLine("{0} ->: {1}", sender, jsonLastBlock);

        }



        private class Miner
        {
            public Miner(BlockChain blockChain, Block block)
            {
                this.block = block;
                this.blockChain = blockChain;
            }
            public BlockChain blockChain { get; set; }
            public Block block { get; set; }
            public void miningRun()
            {
                blockChain.addBlock(block);
                if (blockChain.IsChainValid())
                {
                    Program._cb.Add(blockChain);
              
                }
            }
        }
    }
}
