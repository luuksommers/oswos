using System.Collections.Generic;
using System.Linq;

namespace Oswos.Repository
{
    public class WebsiteRepository : IWebsiteRepository
    {
        private readonly OswosDatabaseEntities _database;

        public WebsiteRepository()
        {
            _database = new OswosDatabaseEntities();
        }

        public void AddOrUpdate(Website website)
        {
            if (website.Id == 0)
            {
                _database.Websites.Add(website);
            }
            else
            {
                var websiteToUpdate = _database.Websites.Single(a => a.Id == website.Id);
                websiteToUpdate.HostName = website.HostName;
                websiteToUpdate.Name = website.Name;
                website.Path = website.Path;
            }
            _database.SaveChanges();
        }

        public void Remove(Website website)
        {
            _database.Websites.Remove(website);
            _database.SaveChanges();
        }

        public Website GetByHost(string host)
        {
            return _database.Websites.FirstOrDefault(a => host.Contains(a.HostName));
        }

        public IEnumerable<Website> GetAll()
        {
            return _database.Websites;
        }
    }
}
