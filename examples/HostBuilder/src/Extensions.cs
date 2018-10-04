using Hosting = Microsoft.Extensions.Hosting;
using WebHosting = Microsoft.AspNetCore.Hosting;

namespace HostBuilderExample
{
    public static class Extensions
    {
        /// <summary>
        /// Convert or wrap a Microsoft.AspNetCore.Hosting.IHostingEnvironment object to
        /// a Microsoft.Extensions.Hosting.IHostingEnvironment object.
        /// </summary>
        /// <param name="hostedEnv">The inputs of Microsoft.Extensions.Hosting.IHostingEnvironemnt object.</param>
        /// <returns>Returns the object that implements Microsoft.AspNetCore.Hosting.IHostingEnvironment.</returns>
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