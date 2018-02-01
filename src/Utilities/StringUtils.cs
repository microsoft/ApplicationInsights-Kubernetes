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

        public static string GetReadableSize(this long numInBytes)
        {
            double doubleBytes = numInBytes;
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (doubleBytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                doubleBytes /= 1024.0;
            }

            return String.Format(CultureInfo.InvariantCulture, "{0:0.#}{1}", doubleBytes, sizes[order]);
        }
    }
}