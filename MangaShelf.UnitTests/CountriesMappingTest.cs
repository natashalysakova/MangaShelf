//using AutoMapper;
//using Backend.Models;
//using Services.Profiles;
//using Services.ViewModels;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class CountriesMappingTest : BasicTest<Country>
//    {
//        public CountriesMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(CountryProfile)); }))
//        {
//        }

//        public override Country GetNewInstance()
//        {
//            return DataSet.Country;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            var model = new CountryCreateModel()
//            {
//                Name = "Test",
//                FlagPNG = "\\pathTo.png",
//                FlagSVG = "\\pathTo.svg",
//                CountryCode = "ua-UK"
//            };

//            var entity = _mapper.Map<Country>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(0, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.FlagSVG, entity.FlagSVG);
//            Assert.AreEqual(model.FlagPNG, entity.FlagPNG);
//            Assert.AreEqual(model.CountryCode, entity.CountryCode);

//        }

//        [TestMethod]
//        public override void TestUpdateModel()
//        {
//            var model = new CountryUpdateModel()
//            {
//                Id = 1,
//                Name = "Test",
//                FlagPNG = "\\pathTo.png",
//                FlagSVG = "\\pathTo.svg",
//                CountryCode = "ua-UK"
//            };

//            var entity = _mapper.Map<Country>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(model.Id, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.FlagSVG, entity.FlagSVG);
//            Assert.AreEqual(model.FlagPNG, entity.FlagPNG);
//            Assert.AreEqual(model.CountryCode, entity.CountryCode);
//        }

//        [TestMethod]
//        public override void TestViewModel()
//        {
//            var entity = DataSet.Country;
//            var viewModel = _mapper.Map<CountryViewModel>(entity);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//            Assert.AreEqual(entity.FlagPNG, viewModel.FlagPNG);
//            Assert.AreEqual(entity.FlagSVG, viewModel.FlagSVG);

//            Assert.AreEqual(1, viewModel.Publishers.Count());
//            Assert.AreEqual(entity.Publishers.ElementAt(0).Name, viewModel.Publishers.ElementAt(0).Name);

//        }
//    }
//}
