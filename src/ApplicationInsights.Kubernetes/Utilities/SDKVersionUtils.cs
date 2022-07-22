﻿using System;
using System.Reflection;

namespace Microsoft.ApplicationInsights.Kubernetes.Utilities
{
    internal sealed class SDKVersionUtils
    {
        #region Singleton
        private SDKVersionUtils() { }
        public static SDKVersionUtils Instance { get; } = new SDKVersionUtils();
        #endregion

        public string CurrentSDKVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_sdkVersion))
                {
                    _sdkVersion = $"{SdkName}:{GetSDKVersion()}";
                }
                return _sdkVersion;
            }
        }

        #region private
        private static string GetSDKVersion()
        {
            Assembly assembly = typeof(SDKVersionUtils).GetTypeInfo().Assembly;
            Version version = assembly.GetName().Version;
            return version.ToString();
        }

        private const string SdkName = "ai-k8s";
        private string _sdkVersion = string.Empty;
        #endregion
    }
}
