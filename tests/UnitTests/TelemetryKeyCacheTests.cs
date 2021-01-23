using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class TelemetryKeyCacheTests
    {
        [Theory]
        [InlineData("abc.def", "abc_def")]
        [InlineData("aaa.bbb.ccc.ddd", "aaa_bbb_ccc_ddd")]
        public void ShouldAlterKeyNameWhenSet(string input, string expected)
        {
            AppInsightsForKubernetesOptions opts = new AppInsightsForKubernetesOptions();
            opts.TelemetryKeyProcessor = (key) => key.Replace('.', '_');

            IOptions<AppInsightsForKubernetesOptions> options = Options.Create<AppInsightsForKubernetesOptions>(opts);
            TelemetryKeyCache keyCache = new TelemetryKeyCache(options);

            string actual = keyCache.GetProcessedKey(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AllowsMoreThanJustReplaceDot()
        {
            string input = "abc.def";
            string expected = "\"abc.def\"";

            AppInsightsForKubernetesOptions opts = new AppInsightsForKubernetesOptions();
            opts.TelemetryKeyProcessor = (key) => $@"""{key}"""; // Add quotes

            IOptions<AppInsightsForKubernetesOptions> options = Options.Create<AppInsightsForKubernetesOptions>(opts);
            TelemetryKeyCache keyCache = new TelemetryKeyCache(options);

            string actual = keyCache.GetProcessedKey(input);

            Assert.Equal(expected, actual);
        }

        
        [Fact]
        public void AllowsRemoveDot()
        {
            string input = "abc.def";
            string expected = "abcdef";

            AppInsightsForKubernetesOptions opts = new AppInsightsForKubernetesOptions();
            opts.TelemetryKeyProcessor = (key) => key.Replace(".", string.Empty); // Remove dot(.)

            IOptions<AppInsightsForKubernetesOptions> options = Options.Create<AppInsightsForKubernetesOptions>(opts);
            TelemetryKeyCache keyCache = new TelemetryKeyCache(options);

            string actual = keyCache.GetProcessedKey(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ReturnsOriginalWhenProcessorNotSet()
        {
            string input = "abc.def";
            string expected = input;

            AppInsightsForKubernetesOptions opts = new AppInsightsForKubernetesOptions();
            opts.TelemetryKeyProcessor = null;  // Making sure it is null.

            IOptions<AppInsightsForKubernetesOptions> options = Options.Create<AppInsightsForKubernetesOptions>(opts);
            TelemetryKeyCache keyCache = new TelemetryKeyCache(options);

            string actual = keyCache.GetProcessedKey(input);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ThereIsACapForCachedKeys()
        {
            AppInsightsForKubernetesOptions opts = new AppInsightsForKubernetesOptions();
            opts.TelemetryKeyProcessor = (key) => key;

            IOptions<AppInsightsForKubernetesOptions> options = Options.Create<AppInsightsForKubernetesOptions>(opts);
            TelemetryKeyCache keyCache = new TelemetryKeyCache(options);

            for (int i = 1; i <= TelemetryKeyCache.CacheCapacity; i++)
            {
                keyCache.GetProcessedKey($"Key{i}");
            }

            // No exception if the key has already been there.
            keyCache.GetProcessedKey("Key20");

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            {
                keyCache.GetProcessedKey("Key41");
            });
            Assert.Equal("Transformed key count is larger than the capacity of 40. This is not allowed. Please verify the TelemetryKeyProcessor option.", exception.Message);
        }
    }
}