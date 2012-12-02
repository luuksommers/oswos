using System;

namespace Oswos.Server
{
    public class StreamRouterException : Exception
    {
        public StreamRouterException(string message, params object[] args)
            : base(string.Format(message,args))
        {
            
        }
    }
}