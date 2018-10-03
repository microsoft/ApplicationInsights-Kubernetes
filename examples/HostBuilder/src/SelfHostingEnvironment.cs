using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Hosting = Microsoft.Extensions.Hosting;
namespace HostBuilderExample
{
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