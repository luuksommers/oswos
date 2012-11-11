using System.Collections.Generic;
using System.Linq;
using Oswos.Repository;

namespace Oswos.Console
{
    public class DemoWebsiteRepository : IWebsiteRepository
    {
        readonly List<Website> _websites = new List<Website>();
        public void AddOrUpdate(Website website)
        {
            if (!_websites.Contains(website))
            {
                _websites.Add(website);
            }
        }

        public void Remove(Website website)
        {
            _websites.Remove(website);
        }

        public Website GetByHost(string host)
        {
            return _websites.SingleOrDefault(a => a.HostName == host);
        }

        public IEnumerable<Website> GetAll()
        {
            return _websites;
        }
    }
}