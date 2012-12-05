using System;
using System.ServiceModel;
using System.Text;
using NLog;
using Oswos.Server.Tcp;

namespace Oswos.Server.Http
{
    public class HttpStreamRouteObject
    {
        private readonly SocketConnection _socketConnection;
        private readonly HttpStreamRouter _httpStreamRouter;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private HttpRequestStream _httpRequestStream;

        public HttpStreamRouteObject(SocketConnection socketConnection, HttpStreamRouter httpStreamRouter)
        {
            _socketConnection = socketConnection;
            _httpStreamRouter = httpStreamRouter;
            _socketConnection.DataReceived += SocketConnectionOnDataReceived;
            _httpRequestStream = new HttpRequestStream();
        }

        private void SocketConnectionOnDataReceived(DataEventArgs dataEventArgs)
        {
            _httpRequestStream.Write(dataEventArgs.Data, dataEventArgs.Offset, dataEventArgs.Length);
            if (_httpRequestStream.Headers != null && _httpRequestStream.Headers.Count > 0)
            {
                Stream_OnHeadersLoaded();
            }
        }

        public void Stream_OnHeadersLoaded()
        {
            var host = string.Empty;


            if (_httpRequestStream.Headers != null && _httpRequestStream.Headers.ContainsKey("Host"))
            {
                host = _httpRequestStream.Headers["Host"];
            }

            //var website = _repository.GetByHost(host);
            //if (website == null)
            //{
            //    var errorMessage =
            //        Encoding.UTF8.GetBytes(_httpRequestStream.HttpVersion + " 500 Website Not Found\x0d\x0a");
            //    _socketConnection.SendData(errorMessage, 0, errorMessage.Length);
            //}
            //else if (_httpRequestStream.Headers == null)
            //{
            //    var errorMessage =
            //        Encoding.UTF8.GetBytes(_httpRequestStream.HttpVersion + " 500 Headers Not Found\x0d\x0a");
            //    _socketConnection.SendData(errorMessage, 0, errorMessage.Length);
            //}
            //else
            //{
                try
                {
                    Logger.Debug("Requesting:" + _httpRequestStream.Uri);
                    var endPoint = _httpStreamRouter.GetNewEndpointFromHost(host);
                    var responseStream = endPoint.ParseRequest(_httpRequestStream);

                    var buffer = new byte[4096];
                    var length = 0;
                    responseStream.Position = 0;
                    while ((length = responseStream.Read(buffer, 0, 4096)) > 0)
                    {
                        _socketConnection.SendData(buffer, 0, length);
                    }

                    var communicationObject = endPoint as ICommunicationObject;
                    if (communicationObject != null && communicationObject.State == CommunicationState.Opened)
                    {
                        communicationObject.Close();
                    }
                }
                catch (Exception ex)
                {
                    var errorMessage =
                        Encoding.UTF8.GetBytes(_httpRequestStream.HttpVersion + " 500 " + ex.Message + "\x0d\x0a");
                    _socketConnection.SendData(errorMessage, 0, errorMessage.Length);
                }
            //}
                
            _socketConnection.Disconnect();
        }
    }
}