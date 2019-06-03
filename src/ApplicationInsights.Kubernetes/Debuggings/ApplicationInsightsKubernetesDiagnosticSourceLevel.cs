namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Diagnostic Source message levels
    /// </summary>
    public static class ApplicationInsightsKubernetesDiagnosticSourceLevel
    {
        /// <summary>
        /// Gets the level of critical.
        /// </summary>
        /// <returns></returns>
        public const string Critical = nameof(Critical);
        /// <summary>
        /// Gets the level of error.
        /// </summary>
        /// <returns></returns>
        public const string Error = nameof(Error);
        /// <summary>
        /// Gets the level of warning.
        /// </summary>
        /// <returns></returns>
        public const string Warning = nameof(Warning);
        /// <summary>
        /// Gets the level of information.
        /// </summary>
        /// <returns></returns>
        public const string Information = nameof(Information);
        /// <summary>
        /// Gets the level of debug.
        /// </summary>
        /// <returns></returns>
        public const string Debug = nameof(Debug);
        /// <summary>
        /// Gets the level of trace.
        /// </summary>
        /// <returns></returns>
        public const string Trace = nameof(Trace);
    }
}