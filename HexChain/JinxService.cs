using Newtonsoft.Json;
public class JinxService : IJinxService
{
    string IJinxService.Send(string message)
    {
        HexChain.Program.cq.Enqueue(message);
        return message;
    }

    string IJinxService.Read()
    {
        var timeout = 500;
        while (HexChain.Program.cqf.Count > 0 || timeout<0 )
        {
            System.Threading.Thread.Sleep(10);
            timeout--;
        }
        return JsonConvert.SerializeObject(
            HexChain.Program._cb.ToArray()[0].getLatestBlock(),Formatting.Indented);
    }
}
