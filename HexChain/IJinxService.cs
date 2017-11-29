using System.ServiceModel;
[ServiceContract]
public interface IJinxService
{
    [OperationContract]
    string Send(string message);

    [OperationContract]
    string Read();
}
