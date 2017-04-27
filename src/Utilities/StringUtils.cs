namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    using System.Globalization;
    internal static class StringUtils
    {
        public static string Invariant(FormattableString formattable)
        {
            return formattable.ToString(CultureInfo.InvariantCulture);
        }
    }
}