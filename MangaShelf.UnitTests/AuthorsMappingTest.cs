//using AutoMapper;
//using Backend.Models;
//using Backend.Models.Enums;
//using Services.ViewModels;
//using Services.Profiles;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class AuthorsMappingTest : BasicTest<Author>
//    {
//        public AuthorsMappingTest() : base(new MapperConfiguration(c => { c.AddMaps(typeof(AuthorsProfile)); }))
//        {
//        }

//        public override Author GetNewInstance()
//        {
//            return DataSet.Author;
//        }

//        [TestMethod]
//        public override void TestCreateModel()
//        {
//            var model = new AuthorCreateModel()
//            {
//                Name = "Test",
//                Roles = Roles.Writer
//            };

//            var entity = _mapper.Map<Author>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(0, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.Roles, entity.Roles);
//        }

//        [TestMethod]
//        public override void TestUpdateModel()
//        {
//            var model = new AuthorUpdateModel()
//            {
//                Id = 1,
//                Name = "Test",
//                Roles = Roles.Writer
//            };

//            var entity = _mapper.Map<Author>(model);

//            Assert.IsNotNull(entity);
//            Assert.AreEqual(model.Id, entity.Id);
//            Assert.AreEqual(model.Name, entity.Name);
//            Assert.AreEqual(model.Roles, entity.Roles);
//            Assert.IsFalse(model.HasError);
//        }

//        [TestMethod]
//        public override void TestViewModel()
//        {
//            var entity = DataSet.Author;
//            var viewModel = _mapper.Map<AuthorViewModel>(entity);

//            Assert.IsNotNull(viewModel);
//            Assert.AreEqual(entity.Id, viewModel.Id);
//            Assert.AreEqual(entity.Name, viewModel.Name);
//        }
//    }
//}