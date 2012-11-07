namespace Oswos.Server.WebsiteAdapter
{
    internal static class OwinConstants
    {
        public const string Version = "owin.Version";
        public const string RequestMethod = "owin.RequestMethod";
        public const string RequestScheme = "owin.RequestScheme";
        public const string RequestPathBase = "owin.RequestPathBase";
        public const string RequestPath = "owin.RequestPath";
        public const string RequestQueryString = "owin.RequestQueryString";
        public const string RequestProtocol = "owin.RequestProtocol";
        public const string RequestBody = "owin.RequestBody";
        public const string RequestHeaders = "owin.RequestHeaders";
        public const string CallCancelled = "owin.CallCancelled";
        public const string ResponseHeaders = "owin.ResponseHeaders";
        public const string ResponseBody = "owin.ResponseBody";
        public const string ResponseStatusCode = "owin.ResponseStatusCode";
        public const string ResponseReasonPhrase = "owin.ResponseReasonPhrase";

        public const string HttpContextBase = "System.Web.HttpContextBase";

        public const string WebSocketVersion = "websocket.Version";
        public const string WebSocketSupport = "websocket.Support";
        public const string WebSocketFunc = "websocket.Func";
        public const string WebSocketSendAsyncFunc = "websocket.SendAsyncFunc";
        public const string WebSocketReceiveAsyncFunc = "websocket.ReceiveAsyncFunc";
        public const string WebSocketCloseAsyncFunc = "websocket.CloseAsyncFunc";
        public const string WebSocketCallCancelled = "websocket.CallCancelled";

        public const string AspNetWebSocketContext = "System.Web.WebSockets.AspNetWebSocketContext";
    }
}