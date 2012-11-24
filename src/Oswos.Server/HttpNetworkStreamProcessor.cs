using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.ServiceModel;
using System.Text;
using NLog;
using Oswos.Repository;
using Oswos.Server.WebsiteAdapter;

namespace Oswos.Server
{
    public class HttpNetworkStreamProcessor : INetworkStreamProcessor
    {
        private readonly IWebsiteRepository _repository;
        static readonly Dictionary<string, ChannelFactory<IWebsiteAdapter>> FactoryCache = new Dictionary<string, ChannelFactory<IWebsiteAdapter>>();
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public HttpNetworkStreamProcessor(IWebsiteRepository repository)
        {
            _repository = repository;
            foreach (var website in repository.GetAll())
            {
                FactoryCache.Add(website.HostName, new ChannelFactory<IWebsiteAdapter>(
                         new NetNamedPipeBinding() { MaxReceivedMessageSize = Int32.MaxValue },
                         new EndpointAddress(string.Format("net.pipe://localhost/{0}", website.HostName))));
            }
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
            Logger.Debug("Found host {0}", host);
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
                    var endPoint = FactoryCache[website.HostName].CreateChannel();
                    var responseStream = endPoint.ParseRequest(requestHttpStream);
                    responseStream.CopyTo(tcpStream);
                }
                catch (Exception e)
                {
                    Logger.ErrorException("ProcessStream", e);
                    var errorMessage = Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 " + e.Message + "\x0d\x0a");
                    tcpStream.Write(errorMessage, 0, errorMessage.Length);
                }
            }
        }
    }
}