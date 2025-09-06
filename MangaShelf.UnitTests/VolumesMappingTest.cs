//using AutoMapper;
//using Backend.Models;
//using Backend.Models.Enums;
//using Services.ViewModels;
//using Services.Profiles;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class VolumesMappingTest : BasicTest<Volume>
//    {
//        public VolumesMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(VolumeProfile)); }))
//        {

//        }

//        public override Volume GetNewInstance()
//        {
//            return DataSet.Volume1;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            var timestamp = DateTime.Now;
//            var model = new VolumeCreateModel()
//            {
//                Authors = new[] { DataSet.Author.Name },
//                CoverUrl = "\\pathTo.jpg",
//                Digitality = VolumeType.Digital,
//                Number = 42,
//                PreorderDate = timestamp.AddDays(-2),
//                PurchaseDate = timestamp,
//                PurchaseStatus = PurchaseStatus.Bought,
//                Rating = 5,
//                ReleaseDate = timestamp.AddDays(-1),
//                SeriesName = DataSet.Series.Name,
//                VolumeType = VolumeItemType.OneShot,
//                Status = Status.Completed,
//                Title = "Test",
//            };

//            var entity = _mapper.Map<Volume>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(0, entity.Id);

//            Assert.AreEqual(model.CoverUrl, entity.CoverUrl);
//            Assert.AreEqual(model.Digitality, entity.Digitality);
//            Assert.AreEqual(model.Number, entity.Number);
//            Assert.AreEqual(model.PreorderDate, entity.PreorderDate);
//            Assert.AreEqual(model.PurchaseDate, entity.PurchaseDate);
//            Assert.AreEqual(model.PurchaseStatus, entity.PurchaseStatus);
//            Assert.AreEqual(model.Rating, entity.Rating);
//            Assert.AreEqual(model.ReleaseDate, entity.ReleaseDate);
//            //Assert.AreEqual(model.VolumeType, entity.OneShot);
//            Assert.AreEqual(model.Status, entity.Status);
//            Assert.AreEqual(model.Title, entity.Title);


//        }

//        [TestMethod]
//        public override void TestUpdateModel()
//        {
//            var timestamp = DateTime.Now;
//            var model = new VolumeUpdateModel()
//            {
//                CoverUrl = "\\pathTo.jpg",
//                PreorderDate = timestamp.AddDays(-2),
//                PurchaseDate = timestamp,
//                PurchaseStatus = PurchaseStatus.Bought,
//                Rating = 5,
//                ReleaseDate = timestamp.AddDays(-1),
//                Status = Status.Completed,
//                Id = 42
//            };

//            var entity = _mapper.Map<Volume>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(entity.Id, model.Id);
//            Assert.AreEqual(model.CoverUrl, entity.CoverUrl);
//            Assert.AreEqual(model.PreorderDate, entity.PreorderDate);
//            Assert.AreEqual(model.PurchaseDate, entity.PurchaseDate);
//            Assert.AreEqual(model.PurchaseStatus, entity.PurchaseStatus);
//            Assert.AreEqual(model.Rating, entity.Rating);
//            Assert.AreEqual(model.ReleaseDate, entity.ReleaseDate);
//            Assert.AreEqual(model.Status, entity.Status);
//        }

//        [TestMethod]
//        public override void TestViewModel()
//        {
//            var entity = DataSet.Volume1;

//            var model = _mapper.Map<VolumeViewModel>(entity);

//            Assert.IsNotNull(model);
//            Assert.AreEqual(entity.Id, model.Id);
//            Assert.AreEqual(entity.Number, model.Number);
//            Assert.AreEqual(entity.ReleaseDate, model.ReleaseDate);
//            Assert.AreEqual(entity.PurchaseDate, model.PurchaseDate);
//            Assert.AreEqual(entity.PreorderDate, model.PreorderDate);
//            Assert.AreEqual(entity.Status, model.Status);
//            Assert.AreEqual(entity.PurchaseStatus, model.PurchaseStatus);
//            Assert.AreEqual(entity.Series.Color, model.SeriesColor);
//            Assert.AreEqual(entity.CoverUrl, model.CoverUrl);
//            Assert.AreEqual(entity.Digitality, model.Digitality);
//            Assert.AreEqual(entity.OneShot, model.OneShot);
//            Assert.AreEqual(entity.Rating, model.Rating);
//            Assert.AreEqual(entity.Series.Name, model.SeriesName);
//            Assert.AreEqual(entity.Series.Ongoing, model.SeriesOngoing);
//            Assert.AreEqual(entity.Series.OriginalName, model.SeriesOriginalName);
//            Assert.AreEqual(entity.Series.Publisher.Name, model.SeriesPublisherName);
//            Assert.AreEqual(entity.Series.Publisher.Country.FlagPNG, model.SeriesPublisherCountryFlag);
//            Assert.AreEqual(entity.Series.TotalVolumes, model.SeriesTotalVolumes);

//            Assert.IsTrue(entity.Authors.Select(x => x.Name).SequenceEqual(model.Authors));
//            Assert.IsFalse(model.HasError);
//        }


//    }
//}