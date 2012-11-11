using System;
using System.ServiceModel;
using Oswos.Server.WebsiteAdapter;

namespace Oswos.Server.WebsiteEndpoint
{
    public class WebsiteEndpoint : MarshalByRefObject
    {
        private ServiceHost _serviceHost;

        /// <summary>
        /// 1. Find interface in path dll's or path\bin dll's
        /// 2. Create instance runnin in a servicehost
        /// 3. Return 
        /// </summary>
        public void Start(string hostName)
        {
            var hostType = typeof(OwinWebsiteAdapter);
            var endpointUrl = string.Format("net.pipe://localhost/{0}", hostName);
            StartService(hostType, endpointUrl);
        }

        private void StartService(Type hostType, string endpointUrl)
        {
            _serviceHost = CreateServiceHost(hostType, endpointUrl);
            _serviceHost.Open();
        }

        private ServiceHost CreateServiceHost(Type hostType, string endpointUrl)
        {
            var serviceHost = new ServiceHost(hostType);
            serviceHost.AddServiceEndpoint(
                typeof(IWebsiteAdapter),
                new NetNamedPipeBinding() { MaxReceivedMessageSize = Int32.MaxValue },
                new Uri(endpointUrl));

            return serviceHost;
        }

        public void Stop()
        {
            _serviceHost.Close();
            _serviceHost = null;
        }
    }
}
