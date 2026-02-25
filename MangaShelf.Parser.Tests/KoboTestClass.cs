namespace MangaShelf.Parser.Tests;

//[TestClass]
//[Ignore]
//public class KoboTestClass
//{
//    private KoboParser _parser = null!;

//    [TestInitialize]
//    public void Initialize()
//    {
//        var services = new ServiceCollection();
//        services.AddLogging(builder =>
//            builder.AddConsole().SetMinimumLevel(LogLevel.Debug));
//        services.AddKeyedSingleton<IHtmlDownloader, BasicHtmlDownloader>(HtmlDownloaderKeys.Basic);
//        services.AddTransient<KoboParser>();

//        _parser = services.BuildServiceProvider().GetRequiredService<KoboParser>();
//    }

//    [TestMethod]
//    public async Task KoboTest()
//    {

//        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/pretty-guardian-sailor-moon-eternal-edition-9");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("Volume 9", result.Title);
//        Assert.AreEqual("Pretty Guardian Sailor Moon Eternal Edition", result.Series);
//        Assert.AreEqual("Naoko Takeuchi", result.Authors);
//        Assert.AreEqual(9, result.VolumeNumber);
//        Assert.AreEqual("https://cdn.kobo.com/book-images/b0ac3524-461c-4af5-bb05-d0f799ad87a1/353/569/90/False/pretty-guardian-sailor-moon-eternal-edition-9.jpg", result.Cover);
//        Assert.AreEqual(DateTime.Parse("2020-11-17"), result.Release);
//        Assert.AreEqual("Kodansha Comics", result.Publisher);
//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);
//        Assert.AreEqual("9781646595792", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
//        Assert.AreEqual(null, result.OriginalSeriesName);

//    }

//    [TestMethod]
//    public async Task KoboTest2()
//    {
//        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/spy-x-family-family-portrait");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("Spy x Family: Family Portrait", result.Title);
//        Assert.AreEqual("Spy x Family Novels", result.Series);
//        Assert.AreEqual("Aya Yajima", result.Authors);
//        Assert.AreEqual(-1, result.VolumeNumber);
//        Assert.AreEqual("https://cdn.kobo.com/book-images/a28d0839-b608-49ab-9f79-e100fef90c0f/353/569/90/False/spy-x-family-family-portrait.jpg", result.Cover);
//        Assert.AreEqual(DateTime.Parse("2023-12-26"), result.Release);

//        Assert.AreEqual("VIZ Media", result.Publisher);
//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);

//        Assert.AreEqual("9781974742691", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
//        Assert.AreEqual(null, result.OriginalSeriesName);


//    }

//    [TestMethod]
//    public async Task KoboPreorderTest()
//    {
//        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/spy-x-family-vol-13");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("Volume 13", result.Title);
//        Assert.AreEqual("Spy x Family", result.Series);
//        Assert.AreEqual("Tatsuya Endo", result.Authors);
//        Assert.AreEqual(13, result.VolumeNumber);
//        Assert.AreEqual("https://cdn.kobo.com/book-images/Images/00000000-0000-0000-0000-000000000000/353/569/90/False/empty_book_cover.jpg", result.Cover);
//        Assert.AreEqual(DateTime.Parse("2025-01-14"), result.Release);
//        Assert.AreEqual("VIZ Media", result.Publisher);
//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);

//        Assert.AreEqual("9781974753246", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);
//        Assert.AreEqual(null, result.OriginalSeriesName);

//    }

//    [TestMethod]
//    public async Task KoboOneShotTest()
//    {
//        var result = await _parser.Parse("https://www.kobo.com/ww/en/ebook/a-girl-on-the-shore");

//        Assert.IsNotNull(result);
//        Assert.AreEqual("A Girl on the Shore", result.Title);
//        Assert.AreEqual("A Girl on the Shore", result.Series);
//        Assert.AreEqual("Inio Asano", result.Authors);
//        Assert.AreEqual(-1, result.VolumeNumber);
//        Assert.AreEqual("https://cdn.kobo.com/book-images/be122b39-d133-4162-8b54-bcca043d07bb/353/569/90/False/a-girl-on-the-shore.jpg", result.Cover);
//        Assert.AreEqual(DateTime.Parse("2016-01-19"), result.Release);
//        Assert.AreEqual("Kodansha USA", result.Publisher);
//        Assert.AreEqual("9781942993766", result.Isbn);
//        Assert.AreEqual(-1, result.TotalVolumes);
//        Assert.AreEqual(SeriesStatus.Unknown, result.SeriesStatus);

//        Assert.AreEqual(VolumeType.Digital, result.VolumeType);

//        Assert.AreEqual(null, result.OriginalSeriesName);

//    }
//}
