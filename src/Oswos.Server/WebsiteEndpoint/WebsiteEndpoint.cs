using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using Oswos.Server.WebsiteAdapter;

namespace Oswos.Server.WebsiteEndpoint
{
    public class WebsiteEndpoint : MarshalByRefObject
    {
        private ServiceHost _serviceHost;

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

        private static IEnumerable<string> GetBinFolders()
        {
            //TODO: The AppDomain.CurrentDomain.BaseDirectory usage is not correct in 
            //some cases. Need to consider PrivateBinPath too
            List<string> toReturn = new List<string>();
            //slightly dirty - needs reference to System.Web.  Could always do it really
            //nasty instead and bind the property by reflection!

                //TODO: as before, this is where the PBP would be handled.
            toReturn.Add(AppDomain.CurrentDomain.BaseDirectory);

            return toReturn;
        }
    }
}
