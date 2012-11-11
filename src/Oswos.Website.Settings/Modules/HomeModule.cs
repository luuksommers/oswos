using System.Net;
using Nancy;
using Oswos.Repository;
using Oswos.Website.Settings.Models;
using Nancy.ModelBinding;

namespace Oswos.Website.Settings.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule(IWebsiteRepository repository)
        {
            Get["/"] = _ => View["Index.sshtml"];

            Get["/dns"] = _ =>
            {
                var wc = new WebClient();
                string result = wc.DownloadString("http://checkip.dyndns.org/").Split(':')[1].Trim();
                return View["Dns.sshtml", new { CurrentIp = result }];
            };

            Get["/websites"] = _ => View["Websites.sshtml", new WebsiteModel(repository)];

            Post["/websites"] = parameters =>
            {
                var websiteModel = this.Bind<Repository.Website>();
                repository.AddOrUpdate(websiteModel);
                return View["Websites.sshtml", new WebsiteModel(repository)];
            };
        }
    }
}
