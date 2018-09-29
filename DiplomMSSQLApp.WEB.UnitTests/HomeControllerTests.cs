using DiplomMSSQLApp.BLL.BusinessModels;
using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Models;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests
{
    [TestFixture]
    public class HomeControllerTests {

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public void Index_Get_AsksForIndexView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(m => m.MapPath(It.IsAny<string>())).Returns("/");
            Mock<HttpContextBase> httpCtxStub = new Mock<HttpContextBase>();
            httpCtxStub.Setup(m => m.Server).Returns(serverMock.Object);
            ControllerContext controllerCtx = new ControllerContext {
                HttpContext = httpCtxStub.Object    // mocking Server.MapPath method
            };
            HomeController controller = new HomeController(mock.Object, null, null) { ControllerContext = controllerCtx };

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
            Assert.IsNull(result.ViewData.ModelState[""]);
        }

        [Test]
        public void Index_Get_RetrievesEmailPropertyFromFilter() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(m => m.MapPath(It.IsAny<string>())).Returns("/");
            Mock<HttpContextBase> httpCtxStub = new Mock<HttpContextBase>();
            httpCtxStub.Setup(m => m.Server).Returns(serverMock.Object);
            ControllerContext controllerCtx = new ControllerContext {
                HttpContext = httpCtxStub.Object    // mocking Server.MapPath method
            };
            HomeController controller = new HomeController(mock.Object, null, null) { ControllerContext = controllerCtx };

            ViewResult result = controller.Index(new EmployeeFilter() { Email = "Brown@mail.ru" }, null) as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual("Brown@mail.ru", model.Filter.Email);
            Assert.IsNull(result.ViewData.ModelState[""]);
        }

        /// <summary>
        /// // CreateAsync_Get method
        /// </summary>
        [Test]
        public async Task CreateAsync_Get_AsksForCreateAsyncView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagDepartments() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { });
            Mock<PostService> pmock = new Mock<PostService>();
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer" }
            });
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // CreateAsync_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_AsksForCreateAsyncView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
            Assert.IsNotNull(result.ViewData.ModelState[""]);
        }

        [Test]
        public async Task CreateAsync_Post_RedirectToIndexIfModelStateIsValid() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            HomeController controller = new HomeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.CreateAsync(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task CreateAsync_Post_RetrievesEmailPropertyFromModelIfModelStateIsNotValid() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(new EmployeeViewModel { Email = "Brown@mail.ru" })) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual("Brown@mail.ru", model.Email);
            Assert.IsNotNull(result.ViewData.ModelState[""]);
        }

        [Test]
        public async Task CreateAsync_Post_SetViewBagDepartmentsIfModelStateIsNotValid() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { });
            Mock<PostService> pmock = new Mock<PostService>();
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
            Assert.IsNotNull(result.ViewData.ModelState[""]);
        }

        [Test]
        public async Task CreateAsync_Post_SetViewBagPostsIfModelStateIsNotValid() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer" }
            });
            HomeController controller = new HomeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer", item.Text);
            Assert.AreEqual("2", item.Value);
            Assert.IsNotNull(result.ViewData.ModelState[""]);
        }
    }
}
