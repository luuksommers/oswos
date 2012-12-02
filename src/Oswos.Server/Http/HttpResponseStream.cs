using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Oswos.Server.Http
{
    [DataContract]
    public class HttpResponseStream : Stream
    {
        public const string CrLf = "\x0d\x0a";

        [DataMember]
        private bool _firstRead = true;

        [DataMember]
        public string HttpVersion { get; set; }
        [DataMember]
        public string Reason { get; set; }
        [DataMember]
        public int StatusCode { get; set; }
        [DataMember]
        public Dictionary<string, string> Headers { get; private set; }
        [DataMember]
        private MemoryStream _bodyStream = new MemoryStream();

        public HttpResponseStream()
        {
            Headers = new Dictionary<string, string>();
        }

        public override void Flush()
        {
            _bodyStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _bodyStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _bodyStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_firstRead)
            {
                var responseStream = new MemoryStream();
                WriteData(responseStream, HttpVersion + " " + StatusCode + " " + Reason + "\x0d\x0a");
                foreach (var header in Headers.Keys)
                {
                    WriteData(responseStream, header + ":" + Headers[header] + "\x0d\x0a");
                }
                if (_bodyStream.CanRead)
                {
                    WriteData(responseStream, "Content-Length" + ":" + _bodyStream.Length + "\x0d\x0a");
                    WriteData(responseStream, "\x0d\x0a");

                    _bodyStream.Position = 0;
                }
                else
                {
                    WriteData(responseStream, "Content-Length" + ":" + 0 + "\x0d\x0a");
                    WriteData(responseStream, "\x0d\x0a");
                }
                responseStream.Position = 0;
                _firstRead = false;
                return responseStream.Read(buffer, offset, count);
            }

            return _bodyStream.Read(buffer, offset, count);
        }

        private void WriteData(MemoryStream stream, string data)
        {
            var dataBytes = Encoding.UTF8.GetBytes(data);
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _bodyStream.Write(buffer, offset, count);
        }

        public override bool CanRead
        {
            get { return _bodyStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _bodyStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override long Length
        {
            get { return _bodyStream.Length; }
        }

        public override long Position
        {
            get { return _bodyStream.Position; }
            set { _bodyStream.Position = value; }
        }
    }
}
