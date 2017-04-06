namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Netcore.Kubernetes.Entity;

    using static Microsoft.ApplicationInsights.Netcore.Kubernetes.StringUtils;

    /// <summary>
    /// Flatten objects for application insights or other external caller to fetch K8s properties.
    /// </summary>
    public class K8sEnvironment
    {
        // Property holder objects
        private Pod myPod;
        private ContainerStatus myContainerStatus;

        // Waiter to making sure initialization code is run before calling into properties.
        internal EventWaitHandle InitializationWaiter { get; private set; }

#pragma warning disable CA2222 // Do not decrease inherited member visibility
        /// <summary>
        /// Private ctor to prevent the ctor being called.
        /// </summary>
        private K8sEnvironment()
#pragma warning restore CA2222 // Do not decrease inherited member visibility
        {
            this.InitializationWaiter = new ManualResetEvent(false);
        }

        /// <summary>
        /// Async factory method to build the instance of this class.
        /// </summary>
        /// <returns></returns>
        public static async Task<K8sEnvironment> CreateAsync(TimeSpan timeout)
        {
            K8sEnvironment instance = null;
            try
            {
                KubeHttpClientSettingsProvider settings = new KubeHttpClientSettingsProvider();
                using (KubeHttpClient httpClient = new KubeHttpClient(settings))
                {
                    K8sQueryClient queryClient = new K8sQueryClient(httpClient);
                    if (await SpinWaitContainerReady(timeout, queryClient, settings.ContainerId).ConfigureAwait(false))
                    {
                        instance = new K8sEnvironment()
                        {
                            ContainerId = settings.ContainerId
                        };

                        Pod myPod = await queryClient.GetMyPodAsync().ConfigureAwait(false);
                        instance.myPod = myPod;
                        Console.WriteLine(Invariant($"Getting container status of container-id: {settings.ContainerId}"));
                        instance.myContainerStatus = myPod.GetContainerStatus(settings.ContainerId);
                    }
                    else
                    {
                        throw new TimeoutException(Invariant($"Kubernetes info is not available within given time of {timeout.TotalMilliseconds} ms."));
                    }
                }
                return instance;
            }
            finally
            {
                // Signal that initialization is done.
                instance?.InitializationWaiter.Set();
            }
        }

        /// <summary>
        /// Wait until the container is ready.
        /// Refer document @ https://kubernetes.io/docs/concepts/workloads/pods/pod-lifecycle/#pod-phase for the Pod's lifecycle.
        /// </summary>
        /// <param name="timeout">Timeout on Application Insights data when the container is not ready after the period.</param>
        /// <param name="client">Query client to try getting info from the Kubernetes cluster API.</param>
        /// <param name="myContainerId">The container that we are interested in.</param>
        /// <returns></returns>
        private static async Task<bool> SpinWaitContainerReady(TimeSpan timeout, K8sQueryClient client, string myContainerId)
        {
            DateTime tiemoutAt = DateTime.Now.Add(timeout);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Pod myPod = null;
            do
            {
                // When my pod become available and it's status become ready, we recognize the container is ready.
                myPod = await client.GetMyPodAsync().ConfigureAwait(false);
                if (myPod != null && myPod.GetContainerStatus(myContainerId).Ready)
                {
                    stopwatch.Stop();
                    Console.WriteLine(Invariant($"K8s info avaialbe in: {stopwatch.ElapsedMilliseconds} ms."));
                    return true;
                }

                // The time to get the container ready dependes on how much time will a container to be initialized.
                // But the minimum seems about 1000ms. Try invoke a probe on readiness every 500ms until the container is ready
                // Or it will timeout per the timeout settings.
                await Task.Delay(500).ConfigureAwait(false);
            } while (DateTime.Now < tiemoutAt);
            return false;
        }

        #region Shorthands to the properties
        /// <summary>
        /// ContainerID for the current K8s entity.
        /// </summary>
        public string ContainerId { get; private set; }

        public string ContainerName
        {
            get
            {
                return this.myContainerStatus?.Name;
            }
        }

        public string Image
        {
            get
            {
                return this.myContainerStatus?.Image;
            }
        }

        public string PodName
        {
            get
            {
                return this.myPod?.Metadata?.Name;
            }
        }

        public string PodLabels
        {
            get
            {
                string result = null;
                IDictionary<string, string> labelDict = myPod?.Metadata?.Labels;
                if (labelDict != null && labelDict.Count > 0)
                {
                    StringBuilder labelStringBuilder = new StringBuilder();
                    foreach (KeyValuePair<string, string> label in labelDict)
                    {
                        labelStringBuilder.AppendFormat(CultureInfo.InvariantCulture, ",{0}:{1}", label.Key, label.Value);
                    }
                    result = labelStringBuilder.ToString().Substring(1);
                }
                return result;
            }
        }
        #endregion
    }
}
