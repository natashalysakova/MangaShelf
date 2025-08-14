namespace MangaShelfTests
{
    [TestClass]
    public sealed class Test1
    {
        [TestInitialize]
        public void TestInit()
        {
            // This method is called before each test method.
        }

        [TestMethod]
        public void AlwaysPass()
        {
            Assert.IsTrue(true, "This test should always pass.");
        }

        [TestMethod]
        [ExpectedException(typeof(AssertFailedException))]
        public void AlwaysFail()
        {
            Assert.IsFalse(true, "This test should always fail.");
        }
    }
}
