using MangaShelf.BL.Interfaces;
using MangaShelf.BL.Parsers;
using Moq;

namespace MangaShelf.BL.Parsers
{
    [TestClass]
    public class ParserFactoryTest
    {
        [TestMethod]
        public void GetParsers_ReturnsAllParsers()
        {
            // Arrange
            var parser1 = new Mock<IPublisherParser>();
            var parser2 = new Mock<IPublisherParser>();
            var parsers = new List<IPublisherParser> { parser1.Object, parser2.Object };
            var factory = new ParserFactory(parsers);

            // Act
            var result = factory.GetParsers();

            // Assert
            CollectionAssert.AreEqual(parsers, result.ToList());
        }

        [TestMethod]
        public void GetParserForUrl_WhenParserCanParse_ReturnsParser()
        {
            // Arrange
            var url = "https://example.com";
            var parser1 = new Mock<IPublisherParser>();
            var parser2 = new Mock<IPublisherParser>();
            
            parser1.Setup(p => p.CanParse(url)).Returns(false);
            parser2.Setup(p => p.CanParse(url)).Returns(true);
            
            var parsers = new List<IPublisherParser> { parser1.Object, parser2.Object };
            var factory = new ParserFactory(parsers);

            // Act
            var result = factory.GetParserForUrl(url);

            // Assert
            Assert.AreEqual(parser2.Object, result);
        }

        [TestMethod]
        public void GetParserForUrl_WhenNoParserCanParse_ReturnsNull()
        {
            // Arrange
            var url = "https://example.com";
            var parser1 = new Mock<IPublisherParser>();
            var parser2 = new Mock<IPublisherParser>();
            
            parser1.Setup(p => p.CanParse(url)).Returns(false);
            parser2.Setup(p => p.CanParse(url)).Returns(false);
            
            var parsers = new List<IPublisherParser> { parser1.Object, parser2.Object };
            var factory = new ParserFactory(parsers);

            // Act
            var result = factory.GetParserForUrl(url);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetParserForUrl_WithEmptyParsersList_ReturnsNull()
        {
            // Arrange
            var url = "https://example.com";
            var parsers = new List<IPublisherParser>();
            var factory = new ParserFactory(parsers);

            // Act
            var result = factory.GetParserForUrl(url);

            // Assert
            Assert.IsNull(result);
        }
    }
}