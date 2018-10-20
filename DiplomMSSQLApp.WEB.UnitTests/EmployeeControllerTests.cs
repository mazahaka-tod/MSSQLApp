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
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests
{
    [TestFixture]
    public class EmployeeControllerTests {
        protected EmployeeController GetNewEmployeeController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps) {
            return new EmployeeController(es, ds, ps);
        }

        protected EmployeeController GetNewEmployeeControllerWithControllerContext(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps, string XRequestedWith = "") {
            return new EmployeeController(es, ds, ps) { ControllerContext = MockingControllerContext(XRequestedWith) };
        }

        protected ControllerContext MockingControllerContext(string XRequestedWith) {
            // mocking Server.MapPath method
            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(m => m.MapPath(It.IsAny<string>())).Returns("./DiplomMSSQLApp.WEB/Results/Employee/");
            // mocking Request.Headers["X-Requested-With"]
            Mock<HttpRequestBase> requestMock = new Mock<HttpRequestBase>();
            requestMock.SetupGet(x => x.Headers).Returns(new System.Net.WebHeaderCollection {
                {"X-Requested-With", XRequestedWith}
            });

            Mock<HttpContextBase> httpCtxStub = new Mock<HttpContextBase>();
            httpCtxStub.SetupGet(m => m.Server).Returns(serverMock.Object);
            httpCtxStub.SetupGet(m => m.Request).Returns(requestMock.Object);

            ControllerContext controllerCtx = new ControllerContext {
                HttpContext = httpCtxStub.Object
            };
            return controllerCtx;
        }

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public void Index_AsksForIndexView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public void Index_AsksForGetEmployeesDataPartialView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null, "XMLHttpRequest");

            PartialViewResult result = controller.Index(null, null) as PartialViewResult;

            Assert.AreEqual("GetEmployeesData", result.ViewName);
        }

        [Test]
        public void Index_RetrievesEmployeesPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<EmployeeDTO>>(), It.IsAny<int>())).Returns(new EmployeeDTO[] {
                new EmployeeDTO {
                    Id = 1,
                    LastName = "Brown",
                    Email = "Brown@mail.ru",
                    Salary = 15000,
                    Bonus = 0.1
                }
            });
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual(1, model.Employees.Count());
            Assert.AreEqual(1, model.Employees.FirstOrDefault().Id);
            Assert.AreEqual("Brown", model.Employees.FirstOrDefault().LastName);
            Assert.AreEqual("Brown@mail.ru", model.Employees.FirstOrDefault().Email);
            Assert.AreEqual(15000, model.Employees.FirstOrDefault().Salary);
            Assert.AreEqual(0.1, model.Employees.FirstOrDefault().Bonus);
        }

        [Test]
        public void Index_RetrievesFilterPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(filter: new EmployeeFilter() {
                Email = "Brown@mail.ru",
                MinSalary = 13000,
                MaxSalary = 23000,
                PostTitle = "Manager",
                DepartmentName = "Management"
            }, filterAsJsonString: null) as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual("Brown@mail.ru", model.Filter.Email);
            Assert.AreEqual(13000, model.Filter.MinSalary);
            Assert.AreEqual(23000, model.Filter.MaxSalary);
            Assert.AreEqual("Manager", model.Filter.PostTitle);
            Assert.AreEqual("Management", model.Filter.DepartmentName);
        }

        [Test]
        public void Index_SetFilterAsJsonString_RetrievesFilterPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(filter: null, filterAsJsonString: "{ \"MinSalary\":10000, \"MaxSalary\":20000 }") as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual(10000, model.Filter.MinSalary);
            Assert.AreEqual(20000, model.Filter.MaxSalary);
        }

        [Test]
        public void Index_RetrievesPageInfoPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual(9, model.PageInfo.TotalItems);
            Assert.AreEqual(3, model.PageInfo.PageSize);
            Assert.AreEqual(3, model.PageInfo.PageNumber);
            Assert.AreEqual(3, model.PageInfo.TotalPages);
        }

        /// <summary>
        /// // CreateAsync_Get method
        /// </summary>
        [Test]
        public async Task CreateAsync_Get_AsksForCreateView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagDepartments() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] {
                new DepartmentDTO() { Id = 2, DepartmentName = "IT" }
            });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("IT", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagDepartmentsDefaultValue() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

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
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagPostsDefaultValue() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] { });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        /// <summary>
        /// // CreateAsync_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.CreateAsync(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(new EmployeeViewModel {
                Id = 2,
                LastName = "Brown",
                Email = "Brown@mail.ru"
            })) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Email);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_SetViewBagDepartments() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] {
                new DepartmentDTO() { Id = 2, DepartmentName = "IT" }
            });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("IT", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_SetViewBagDepartmentsDefaultValue() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer" }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_SetViewBagPostsDefaultValue() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] { });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        /// <summary>
        /// // EditAsync_Get method
        /// </summary>
        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_AsksForEditView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new EmployeeDTO {
                Id = _id.Value,
                LastName = "Brown",
                Email = "Brown@mail.ru"
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Email);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        [Test]
        public async Task EditAsync_Get_SetViewBagDepartments() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] {
                new DepartmentDTO() { Id = 2, DepartmentName = "IT" }
            });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("IT", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task EditAsync_Get_SetViewBagDepartmentsDefaultValue() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        [Test]
        public async Task EditAsync_Get_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer" }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task EditAsync_Get_SetViewBagPostsDefaultValue() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] { });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        /// <summary>
        /// // EditAsync_Post method
        /// </summary>
        [Test]
        public async Task EditAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.EditAsync(new EmployeeViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(new EmployeeViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(new EmployeeViewModel {
                Id = 2,
                LastName = "Brown",
                Email = "Brown@mail.ru"
            })) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Email);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagDepartments() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] {
                new DepartmentDTO() { Id = 2, DepartmentName = "IT" }
            });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(new EmployeeViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("IT", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagDepartmentsDefaultValue() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new DepartmentDTO[] { });
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(new EmployeeViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Departments as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer" }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(new EmployeeViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagPostsDefaultValue() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] { });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.EditAsync(new EmployeeViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("unknown", item.Text);
            Assert.AreEqual("1", item.Value);
        }

        /// <summary>
        /// // DetailsAsync method
        /// </summary>
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_AsksForDetailsView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new EmployeeDTO {
                Id = _id.Value,
                LastName = "Brown",
                Email = "Brown@mail.ru"
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Email);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        /// <summary>
        /// // DeleteAsync_Get method
        /// </summary>
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new EmployeeDTO {
                Id = _id.Value,
                LastName = "Brown",
                Email = "Brown@mail.ru"
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Email);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        /// <summary>
        /// // DeleteAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteAsync_Post_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmedAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAll_Post method
        /// </summary>
        [Test]
        public async Task DeleteAll_Post_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteAllAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // ExportJsonAsync method
        /// </summary>
        [Test]
        public async Task ExportJsonAsync_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.ExportJsonAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // TestCreateAsync method
        /// </summary>
        [Test]
        public async Task TestCreateAsync_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.TestCreateAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // TestReadAsync method
        /// </summary>
        [Test]
        public async Task TestReadAsync_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.TestReadAsync(1, 0)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // TestUpdateAsync method
        /// </summary>
        [Test]
        public async Task TestUpdateAsync_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.TestUpdateAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // TestDeleteAsync method
        /// </summary>
        [Test]
        public async Task TestDeleteAsync_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.TestDeleteAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // About method
        /// </summary>
        [Test]
        public void About_AsksForAboutView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = controller.About() as ViewResult;

            Assert.AreEqual("About", result.ViewName);
        }

        /// <summary>
        /// // Contact method
        /// </summary>
        [Test]
        public void Contact_AsksForContactView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = controller.Contact() as ViewResult;

            Assert.AreEqual("Contact", result.ViewName);
        }
    }
}
