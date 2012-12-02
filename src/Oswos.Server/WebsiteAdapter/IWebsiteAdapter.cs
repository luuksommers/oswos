using System.IO;
using System.ServiceModel;
using Oswos.Server.Http;

namespace Oswos.Server.WebsiteAdapter
{
    [ServiceContract]
    public interface IWebsiteAdapter
    {
        [OperationContract]
        HttpResponseStream ParseRequest(HttpRequestStream requestStream);
    }
}