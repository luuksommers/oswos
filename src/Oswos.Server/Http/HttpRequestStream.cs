using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Oswos.Server.Http
{
    [DataContract]
    public class HttpRequestStream : Stream
    {
        public const string CrLf = "\x0d\x0a";
        private bool _headersRead = false;
        private string _headerData;

        [DataMember]
        public string Method { get; private set; }
        [DataMember]
        public string Uri { get; private set; }
        [DataMember]
        public string HttpVersion { get; private set; }
        [DataMember]
        public Dictionary<string, string> Headers { get; private set; }
        [DataMember]
        private MemoryStream _bodyStream = new MemoryStream();

        public bool HeadersRead
        {
            get { return _headersRead; }
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
            return _bodyStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var bodyStart = 0;

            if (!_headersRead)
            {
                var streamData = Encoding.UTF8.GetString(buffer, 0, count);
                _headerData += streamData;
                bodyStart = _headerData.IndexOf(CrLf + CrLf, StringComparison.Ordinal);
                if (bodyStart >= 0)
                {
                    _headersRead = true;
                    _headerData = _headerData.Substring(0, bodyStart);
                }
            }

            if (_headersRead)
            {
                var httpLines = _headerData.Split(new[] { CrLf }, StringSplitOptions.None);
                if (httpLines.Length > 0)
                {
                    var requestLine = httpLines[0];
                    var splitRequest = requestLine.Split(' ');

                    Method = splitRequest.Length > 0 ? splitRequest[0] : string.Empty;
                    Uri = splitRequest.Length > 1 ? splitRequest[1] : string.Empty;
                    HttpVersion = splitRequest.Length > 2 ? splitRequest[2] : string.Empty;
                }

                if (httpLines.Length > 1)
                {
                    Headers = new Dictionary<string, string>(StringComparer.Ordinal);

                    for (int headerLineIndex = 1; headerLineIndex < httpLines.Length; headerLineIndex++)
                    {
                        if (string.IsNullOrEmpty(httpLines[headerLineIndex])) // The start of the body
                            break;

                        var header = httpLines[headerLineIndex];

                        Headers.AddOrUpdate(
                            header.Split(':')[0],
                            header.Split(':')[1].Trim());
                    }
                }
            }

            if (bodyStart >= 0 && count - (bodyStart + 4) > 0)
            {
                _bodyStream.Write(buffer, (bodyStart + 4), count - (bodyStart + 4));
            }
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
