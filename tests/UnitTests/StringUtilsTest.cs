using Microsoft.ApplicationInsights.Kubernetes;
using Xunit;
namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
{
    public class StringUtilsTest
    {
        [Theory(DisplayName = "Verify proper output of size")]
        [InlineData(25, "25B")]
        [InlineData(1023, "1023B")]
        [InlineData(1024, "1KB")]
        [InlineData(1025, "1KB")]
        [InlineData(2097152, "2MB")]
        [InlineData(3758096384, "3.5GB")]
        [InlineData(3848290697216, "3.5TB")]

        public void VerifyServerCertificateShouldVerifyIssuer(long input, string expected)
        {
            string actual = input.GetReadableSize();
            Assert.Equal(expected, actual);
        }
    }
}
