using System;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class KuteHttpClientSettingsProviderTests
    {
        [Fact(DisplayName = "ParseContainerId should return correct result")]
        public void ParseContainerIdShouldWork()
        {
            const string testCase_1 = "12:memory:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "11:freezer:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "10:devices:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "9:pids:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "8:hugetlb:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "7:net_cls,net_prio:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "6:cpu,cpuacct:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "5:perf_event:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "4:blkio:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "3:rdma:/\n" +
                "2:cpuset:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553\n" +
                "1:name=systemd:/kubepods/besteffort/pod3775c228-ceef-11e7-9bd3-0a58ac1f0867/b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553";
            Assert.Equal("b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553", KubeHttpClientSettingsProvider.ParseContainerId(testCase_1));

            const string testCase_2 = "12:rdma:/\n" +
                "11:pids:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "10:memory:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "9:freezer:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "8:perf_event:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "7:blkio:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "6:hugetlb:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "5:cpuset:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "4:cpu,cpuacct:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "3:net_cls,net_prio:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "2:devices:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a\n" +
                "1:name=systemd:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a";
            Assert.Equal("4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a", KubeHttpClientSettingsProvider.ParseContainerId(testCase_2));

            const string testCase_3 = "2:cpu,cpuacct:\n1:name=systemd:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a";
            Assert.Throws<InvalidCastException>(() => KubeHttpClientSettingsProvider.ParseContainerId(testCase_3));
        }

        [Fact(DisplayName = "Base address is formed by constructor")]
        public void BaseAddressShouldBeFormed()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.NotNull(target);
            Uri expected = new Uri("https://127.0.0.1:8001", UriKind.Absolute);
            Assert.Equal(expected.AbsoluteUri, target.ServiceBaseAddress.AbsoluteUri);
            Assert.Equal(expected.Port, target.ServiceBaseAddress.Port);
        }

        [Fact(DisplayName = "Base address is formed by constructor of windows kube settings provider")]
        public void BaseAddressShouldBeFormedWin()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpSettingsWinContainerProvider(
                serviceAccountFolder: ".",
                namespaceFileName: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Uri expected = new Uri("https://127.0.0.1:8001", UriKind.Absolute);
            Assert.Equal(expected.AbsoluteUri, target.ServiceBaseAddress.AbsoluteUri);
            Assert.Equal(expected.Port, target.ServiceBaseAddress.Port);
        }

        [Fact(DisplayName = "Container id is set to null for windows container settings")]
        public void ContainerIdIsAlwaysNullForWinSettings()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpSettingsWinContainerProvider(
                serviceAccountFolder: ".",
                namespaceFileName: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.Null(target.ContainerId);
        }

        [Fact(DisplayName = "Token can be fetched")]

        public void TokenShoudBeFetched()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                pathToToken: "token",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.Equal("Test-token", target.GetToken());
        }

        [Fact(DisplayName = "Token can be fetched by windows settings provider")]
        public void TokenShouldBeFetchedForWin()
        {
            IKubeHttpClientSettingsProvider target = new KubeHttpSettingsWinContainerProvider(
                serviceAccountFolder: ".",
                namespaceFileName: "namespace",
                tokenFileName:"token",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");

            Assert.Equal("Test-token", target.GetToken());
        }

        [Fact(DisplayName = "Return true when certificate chain is valid")]
        public void TrueWhenValidCertificate()
        {
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.NotNull(target);

            // The following certificates are valid in a chain.
            X509Certificate2 serverCert = new X509Certificate2(Convert.FromBase64String("MIIDhTCCAm2gAwIBAgIBTTANBgkqhkiG9w0BAQ0FADAAMB4XDTE4MDcyNzE5MDMwMFoXDTE5MDcyNzE5MDMwMFowajELMAkGA1UEBhMCVVMxCzAJBgNVBAgTAldBMRAwDgYDVQQHEwdSZWRtb25kMRIwEAYDVQQKEwlNaWNyb3NvZnQxDzANBgNVBAsTBkRldkRpdjEXMBUGA1UEAwwOKi5zYWFyc2FzZS5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDRyPPbX6H7RcFr6w3qHxoCe72XhkHhxJFjyth49LFINb0ZQtmZYjGEYkh88jZFFnNU7eHS3wSmL5nmmHVZDBdqWvP2Jwhla8NH7ZkMs1romf6QUG2ZWD/jF2v7gHefBrlz7ydJS8IPNbHHmSayofwjSiET3h+e5bl46n3tpDoFcy0EKfHNFuVScdjGMnFxOWfCCdC+DSPWuO26pinTkpeKMYaW2RAORT3nmnZQW+djFrAar7qPkjXPPHw8gYEcHexwaaiDxmi6G7xBiuW8bivQtONoM4Q190FK20WzqtZ2AxYJMGFcgZIpYFBaJzJ/p6Ykt1x5ToCZVWMaWABIPKYJAgMBAAGjgZ8wgZwwDAYDVR0TAQH/BAIwADAdBgNVHQ4EFgQUa83fBElj1T6wiDWEposkT9LdL7EwCwYDVR0PBAQDAgXgMC0GA1UdEQQmMCSCDiouc2FhcnNhc2UuY29tghIqLnNjbS5zYWFyc2FzZS5jb20wEQYJYIZIAYb4QgEBBAQDAgZAMB4GCWCGSAGG+EIBDQQRFg94Y2EgY2VydGlmaWNhdGUwDQYJKoZIhvcNAQENBQADggEBAKpj+RIIjrTsb5j7fCwXpT8ThtZUqzaBLBzBhOmHyMRClaRYTmaNUfCRWhCKe/J9U0fuF79D30egr5vv+VO3Y4PFQ2FrNiqhz8hrNpdEbFW5lHSjjYkCWF/UM4BppRrRiKyVcj5CBJuhTyi+0yl1laZMQ2VZAWYcCAofK7DE+5tUTAZBNcNEfCmlERmFr3WS2gkTPAbNUhNFhJ07oL7O8iBuild4N3t1l4DowRz1LP4epXXnrIbX6BMCNbsRnH2F5edPevTBIVgzRKrBFRhGg/meOoSOC2GUzALxEbVYX2GFUVWceq4dH94wNntNJ4gbSA77tzAtgLqLTwlkZLPHAeY="));
            X509Certificate2 clientCert = new X509Certificate2(Convert.FromBase64String("MIIC7TCCAdWgAwIBAgIBAzANBgkqhkiG9w0BAQ0FADAAMB4XDTE4MDcyNzE4NTQwMFoXDTI4MDcyNzE4NTQwMFowADCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBALT8CmzKuVxIJD6PkXS+MzFewvLc+R3LCZBoEX06oSziTpEO1jbnm14dGdNIFg4ZsmaLcM9MZsnl9etb44n11JXvFTBLR0vniF0e9bw+qfHHZ0tO92oW9SBlJCdwdZiSdg0zHfcxT8IoUDi+iE4OIBNgiwHwxkHnQsIMTN6MLtsbFtD8LKsfoDHbD4yU12jaicPdn6ifVJShuq6oI2pkFgzh1xUzNpGmcNzXTNci0qU2KGbpnzlY6ufbJROtNaNmADm5AMmoz6cYb2iYUpuf6M92xmWciTpcEDEf4cumkdPD3BVxIxWf9/klzf6vXpHeY5EJJxow5NmnTZ9hNY24HgkCAwEAAaNyMHAwDwYDVR0TAQH/BAUwAwEB/zAdBgNVHQ4EFgQUU4m02sIVMgcrCh1VmSPQxnio808wCwYDVR0PBAQDAgEGMBEGCWCGSAGG+EIBAQQEAwIABzAeBglghkgBhvhCAQ0EERYPeGNhIGNlcnRpZmljYXRlMA0GCSqGSIb3DQEBDQUAA4IBAQAalR3w8vVXt49uul7+fr+tnILP9hBeQt9lHd5g/JTw2KWCbQUqU5aQElZMf6uvhr/jvgEK2Ft/dDYM4lY2gs1L6LjFLbF/MXEJPGPbTlLU1UHvP7jCmGbgj45XtVHr+7kOO6t9XoQftn0RuY7jGf7E8kqJBRVMrv3lumfiWD3u3b+hAeLhBgyUMD+kjd0VVTvkmughEQPz7QXV6aYu86kNtofXpvz9dyhTmhjPlZanJUeJjLP3RaMAUpFbLyXkdHq5XYnK6anf5T7r/C+QIYq/AeKd4xQ2rJCJmtu37sdX5g1wN2lR2NOnJx6GTrv8lzN7FzIAIn9oSJGVrXKdD1Kk"));
            bool result = target.CertificateValidationCallBack(null, serverCert, clientCert, new X509Chain(), System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors);
            Assert.True(result);
        }

        [Fact(DisplayName = "Return false when certificate chain is invalid")]
        public void FalseWhenInvalidCertificate()
        {
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.NotNull(target);

            // The following certificates are not matched.
            X509Certificate2 serverCert = new X509Certificate2(Convert.FromBase64String("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"));
            X509Certificate2 clientCert = new X509Certificate2(Convert.FromBase64String("MIIFBTCCAu2gAwIBAgIJAOkgZEv/asZ7MA0GCSqGSIb3DQEBBQUAMBkxFzAVBgNVBAMMDmNhLmV4YW1wbGUuY29tMB4XDTEzMDIyMjIyNDQ0MloXDTE4MDIyMTIyNDQ0MlowGTEXMBUGA1UEAwwOY2EuZXhhbXBsZS5jb20wggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQDwaJhWm9FPsrwarEi70M0nB3kSiM/bOtRWDIH1fW8t0eLy7k6Ji7nuG2Z8tqspVKraEV09GVPiZYY8QzqMsntn937TLfqm21+sZ7bQT0JQAF+IKVQM2H2PCpvzakufktBvWgqBAzKOVHYEFrrEbqRqcfvM6RXBRKG9UkBv6cz/uYNsMBApH5EfIRYY8Cpg7R4ZqsifjbpfhC/vHRUzrs6STDW39YReHiU3/oTTIv7R1hTRfh8grEztWhknoG/4OMDVIhnjXFIwokHj5rEV3fuLLFDMiZTwiVr2GeV8/yK0uipgtUaSkDdCc2VMY7idOYT1+GBobc01S4wIfHMxwzEIGUhjOKyTwgwdTdCj3H4TqAZFXCViek9RK3wUsAUusp4ltn+jbiFr/FZnJWFMfVSmjInBziXjsQ+ZtCyxwVwE5vS9AVeeB6yTLznTW3DylMV45Rx4roFQETa3sx60rhiMl9CBqV99gQKOw+05oo0oEyZwsA8vLf1+orIDGVnqTN86VSI3n3lq1JBSB177doBSeeXX7xCRJ4nwfXKwphNnpwWU0tr+L5oEbiyXlbGTf8frbA9Rcwz+ZU2hcroAL9vTNguBO3Hb5kbVkMPCEJJ0mJPOdCVM2sxrBuH6quCm/PdOKESsDeQKw3FlBs1Xlokmlf5PyhS5T1oLtwnKrjpZFQIDAQABo1AwTjAdBgNVHQ4EFgQUy5axd6XcUkPK1gMDM+mG/B+uTEYwHwYDVR0jBBgwFoAUy5axd6XcUkPK1gMDM+mG/B+uTEYwDAYDVR0TBAUwAwEB/zANBgkqhkiG9w0BAQUFAAOCAgEAHQLK7Zns9KJg3vikfp9OoNTwROnW7pCNUZHMwDDoO3pI9TWrAtDDB2o+ReBLlVoXh/kX3ragE+dra7jvp5sDR4Bbylf1exy0AQT0wHXhqN547J5Xg/Xr/bWrNUPquIX1DNLjcW4ALHBAZwp8SAC2SDLV70f+kSnzLTZHwKLWD/3JiUAotEgz7KomW9jA6kXhq5uvPIQ/d9JQ+BaXlvA0BM95DBhwYEjTaizk6PoslPeouXM7EScl2uGPaPhaSTVZwgfwBfIfsTadKVgseF9BVt/pjHOyKzgkdzRcSvv7QZFGUcsm6XKj+MH5JuLruuLBw+ldFL1q+7mkN/tol550pX6vABaIjZDZgHv5NAqZ/6l2ye6HNDsN+DkLAncLENzh/YfUtW5F+suB0114wanwUTzcEhkyU27eubNiJkc2IOhqwPq6lBrBPrpyVYcWVCc0tR4xhwm7Sh+VZsaW7FUWcQmgVo+ugly+z8x8e3zEe2MXWpKKktdTrFjnj1ey3aRxYZ9rwHDa7CFbATgp3mMYELNGDKUOV9vMrRTshxlZ+fzu9ypq8XAyxP/fwunAyWtQpRJsV2j4UKqO86+QcQqjQAye1n/6oo7RbH9UdNULaGwtG0p5xOmJub3qy4gaM7Xl/etf5MjsNKGgAt3gHnSWC9Zqgx4sP61XK3T/4JJaXVs="));
            bool result = target.CertificateValidationCallBack(null, serverCert, clientCert, new X509Chain(), System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors);
            Assert.False(result);
        }

        [Fact(DisplayName = "Return false when certificate out of date")]
        public void FalseWhenOutOfDateCertificate()
        {
            KubeHttpClientSettingsProvider target = new KubeHttpClientSettingsProvider(
                pathToCGroup: "TestCGroup",
                pathToNamespace: "namespace",
                kubernetesServiceHost: "127.0.0.1",
                kubernetesServicePort: "8001");
            Assert.NotNull(target);

            // The following certificates are expired.
            X509Certificate2 serverCert = new X509Certificate2(Convert.FromBase64String("MIIDdDCCAt2gAwIBAgIBADANBgkqhkiG9w0BAQUFADCBmzELMAkGA1UEBhMCSlAxDjAMBgNVBAgTBVRva3lvMRAwDgYDVQQHEwdDaHVvLWt1MREwDwYDVQQKEwhGcmFuazRERDEYMBYGA1UECxMPV2ViQ2VydCBTdXBwb3J0MRgwFgYDVQQDEw9GcmFuazRERCBXZWIgQ0ExIzAhBgkqhkiG9w0BCQEWFHN1cHBvcnRAZnJhbms0ZGQuY29tMCIYDzE3NjkwODE1MTU0NjQxWhgPMTgyMTA1MDUxNjUzMjFaMIHbMQswCQYDVQQGEwJGUjEXMBUGA1UECBQOw45sZS1kZS1GcmFuY2UxDjAMBgNVBAcTBVBhcmlzMRwwGgYDVQQKExNIZXJlZGl0YXJ5IE1vbmFyY2h5MRYwFAYDVQQLEw1IZWFkIG9mIFN0YXRlMSkwJwYJKoZIhvcNAQkBFhpuYXBwaUBncmVhdGZyZW5jaGVtcGlyZS5mcjEbMBkGA1UEAxMSRW1wZXJvciBOYXBvbGVvbiBJMRIwEAYDVQQEEwlCb25hcGFydGUxETAPBgNVBCoTCE5hcG9sZW9uMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvdcwMpsM3EgNmeO/fvcuZsirnBZCh0yJlySGmev792ohOBvfC+v27ZKcXN+H7V+xSxEDDAFq9SCEk4MZ8h/ggtw63T6SEYeNGtnfyLv8atienun/6ocDp0+26xvj8NxmaKL4MQM9j9aYgt2EOxUTH5kBc7mc621q2RJi0q/y0/SdX2Pp/3MKDirOs81vfc2icEaYAisd5IOF9vpMpLr3b3Qg9T66/4hQS6DgIOkfUurWqe33sA2RRv7ql1gcxL1ImBxBtYQGsujn8fCNRK5jtMtICkEi9tks/tYzSaqgby3QGfbA18xl7FLLjnZDLVX3ZVchhveR78/f7U/xh8C2WQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBAFmEiU2vn10fXvL+nJdRHJsf0P+f6v8H+vbkomog4gVbagDuFACJfAdKJhnc/gzkCF1fyeowOD68k4e0H1vyLuk23BUmjW41nOjdg8LrTAS8fMwkj5FVSKR2mHciHWgY/BU4UypYJtcgajH1bsqwUI50wfbggW4VzLD842q5LhnW"));
            X509Certificate2 clientCert = new X509Certificate2(Convert.FromBase64String("MIIDdDCCAt2gAwIBAgIBADANBgkqhkiG9w0BAQUFADCBmzELMAkGA1UEBhMCSlAxDjAMBgNVBAgTBVRva3lvMRAwDgYDVQQHEwdDaHVvLWt1MREwDwYDVQQKEwhGcmFuazRERDEYMBYGA1UECxMPV2ViQ2VydCBTdXBwb3J0MRgwFgYDVQQDEw9GcmFuazRERCBXZWIgQ0ExIzAhBgkqhkiG9w0BCQEWFHN1cHBvcnRAZnJhbms0ZGQuY29tMCIYDzE3NjkwODE1MTU0NjQxWhgPMTgyMTA1MDUxNjUzMjFaMIHbMQswCQYDVQQGEwJGUjEXMBUGA1UECBQOw45sZS1kZS1GcmFuY2UxDjAMBgNVBAcTBVBhcmlzMRwwGgYDVQQKExNIZXJlZGl0YXJ5IE1vbmFyY2h5MRYwFAYDVQQLEw1IZWFkIG9mIFN0YXRlMSkwJwYJKoZIhvcNAQkBFhpuYXBwaUBncmVhdGZyZW5jaGVtcGlyZS5mcjEbMBkGA1UEAxMSRW1wZXJvciBOYXBvbGVvbiBJMRIwEAYDVQQEEwlCb25hcGFydGUxETAPBgNVBCoTCE5hcG9sZW9uMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvdcwMpsM3EgNmeO/fvcuZsirnBZCh0yJlySGmev792ohOBvfC+v27ZKcXN+H7V+xSxEDDAFq9SCEk4MZ8h/ggtw63T6SEYeNGtnfyLv8atienun/6ocDp0+26xvj8NxmaKL4MQM9j9aYgt2EOxUTH5kBc7mc621q2RJi0q/y0/SdX2Pp/3MKDirOs81vfc2icEaYAisd5IOF9vpMpLr3b3Qg9T66/4hQS6DgIOkfUurWqe33sA2RRv7ql1gcxL1ImBxBtYQGsujn8fCNRK5jtMtICkEi9tks/tYzSaqgby3QGfbA18xl7FLLjnZDLVX3ZVchhveR78/f7U/xh8C2WQIDAQABMA0GCSqGSIb3DQEBBQUAA4GBAFmEiU2vn10fXvL+nJdRHJsf0P+f6v8H+vbkomog4gVbagDuFACJfAdKJhnc/gzkCF1fyeowOD68k4e0H1vyLuk23BUmjW41nOjdg8LrTAS8fMwkj5FVSKR2mHciHWgY/BU4UypYJtcgajH1bsqwUI50wfbggW4VzLD842q5LhnW"));
            bool result = target.CertificateValidationCallBack(null, serverCert, clientCert, new X509Chain(), System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors);
            Assert.False(result);
        }
    }
}
