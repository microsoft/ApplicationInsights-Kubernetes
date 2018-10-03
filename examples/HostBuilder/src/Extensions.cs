using Hosting = Microsoft.Extensions.Hosting;
using WebHosting = Microsoft.AspNetCore.Hosting;
namespace HostBuilderExample
{
    public static class Extensions
    {
        public static WebHosting.IHostingEnvironment ToAspNetCoreHostingEnvironment(this Hosting.IHostingEnvironment hostedEnv)
        {
            if (hostedEnv is WebHosting.IHostingEnvironment webEnv)
            {
                return webEnv;
            }
            else
            {
                return new SelfHostingEnvironment(hostedEnv);
            }
        }
    }
}