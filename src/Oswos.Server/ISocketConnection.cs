using Oswos.Server.Tcp;

namespace Oswos.Server
{
    public delegate void DataReceivedEvent(DataEventArgs e);
    public delegate void DisconnectedEvent();

    public interface ISocketConnection
    {
        void ListenForData();
        void SendData(byte[] data, int offset, int count);
        event DisconnectedEvent Disconnected;
        event DataReceivedEvent DataReceived;
        void Disconnect();
    }
}