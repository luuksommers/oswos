using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oswos.Server;

namespace Oswos.Tests.Server
{

    [TestClass]
    public class HttpStreamTests
    {
        [TestMethod]
        public void HttpStreamConverter_Parses_Method()
        {
            string methodAndUri = "Method";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual("Method", httpStreamConverter.Method);
        }

        [TestMethod]
        public void HttpStreamConverter_Parses_Uri()
        {
            string methodAndUri = "bla Uri";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual("Uri", httpStreamConverter.Uri);
        }

        [TestMethod]
        public void HttpStreamConverter_Parses_Version()
        {
            string methodAndUri = "bla bla Version";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual("Version", httpStreamConverter.HttpVersion);
        }

        [TestMethod]
        public void HttpStreamConverter_Skips_No_Data()
        {
            string methodAndUri = "";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual(string.Empty, httpStreamConverter.Method);
            Assert.AreEqual(string.Empty, httpStreamConverter.Uri);
            Assert.AreEqual(string.Empty, httpStreamConverter.HttpVersion);
        }


        [TestMethod]
        public void HttpStreamConverter_Reads_Single_Header()
        {
            string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0aTest:OK";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            Assert.AreEqual(1, httpStreamConverter.Headers.Count);
        }

        [TestMethod]
        public void HttpStreamConverter_Parses_Single_Header()
        {
            string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0aTest:OK";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            Assert.IsTrue(httpStreamConverter.Headers.ContainsKey("Test"));
            Assert.AreEqual("OK", httpStreamConverter.Headers["Test"]);
        }

        [TestMethod]
        public void HttpStreamConverter_Parses_Second_Header()
        {
            string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0aTest:OK\x0d\x0aTestLine2:Also Ok\x0d\x0a";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            Assert.IsTrue(httpStreamConverter.Headers.ContainsKey("TestLine2"));
            Assert.AreEqual("Also Ok", httpStreamConverter.Headers["TestLine2"]);
        }

        [TestMethod]
        public void HttpStreamConverter_Streams_Body_On_Read()
        {
            string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0a\x0d\x0aThis is the body";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            var bodyBuffer = new byte[1024];
            var bytesRead = httpStreamConverter.Read(bodyBuffer, 0, bodyBuffer.Length);
            var body = Encoding.UTF8.GetString(bodyBuffer, 0, bytesRead);

            Assert.AreEqual("This is the body", body);
        }


        [TestMethod]
        public void HttpStreamConverter_Streams_Body_On_MultipleWrite()
        {
            string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0a\x0d\x0aThis ";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStreamConverter = new HttpStream();
            httpStreamConverter.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            // Second write
            httpMessage = "is the body";
            httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);
            httpStreamConverter.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            var bodyBuffer = new byte[1024];
            var bytesRead = httpStreamConverter.Read(bodyBuffer, 0, bodyBuffer.Length);
            var body = Encoding.UTF8.GetString(bodyBuffer, 0, bytesRead);

            Assert.AreEqual("This is the body", body);
        }
    }
}
