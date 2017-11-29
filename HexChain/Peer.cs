using System;
using System.ServiceModel;
using System.Threading;
using System.ServiceModel.Description;

namespace HexChain
{
    public class Peer
    {
        public string Id { get; private set; }

        public IPing Channel;
        public IPing Host;

        public Peer(string id)
        {
            Id = id;
        }


        private readonly AutoResetEvent _stopFlag = new AutoResetEvent(false);

        public void Run()
        {
            Console.WriteLine("[ Starting Service ]");
            StartService();

            Console.WriteLine("[ Service Started ]");
            _stopFlag.WaitOne();

            Console.WriteLine("[ Stopping Service ]");
            StopService();

            Console.WriteLine("[ Service Stopped ]");
        }

        public void Stop()
        {
            _stopFlag.Set();
        }

        public void StopService()
        {
            ((ICommunicationObject)Channel).Close();
            if (_factory != null)
                _factory.Close();
        }

        public void StartService()
        {
            var binding = new NetPeerTcpBinding();
            binding.Security.Mode = SecurityMode.None;

            var endpoint = new ServiceEndpoint(
                ContractDescription.GetContract(typeof(IPing)),
                binding,
                new EndpointAddress("net.p2p://HexChain"));

            Host = new PingImplementation();

            _factory = new DuplexChannelFactory<IPing>(
                new InstanceContext(Host),
                endpoint);

            var channel = _factory.CreateChannel();

            ((ICommunicationObject)channel).Open();

            // wait until after the channel is open to allow access.
            Channel = channel;
        }
        private DuplexChannelFactory<IPing> _factory;
    }
}
