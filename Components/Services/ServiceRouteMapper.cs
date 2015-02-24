using DotNetNuke.Web.Api;

namespace LinkWise.Modules.OneDrive.Service
{
    public class ServiceRouteMapper : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapHttpRoute("OneDrive", "default", "{controller}/{action}", new[] { "LinkWise.Modules.OneDrive.Components.Services" });
        }
    }
}