namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Diagnostic Source message levels
    /// </summary>
    public enum ApplicationInsightsKubernetesDiagnosticSourceLevel
    {
        /// <summary>
        /// Gets the level of critical.
        /// </summary>
        /// <returns></returns>
        Critical = 0,
        /// <summary>
        /// Gets the level of error.
        /// </summary>
        /// <returns></returns>
        Error,
        /// <summary>
        /// Gets the level of warning.
        /// </summary>
        /// <returns></returns>
        Warning,
        /// <summary>
        /// Gets the level of information.
        /// </summary>
        /// <returns></returns>
        Information,
        /// <summary>
        /// Gets the level of debug.
        /// </summary>
        /// <returns></returns>
        Debug,
        /// <summary>
        /// Gets the level of trace.
        /// </summary>
        /// <returns></returns>
        Trace,
    }
}