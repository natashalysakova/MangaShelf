//using AutoMapper;
//using Backend.Models;
//using Services.ViewModels;
//using Services.Profiles;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class PublisherMappingTest : BasicTest<Publisher>
//    {
//        public PublisherMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(PublisherProfile)); }))
//        {

//        }

//        public override Publisher GetNewInstance()
//        {
//            return DataSet.Publisher;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            var model = new PublisherCreateModel()
//            {
//                Name = "Test",
//                CountryId = 42
//            };

//            var entity = _mapper.Map<Publisher>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(0, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.CountryId, entity.CountryId);
//        }

//        [TestMethod]
//        public override void TestUpdateModel()
//        {
//            var model = new PublisherUpdateModel()
//            {
//                Id = 1,
//                Name = "Test",
//                CountryId = 42
//            };

//            var entity = _mapper.Map<Publisher>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(model.Id, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.CountryId, entity.CountryId);
//        }

//        [TestMethod]
//        public override void TestViewModel()
//        {
//            var entity = DataSet.Publisher;
//            var viewModel = _mapper.Map<PublisherViewModel>(entity);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//            Assert.AreEqual(entity.CountryId, viewModel.CountryId);
//            Assert.AreEqual(entity.Series.Count, viewModel.SeriesCount);
//            Assert.AreEqual(entity.Country.Name, viewModel.CountryName);
//            Assert.AreEqual(entity.Country.FlagPNG, viewModel.CountryFlagPNG);
//            Assert.AreEqual(entity.Series.Count(), viewModel.Series.Count());
//        }
//    }

//    [TestClass]
//    public class IssueMappingTest : BasicTest<Issue>
//    {
//        public IssueMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(IssueProfile)); }))
//        {
//        }

//        public override Issue GetNewInstance()
//        {
//            return DataSet.Issue;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            throw new NotImplementedException();
//        }
//        [TestMethod]

//        public override void TestUpdateModel()
//        {
//            throw new NotImplementedException();
//        }
//        [TestMethod]

//        public override void TestViewModel()
//        {
//            var entity = DataSet.Issue;
//            var viewModel = _mapper.Map<IssueViewModel>(entity);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//            Assert.AreEqual(entity.GetType().Name, viewModel.Type);
//            Assert.AreEqual(entity.Number, viewModel.Number);
//        }
//    }

//    [TestClass]
//    public class BonusMappingTest : BasicTest<Bonus>
//    {
//        public BonusMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(IssueProfile)); }))
//        {
//        }

//        public override Bonus GetNewInstance()
//        {
//            return DataSet.BonusIssue;
//        }
//        [TestMethod]

//        public override void TestCreateModel()
//        {
//            throw new NotImplementedException();
//        }
//        [TestMethod]

//        public override void TestUpdateModel()
//        {
//            throw new NotImplementedException();
//        }
//        [TestMethod]

//        public override void TestViewModel()
//        {
//            var entity = DataSet.BonusIssue;
//            var viewModel = _mapper.Map<IssueViewModel>(entity);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//            Assert.AreEqual(entity.GetType().Name, viewModel.Type);
//            Assert.AreEqual(entity.Number, viewModel.Number);
//        }
//    }
//}