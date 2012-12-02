using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using NLog;
using Oswos.Repository;
using Oswos.Server.WebsiteAdapter;

namespace Oswos.Server.Http
{
    public class HttpStreamRouter : IStreamRouter
    {
        private readonly IWebsiteRepository _repository;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        static readonly Dictionary<string, ChannelFactory<IWebsiteAdapter>> FactoryCache = new Dictionary<string, ChannelFactory<IWebsiteAdapter>>();
        
        public HttpStreamRouter(IWebsiteRepository repository)
        {
            _repository = repository;
            foreach (var website in repository.GetAll())
            {
                if (!FactoryCache.ContainsKey(website.HostName))
                {
                    FactoryCache.Add(website.HostName, new ChannelFactory<IWebsiteAdapter>(
                                                           new NetNamedPipeBinding()
                                                               {
                                                                   MaxReceivedMessageSize =
                                                                       Int32.MaxValue
                                                               },
                                                           new EndpointAddress(string.Format(
                                                               "net.pipe://localhost/{0}", website.HostName))));
                }
            }
        }

        public void Route(ISocketConnection socketConnection)
        {
            var httpSocketConnection = socketConnection as HttpSocketConnection;
            if (httpSocketConnection == null)
            {
                throw new StreamRouterException("HttpStreamRouter can only accept HttpSocketConnections");
            }

            new StreamRouteObject(_repository, httpSocketConnection);
        }

        public class StreamRouteObject
        {
            private readonly IWebsiteRepository _repository;
            private readonly HttpSocketConnection _socketConnection;

            public StreamRouteObject(IWebsiteRepository repository, HttpSocketConnection socketConnection)
            {
                _repository = repository;
                _socketConnection = socketConnection;
                _socketConnection.RequestStream.OnHeadersLoaded += Stream_OnHeadersLoaded;
            }

            public void Stream_OnHeadersLoaded(object source)
            {
                var host = string.Empty;
                var requestHttpStream = source as HttpRequestStream;

                if (requestHttpStream.Headers != null && requestHttpStream.Headers.ContainsKey("Host"))
                {
                    host = requestHttpStream.Headers["Host"];
                }

                var website = _repository.GetByHost(host);
                if (website == null)
                {
                    var errorMessage =
                        Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 Website Not Found\x0d\x0a");
                    _socketConnection.SendData(errorMessage, 0, errorMessage.Length);
                }
                else if (requestHttpStream.Headers == null)
                {
                    var errorMessage =
                        Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 Headers Not Found\x0d\x0a");
                    _socketConnection.SendData(errorMessage, 0, errorMessage.Length);
                }
                else
                {
                    try
                    {
                        _logger.Debug("Requesting:" + requestHttpStream.Uri);
                        var endPoint = FactoryCache[website.HostName].CreateChannel();
                        var responseStream = endPoint.ParseRequest(requestHttpStream);

                        var buffer = new byte[4096];
                        var length = 0;
                        while ((length = responseStream.Read(buffer, 0, 4096)) > 0)
                        {
                            _socketConnection.SendData(buffer, 0, length);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            Encoding.UTF8.GetBytes(requestHttpStream.HttpVersion + " 500 " + ex.Message + "\x0d\x0a");
                        _socketConnection.SendData(errorMessage, 0, errorMessage.Length);
                    }
                }
                _socketConnection.Disconnect();
            }
        }
    }
}
