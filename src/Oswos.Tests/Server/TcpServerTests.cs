using System.Net.Sockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oswos.Server;

namespace Oswos.Tests.Server
{

    [TestClass]
    public class TcpServerTests
    {
        public class Ignore : INetworkStreamProcessor
        {
            public void ProcessStream(NetworkStream tcpStream)
            {
                
            }
        }

        [TestMethod]
        public void TcpServer_Accepts_Tcp_Connections()
        {
            // Arrange
            var server = new TcpServer(new Ignore());
            server.Start(65000);

            // Act
            var client = new TcpClient();
            client.Connect("localhost", 65000);

            // Assert
            Assert.IsTrue(client.Connected);
        }
    }
}
