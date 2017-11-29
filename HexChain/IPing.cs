using System.ServiceModel;

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
}
