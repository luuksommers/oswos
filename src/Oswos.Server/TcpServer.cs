using System;
using System.Net;
using System.Net.Sockets;

namespace Oswos.Server
{
    public class TcpServer
    {
        private readonly INetworkStreamProcessor _streamProcessor;
        private static bool _serverRunning = true;
        private TcpListener _tcpServer;

        public TcpServer(INetworkStreamProcessor streamProcessor)
        {
            _streamProcessor = streamProcessor;
        }

        public void Start(int port)
        {
            _tcpServer = new TcpListener(IPAddress.Any, port);
            _tcpServer.Start();
            ListenForClients(_tcpServer);
        }

        public void Stop()
        {
            _serverRunning = false;
            _tcpServer.Stop();
        }

        private async void ListenForClients(TcpListener tcpServer)
        {
            while (_serverRunning)
            {
                var tcpClient = await tcpServer.AcceptTcpClientAsync();
                ProcessClient(tcpClient);
            }
        }

        private async void ProcessClient(TcpClient tcpClient)
        {
            var tcpStream = tcpClient.GetStream();

            _streamProcessor.ProcessStream(tcpStream);

            if (tcpClient.Connected)
            {
                try
                {
                    tcpClient.Client.Disconnect(true);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}