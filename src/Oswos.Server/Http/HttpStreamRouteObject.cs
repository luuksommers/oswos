using System;
using System.ServiceModel;
using NLog;
using Oswos.Server.Tcp;

namespace Oswos.Server.Http
{
    public class HttpStreamRouteObject
    {
        private readonly ISocketConnection _socketConnection;
        private readonly HttpStreamRouter _httpStreamRouter;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly HttpRequestStream _httpRequestStream;

        public HttpStreamRouteObject(ISocketConnection socketConnection, HttpStreamRouter httpStreamRouter)
        {
            _socketConnection = socketConnection;
            _httpStreamRouter = httpStreamRouter;
            _socketConnection.DataReceived += SocketConnectionOnDataReceived;
            _httpRequestStream = new HttpRequestStream();
        }

        private void SocketConnectionOnDataReceived(DataEventArgs dataEventArgs)
        {
            _httpRequestStream.Write(dataEventArgs.Data, dataEventArgs.Offset, dataEventArgs.Length);
            if (_httpRequestStream.HeadersRead)
            {
                FindWebsiteAndSendRequestToEndpoint();
            }
        }

        public void FindWebsiteAndSendRequestToEndpoint()
        {
            var host = string.Empty;

            if (_httpRequestStream.Headers.ContainsKey("Host"))
            {
                host = _httpRequestStream.Headers["Host"];
            }

            try
            {
                Logger.Debug("Requesting:" + _httpRequestStream.Uri);
                var endPoint = _httpStreamRouter.CreateWebsiteEndpoint(host);
                var responseStream = endPoint.ParseRequest(_httpRequestStream);

                SendResponse(responseStream);

                var communicationObject = endPoint as ICommunicationObject;
                if (communicationObject != null && communicationObject.State == CommunicationState.Opened)
                {
                    communicationObject.Close();
                }
            }
            catch (WebsiteNotFoundException ex)
            {
                var response = new HttpResponseStream();
                response.HttpVersion = _httpRequestStream.HttpVersion;
                response.StatusCode = 500;
                response.Reason = ex.Message;
                SendResponse(response);
            }
            catch (Exception ex)
            {
                var response = new HttpResponseStream();
                response.HttpVersion = _httpRequestStream.HttpVersion;
                response.StatusCode = 500;
                response.Reason = ex.Message;
                SendResponse(response);
            }

            _socketConnection.Disconnect();
        }

        private void SendResponse(HttpResponseStream responseStream)
        {
            var buffer = new byte[4096];
            var length = 0;
            responseStream.Position = 0;

            while ((length = responseStream.Read(buffer, 0, 4096)) > 0)
            {
                _socketConnection.SendData(buffer, 0, length);
            }
        }
    }
}