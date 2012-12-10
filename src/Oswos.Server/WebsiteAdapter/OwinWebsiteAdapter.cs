using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Oswos.Server.Http;

namespace Oswos.Server.WebsiteAdapter
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.PerCall, ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class OwinWebsiteAdapter : IWebsiteAdapter
    {
        private readonly object _startupClassInstance;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Type _startupClassType;

        public OwinWebsiteAdapter()
        {
            _startupClassType = GetOwinStartupClassType(AppDomain.CurrentDomain.BaseDirectory);
            _startupClassInstance = Activator.CreateInstance(_startupClassType);
        }

        public HttpResponseStream ParseRequest(HttpRequestStream requestStream)
        {
            Logger.Debug("Sending Request to:" + _startupClassType.Name);
            var environment = new Dictionary<string, object>();

            var requestPath = requestStream.Uri;
            var querystring = string.Empty;
            if (requestStream.Uri.Contains("?"))
            {
                requestPath = requestStream.Uri.Split('?')[0];
                querystring = requestStream.Uri.Split('?')[1];
            }
            environment.Add("owin.RequestMethod", requestStream.Method);
            environment.Add("owin.RequestPath", requestPath);
            environment.Add("owin.RequestScheme", "http");

            environment.Add("owin.RequestPathBase", string.Empty);
            environment.Add("owin.RequestQueryString", querystring);
            environment.Add("owin.RequestBody", requestStream);

            var headers = new Dictionary<string, string[]>(StringComparer.Ordinal);
            foreach (var headerKey in requestStream.Headers.Keys)
            {
                headers.Add(
                    headerKey,
                    requestStream.Headers[headerKey].Split(',').Select(a => a.Trim()).ToArray());
            }
            environment.Add("owin.RequestHeaders", headers);

            environment.Add("owin.ResponseHeaders", new Dictionary<string, string[]>(StringComparer.Ordinal));
            environment.Add("owin.ResponseBody", new HttpResponseStream());

            InvokeStartupClass(environment);

            //// Parse response
            var responseHeaders = Get<IDictionary<string, String[]>>(environment, "owin.ResponseHeaders");
            var responseStatusCode = Get<int>(environment, "owin.ResponseStatusCode");
            var responseReason = Get<string>(environment, "owin.ResponseReasonPhrase");
            var responseStream = Get<HttpResponseStream>(environment, "owin.ResponseBody");

            responseStream.HttpVersion = requestStream.HttpVersion;
            responseStream.StatusCode = responseStatusCode;
            responseStream.Reason = responseReason;

            foreach (var header in responseHeaders.Keys)
            {
                responseStream.Headers.Add(header, string.Join(",", responseHeaders[header]));
            }

            Logger.Debug("Sending Fishished");
            return responseStream;
        }

        private void InvokeStartupClass(Dictionary<string, object> environment)
        {
            var startupMethod = _startupClassType.GetMethod("ProcessRequest");
            try
            {
                var task = (Task)startupMethod.Invoke(_startupClassInstance, new object[]{ environment});
                task.Wait(5000);
            }
            catch (AggregateException exception)
            {
                Logger.DebugException("OwinWebsiteAdapter.ParseRequest", exception);
                Set(environment, "owin.ResponseStatusCode", 500);
                Set(environment, "owin.ResponseReasonPhrase", exception.Message);
                Set(environment, "owin.ResponseBody", new MemoryStream(Encoding.UTF8.GetBytes(exception.InnerException.Message)));
            }
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

        public Type GetOwinStartupClassType(string path)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException(path);

            foreach (string file in Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
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

            throw new ArgumentException("No IWebsiteEndpointHost found in directory {0}", path);
        }
    }
}

