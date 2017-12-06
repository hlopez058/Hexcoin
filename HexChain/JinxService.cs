using System.ServiceModel;
using Newtonsoft.Json;
using HexChain;
using System;
using System.Linq;

public class JinxService : IJinxService
{
    string IJinxService.Send(string message)
    {
        //build a block for the new transaction
        var trans =
            new Transaction()
            {
                public_key = HexChain.Program.PublicID,
                timestamp = DateTime.Now.ToString("o"),
                data = "val:" + message
            };

        var transJson = JsonConvert.SerializeObject(trans);
        Program.JinxBuffer.Enqueue(transJson);
       // Program.HexChainBuffer.Enqueue(transJson);


        return message;
    }

    string IJinxService.Read()
    {
        if (HexChain.Program.HexChains.Count > 0)
        {
            return JsonConvert.SerializeObject(
                HexChain.Program.HexChains.First().getLatestBlock(), Formatting.Indented);
        }return "";
    }
}

[ServiceContract]
public interface IJinxService
{
    [OperationContract]
    string Send(string message);

    [OperationContract]
    string Read();
}
