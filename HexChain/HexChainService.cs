using System.ServiceModel;
using System.ServiceModel.Web;
using Newtonsoft.Json;
using System.Web.Hosting;
using HexChain;

[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.Single,
    ConcurrencyMode = ConcurrencyMode.Multiple,
    AddressFilterMode = AddressFilterMode.Any)]
public class HexChainService : IHexChainService
{
    public string Send(string message)
    {
        //recieve a transaction and create a block from it
        HexChain.Program.HexChainBuffer.Enqueue(message);
        
        return "received transaction";
    }

    public string SendValidBlock(string message)
    {
        //recieve a block and try to validate it
        //HexChain.Program.HexChainBuffer.Enqueue(message);

        //verify block is indeed a valid block
        HexChain.Program._cb.ToArray()[0].chain.Add(JsonConvert.DeserializeObject<Block>(message));

        //add the block to the chain

        return "received block";
    }

}

[ServiceContract]
public interface IHexChainService
{
    [OperationContract]
    [WebInvoke(Method = "GET",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Wrapped,
        UriTemplate = "Send/{message}")]
    string Send(string message);

    [OperationContract]
    [WebInvoke(Method = "GET",
    ResponseFormat = WebMessageFormat.Json,
    BodyStyle = WebMessageBodyStyle.Wrapped,
    UriTemplate = "SendValidBlock/{message}")]
    string SendValidBlock(string message);
}
