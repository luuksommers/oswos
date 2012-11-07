using Oswos.Repository;
using Oswos.Server;
using Oswos.Server.WebsiteEndpoint;

namespace Oswos.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new WebsiteRepository();

            var endpointHost = new WebsiteEndpointServer(repository);
            endpointHost.Start();

            var server = new TcpServer(new HttpNetworkStreamProcessor(repository));
            server.Start(9000);

            System.Console.WriteLine("Press enter to shutdown");
            System.Console.ReadLine();

            endpointHost.Stop();
            server.Stop();
        }
    }
}
