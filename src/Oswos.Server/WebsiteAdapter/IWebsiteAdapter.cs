using System.IO;
using System.ServiceModel;

namespace Oswos.Server.WebsiteAdapter
{
    [ServiceContract]
    public interface IWebsiteAdapter
    {
        [OperationContract]
        Stream ParseRequest(HttpStream stream);
    }
}