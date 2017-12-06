using System;
using System.ServiceModel;

namespace HexChain
{
    partial class Program
    {
        public class LocalInterfaceServer
        {
            string LocalHostEndpoint { get; set; }
            string LocalClientBinding { get; set; }
            ServiceHost svchost;

            public LocalInterfaceServer(string localHostEndpoint, string localClientBinding)
            {
                this.LocalClientBinding = localClientBinding;
                this.LocalHostEndpoint =  localHostEndpoint;
                
            }

            public void Create()
            {
                this.svchost = new ServiceHost(typeof(JinxService));
                try
                {
                    var jx = LocalHostEndpoint;
                    var jxtcp = LocalHostEndpoint.Replace("http", "net.tcp");
                    if (LocalClientBinding != "tcp")
                    {
                        this.svchost.AddServiceEndpoint("IJinxService", 
                            new BasicHttpBinding(), jx);
                    }
                    else
                    {
                        this.svchost.AddServiceEndpoint("IJinxService", 
                            new NetTcpBinding(SecurityMode.None, false), jxtcp);
                    }
                }
                catch (Exception eX)
                {
                    this.svchost = null;
                    throw new ArgumentException("Jinx Host Service can not be started \n\nError Message [" + eX.Message + "]");
                }
                if (this.svchost == null)
                {
                    throw new ArgumentException("Jinx Host Service could not be established");
                }

            }

            public void Open()
            {
                this.svchost.Open();
                Console.WriteLine("Jinx Host Service Listening. {0}.", LocalHostEndpoint);
            }
            public void Close()
            {
               this.svchost.Close();
               this.svchost = null;
               Console.WriteLine("Jinx Host Service stopped.");
            }

            internal void Listen()
            {

                //Start the local interface web service (SOAP)
                this.Create();
                this.Open();
                while(this.svchost.State == CommunicationState.Opened)
                {
                    //listen
                }
                this.Close();

            }
        }
        
        
      
    }
}


