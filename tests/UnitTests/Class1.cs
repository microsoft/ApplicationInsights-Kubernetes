namespace UnitTests
{
    using Xunit;

    public class Class1
    {
        [Fact]
        public void PassingTest()
        {
            Assert.Equal(4, 4);
        }

        [Fact]
        public void FailedTest()
        {
            Assert.Equal(4, 5);
        }
    }
}
