using System;
using System.Collections.Generic;

namespace HexChain
{
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
                    public_key = DateTime.Now.ToString(),
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
            var newBlock = new Block(transaction, 
                                    this.getLatestBlock().hash);

            newBlock.hash = newBlock.computeHash();

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
