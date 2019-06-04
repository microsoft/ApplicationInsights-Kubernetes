namespace Microsoft.ApplicationInsights.Kubernetes.Debugging
{
    /// <summary>
    /// Diagnostic Source message levels
    /// </summary>
    public enum InspectLevel
    {
        /// <summary>
        /// Gets the level of critical.
        /// </summary>
        /// <returns></returns>
        Critical = 5,
        /// <summary>
        /// Gets the level of error.
        /// </summary>
        /// <returns></returns>
        Error = 4,
        /// <summary>
        /// Gets the level of warning.
        /// </summary>
        /// <returns></returns>
        Warning = 3,
        /// <summary>
        /// Gets the level of information.
        /// </summary>
        /// <returns></returns>
        Information = 2,
        /// <summary>
        /// Gets the level of debug.
        /// </summary>
        /// <returns></returns>
        Debug = 1,
        /// <summary>
        /// Gets the level of trace.
        /// </summary>
        /// <returns></returns>
        Trace = 0,
    }
}