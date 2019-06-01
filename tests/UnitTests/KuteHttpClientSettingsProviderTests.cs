using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.ApplicationInsights.Kubernetes;
using Xunit;

namespace Microsoft.ApplicationInsights.Netcore.Kubernetes
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
            X509Certificate2 serverCert = new X509Certificate2(Convert.FromBase64String("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"));
            X509Certificate2 clientCert = new X509Certificate2(Convert.FromBase64String("MIIEyDCCArCgAwIBAgIRAIfX73wgV0Iuukv1CtrAn68wDQYJKoZIhvcNAQELBQAwDTELMAkGA1UEAxMCY2EwHhcNMTgwMzE3MTgzOTQ4WhcNMjAwMzE2MTgzOTQ4WjANMQswCQYDVQQDEwJjYTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBANFBpYTAsxhjoNs9SYbXpM0GQnhg/wDv75Ldwufn3O3b4Li0T8BwlD1B9yOJlLLo+zNcBgPbfDbpU3QYuhl5AS7RufpmMcXqiIZeerhm18IQOfJQsBYSw5M58WLOzOK+SLD7jFmuNeeBNMjuSihktM+VGOuuU4YP0VOgURVns3lAX2zJjPKKenN68nJ612JWapZwNB/ZKVzecIAnFNzEEj66XbHCpnPXt4JKiMJszqTsp6v69S2kPnJ87TxPbbLAiItX4AQ3McVDf1wzI5SQvsC5sfklHdbOCKgkWBYwjDmzaZ/KkA+9zHBJKBg8/a4WjKxVUKI0RQ6f2j/VCj8ewXjQx4MRyHO/lUmAC3cDALXXTgob8jo2p4wkOqd6r53bdfsoDfbA8AwIF3f8uT3X2OsBGeNSgTZau61jOpGa2EZ7yb47hs7R3NceNkcGHjrDMm0EMDia0fLYFx9/wxjX2W4qR4NWOHahYgZPLcQMwJ83DfQQUiYlObPH68+N6eZo+Lf2vPbyEuhzo7Ub//USXiZQnnnWHkcWEC58o26Hg+CPONT9rakdb/mmHOgy4iIDv9N7aIoFgNpBRG03aaXeYweG53NwcCei+ypL4hOzcDzerMaHU0rcowWN7rgKBkkjoM1REEM+tKFpqpRxbsyWcfzPTFTslpt5GrAD634+8IP5AgMBAAGjIzAhMA4GA1UdDwEB/wQEAwICpDAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4ICAQBu3FldWoItvDqTfA9VIWCQ63EoSgLN1ou6kk3DoDKe+sgqQGnFnmPcKtSqDLLpk4YSEAvpB1twU1i3zyeq1xjNTFsSoytr/clgZ7en3LOVBlP9tErQkzTYqvDnmyFnKHhPjNrjjHbZmobUzopP99rJFhS2+OSZobxbbPSyWGCqxlthWqRF9biY1Pm3bCCbSrwz/leXW2HfSR5ZuiMOMOcXn2LrScD/6OlVMfQZRBkzOLN+kouVtdpjhRXSMGoBH83AnSol01ZXTm1SsS/C9ttj0c5cXY6Lfd7KxwCM1K2JtYexkY82gEKl1jrFSknVfTF6rgoZbbtvDQrovSeHiQQ8iyVuaEKCfZD0OkAuzuFckxgIjZm8NswzQAGgYAavylXjD5BIBBdP/ufoSSlsWMJ2bRT6dpQIukcyKVbJ+DKWtV/44JWRBuWgz4FWZIijGnX6ic+zDTYwVGzmupj7T5iGijicR63HyDwt3NamZ1mktXBij6rZmiLs5vqfUNlPKikyqJF8WNu5mFJcKnPgoQALsHvjb03llQsq4m3o6N1+MkGnPFiMVec0ZuZuzwcJfPAO+OgQ6w8Ei+Y0afxg/OGhxjOndlzCmqxP6VeXyDTJCopCCZlyFlXpHeFiLZq6PynMepzPXm+Rp9MDTdlG9HjPWxihRH117MDhzK9NzYQDJQ=="));
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
