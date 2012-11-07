using Nancy;

namespace Oswos.Website.Default.Modules
{
    public class HomeModule : NancyModule
    {
        public HomeModule()
        {
            Get["/"] = _ => View["Index.sshtml"];
        }
    }
}
