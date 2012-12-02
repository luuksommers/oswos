using System.IO;
using System.Net.Sockets;
using Oswos.Server.Tcp;

namespace Oswos.Server.Http
{
    public class HttpSocketConnection : SocketConnection
    {
        private readonly HttpRequestStream _httpRequestStream;
        public HttpRequestStream RequestStream
        {
            get { return _httpRequestStream; }
        }

        public HttpSocketConnection(Socket socket)
            : base(socket)
        {
            _httpRequestStream = new HttpRequestStream();
            DataReceived += DataReceivedEvent;
        }

        private void DataReceivedEvent(DataEventArgs e)
        {
            _httpRequestStream.Write(e.Data, e.Offset, e.Length);
        }
    }
}