﻿using System;
using System.Collections.Generic;
using System.ServiceModel;
using Oswos.Repository;
using Oswos.Server.Tcp;
using Oswos.Server.WebsiteAdapter;

namespace Oswos.Server.Http
{
    public class HttpStreamRouter : IStreamRouter
    {
        private static readonly Dictionary<string, ChannelFactory<IWebsiteAdapter>> FactoryCache =
            new Dictionary<string, ChannelFactory<IWebsiteAdapter>>();
        public List<HttpStreamRouteObject> objects = new List<HttpStreamRouteObject>();

        public HttpStreamRouter(IWebsiteRepository repository)
        {
            foreach (var website in repository.GetAll())
            {
                if (!FactoryCache.ContainsKey(website.HostName))
                {
                    FactoryCache.Add(website.HostName, new ChannelFactory<IWebsiteAdapter>(
                                                           new NetNamedPipeBinding()
                                                               {
                                                                   MaxReceivedMessageSize = Int32.MaxValue
                                                               },
                                                           new EndpointAddress(string.Format(
                                                               "net.pipe://localhost/{0}", website.HostName))));
                }
            }
        }

        public void Route(ISocketConnection socketConnection)
        {
            var httpSocketConnection = socketConnection as SocketConnection;
            if (httpSocketConnection == null)
            {
                throw new StreamRouterException("HttpStreamRouter can only accept SocketConnection");
            }

            var streamRouteObject = new HttpStreamRouteObject(httpSocketConnection, this);
            objects.Add(streamRouteObject);
            httpSocketConnection.Disconnected += () => objects.Remove(streamRouteObject);
        }

        public IWebsiteAdapter CreateWebsiteEndpoint(string host)
        {
            var website = new WebsiteRepository().GetByHost(host);
            if (website == null)
            {
                throw new WebsiteNotFoundException(host);
            }
            return FactoryCache[website.HostName].CreateChannel();
        }
    }
}
