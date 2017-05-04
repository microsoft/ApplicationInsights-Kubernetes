using System;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Kubernetes;
using Microsoft.ApplicationInsights.Kubernetes.Entities;
using Microsoft.Extensions.Logging;

namespace ApplicationInsights.Kubernetes.Samples.Watcher
{
    class Program
    {
        static void Main(string[] args)
        {
            ILoggerFactory loggerFactory = new LoggerFactory();
            loggerFactory.AddConsole(LogLevel.Trace);

            Task.Run(async () => await (new Program()).RunAsync(loggerFactory).ConfigureAwait(false));
            Console.WriteLine("Press any key to exit . . .");
            Console.ReadKey(true);
        }

        private async Task RunAsync(ILoggerFactory loggerFactory)
        {
            KubectlProxySettingsProvider settingsProvider = new KubectlProxySettingsProvider(null, null, "9fbc75f651de532e2833d9a867d9e97d4b86e551f38d230b68c00585c7e1cc15");
            using (KubeHttpClient client = new KubeHttpClient(settingsProvider))
            {
                K8sWatcher<K8sPod, K8sPodMetadata> watcher = new K8sPodWatcher(loggerFactory);
                watcher.Changed += Watcher_Changed;
                await watcher.StartWatchAsync(client).ConfigureAwait(false);
                Console.WriteLine("Should never be hit...");
            }
        }

        private void Watcher_Changed(object sender, K8sWatcherEventArgs e)
        {
            Console.WriteLine("Event raised: " + e.ToString());

        }
    }
}