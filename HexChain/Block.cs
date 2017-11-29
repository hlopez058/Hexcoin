using Newtonsoft.Json;

namespace HexChain
{
    public class Block
    {
        public string previousHash { get; set; }
        public string hash { get; set; }
        public Transaction transaction;
        public int nonce = 0;
        public Block(Transaction transaction, string previousHash="0")
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
            
            foreach(var s in arr)
            {
                hash_diff += "0";
            }

            while (this.hash.Substring(0,difficulty)!= hash_diff)
            {
                this.nonce++;
                this.hash = this.computeHash();
            }

            //Console.WriteLine("Block mined: " + this.hash);
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
}
