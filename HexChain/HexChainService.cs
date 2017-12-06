using System.ServiceModel;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using System.Web.Hosting;
using HexChain;
using System.Security.Cryptography;
using System.Linq;
using System;

[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    AddressFilterMode = AddressFilterMode.Any)]
public class HexChainService : IHexChainService
{
    public string Send(string message,string isblock)
    {
        if (Convert.ToBoolean(isblock))
        {
           return ProcessValidBlock(message);
        }
        else
        {
            //decrypt the message
            var msg = Encrypt.DecryptString(message, Program.LicenseKey);

            //recieve a transaction and create a block from it
            HexChain.Program.HexChainBuffer.Enqueue(msg);

            return "received transaction";

        }

    }

    private string ProcessValidBlock(string message) {
        var msg = Encrypt.DecryptString(message, Program.LicenseKey);

        //client says this block is valid 
        var proposed_block = JsonConvert.DeserializeObject<Block>(msg);

        //does this block have a longer chain than the current chain

        //verify block is indeed a valid block

        //add the block to the chain

        HexChain.Program.HexChains.First().chain.Add(proposed_block);
        Console.WriteLine("Valid Block Accepted \n{0}", proposed_block.hash);
        return "received block";
    }

    public string SendValidBlock(string message)
    {
        
        var msg = Encrypt.DecryptString(message, Program.LicenseKey);

        //client says this block is valid 
        var proposed_block = JsonConvert.DeserializeObject<Block>(msg);

        //does this block have a longer chain than the current chain

        //verify block is indeed a valid block

        //add the block to the chain

        HexChain.Program.HexChains.First().chain.Add(proposed_block);
        Console.WriteLine("Valid Block Accepted \n{0}", proposed_block.hash);
        return "received block";
    }

    public string Discover(string message)
    {
        return Program.PublicID;
    }

}

[ServiceContract]
public interface IHexChainService
{
    [OperationContract]
    [WebInvoke(Method = "POST",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Wrapped,
        UriTemplate = "Send/msg={message}&is={isblock}")]
    string Send(string message, string isblock);

    [OperationContract]
    [WebInvoke(Method = "POST",
    RequestFormat =WebMessageFormat.Json,
    ResponseFormat = WebMessageFormat.Json,
    BodyStyle = WebMessageBodyStyle.Wrapped,
    UriTemplate = "SendValidBlock/{message}")]
    string SendValidBlock(string message);

    [OperationContract]
    [WebInvoke(Method = "GET",
    ResponseFormat = WebMessageFormat.Json,
    BodyStyle = WebMessageBodyStyle.Wrapped,
    UriTemplate = "Discover/{message}")]
    string Discover(string message);
}
