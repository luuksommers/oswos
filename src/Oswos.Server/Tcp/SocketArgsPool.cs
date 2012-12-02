using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Oswos.Server
{
    /// <summary>
    /// Pools SocketAsyncEventArgs objects to avoid repeated allocations.
    /// </summary>
    public class SocketArgsPool : IDisposable
    {
        public const int   MaxCapacity = 100;
        private static SocketArgsPool _instance;
        private static readonly object _instanceLock = new object();
        private readonly Stack<SocketAsyncEventArgs> _argsPool;

        public static SocketArgsPool Instance
        {
            get
            {
                if( _instance == null )
                {
                    lock( _instanceLock )
                    {
                        if( _instance == null )
                        {
                            _instance = new SocketArgsPool( MaxCapacity );
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Pools SocketAsyncEventArgs objects to avoid repeated allocations.
        /// </summary>
        /// <param name="capacity">The ammount to SocketAsyncEventArgs to create and pool.</param>
        private SocketArgsPool( int capacity )
        {
            _argsPool = new Stack<SocketAsyncEventArgs>( capacity );

            for( int i = 0; i < capacity; i++ )
            {
                CheckIn(new SocketAsyncEventArgs());
            }
        }

        /// <summary>
        /// Checks an SocketAsyncEventArgs back into the pool.
        /// </summary>
        /// <param name="item">The SocketAsyncEventsArgs to check in.</param>
        public void CheckIn(SocketAsyncEventArgs item)
        {
            lock (_argsPool)
            {
                if( item != null )
                {
                    _argsPool.Push( item );
                }
            }
        }

        /// <summary>
        /// Check out an SocketAsyncEventsArgs from the pool.
        /// </summary>
        /// <returns>The SocketAsyncEventArgs.</returns>
        public SocketAsyncEventArgs CheckOut()
        {
            lock (_argsPool)
            {
                return _argsPool.Pop();
            }
        }

        /// <summary>
        /// The number of available objects in the pool.
        /// </summary>
        public int Available
        {
            get
            {
                lock (_argsPool)
                {
                    return _argsPool.Count;
                }
            }
        }

        #region IDisposable Members
        private Boolean disposed = false;

        ~SocketArgsPool()
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
                    foreach (SocketAsyncEventArgs args in _argsPool)
                    {
                        args.Dispose();
                    }
                }

                disposed = true;
            }
        }
        #endregion
    }
}
