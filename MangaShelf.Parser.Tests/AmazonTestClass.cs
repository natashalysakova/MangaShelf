namespace MangaShelf.Parser.Tests;

//[TestClass]
//[Ignore]
//public class AmazonTestClass
//{
//    private AmazonParser _parser;

//    [TestInitialize]
//    public void Initialize()
//    {
//        var services = new ServiceCollection();
//        services.AddLogging(builder =>
//            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
//        services.AddKeyedSingleton<IHtmlDownloader, AdvancedHtmlDownloader>(HtmlDownloaderKeys.Advanced);
//        services.AddTransient<AmazonParser>();

//        _parser = services.BuildServiceProvider().GetRequiredService<AmazonParser>();
//    }

//    [TestMethod]
//    public async Task AmazonTest()
//    {

//        var result = await _parser.Parse("https://www.amazon.com/dp/B08FVLVXX6/");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("Volume 1", result.Title);
//        Assert.AreEqual("Solo Leveling", result.Series);
//        Assert.AreEqual("DUBU", result.Authors);
//        Assert.AreEqual(1, result.VolumeNumber);
//        Assert.AreEqual("https://m.media-amazon.com/images/I/81y9XvteVOL._SL1500_.jpg", result.Cover);
//        Assert.AreEqual(DateTime.Parse("2021-03-02"), result.Release);
//        Assert.AreEqual("Yen Press", result.Publisher);
//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);
//        Assert.AreEqual("B08FVLVXX6", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
//        Assert.AreEqual(null, result.OriginalSeriesName);
//    }

//    [TestMethod]
//    public async Task AmazonPreorderTest()
//    {
//        var result = await _parser.Parse("https://www.amazon.com/gp/product/B0D7Z8TGNQ");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("Volume 8", result.Title);
//        Assert.AreEqual("[Oshi no Ko]", result.Series);
//        Assert.AreEqual("Aka Akasaka,Mengo Yokoyari", result.Authors);
//        Assert.AreEqual(8, result.VolumeNumber);
//        Assert.AreEqual(string.Empty, result.Cover);
//        Assert.AreEqual(DateTime.Parse("2024-11-19"), result.Release);
//        Assert.AreEqual("Yen Press", result.Publisher);
//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);
//        Assert.AreEqual("B0D7Z8TGNQ", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
//        Assert.AreEqual(null, result.OriginalSeriesName);

//    }

//    [TestMethod]
//    public async Task AmazonOneShotTest()
//    {
//        var result = await _parser.Parse("https://www.amazon.com/dp/B01N0LT06V");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("Nijigahara Holograph", result.Title);
//        Assert.AreEqual("Nijigahara Holograph", result.Series);
//        Assert.AreEqual("Inio Asano", result.Authors);
//        Assert.AreEqual(-1, result.VolumeNumber);
//        Assert.AreEqual("https://m.media-amazon.com/images/I/91lxpgZLQOL._SL1500_.jpg", result.Cover);
//        Assert.AreEqual(DateTime.Parse("2014-03-19"), result.Release);
//        Assert.AreEqual("Fantagraphics", result.Publisher);
//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);
//        Assert.AreEqual("B01N0LT06V", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
//        Assert.AreEqual(null, result.OriginalSeriesName);
//    }
//}
