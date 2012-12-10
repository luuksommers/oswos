using System;

namespace Oswos.Server.Http
{
    public class WebsiteNotFoundException : Exception
    {
        public WebsiteNotFoundException(string host) : 
            base(string.Format("Website {0} not found in repository", host))
        {
            
        }
    }
}