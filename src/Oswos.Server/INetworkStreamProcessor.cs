using System.Net.Sockets;

namespace Oswos.Server
{
    public interface INetworkStreamProcessor
    {
        void ProcessStream(NetworkStream tcpStream);
    }
}