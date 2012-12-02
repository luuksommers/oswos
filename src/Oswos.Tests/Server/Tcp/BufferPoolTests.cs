using System;
using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oswos.Server;
using Oswos.Server.Tcp;

namespace Oswos.Tests.Server
{
    [TestClass]
    public class BufferPoolTests
    {
        [TestMethod]
        public void BufferPool_Has_Same_Capacity_As_SockerArgs_Pool()
        {
            Assert.AreEqual(SocketArgsPool.MaxCapacity, BufferPool.Instance.Available);
        }

        [TestMethod]
        public void BufferPool_CheckOut_Sets_SocketArg_Data_Buffer()
        {
            var socketArg = new SocketAsyncEventArgs();
            BufferPool.Instance.CheckOut(socketArg);
            Assert.IsNotNull(socketArg.Buffer);
        }

        [TestMethod]
        public void BufferPool_CheckIn_Resets_SocketArg_Data_Buffer()
        {
            var socketArg = new SocketAsyncEventArgs();
            BufferPool.Instance.CheckOut(socketArg);
            BufferPool.Instance.CheckIn(socketArg);
            Assert.IsNull(socketArg.Buffer);
        }
    }
}
