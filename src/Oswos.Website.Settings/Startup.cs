using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oswos.Website.Settings
{
    public class Startup
    {
        private static readonly Nancy.Hosting.Owin.NancyOwinHost owinHost;

        static Startup()
        {
            var bootstrapper = new CustomBootstrapper();
            owinHost = new Nancy.Hosting.Owin.NancyOwinHost(bootstrapper);
        }

        public Task ProcessRequest(IDictionary<string, object> environment)
        {
            return owinHost.ProcessRequest(environment);
        }
    }
}