using System;
using System.Net.Sockets;

namespace Oswos.Server.Tcp
{
    /// <summary>
    /// EventArgs class holding a Socket.
    /// </summary>
    public class SocketEventArgs : EventArgs
    {
        public Socket Socket { get; set; }
    }
}