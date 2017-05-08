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
    }
}
