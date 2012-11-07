using System;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using Oswos.Repository;
using Oswos.Server.WebsiteAdapter;

namespace Oswos.Server
{
    public class HttpNetworkStreamProcessor : INetworkStreamProcessor
    {
        private IWebsiteRepository _repository;

        public HttpNetworkStreamProcessor(IWebsiteRepository repository)
        {
            _repository = repository;
        }

        public void ProcessStream(NetworkStream tcpStream)
        {
            var requestHttpStream = new HttpStream();
            var buffer = new byte[4096];
            var bytesRead = tcpStream.Read(buffer, 0, 4096);
            requestHttpStream.Write(buffer, 0, bytesRead);

            var host = string.Empty;
            if (requestHttpStream.Headers != null && requestHttpStream.Headers.ContainsKey("Host"))
            {
                host = requestHttpStream.Headers["Host"];
            }

            var website = _repository.GetByHost(host);
            if (website == null)
            {
                var errorMessage = Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 Website Not Found\x0d\x0a");
                tcpStream.Write(errorMessage, 0, errorMessage.Length);
            }
            else if (requestHttpStream.Headers == null)
            {
                var errorMessage = Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 Headers Not Found\x0d\x0a");
                tcpStream.Write(errorMessage, 0, errorMessage.Length);
            }
            else
            {
                try
                {
                    using (var factory = new ChannelFactory<IWebsiteAdapter>(
                        new NetTcpBinding() { MaxReceivedMessageSize = Int32.MaxValue },
                        new EndpointAddress(string.Format("net.tcp://localhost:{0}/WebsiteEndpoint", 55305 + website.Id))))
                    {
                        var endPoint = factory.CreateChannel();
                        var responseStream = endPoint.ParseRequest(requestHttpStream);

                        responseStream.CopyTo(tcpStream);
                    }
                }
                catch (Exception e)
                {
                    var errorMessage = Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 " + e.Message + "\x0d\x0a");
                    tcpStream.Write(errorMessage, 0, errorMessage.Length);
                }
            }
        }
    }
}