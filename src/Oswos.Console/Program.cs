using System;
using System.IO;
using NLog;
using Oswos.Repository;
using Oswos.Server;
using Oswos.Server.WebsiteEndpoint;

namespace Oswos.Console
{
    class Program
    {
        private const int DefaultPort = 80;
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
            Logger logger = LogManager.GetLogger("console");
            logger.Info("Starting default oswos.net server...");

            var repository = new WebsiteRepository();

            var endpointHost = new WebsiteEndpointServer(repository);
            endpointHost.Start();

            var server = new TcpServer(new HttpNetworkStreamProcessor(repository));
            server.Start(DefaultPort);

            logger.Info("Listening at port {0}", DefaultPort);
            logger.Info("Open a browser and connect to http://localhost:{0}", DefaultPort);
            logger.Info(string.Empty);

            logger.Info("Press enter to shutdown");
            System.Console.ReadLine();

            endpointHost.Stop();
            server.Stop();
        }
    }
}
