#nullable enable

using System.IO;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

internal interface IStreamLineReader
{
    /// <summary>
    /// Reads one line from the stream until the end of the stream.
    /// </summary>
    bool TryReadLine(StreamReader sourceStream, out string? line);
}
