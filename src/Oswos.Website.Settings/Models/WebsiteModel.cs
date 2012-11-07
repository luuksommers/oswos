using System.Collections.Generic;
using Oswos.Repository;

namespace Oswos.Website.Settings.Models
{
    public class WebsiteModel
    {
        public WebsiteModel()
        {

        }

        public WebsiteModel(IWebsiteRepository repository)
        {
            Websites = repository.GetAll();
        }

        public IEnumerable<Repository.Website> Websites { get; set; }
    }
}