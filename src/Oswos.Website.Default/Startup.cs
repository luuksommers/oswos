using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oswos.Website.Default
{
    public class Startup
    {
        public Task ProcessRequest(IDictionary<string, object> environment)
        {
            var bootstrapper = new CustomBootstrapper();
            var nancyOwinHost = new Nancy.Hosting.Owin.NancyOwinHost(bootstrapper);
            return nancyOwinHost.ProcessRequest(environment);
        }
    }
}