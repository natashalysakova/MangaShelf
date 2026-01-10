//using AutoMapper;
//using Backend.Models;
//using Services.ViewModels;
//using Services.Profiles;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class SeriesMappingsTest : BasicTest<Series>
//    {
//        public SeriesMappingsTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(SeriesProfile)); }))
//        {
//        }

//        public override Series GetNewInstance()
//        {
//            return DataSet.Series;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            var model = new SeriesCreateModel()
//            {
//                Name = "Test",
//                Completed = true,
//                Ongoing = true,
//                TotalVolumes = 2,
//                Type = Backend.Models.Enums.Type.Manga
//            };

//            var entity = _mapper.Map<Series>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(0, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.Ongoing, entity.Ongoing);
//            Assert.AreEqual(model.Completed, entity.Completed);
//            Assert.AreEqual(model.TotalVolumes, entity.TotalVolumes);
//            Assert.AreEqual(0, entity.TotalIssues);
//            Assert.AreEqual(model.Type, entity.Type);

//        }

//        [TestMethod]
//        public override void TestUpdateModel()
//        {
//            var model = new SeriesUpdateModel()
//            {
//                Id = 42,
//                Name = "Test",
//                Color = "#FFFFFF",
//                Completed = true,
//                Ongoing = true,
//                PublisherId = DataSet.Publisher.Id,
//                TotalVolumes = 2,
//                Type = Backend.Models.Enums.Type.Manga,
//                OriginalName = "Test",
//                VolumeCount = 2,
//                TotalIssues = 1
//            };

//            var entity = _mapper.Map<Series>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(model.Id, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.Color, entity.Color);
//            Assert.AreEqual(model.Ongoing, entity.Ongoing);
//            Assert.AreEqual(model.Completed, entity.Completed);
//            Assert.AreEqual(model.PublisherId, entity.PublisherId);
//            Assert.AreEqual(model.TotalVolumes, entity.TotalVolumes);
//            Assert.AreEqual(model.TotalIssues, entity.TotalIssues);

//            Assert.AreEqual(model.Type, entity.Type);
//            Assert.AreEqual(model.OriginalName, entity.OriginalName);
//            Assert.IsFalse(model.HasError);

//        }

//        [TestMethod]
//        public override void TestViewModel()
//        {
//            var entity = DataSet.Series;
//            var viewModel = _mapper.Map<SeriesViewModel>(entity);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//            Assert.AreEqual(entity.Color, viewModel.Color);
//            Assert.AreEqual(entity.Ongoing, viewModel.Ongoing);
//            Assert.AreEqual(entity.Completed, viewModel.Completed);
//            Assert.AreEqual(entity.TotalVolumes, viewModel.TotalVolumes);
//            Assert.AreEqual(entity.Type, entity.Type);
//            Assert.AreEqual(entity.TotalIssues, viewModel.TotalIssues);

//            Assert.AreEqual(entity.OriginalName, viewModel.OriginalName);
//            Assert.AreEqual(entity.Volumes.Count, viewModel.VolumesCount);

//        }
//    }

//}