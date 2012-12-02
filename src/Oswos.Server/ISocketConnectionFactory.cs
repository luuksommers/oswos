using System.Net.Sockets;

namespace Oswos.Server
{
    public interface ISocketConnectionFactory
    {
        ISocketConnection Create(Socket e);
    }
}