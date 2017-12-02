using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace HexChain
{
    public class Transaction
    {
        public string public_key;
        public string index;
        public string timestamp;
        public string data;
    }

    public class Block
    {
        public string previousHash { get; set; }
        public string hash { get; set; }
        public Transaction transaction;
        public int nonce = 0;
        public Block(Transaction transaction, string previousHash = "0")
        {
            this.previousHash = previousHash;
            this.transaction = transaction;
            this.hash = this.computeHash();

        }

        public string computeHash()
        {
            var stringified = JsonConvert.SerializeObject(this.transaction) +
                this.previousHash + this.nonce;
            return Getsha256(stringified);
        }

        static string Getsha256(string randomString)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            System.Text.StringBuilder hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(System.Text.Encoding.UTF8.GetBytes(randomString), 0, System.Text.Encoding.UTF8.GetByteCount(randomString));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public void mineBlock(int difficulty)
        {
            string[] arr = new string[difficulty];
            var hash_diff = "";

            foreach (var s in arr)
            {
                hash_diff += "0";
            }

            while (this.hash.Substring(0, difficulty) != hash_diff)
            {
                this.nonce++;
                this.hash = this.computeHash();
            }

        }

        public bool validateBlock()
        {
            //validate data in block
            var data = this.transaction.data;

            //validate the data
            int val = -9999;
            int.TryParse(data, out val);
            if (val >= 0) { return true; }

            return false;
        }
    }


    public class BlockChain
    {
        public List<Block> chain;
        public int difficulty;
        public BlockChain()
        {
            this.chain = new List<Block>();

            this.chain.Add(createGenesisBlock());

            this.difficulty = 4;
        }
        static private Block createGenesisBlock()
        {
            var genesisTransaction =
                new Transaction()
                {
                    public_key = Program.PublicID,
                    timestamp = DateTime.Now.ToString("o"),
                    index = "0",
                    data = "Genesis Block"
                };
        
            var genBlock = new Block(genesisTransaction, "0");
            genBlock.hash = genBlock.computeHash();
            return genBlock;
        }

        public Block getLatestBlock()
        {
            return this.chain[this.chain.Count-1];
        }

        public Block buildBlock(Transaction transaction)
        {
            var newBlock = new Block(transaction,
                                    this.getLatestBlock().hash);

            newBlock.hash = newBlock.computeHash();

            return newBlock;

        }
        public void addBlock(Block newBlock)
        {
            newBlock.hash = newBlock.computeHash();

            newBlock.mineBlock(this.difficulty);

            this.chain.Add(newBlock);

        }


        public void addBlock(Transaction transaction)
        {
            transaction.index = Convert.ToString(this.chain.Count);

            var newBlock = new Block(transaction, 
                                    this.getLatestBlock().hash);

            
            newBlock.hash = newBlock.computeHash();

            Console.WriteLine("Mining :\n\t{0}", newBlock.hash);

            newBlock.mineBlock(this.difficulty);

            this.chain.Add(newBlock);
            
        }

        public bool IsChainValid()
        {
            for(int i = 1; i < this.chain.Count; i++)
            {

                //recalc this blocks hash
                if (this.chain[i].hash != this.chain[i].computeHash())
                {
                    return false;
                }

                //check if relationship with prev is correct
                if (this.chain[i].previousHash!= this.chain[i - 1].hash)
                {
                    return false;
                }

            }

            return true;
        }
    }
}
