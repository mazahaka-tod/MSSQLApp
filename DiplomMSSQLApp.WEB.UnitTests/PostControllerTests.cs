using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Models;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class PostControllerTests {
        protected PostController GetNewPostController(IService<PostDTO> ps, IService<EmployeeDTO> es, IService<DepartmentDTO> ds) {
            return new PostController(es, ps, ds);
        }

        protected PostController GetNewPostControllerWithControllerContext(IService<PostDTO> ps, IService<EmployeeDTO> es, IService<DepartmentDTO> ds) {
            return new PostController(es, ps, ds) { ControllerContext = MockingServerMapPathMethod() };
        }

        protected ControllerContext MockingServerMapPathMethod() {
            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(m => m.MapPath(It.IsAny<string>())).Returns("./DiplomMSSQLApp.WEB/Results/Post/");
            Mock<HttpContextBase> httpCtxStub = new Mock<HttpContextBase>();
            httpCtxStub.Setup(m => m.Server).Returns(serverMock.Object);
            ControllerContext controllerCtx = new ControllerContext {
                HttpContext = httpCtxStub.Object
            };
            return controllerCtx;
        }

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public async Task Index_AsksForIndexView() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Index()) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }
        
        [Test]
        public async Task Index_RetrievesPostsPropertyFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<PostDTO>>(), It.IsAny<int>())).Returns(new PostDTO[] {
                new PostDTO {
                    Id = 2,
                    Title = "Programmer"
                }
            });
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Index()) as ViewResult;

            PostListViewModel model = result.ViewData.Model as PostListViewModel;
            Assert.AreEqual(1, model.Posts.Count());
            Assert.AreEqual(2, model.Posts.FirstOrDefault().Id);
            Assert.AreEqual("Programmer", model.Posts.FirstOrDefault().Title);
        }
        
        [Test]
        public async Task Index_RetrievesPageInfoPropertyFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Index()) as ViewResult;

            PostListViewModel model = result.ViewData.Model as PostListViewModel;
            Assert.AreEqual(9, model.PageInfo.TotalItems);
            Assert.AreEqual(3, model.PageInfo.PageSize);
            Assert.AreEqual(3, model.PageInfo.PageNumber);
            Assert.AreEqual(3, model.PageInfo.TotalPages);
        }
        
        /// <summary>
        /// // Create_Get method
        /// </summary>
        [Test]
        public async Task Create_Get_AsksForCreateView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(null, null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }
        
        /// <summary>
        /// // CreateAsync_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.CreateAsync(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }
        
        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.CreateAsync(new PostViewModel {
                Id = 2,
                Title = "Programmer"
            })) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        /// <summary>
        /// // EditAsync_Get method
        /// </summary>
        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_AsksForEditView() {
            Mock<PostService> mock = new Mock<PostService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }
        
        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new PostDTO {
                Id = _id.Value,
                Title = "Programmer"
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.EditAsync(2)) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }
        
        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }
        
        /// <summary>
        /// // EditAsync_Post method
        /// </summary>
        [Test]
        public async Task EditAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.EditAsync(new PostViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.EditAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.EditAsync(new PostViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }
        
        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.EditAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.EditAsync(new PostViewModel {
                Id = 2,
                Title = "Programmer"
            })) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        /// <summary>
        /// // DetailsAsync method
        /// </summary>
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_AsksForDetailsView() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }
        
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new PostDTO {
                Id = _id.Value,
                Title = "Programmer"
            });
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(2)) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }
        
        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }
        
        /// <summary>
        /// // DeleteAsync_Get method
        /// </summary>
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }
        
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new PostDTO {
                Id = _id.Value,
                Title = "Programmer"
            });
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(2)) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmedAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmedAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new Exception(""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteConfirmedAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new Exception(""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteConfirmedAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Нельзя удалить должность, пока в ней работает хотя бы один сотрудник.", model[0]);
        }

        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            PostController controller = GetNewPostController(null, null, null);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAllAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteAllAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.DeleteAllAsync()).Throws(new Exception(""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAllAsync()) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.DeleteAllAsync()).Throws(new Exception(""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAllAsync()) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Нельзя удалить должность, пока в ней работает хотя бы один сотрудник.", model[0]);
        }
        
        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public async Task ExportJsonAsync_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.ExportJsonAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
    }
}
