using Microsoft.ApplicationInsights.Kubernetes.ContainerIdProviders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.ApplicationInsights.Kubernetes
{
    public class CGroupContainerIdProviderTests
    {
        [Fact(DisplayName = "ParseContainerId should return correct result")]
        public void ParseContainerIdShouldWork()
        {
            CGroupContainerIdProvider target = new CGroupContainerIdProvider();
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
            Assert.Equal("b414a8fd62411213667643030d7ebf7264465df1b724fc6e7315106d0ed60553", target.ParseContainerId(testCase_1));

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
            Assert.Equal("4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a", target.ParseContainerId(testCase_2));

            const string testCase_3 = "2:cpu,cpuacct:\n1:name=systemd:/docker/4561e2e3ceb8377038c27ea5c40aa64a44c2dc02e53a141d20b7c98b2af59b1a";
            Assert.Null(target.ParseContainerId(testCase_3));
        }
    }
}
