using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Oswos.Repository;
using Oswos.Server;
using Oswos.Server.Http;

namespace Oswos.Tests.Server.Http
{
    [TestClass]
    public class HttpStreamRouterTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            //var repositoryMock = new Mock<WebsiteRepository>();
            //repositoryMock.Setup(a => a.GetAll()).Returns(() =>
            //                                                  {
            //                                                      return new List<Website>()
            //                                                                 {
            //                                                                     new Website()
            //                                                                         {
            //                                                                             HostName = "test", 
            //                                                                             Name = "UnitTest", 
            //                                                                             Id = 1, 
            //                                                                             Path = "\\Websites"
            //                                                                         }
            //                                                                 };
            //                                                  });

            //var connectionMock = new Mock<ISocketConnection>();
            
            //var router = new HttpStreamRouter(repositoryMock.Object);

            //router.Route(connectionMock.Object);

        }
    }
}
