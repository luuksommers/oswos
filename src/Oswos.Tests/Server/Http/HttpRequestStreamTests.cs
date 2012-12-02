using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oswos.Server.Http;

namespace Oswos.Tests.Server.Http
{

    [TestClass]
    public class HttpRequestStreamTests
    {
        [TestMethod]
        public void HttpRequestStream_Parses_Method()
        {
            const string methodAndUri = "Method";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStream = new HttpRequestStream();
            httpStream.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual("Method", httpStream.Method);
        }

        [TestMethod]
        public void HttpRequestStream_Parses_Uri()
        {
            const string methodAndUri = "bla Uri";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStream = new HttpRequestStream();
            httpStream.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual("Uri", httpStream.Uri);
        }

        [TestMethod]
        public void HttpRequestStream_Parses_Version()
        {
            const string methodAndUri = "bla bla Version";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStream = new HttpRequestStream();
            httpStream.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual("Version", httpStream.HttpVersion);
        }

        [TestMethod]
        public void HttpRequestStream_Skips_No_Data()
        {
            const string methodAndUri = "";
            var methodAndUriBytes = Encoding.UTF8.GetBytes(methodAndUri);

            var httpStream = new HttpRequestStream();
            httpStream.Write(methodAndUriBytes, 0, methodAndUriBytes.Length);

            Assert.AreEqual(string.Empty, httpStream.Method);
            Assert.AreEqual(string.Empty, httpStream.Uri);
            Assert.AreEqual(string.Empty, httpStream.HttpVersion);
        }


        [TestMethod]
        public void HttpRequestStream_Reads_Single_Header()
        {
            const string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0aTest:OK";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStream = new HttpRequestStream();
            httpStream.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            Assert.AreEqual(1, httpStream.Headers.Count);
        }

        [TestMethod]
        public void HttpRequestStream_Parses_Single_Header()
        {
            const string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0aTest:OK";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStream = new HttpRequestStream();
            httpStream.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            Assert.IsTrue(httpStream.Headers.ContainsKey("Test"));
            Assert.AreEqual("OK", httpStream.Headers["Test"]);
        }

        [TestMethod]
        public void HttpRequestStream_Parses_Second_Header()
        {
            const string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0aTest:OK\x0d\x0aTestLine2:Also Ok\x0d\x0a";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStream = new HttpRequestStream();
            httpStream.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            Assert.IsTrue(httpStream.Headers.ContainsKey("TestLine2"));
            Assert.AreEqual("Also Ok", httpStream.Headers["TestLine2"]);
        }

        [TestMethod]
        public void HttpRequestStream_Streams_Body_On_Read()
        {
            const string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0a\x0d\x0aThis is the body";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStream = new HttpRequestStream();
            httpStream.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            var bodyBuffer = new byte[1024];
            httpStream.Position = 0;
            var bytesRead = httpStream.Read(bodyBuffer, 0, bodyBuffer.Length);
            var body = Encoding.UTF8.GetString(bodyBuffer, 0, bytesRead);

            Assert.AreEqual("This is the body", body);
        }


        [TestMethod]
        public void HttpRequestStream_Streams_Body_On_MultipleWrite()
        {
            string httpMessage = "GET http://localhost:9000/ HTTP/1.1\x0d\x0a\x0d\x0aThis ";
            var httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);

            var httpStream = new HttpRequestStream();
            httpStream.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            // Second write
            httpMessage = "is the body";
            httpMessageBytes = Encoding.UTF8.GetBytes(httpMessage);
            httpStream.Write(httpMessageBytes, 0, httpMessageBytes.Length);

            var bodyBuffer = new byte[1024];
            httpStream.Position = 0;
            var bytesRead = httpStream.Read(bodyBuffer, 0, bodyBuffer.Length);
            var body = Encoding.UTF8.GetString(bodyBuffer, 0, bytesRead);

            Assert.AreEqual("This is the body", body);
        }
    }
}
