using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using Oswos.Repository;

namespace Oswos.Server.WebsiteEndpoint
{
    public class WebsiteEndpointServer : IDisposable
    {
        private readonly IWebsiteRepository _repository;
        private readonly List<WebsiteEndpoint> _websiteEndpoints;
        private readonly List<AppDomain> _appDomains;

        public WebsiteEndpointServer(IWebsiteRepository repository)
        {
            _repository = repository;
            _websiteEndpoints = new List<WebsiteEndpoint>();
            _appDomains = new List<AppDomain>();
        }

        public void Start()
        {
            foreach (var website in _repository.GetAll())
            {
                StartWebsiteEndpoint(website);
            }
        }

        private void StartWebsiteEndpoint(Website website)
        {
            var domainSetup = new AppDomainSetup()
                    {
                        ApplicationName = website.Name,
                        ApplicationBase = website.Path,
                        PrivateBinPath = website.Path + "\\bin",
                        ConfigurationFile = Path.Combine(website.Path, "Web.config")
                    };

            var evidence = AppDomain.CurrentDomain.Evidence;

            AppDomain appDomain = AppDomain.CreateDomain(
                website.Name,
                evidence,
                domainSetup);

            var websiteEndpoint = (WebsiteEndpoint)appDomain.CreateInstanceFromAndUnwrap(
                Assembly.GetExecutingAssembly().Location,
                typeof(WebsiteEndpoint).FullName);

            _websiteEndpoints.Add(websiteEndpoint);
            _appDomains.Add(appDomain);

            websiteEndpoint.Start(55305 + website.Id, website.Path);
        }

        public void Stop()
        {
            foreach (var websiteEndpoint in _websiteEndpoints)
            {
                try
                {
                    websiteEndpoint.Stop();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            foreach (var appDomain in _appDomains)
            {
                try
                {
                    AppDomain.Unload(appDomain);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}