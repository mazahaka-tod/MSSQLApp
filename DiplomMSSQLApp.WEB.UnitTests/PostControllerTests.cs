using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Models;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class PostControllerTests {
        protected PostController GetNewPostController(IService<PostDTO> ps, IService<EmployeeDTO> es, IService<DepartmentDTO> ds) {
            return new PostController(es, ps, ds);
        }

        protected PostController GetNewPostControllerWithControllerContext(IService<PostDTO> ps, IService<EmployeeDTO> es, IService<DepartmentDTO> ds, bool isXRequestedWith = false) {
            return new PostController(es, ps, ds) { ControllerContext = MockingControllerContext(isXRequestedWith) };
        }

        protected ControllerContext MockingControllerContext(bool isXRequestedWith) {
            // mocking Server.MapPath method
            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(m => m.MapPath(It.IsAny<string>())).Returns("./DiplomMSSQLApp.WEB/Results/");
            // mocking Request.Headers["X-Requested-With"]
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            if (isXRequestedWith)
                requestMock.SetupGet(x => x.Headers).Returns(new WebHeaderCollection {
                    {"X-Requested-With", "XMLHttpRequest"}
                });
            else
                requestMock.SetupGet(x => x.Headers).Returns(new WebHeaderCollection());
            // mocking HttpContext
            Mock<HttpContextBase> httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.SetupGet(m => m.Server).Returns(serverMock.Object);
            httpContextMock.SetupGet(m => m.Request).Returns(requestMock.Object);
            // mocking ControllerContext
            ControllerContext controllerContextMock = new ControllerContext {
                HttpContext = httpContextMock.Object
            };
            return controllerContextMock;
        }

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public void Index_SyncRequest_AsksForIndexView() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }
        
        [Test]
        public void Index_SyncRequest_RetrievesPostsPropertyFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<PostDTO>>(), It.IsAny<int>())).Returns(new PostDTO[] {
                new PostDTO {
                    Id = 2,
                    Title = "Programmer",
                    Department = new DepartmentDTO { Code = 123 },
                    NumberOfUnits = 1,
                    Salary = 100000,
                    Premium = 20000
                }
            });
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            PostListViewModel model = result.ViewData.Model as PostListViewModel;
            Assert.AreEqual(1, model.Posts.Count());
            Assert.AreEqual(2, model.Posts.FirstOrDefault().Id);
            Assert.AreEqual("Programmer", model.Posts.FirstOrDefault().Title);
        }
        
        [Test]
        public void Index_SyncRequest_RetrievesPageInfoPropertyFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            PostListViewModel model = result.ViewData.Model as PostListViewModel;
            Assert.AreEqual(9, model.PageInfo.TotalItems);
            Assert.AreEqual(3, model.PageInfo.PageSize);
            Assert.AreEqual(3, model.PageInfo.PageNumber);
            Assert.AreEqual(3, model.PageInfo.TotalPages);
        }

        [Test]
        public void Index_AsyncRequest_RetrievesPostsPropertyFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<PostDTO>>(), It.IsAny<int>())).Returns(new PostDTO[] {
                new PostDTO {
                    Id = 1,
                    Title = "Programmer",
                    Department = new DepartmentDTO { Code = 123 },
                    NumberOfUnits = 1,
                    Salary = 100000,
                    Premium = 20000
                }
            });
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;
            object post = (result.Data.GetType().GetProperty("Posts").GetValue(result.Data) as object[])[0];
            int id = (int)post.GetType().GetProperty("Id").GetValue(post);
            string title = post.GetType().GetProperty("Title").GetValue(post).ToString();

            Assert.AreEqual(1, id);
            Assert.AreEqual("Programmer", title);
        }

        [Test]
        public void Index_AsyncRequest_JsonRequestBehaviorEqualsAllowGet() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;

            Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
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
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.Create(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        [Test]
        public async Task Create_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }
        
        [Test]
        public async Task Create_Post_ModelStateIsNotValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Create(new PostViewModel {
                Id = 2,
                Title = "Programmer"
            })) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_ModelStateIsValid_AsksForEditView() {
            Mock<PostService> mock = new Mock<PostService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }
        
        [Test]
        public async Task Edit_Get_ModelStateIsValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new PostDTO {
                Id = _id.Value,
                Title = "Programmer"
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Edit(2)) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        [Test]
        public async Task Edit_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }
        
        [Test]
        public async Task Edit_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }
        
        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.Edit(new PostViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.EditAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Edit(new PostViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }
        
        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.EditAsync(It.IsAny<PostDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { new DepartmentDTO { } });
            PostController controller = GetNewPostController(mock.Object, null, dmock.Object);

            ViewResult result = (await controller.Edit(new PostViewModel {
                Id = 2,
                Title = "Programmer"
            })) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        /// <summary>
        /// // Details method
        /// </summary>
        [Test]
        public async Task Details_ModelStateIsValid_AsksForDetailsView() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }
        
        [Test]
        public async Task Details_ModelStateIsValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new PostDTO {
                Id = _id.Value,
                Title = "Programmer"
            });
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Details(2)) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }
        
        [Test]
        public async Task Details_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }
        
        [Test]
        public async Task Details_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }
        
        /// <summary>
        /// // Delete_Get method
        /// </summary>
        [Test]
        public async Task Delete_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }
        
        [Test]
        public async Task Delete_Get_ModelStateIsValid_RetrievesPostFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new PostDTO {
                Id = _id.Value,
                Title = "Programmer"
            });
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(2)) as ViewResult;

            PostViewModel model = result.ViewData.Model as PostViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Programmer", model.Title);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmed_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmed(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteConfirmed_Post_ModelStateIsNotValid_AsksForErrorView() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new ValidationException("DeleteAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteConfirmed(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteConfirmed_Post_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<PostService> mock = new Mock<PostService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new ValidationException("DeleteAsync method throws Exception", ""));
            PostController controller = GetNewPostController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteConfirmed(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("DeleteAsync method throws Exception", model[0]);
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
        /// // DeleteAllConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllConfirmed_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteAllConfirmed()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        /// <summary>
        /// // ExportJson method
        /// </summary>
        [Test]
        public async Task ExportJson_RedirectToIndex() {
            Mock<PostService> mock = new Mock<PostService>();
            PostController controller = GetNewPostControllerWithControllerContext(mock.Object, null, null);

            FilePathResult result = (await controller.ExportJson()) as FilePathResult;

            Assert.AreEqual("application/json", result.ContentType);
            Assert.AreEqual("Posts.json", result.FileDownloadName);
            Assert.AreEqual("./DiplomMSSQLApp.WEB/Results/Posts.json", result.FileName);
        }
    }
}
