using System;
using System.Net;
using System.Net.Sockets;

namespace Oswos.Server.Tcp
{
    /// <summary>
    /// Listens for socket connection on a given address and port.
    /// </summary>
    public class SocketListener : IDisposable
    {
        private int _connectionBacklog = 200;
        private IPEndPoint _endPoint;

        private Socket _listenerSocket;

        /// <summary>
        /// Length of the connection backlog.
        /// </summary>
        public int ConnectionBacklog
        {
            get { return _connectionBacklog; }
            set
            {
                lock (this)
                {
                    if (IsRunning)
                        throw new InvalidOperationException("Property cannot be changed while server running.");
                    else
                        _connectionBacklog = value;
                }
            }
        }
        /// <summary>
        /// The IPEndPoint to bind the listening socket to.
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return _endPoint; }
            set
            {
                lock (this)
                {
                    if (IsRunning)
                        throw new InvalidOperationException("Property cannot be changed while server running.");
                    else
                        _endPoint = value;
                }
            }
        }
        /// <summary>
        /// Is the class currently listening.
        /// </summary>
        public Boolean IsRunning
        {
            get { return _listenerSocket != null; }
        }

        /// <summary>
        /// Listens for socket connection on a given address and port.
        /// </summary>
        /// <param name="address">The address to listen on.</param>
        /// <param name="port">The port to listen on.</param>
        /// <param name="connectionBacklog">The connection backlog.</param>
        public SocketListener(string address, int port, int connectionBacklog)
            : this(IPAddress.Parse(address), port, connectionBacklog)
        { }
        /// <summary>
        /// Listens for socket connection on a given address and port.
        /// </summary>
        /// <param name="address">The address to listen on.</param>
        /// <param name="port">The port to listen on.</param>
        /// <param name="connectionBacklog">The connection backlog.</param>
        public SocketListener(IPAddress address, int port, int connectionBacklog)
            : this(new IPEndPoint(address, port), connectionBacklog)
        { }
        /// <summary>
        /// Listens for socket connection on a given address and port.
        /// </summary>
        /// <param name="endPoint">The endpoint to listen on.</param>
        /// <param name="connectionBacklog">The connection backlog.</param>
        public SocketListener(IPEndPoint endPoint, int connectionBacklog)
        {
            _endPoint = endPoint;
            _connectionBacklog = connectionBacklog;
        }

        /// <summary>
        /// Start listening for socket connections.
        /// </summary>
        public void Start()
        {
            lock (this)
            {
                if (!IsRunning)
                    CreateListener();
                else
                    throw new InvalidOperationException("The Server is already running.");
            }

        }

        private void CreateListener()
        {
            lock (this)
            {
                try
                {
                    if (_listenerSocket != null)
                        _listenerSocket.Close();
                }
                catch { }

                try
                {
                    _listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    _listenerSocket.Bind(_endPoint);
                    _listenerSocket.Listen(ConnectionBacklog);
                    ListenForConnection(null);
                }
                catch (Exception e)
                {

                }
            }
        }

        /// <summary>
        /// Stop listening for socket connections.
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (_listenerSocket == null)
                    return;

                _listenerSocket.Close();
                _listenerSocket = null;
            }
        }

        /// <summary>
        /// Asynchronously listens for new connections.
        /// </summary>
        /// <param name="acceptEventArg"></param>
        private void ListenForConnection(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(SocketAccepted);
            }
            else
            {
                // socket must be cleared since the context object is being reused
                acceptEventArg.AcceptSocket = null;
            }

            try
            {
                _listenerSocket.InvokeAsyncMethod(_listenerSocket.AcceptAsync, SocketAccepted, acceptEventArg);
            }
            catch (SocketException)
            {
                CreateListener();
            }
            catch (ObjectDisposedException)
            {
                CreateListener();
            }
        }
        /// <summary>
        /// Invoked when an asynchrounous accept completes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The SocketAsyncEventArgs for the operation.</param>
        public void SocketAccepted(object sender, SocketAsyncEventArgs e)
        {
            //SocketError error = e.SocketError;
            if (e.SocketError == SocketError.OperationAborted)
            {
                CreateListener(); // Hier moeten we wellicht de service restarten?
                return;
            }

            Socket acceptSocket = null;
            if (e.SocketError == SocketError.Success)
            {
                acceptSocket = e.AcceptSocket;
            }

            lock (this)
            {
                ListenForConnection(e);
            }

            if (acceptSocket != null)
            {
                OnSocketConnected(acceptSocket);
            }
        }

        /// <summary>
        /// Fired when a new connection is received.
        /// </summary>
        public event EventHandler<SocketEventArgs> SocketConnected;
        /// <summary>
        /// Fires the SocketConnected event.
        /// </summary>
        /// <param name="client">The new client socket.</param>
        private void OnSocketConnected(Socket client)
        {
            if (SocketConnected != null)
                SocketConnected(this, new SocketEventArgs() { Socket = client });
        }

        private bool disposed = false;
        ~SocketListener()
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
                    Stop();
                }

                disposed = true;
            }
        }
    }
}