#nullable enable

using System.IO;

namespace Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;

/// <summary>
/// Moves stream reader forward for 1 line.
/// </summary>
internal class StreamLineReader : IStreamLineReader
{
    /// <summary>
    /// Try get a new line of string from the stream reader.
    /// </summary>
    /// <param name="sourceStream">The stream reader.</param>
    /// <param name="line">The output of a line of text.</param>
    /// <returns>True when it is not the end of the file yet. Otherwise, false.</returns>
    public bool TryReadLine(StreamReader sourceStream, out string? line)
    {
        if (sourceStream.EndOfStream)
        {
            line = null;
            return false;
        }
        line = sourceStream.ReadLine();
        return true;
    }
}
