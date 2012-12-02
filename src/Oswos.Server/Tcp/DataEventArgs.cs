using System;
using System.Net;

namespace Oswos.Server.Tcp
{
    /// <summary>
    /// EventArgs class holding a Byte[].
    /// </summary>
    public class DataEventArgs : EventArgs
    {
        public IPEndPoint RemoteEndPoint { get; set; }
        public Byte[] Data { get; set; }
        public Int32 Offset { get; set; }
        public Int32 Length { get; set; }
    }
}
