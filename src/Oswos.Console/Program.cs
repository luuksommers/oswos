using System;
using System.IO;
using Oswos.Repository;
using Oswos.Server;
using Oswos.Server.WebsiteEndpoint;

namespace Oswos.Console
{
    class Program
    {
        private const int DefaultPort = 9000;
        static string GetSolutionFolder()
        {
            var directory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            do
            {
                directory = directory.Parent;
            } while (directory != null && directory.Name != "src");
            return directory == null ? string.Empty : directory.FullName;
        }

        static void Main(string[] args)
        {
            System.Console.WriteLine("Starting default oswos.net server");
            System.Console.WriteLine("Listening at port {0}", DefaultPort);
            System.Console.WriteLine("Open a browser and connect to http://localhost:{0}", DefaultPort);
            System.Console.WriteLine();

            var repository = new DemoWebsiteRepository();
            repository.AddOrUpdate(new Website()
                                       {
                                           HostName = "localhost", 
                                           Id = 1, 
                                           Name = "Settings", 
                                           Path = Path.Combine(GetSolutionFolder(), "Oswos.Website.Settings")
                                       });

            var endpointHost = new WebsiteEndpointServer(repository);
            endpointHost.Start();

            var server = new TcpServer(new HttpNetworkStreamProcessor(repository));
            server.Start(DefaultPort);

            System.Console.WriteLine("Press enter to shutdown");
            System.Console.ReadLine();

            endpointHost.Stop();
            server.Stop();
        }
    }
}
