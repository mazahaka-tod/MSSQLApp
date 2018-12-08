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
    public class EmployeeControllerTests {
        protected EmployeeController GetNewEmployeeController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps) {
            return new EmployeeController(es, ds, ps);
        }

        protected EmployeeController GetNewEmployeeControllerWithControllerContext(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<PostDTO> ps, bool isXRequestedWith = false) {
            return new EmployeeController(es, ds, ps) { ControllerContext = MockingControllerContext(isXRequestedWith) };
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
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public void Index_SyncRequest_RetrievesEmployeesPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<EmployeeDTO>>(), It.IsAny<int>())).Returns(new EmployeeDTO[] {
                new EmployeeDTO {
                    Id = 1,
                    LastName = "Brown",
                    Contacts = new BLL.DTO.Contacts { Email = "Brown@mail.ru" },
                    Post = new PostDTO {
                        Salary = 15000,
                        Premium = 0.1
                    }
                }
            });
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual(1, model.Employees.Count());
            Assert.AreEqual(1, model.Employees.FirstOrDefault().Id);
            Assert.AreEqual("Brown", model.Employees.FirstOrDefault().LastName);
            Assert.AreEqual("Brown@mail.ru", model.Employees.FirstOrDefault().Contacts.Email);
            Assert.AreEqual(15000, model.Employees.FirstOrDefault().Post.Salary);
            Assert.AreEqual(0.1, model.Employees.FirstOrDefault().Post.Premium);
        }

        [Test]
        public void Index_SyncRequest_RetrievesFilterPropertyFromModel() {
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
        public void Index_SyncRequest_SetFilterAsJsonString_RetrievesFilterPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            ViewResult result = controller.Index(filter: null, filterAsJsonString: "{ \"MinSalary\":10000, \"MaxSalary\":20000 }") as ViewResult;

            EmployeeListViewModel model = result.ViewData.Model as EmployeeListViewModel;
            Assert.AreEqual(10000, model.Filter.MinSalary);
            Assert.AreEqual(20000, model.Filter.MaxSalary);
        }

        [Test]
        public void Index_SyncRequest_RetrievesPageInfoPropertyFromModel() {
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

        [Test]
        public void Index_AsyncRequest_RetrievesEmployeesPropertyFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<EmployeeDTO>>(), It.IsAny<int>())).Returns(new EmployeeDTO[] {
                new EmployeeDTO {
                    Id = 1,
                    LastName = "Brown",
                    Contacts = new BLL.DTO.Contacts(),
                    Post = new PostDTO {
                        Department = new DepartmentDTO()
                    }
                }
            });
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;
            object employee = (result.Data.GetType().GetProperty("Employees").GetValue(result.Data) as object[])[0];
            int id = (int)employee.GetType().GetProperty("Id").GetValue(employee);
            string lastname = employee.GetType().GetProperty("LastName").GetValue(employee).ToString();

            Assert.AreEqual(1, id);
            Assert.AreEqual("Brown", lastname);
        }

        [Test]
        public void Index_AsyncRequest_JsonRequestBehaviorEqualsAllowGet() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;

            Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
        }

        /// <summary>
        /// // Create_Get method
        /// </summary>
        [Test]
        public async Task Create_Get_AsksForCreateView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.CountAsync()).ReturnsAsync(2);
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] { new PostDTO { NumberOfUnits = 3 } });
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Get_NoVacancy_AsksForErrorView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.CountAsync()).ReturnsAsync(2);
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] { new PostDTO { NumberOfUnits = 2 } });
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Create_Get_SetViewBagPosts() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.CountAsync()).ReturnsAsync(2);
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer", NumberOfUnits = 3, Department = new DepartmentDTO { DepartmentName = "IT" } }
            });
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task Create_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.Create(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Create_Post_ModelStateIsInvalid_AsksForCreateView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelStateIsInvalid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Create(new EmployeeViewModel {
                Id = 2,
                LastName = "Brown",
                Contacts = new Models.Contacts { Email = "Brown@mail.ru" }
            })) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Contacts.Email);
        }

        [Test]
        public async Task Create_Post_ModelStateIsInvalid_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer", Department = new DepartmentDTO { DepartmentName = "IT" } }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_ModelStateIsValid_AsksForEditView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new EmployeeDTO {
                Id = _id.Value,
                LastName = "Brown",
                Contacts = new BLL.DTO.Contacts { Email = "Brown@mail.ru" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Contacts.Email);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsInvalid_AsksForErrorView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(emock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        [Test]
        public async Task Edit_Get_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer", Department = new DepartmentDTO { DepartmentName = "IT" } }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.Edit(new EmployeeViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsInvalid_AsksForEditView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(new EmployeeViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsInvalid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(new EmployeeViewModel {
                Id = 2,
                LastName = "Brown",
                Contacts = new Models.Contacts { Email = "Brown@mail.ru" }
            })) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Contacts.Email);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsInvalid_SetViewBagPosts() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.EditAsync(It.IsAny<EmployeeDTO>())).Throws(new ValidationException("", ""));
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<PostService> pmock = new Mock<PostService>();
            pmock.Setup(m => m.GetAllAsync()).ReturnsAsync(new PostDTO[] {
                new PostDTO() { Id = 2, Title = "Programmer", Department = new DepartmentDTO { DepartmentName = "IT" } }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, dmock.Object, pmock.Object);

            ViewResult result = (await controller.Edit(new EmployeeViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Posts as SelectList).FirstOrDefault();
            Assert.AreEqual("Programmer [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Details method
        /// </summary>
        [Test]
        public async Task Details_ModelStateIsValid_AsksForDetailsView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }

        [Test]
        public async Task Details_ModelStateIsValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new EmployeeDTO {
                Id = _id.Value,
                LastName = "Brown",
                Contacts = new BLL.DTO.Contacts { Email = "Brown@mail.ru" }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Contacts.Email);
        }

        [Test]
        public async Task Details_ModelStateIsNotValid_AsksForErrorView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Details_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // Delete_Get method
        /// </summary>
        [Test]
        public async Task Delete_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsValid_RetrievesEmployeeFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new EmployeeDTO {
                Id = _id.Value,
                LastName = "Brown",
                Contacts = new BLL.DTO.Contacts { Email = "Brown@mail.ru" }
            });
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            EmployeeViewModel model = result.ViewData.Model as EmployeeViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("Brown", model.LastName);
            Assert.AreEqual("Brown@mail.ru", model.Contacts.Email);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmed_Post_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmed(1)) as RedirectToRouteResult;

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
        /// // DeleteAllConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllConfirmed_Post_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeController(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.DeleteAllConfirmed()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // ExportJson method
        /// </summary>
        [Test]
        public async Task ExportJson_RedirectToIndex() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            EmployeeController controller = GetNewEmployeeControllerWithControllerContext(mock.Object, null, null);

            RedirectToRouteResult result = (await controller.ExportJson()) as RedirectToRouteResult;

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
