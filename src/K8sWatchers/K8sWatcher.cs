namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.Kubernetes.Entities;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;

    using static Microsoft.ApplicationInsights.Kubernetes.StringUtils;

    internal class K8sWatcher<TEntity, TMetadata> : IDisposable
        where TEntity : K8sEntity<TMetadata>
        where TMetadata : K8sObjectMetadata
    {
        private ILogger<K8sWatcher<TEntity, TMetadata>> logger;
        CancellationTokenSource cancellationTokenSource;
        bool isDisposed = false;

        public K8sWatcher(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory?.CreateLogger<K8sWatcher<TEntity, TMetadata>>();
        }

        private Dictionary<string, string> VersionTracker { get; set; }

        public event EventHandler<K8sWatcherEventArgs> Changed;

        protected virtual void OnChanged(K8sWatcherEventArgs e)
        {
            Changed?.Invoke(this, e);
        }

        protected virtual string RelativePath { get; }

        public virtual async Task StartWatchAsync(IKubeHttpClient kubeHttpClient)
        {
            Arguments.IsNotNull(kubeHttpClient, nameof(kubeHttpClient));
            this.cancellationTokenSource = new CancellationTokenSource();

            string line = null;
            using (Stream inStream = await kubeHttpClient.GetStreamAsync(kubeHttpClient.GetQueryUrl(this.RelativePath)).ConfigureAwait(false))
            using (StreamReader reader = new StreamReader(inStream))
            {
                try
                {
                    line = await reader.ReadLineAsync().ConfigureAwait(false);
                    while (!string.IsNullOrEmpty(line))
                    {
                        if (cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            throw new TaskCanceledException();
                        }

                        // TODO: process the line, raise the event when necessary
                        Process(line);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Stop is called.
                }
            }
        }

        private void Process(string jsonLine)
        {
            try
            {
                K8sWatcherObject<TEntity, TMetadata> watcherObject = JsonConvert.DeserializeObject<K8sWatcherObject<TEntity, TMetadata>>(jsonLine);
                string key = watcherObject?.EventObject?.Metadata?.Uid;
                string value = watcherObject?.EventObject?.Metadata?.ResourceVersion;

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                {
                    bool eventHappens = false;
                    if (this.VersionTracker.ContainsKey(key))
                    {
                        if (this.VersionTracker[key] != value)
                        {
                            // Event happens
                            eventHappens = true;
                        }
                    }
                    else
                    {
                        // Newly found object
                        eventHappens = true;
                    }

                    if (eventHappens)
                    {
                        // Raise the event and recornd the last version.
                        this.OnChanged(GetK8sWatcherEventArgs(watcherObject));
                        this.VersionTracker[key] = value;
                        logger?.LogTrace(Invariant($"Object [{watcherObject?.EventObject?.Metadata?.Name}({watcherObject?.EventObject?.Metadata?.Uid})] of kind [{watcherObject?.EventObject?.Kind}] changed. Event type: {watcherObject?.EventType}"));
                    }
                }
                else
                {
                    logger?.LogDebug(Invariant($"Watch object key or value is null: {key} = {value}"));
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex.ToString());
            }
        }

        private K8sWatcherEventArgs GetK8sWatcherEventArgs(K8sWatcherObject<TEntity, TMetadata> watcherObject)
        {
            K8sWatcherEventArgs newArgs = new K8sWatcherEventArgs()
            {
                ObjectUid = watcherObject.EventObject?.Metadata?.Uid,
                EventType = watcherObject.EventType
            };

            return newArgs;
        }

        public virtual void StopWatch()
        {
            this.cancellationTokenSource?.Cancel();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (!this.isDisposed)
                {
                    this.isDisposed = true;

                    if (this.cancellationTokenSource != null)
                    {
                        this.cancellationTokenSource.Dispose();
                        this.cancellationTokenSource = null;
                    }
                }
            }
        }
    }
}
