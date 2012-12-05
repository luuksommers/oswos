using System.Net.Sockets;
using Oswos.Server.Tcp;

namespace Oswos.Server.Http
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
            var connection = new SocketConnection(e);
            _router.Route(connection);
            return connection;
        }
    }
}