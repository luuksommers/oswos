using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace Oswos.Server.Tcp
{
    /// <summary>
    /// Pools data buffers to prevent both frequent allocation and memory fragmentation
    /// due to pinning in high volume scenarios.
    /// See https://blogs.msdn.com/yunjin/archive/2004/01/27/63642.aspx
    /// </summary>
    public class BufferPool
    {
        private readonly int _totalBytes;
        private readonly byte[] _buffer;
        private readonly Stack<int> _freeIndexPool;
        private readonly int _bufferSize;
        private int _currentIndex;
        private const int BufferSize = 4096;
        private static BufferPool _instance;
        private static object _instanceLock = new object();

        public static BufferPool Instance
        {
            get
            {
                if( _instance == null )
                {
                    lock( _instanceLock )
                    {
                        if( _instance == null )
                        {
                            _instance = new BufferPool( SocketArgsPool.MaxCapacity, BufferSize );
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Pools data buffers to prevent both frequent allocation and memory fragmentation
        /// due to pinning in high volume scenarios.
        /// </summary>
        /// <param name="numberOfBuffers">The total number of buffers that will be allocated.</param>
        /// <param name="bufferSize">The size of each buffer.</param>
        private BufferPool( int numberOfBuffers, int bufferSize )
        {
            _totalBytes = numberOfBuffers * bufferSize;
            _bufferSize = bufferSize;

            _currentIndex = 0;
            _freeIndexPool = new Stack<int>();
            _buffer = new Byte[_totalBytes];
        }

        /// <summary>
        /// Checks out some buffer space from the pool.
        /// </summary>
        /// <param name="args">The ScoketAsyncEventArgs which needs a buffer.</param>
        public void CheckOut(SocketAsyncEventArgs args)
        {
            lock (_freeIndexPool)
            {
                if (_freeIndexPool.Count > 0)
                    args.SetBuffer(_buffer, _freeIndexPool.Pop(), _bufferSize);
                else
                {
                    args.SetBuffer(_buffer, _currentIndex, _bufferSize);
                    _currentIndex += _bufferSize;
                }
            }
        }

        /// <summary>
        /// Checks a buffer back in to the pool.
        /// </summary>
        /// <param name="args">The SocketAsyncEventArgs which has finished with it buffer.</param>
        public void CheckIn(SocketAsyncEventArgs args)
        {
            lock (_freeIndexPool)
            {
                if( args != null )
                {
                    _freeIndexPool.Push( args.Offset );
                    args.SetBuffer( null, 0, 0 );
                }
            }
        }

        /// <summary>
        /// The number of available objects in the pool.
        /// </summary>
        public int Available
        {
            get
            {
                lock (_freeIndexPool)
                { 
                    return ((_totalBytes - _currentIndex) / _bufferSize) + _freeIndexPool.Count; 
                }
            }
        }
    }
}
