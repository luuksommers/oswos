using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Oswos.Server.Http;

namespace Oswos.Tests.Server.Http
{
    [TestClass]
    public class HttpResponseStreamTests
    {
        [TestMethod]
        public void HttpResponseStream_Sends_Version_Status_Reason_In_Stream()
        {
            var responseStream = new HttpResponseStream();
            responseStream.StatusCode = 200;
            responseStream.HttpVersion = "HTTP/1.1";
            responseStream.Reason = "This is a test";

            var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);

            var expectedData = "HTTP/1.1 200 This is a test" + "\x0d\x0a" + "Content-Length:0\x0d\x0a\x0d\x0a";
            var responseData = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.AreEqual(expectedData, responseData);
        }

        [TestMethod]
        public void HttpResponseStream_Sends_Headers_In_Stream()
        {
            var responseStream = new HttpResponseStream();
            responseStream.StatusCode = 200;
            responseStream.HttpVersion = "HTTP/1.1";
            responseStream.Reason = "This is a test";
            responseStream.Headers.Add("Header1", "Value1");

            var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);

            var expectedData = "HTTP/1.1 200 This is a test\x0d\x0aHeader1:Value1" + "\x0d\x0a" + "Content-Length:0\x0d\x0a\x0d\x0a";
            var responseData = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.AreEqual(expectedData, responseData);
        }

        [TestMethod]
        public void HttpResponseStream_Sends_MessageBody_In_Stream()
        {
            var responseStream = new HttpResponseStream();
            responseStream.StatusCode = 200;
            responseStream.HttpVersion = "HTTP/1.1";
            responseStream.Reason = "This is a test";
            responseStream.Headers.Add("Header1", "Value1");
            var bodyText = Encoding.UTF8.GetBytes("This is the body");
            responseStream.Write(bodyText, 0, bodyText.Length);

            var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);

            var expectedData = "HTTP/1.1 200 This is a test\x0d\x0aHeader1:Value1" + "\x0d\x0a" + "Content-Length:16\x0d\x0a\x0d\x0aThis is the body";
            var responseData = Encoding.UTF8.GetString(memoryStream.ToArray());

            Assert.AreEqual(expectedData, responseData);
        }

        [TestMethod]
        [DeploymentItem("Resources\\oswos.fw.png")]
        public void HttpResponseStream_Can_Send_Large_Data_In_Stream()
        {
            var responseStream = new HttpResponseStream();
            responseStream.StatusCode = 200;
            responseStream.HttpVersion = "HTTP/1.1";
            responseStream.Reason = "This is a test";
            responseStream.Headers.Add("Header1", "Value1");
            var fileStream = File.OpenRead("oswos.fw.png");
            fileStream.CopyTo(responseStream);

            var memoryStream = new MemoryStream();
            responseStream.CopyTo(memoryStream);
        }
    }
}
