//using AutoMapper;
//using Backend.Models;
//using Services.ViewModels;
//using Services.Profiles;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class FilterMappingTest : BasicTest<Filter>
//    {
//        public FilterMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(FiltersProfile)); }))
//        {
//        }

//        public override Filter GetNewInstance()
//        {
//            return DataSet.Filter;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            var model = new FilterCreateModel()
//            {
//                Name = "Test",
//                Group = "standart",
//                Json = "{test: json}"

//            };

//            var entity = _mapper.Map<Filter>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(0, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.Group, entity.Group);
//            Assert.AreEqual(0, entity.DisplayOrder);
//            Assert.AreEqual(model.Json, entity.Json);
//        }

//        [TestMethod]
//        public override void TestUpdateModel()
//        {
//            var model = new FilterUpdateModel()
//            {
//                Id = 1,
//                Name = "Test",
//                DisplayOrder = 42,
//            };

//            var entity = _mapper.Map<Filter>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(model.Id, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.DisplayOrder, entity.DisplayOrder);

//            Assert.AreNotEqual(string.Empty, entity.Group);
//            Assert.AreNotEqual(string.Empty, entity.Json);
//        }

//        [TestMethod]
//        public override void TestViewModel()
//        {
//            var entity = DataSet.Filter;
//            var viewModel = _mapper.Map<FilterViewModel>(entity);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//            Assert.AreEqual(entity.Group, viewModel.Group);
//            Assert.AreEqual(entity.DisplayOrder, viewModel.DisplayOrder);
//            Assert.AreEqual(entity.Json, viewModel.Json);
//        }
//    }
//}
