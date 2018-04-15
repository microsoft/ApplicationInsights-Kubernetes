namespace Microsoft.ApplicationInsights.Kubernetes
{
    using System;
    internal static class Arguments
    {
        public static T IsNotNull<T>(T subject, string argumentName)
        {
            if (subject == null)
            {
                throw new ArgumentNullException(argumentName);
            }
            return subject;
        }

        public static string IsNotNullOrEmpty(string subject, string argumentName)
        {
            if (string.IsNullOrEmpty(subject)) {
                throw new ArgumentNullException(argumentName);
            }
            return subject;
        }
    }
}
