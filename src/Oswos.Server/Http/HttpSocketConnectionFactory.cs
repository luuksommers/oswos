using System.Net.Sockets;
using Oswos.Server.Http;

namespace Oswos.Server
{
    public class HttpSocketConnectionFactory : ISocketConnectionFactory
    {
        private readonly IStreamRouter _router;

        public HttpSocketConnectionFactory(IStreamRouter router)
        {
            _router = router;
        }

        public ISocketConnection Create(Socket e)
        {
            var connection = new HttpSocketConnection(e);
            _router.Route(connection);
            return connection;
        }
    }
}