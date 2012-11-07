using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.ServiceModel;

namespace Oswos.Server.WebsiteAdapter
{
    public class OwinWebsiteAdapter : IWebsiteAdapter
    {
        private readonly dynamic _website;

        public OwinWebsiteAdapter()
        {
            var type = FindInterface(AppDomain.CurrentDomain.BaseDirectory);
            _website = Activator.CreateInstance(type);
        }

        public Stream ParseRequest(HttpStream stream)
        {
            var environment = new Dictionary<string, object>();

            environment.Add("owin.RequestMethod", stream.Method);
            environment.Add("owin.RequestPath", stream.Uri);
            environment.Add("owin.RequestScheme", "http");

            environment.Add("owin.RequestPathBase", string.Empty);
            environment.Add("owin.RequestQueryString", string.Empty);
            environment.Add("owin.RequestBody", stream);

            var headers = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach( var headerKey in stream.Headers.Keys)
            {
                headers.Add(
                    headerKey,
                    stream.Headers[headerKey].Split(',').Select(a=>a.Trim()).ToArray());
            }
            environment.Add("owin.RequestHeaders", headers);

            environment.Add("owin.ResponseHeaders", new Dictionary<string, string[]>(StringComparer.Ordinal));
            environment.Add("owin.ResponseBody", new MemoryStream());
            try
            {
                _website.ProcessRequest(environment).Wait();
            }
            catch (AggregateException exception)
            {
                Set(environment, "owin.ResponseStatusCode", 500);
                Set(environment, "owin.ResponseReasonPhrase", exception.Message);
                Set(environment, "owin.ResponseBody", new MemoryStream(Encoding.UTF8.GetBytes(exception.InnerException.Message)));
            }

            //// Parse response
            var responseHeaders = Get<IDictionary<string, String[]>>(environment, "owin.ResponseHeaders");
            var responseStatusCode = Get<int>(environment, "owin.ResponseStatusCode");
            var responseReason = Get<string>(environment, "owin.ResponseReasonPhrase");
            var responseBodyStream = Get<MemoryStream>(environment, "owin.ResponseBody");
            
            var responseStream = new MemoryStream();
            WriteData(responseStream, stream.HttpVersion + " " + responseStatusCode + " " + responseReason + "\x0d\x0a");
            foreach (var header in responseHeaders.Keys)
            {
                WriteData(responseStream, header + ":" + string.Join(",", responseHeaders[header]) + "\x0d\x0a");
            }

            WriteData(responseStream, "Content-Length" + ":" + responseBodyStream.Length + "\x0d\x0a");
            WriteData(responseStream, "\x0d\x0a");

            responseBodyStream.Position = 0;
            responseBodyStream.CopyToAsync(responseStream);
            responseStream.Position = 0;
            return responseStream;
        }

        private void WriteData(MemoryStream stream, string data)
        {
            //Console.Write(data);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            stream.Write(dataBytes, 0, dataBytes.Length);
        }

        private static T Get<T>(IDictionary<string, object> env, string key)
        {
            object value;
            return env.TryGetValue(key, out value) && value is T ? (T)value : default(T);
        }

        private static void Set(IDictionary<string, object> env, string key, object value)
        {
            env[key] = value;
        }

        public Type FindInterface(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);

            foreach (string file in Directory.GetFiles(path, "*.dll"))
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.FullName.EndsWith("Startup") && type.GetMethod("ProcessRequest") != null)
                        {
                            return type;
                        }
                    }
                }
                catch
                {
                }
            }

            var binDirectory = Path.Combine(path, "bin");
            if (Directory.Exists(binDirectory))
            {
                foreach (string file in Directory.GetFiles(binDirectory, "*.dll"))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFile(file);
                        foreach (var type in assembly.GetTypes())
                        {
                            if (type.FullName.EndsWith("Startup") && type.GetMethod("ProcessRequest") != null)
                            {
                                return type;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            throw new ArgumentException("No IWebsiteEndpointHost found in directory {0}", path);
        }
    }
}

