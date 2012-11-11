using System;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace Oswos.Server
{
    public class TcpServer
    {
        private readonly INetworkStreamProcessor _streamProcessor;
        private static bool _serverRunning = true;
        private TcpListener _tcpServer;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                try
                {
                    var tcpClient = await tcpServer.AcceptTcpClientAsync();
                    ProcessClient(tcpClient);
                }
                catch (Exception exception)
                {
                    Logger.ErrorException("ListenForClients", exception);
                    Console.WriteLine(exception.Message);
                }
            }
        }

        private async void ProcessClient(TcpClient tcpClient)
        {
            Console.WriteLine("Connection from {0}", tcpClient.Client.RemoteEndPoint);
            var tcpStream = tcpClient.GetStream();

            _streamProcessor.ProcessStream(tcpStream);

            if (tcpClient.Connected)
            {
                try
                {
                    tcpClient.Client.Disconnect(true);
                }
                catch (Exception exception)
                {
                    Logger.ErrorException("ProcessClient", exception);
                    Console.WriteLine(exception.Message);
                }
            }
        }
    }
}