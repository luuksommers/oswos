using System;
using System.Net;
using System.Net.Sockets;
using NLog;

namespace Oswos.Server.Tcp
{
    public delegate void DataReceivedEvent(DataEventArgs e);
    public delegate void DisconnectedEvent();

    public class SocketConnection : ISocketConnection, IDisposable
    {
        public event DisconnectedEvent Disconnected;
        public event DataReceivedEvent DataReceived;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private SocketAsyncEventArgs _eventReceiveArgs;
        private Socket _socket;

        public SocketConnection(Socket socket)
        {
            _socket = socket;
            _eventReceiveArgs = SocketArgsPool.Instance.CheckOut();
            _eventReceiveArgs.Completed += ReceivedCompleted;

            BufferPool.Instance.CheckOut(_eventReceiveArgs);
        }

        public void SendData(byte[] data, int offset, int count)
        {
            lock (this)
            {
                if (_socket != null && _socket.Connected)
                {
                    SocketAsyncEventArgs sendArgs = SocketArgsPool.Instance.CheckOut();

                    sendArgs.SetBuffer(data, offset, count);
                    sendArgs.Completed += SendCompleted;

                    _socket.InvokeAsyncMethod(_socket.SendAsync,
                        SendCompleted, sendArgs);
                }
            }
        }

        public void Disconnect()
        {
            CloseConnection();
        }

        public void ListenForData()
        {
            lock (this)
            {
                if (_socket != null && _socket.Connected)
                {
                    _socket.InvokeAsyncMethod(_socket.ReceiveAsync,
                        ReceivedCompleted, _eventReceiveArgs);
                }
            }
        }

        private void SendCompleted(object sender, SocketAsyncEventArgs args)
        {
            args.Completed -= SendCompleted;
            SocketArgsPool.Instance.CheckIn(args);
        }

        private void ReceivedCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred == 0)
            {
                CloseConnection(); //Graceful disconnect
                return;
            }
            if (args.SocketError != SocketError.Success)
            {
                CloseConnection(); //NOT graceful disconnect
                return;
            }

            Byte[] data = new Byte[args.BytesTransferred];
            Array.Copy(args.Buffer, args.Offset, data, 0, data.Length);

            // Fire DataReceived event
            if (DataReceived != null)
            {
                DataReceived(new DataEventArgs() { RemoteEndPoint = args.RemoteEndPoint as IPEndPoint, Data = data, Length = data.Length, Offset = 0 });
            }

            ListenForData();
        }

        private void CloseConnection()
        {
            lock (this)
            {
                if (_socket != null && _socket.Connected)
                {
                    Console.WriteLine("{0} - Close Connection");
                    try
                    {
                        _socket.Shutdown(SocketShutdown.Send);
                    }
                    catch (SocketException e)
                    {

                    }
                    catch (ObjectDisposedException e)
                    {
                    }

                    _socket.Close();
                    _socket = null;

                    // Fire Disconnected event
                    if (Disconnected != null)
                    {
                        Disconnected();
                    }
                }

                if (_eventReceiveArgs != null)
                {
                    _eventReceiveArgs.Completed -= ReceivedCompleted;
                    _eventReceiveArgs.Dispose();

                    _eventReceiveArgs = new SocketAsyncEventArgs();
                    BufferPool.Instance.CheckIn(_eventReceiveArgs);
                    SocketArgsPool.Instance.CheckIn(_eventReceiveArgs);

                    _eventReceiveArgs = null;
                }
            }
        }

        private Boolean disposed = false;
        ~SocketConnection()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    CloseConnection();
                }

                disposed = true;
            }
        }
    }
}
