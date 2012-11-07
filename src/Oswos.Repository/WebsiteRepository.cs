using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Oswos.Repository
{
    public class WebsiteRepository : IWebsiteRepository
    {
        private readonly OswosDatabaseEntities _database;

        public WebsiteRepository()
        {
            _database = new OswosDatabaseEntities();

            if(!_database.Websites.Any(a=>a.Name=="settings"))
            {
                var websitePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Settings");
                if (Directory.Exists(websitePath))
                {
                    AddOrUpdate(new Website()
                                             {HostName = "localhost", Id = 100, Name = "settings", Path = websitePath});
                }
                websitePath = ConfigurationManager.AppSettings["oswos:SettingsWebsitePath"];
                if (Directory.Exists(websitePath))
                {
                    AddOrUpdate(new Website() { HostName = "localhost", Id = 100, Name = "settings", Path = websitePath });
                }
            }
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
            return _database.Websites.FirstOrDefault(a => a.HostName == host);
        }

        public IEnumerable<Website> GetAll()
        {
            return _database.Websites;
        }
    }
}
