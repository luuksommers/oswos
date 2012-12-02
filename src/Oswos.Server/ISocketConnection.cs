using Oswos.Server.Tcp;

namespace Oswos.Server
{
    public interface ISocketConnection
    {
        void ListenForData();
        event DisconnectedEvent Disconnected;
    }
}