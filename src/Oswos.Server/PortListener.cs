using System;
using System.Collections.Generic;
using System.Net;
using NLog;
using Oswos.Server.Tcp;

namespace Oswos.Server
{
    public class PortListener
    {
        private readonly ISocketConnectionFactory _socketConnectionFactory;
        private SocketListener _socketListener;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly ConnectionMonitor _monitor = new ConnectionMonitor();

        public PortListener(ISocketConnectionFactory socketConnectionFactory)
        {
            _socketConnectionFactory = socketConnectionFactory;
        }

        public void Start(int port)
        {
            _socketListener = new SocketListener(IPAddress.Any, port, 50); // 50 pending connections should be enough
            _socketListener.SocketConnected += SocketListenerSocketConnected;
            _socketListener.Start();
        }

        public void Stop()
        {
            if (_socketListener != null && _socketListener.IsRunning)
            {
                _socketListener.SocketConnected -= SocketListenerSocketConnected;
                _socketListener.Stop();
            }
        }

        public void SocketListenerSocketConnected(object sender, SocketEventArgs e)
        {
            _logger.Debug("Connection from {0}", e.Socket.RemoteEndPoint.ToString());

            var connection = _socketConnectionFactory.Create(e.Socket);
            _monitor.IncrementNumberOfConnections();
            connection.Disconnected += () => _monitor.DecrementNumberOfConnections();
            connection.ListenForData();
        }
    }
}
