using MangaShelf.Api.Controllers;
using Microsoft.Extensions.Logging;

namespace MangaShelf.Api.Tests
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

        [TestMethod]
        public void XheckTheWeatther()
        {
            var looger = new LoggerFactory().CreateLogger<WeatherForecastController>();
            WeatherForecastController controller = new WeatherForecastController(looger);
            var result = controller.Get();

            Assert.IsNotNull(result, "The result should not be null.");
        }
    }
}
