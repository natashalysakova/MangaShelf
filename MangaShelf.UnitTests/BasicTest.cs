//using AutoMapper;
//using Backend.Models;
//using Backend.Models.Enums;
//using Microsoft.AspNetCore.Mvc.Formatters;
//using Services.ViewModels;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public abstract class BasicTest<T> where T : class, IIdEntity, new()
//    {
//        protected IMapper _mapper;
//        MapperConfiguration _configuration;

//        public BasicTest(MapperConfiguration configuration)
//        {
//            _configuration = configuration;
//            _mapper = _configuration.CreateMapper();
//        }

//        [TestMethod]
//        public void ValidationTest()
//        {
//            _configuration.AssertConfigurationIsValid();
//        }

//        public void IdNameViewMapping()
//        {
//            var tmp = GetNewInstance();
//            var viewModel = _mapper.Map<IdNameView>(tmp);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(tmp.Id, viewModel.Id);
//            Assert.IsFalse(string.IsNullOrEmpty(viewModel.Name));
//        }

//        public abstract T GetNewInstance();
//        public abstract void TestViewModel();
//        public abstract void TestCreateModel();
//        public abstract void TestUpdateModel();



//    }

//    public static class DataSet
//    {
//        static DataSet()
//        {
//            Author.Volumes = new List<Volume>() { Volume1, Volume2 };
//            Volume1.Authors = new List<Author>() { Author };
//            Volume1.Series = Series;
//            Volume1.Issues = new List<Issue>() { Issue, BonusIssue };

//            Issue.Volume = Volume1;
//            BonusIssue.Volume = Volume1;                

//            Volume2.Authors = new List<Author>() { Author };
//            Volume2.Series = Series;

//            Series.Volumes = new List<Volume>() { Volume1, Volume2 };
//            Series.Publisher = Publisher;

//            Publisher.Series = new List<Series>() { Series };
//            Publisher.Country = Country;

//            Country.Publishers = new List<Publisher>() { Publisher };

//        }

//        public static Issue Issue = new Issue()
//        {
//            Id = 41,
//            Name = "Chapter 16",
//            Number = 16,
//            VolumeId = 542
//        };

//        public static Bonus BonusIssue = new Bonus()
//        {
//            Id = 51,
//            Name = "Bonus chapter 12",
//            Number = 12,
//            VolumeId = 542
//        };

//        public static Volume Volume1 = new Volume()
//        {
//            CoverUrl = "\\pathTo.jpg",
//            CreationDate = DateTime.Today.AddDays(-1),
//            Digitality = VolumeType.Digital,
//            Id = 542,
//            ModificationDate = DateTime.Today,
//            Number = 1,
//            OneShot = true,
//            PreorderDate = DateTime.Today.AddDays(-5),
//            PurchaseDate = DateTime.Today.AddDays(-2),
//            ReleaseDate = DateTime.Today.AddDays(-3),
//            PurchaseStatus = PurchaseStatus.Bought,
//            Rating = 4,
//            SeriesId = 442,
//            Status = Status.Completed,
//            Title = "Title Volume 1",

//        };
//        public static Volume Volume2 = new Volume()
//        {
//            CoverUrl = "\\pathTo2.jpg",
//            CreationDate = DateTime.Today.AddDays(-4),
//            Digitality = VolumeType.Digital,
//            Id = 542,
//            ModificationDate = DateTime.Today,
//            Number = 2,
//            OneShot = true,
//            PreorderDate = DateTime.Today.AddDays(-8),
//            PurchaseDate = DateTime.Today.AddDays(-5),
//            ReleaseDate = DateTime.Today.AddDays(-6),
//            PurchaseStatus = PurchaseStatus.Bought,
//            Rating = 2,
//            SeriesId = 442,
//            Status = Status.Dropped,
//            Title = "Title Volume 2",

//        };
//        public static Country Country = new Country()
//        {
//            CountryCode = "ua-UK",
//            FlagPNG = "\\pathTo.png",
//            FlagSVG = "\\pathTo.svg",
//            Id = 242,
//            Name = "Neverland"
//        };
//        public static Publisher Publisher = new Publisher
//        {
//            CountryId = 242,
//            Id = 342,
//            Name = "NashMalopus"
//        };
//        public static Series Series = new Series()
//        {
//            Id = 442,
//            Color = "#FF0000",
//            ComplimentColor = "#FFFFFF",
//            Completed = true,
//            Name = "Test Series",
//            Ongoing = true,
//            OriginalName = "Nakayama San wa ooishi desu",
//            PublisherId = 342,
//            TotalVolumes = 42,
//            TotalIssues = 12,
//            Type = Backend.Models.Enums.Type.Manga
//        };
//        public static Author Author = new Author()
//        {
//            Id = 142,
//            Name = "Test Auhtor",
//            Roles = Roles.Writer
//        };
//        public static Filter Filter = new Filter()
//        {
//            DisplayOrder = 42,
//            Id = 642,
//            Group = "standart",
//            Json = "{kek: lol}",
//            Name = "Test Filter"
//        };

//    }
//}