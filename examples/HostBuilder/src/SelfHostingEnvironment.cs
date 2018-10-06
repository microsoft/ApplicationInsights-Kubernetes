using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Hosting = Microsoft.Extensions.Hosting;
namespace HostBuilderExample
{
    /// <summary>
    /// This is a simple wrapper of Microsoft.AspNetCore.Hosting.IHostingEnvironment.
    /// It is mainly to mitigate the fact there is no class that implements both
    /// that and Microsoft.Extensions.Hosting.IHostingEnvironment.
    /// It is possibile to merge the implementations.
    /// </summary>
    public class SelfHostingEnvironment : IHostingEnvironment
    {
        public SelfHostingEnvironment(Hosting.IHostingEnvironment env)
        {
            EnvironmentName = env.EnvironmentName ?? "SelfHostedEnvironment";
            ApplicationName = env.ApplicationName ?? "Host Build Example App";
            ContentRootPath = env.ContentRootPath;
            ContentRootFileProvider = env.ContentRootFileProvider;
            WebRootPath = null;
            WebRootFileProvider = null;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}