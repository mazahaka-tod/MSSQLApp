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
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class AnnualLeaveControllerTests {
        protected AnnualLeaveController GetNewAnnualLeaveController(IService<AnnualLeaveDTO> als, IService<EmployeeDTO> es) {
            return new AnnualLeaveController(als, es);
        }

        protected AnnualLeaveController GetNewAnnualLeaveControllerWithControllerContext(IService<AnnualLeaveDTO> als, IService<EmployeeDTO> es, bool isXRequestedWith = false) {
            return new AnnualLeaveController(als, es) { ControllerContext = MockingControllerContext(isXRequestedWith) };
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
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveControllerWithControllerContext(mock.Object, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public void Index_SyncRequest_RetrievesAnnualLeavesPropertyFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<AnnualLeaveDTO>>(), It.IsAny<int>())).Returns(new AnnualLeaveDTO[] {
                new AnnualLeaveDTO {
                    Id = 7
                }
            });
            AnnualLeaveController controller = GetNewAnnualLeaveControllerWithControllerContext(mock.Object, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            AnnualLeaveListViewModel model = result.ViewData.Model as AnnualLeaveListViewModel;
            Assert.AreEqual(1, model.AnnualLeaves.Count());
            Assert.AreEqual(7, model.AnnualLeaves.FirstOrDefault().Id);
        }

        [Test]
        public void Index_SyncRequest_RetrievesPageInfoPropertyFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            AnnualLeaveController controller = GetNewAnnualLeaveControllerWithControllerContext(mock.Object, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            AnnualLeaveListViewModel model = result.ViewData.Model as AnnualLeaveListViewModel;
            Assert.AreEqual(9, model.PageInfo.TotalItems);
            Assert.AreEqual(3, model.PageInfo.PageSize);
            Assert.AreEqual(3, model.PageInfo.PageNumber);
            Assert.AreEqual(3, model.PageInfo.TotalPages);
        }

        [Test]
        public void Index_AsyncRequest_RetrievesAnnualLeavesPropertyFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<AnnualLeaveDTO>>(), It.IsAny<int>())).Returns(new AnnualLeaveDTO[] {
                new AnnualLeaveDTO {
                    Id = 7,
                    Employee = new EmployeeDTO { Post = new PostDTO { Department = new DepartmentDTO() } }
                }
            });
            AnnualLeaveController controller = GetNewAnnualLeaveControllerWithControllerContext(mock.Object, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;
            object AnnualLeave = (result.Data.GetType().GetProperty("AnnualLeaves").GetValue(result.Data) as object[])[0];
            int id = (int)AnnualLeave.GetType().GetProperty("Id").GetValue(AnnualLeave);

            Assert.AreEqual(7, id);
        }

        [Test]
        public void Index_AsyncRequest_JsonRequestBehaviorEqualsAllowGet() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveControllerWithControllerContext(mock.Object, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;

            Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
        }

        /// <summary>
        /// // Create_Get method
        /// </summary>
        [Test]
        public async Task Create_Get_ExistsEmployees_AsksForCreateView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CountAsync()).ReturnsAsync(1);
            AnnualLeaveController controller = GetNewAnnualLeaveController(null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Get_ExistsEmployees_SetViewBagEmployees() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CountAsync()).ReturnsAsync(1);
            mock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович", Post = new PostDTO { Department = new DepartmentDTO { DepartmentName = "IT" } } }
            });
            AnnualLeaveController controller = GetNewAnnualLeaveController(null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task Create_Get_NoEmployees_AsksForErrorView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CountAsync()).ReturnsAsync(0);
            AnnualLeaveController controller = GetNewAnnualLeaveController(null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        /// <summary>
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task Create_Post_ModelIsValid_RedirectToIndex() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            RedirectToRouteResult result = (await controller.Create(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Create_Post_ModelIsInvalid_AsksForCreateView() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.CreateAsync(It.IsAny<AnnualLeaveDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelIsInvalid_RetrievesAnnualLeaveFromModel() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.CreateAsync(It.IsAny<AnnualLeaveDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Create(new AnnualLeaveViewModel {
                Id = 7
            })) as ViewResult;

            AnnualLeaveViewModel model = result.ViewData.Model as AnnualLeaveViewModel;
            Assert.AreEqual(7, model.Id);
        }

        [Test]
        public async Task Create_Post_ModelIsInvalid_SetViewBagEmployees() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.CreateAsync(It.IsAny<AnnualLeaveDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович", Post = new PostDTO { Department = new DepartmentDTO { DepartmentName = "IT" } } }
            });
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_ModelIsValid_AsksForEditView() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelIsValid_RetrievesAnnualLeaveFromModel() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new AnnualLeaveDTO {
                Id = _id.Value
            });
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(7)) as ViewResult;

            AnnualLeaveViewModel model = result.ViewData.Model as AnnualLeaveViewModel;
            Assert.AreEqual(7, model.Id);
        }

        [Test]
        public async Task Edit_Get_ModelIsInvalid_AsksForErrorView() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        [Test]
        public async Task Edit_Get_SetViewBagEmployees() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович", Post = new PostDTO { Department = new DepartmentDTO { DepartmentName = "IT" } } }
            });
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelIsValid_RedirectToIndex() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            RedirectToRouteResult result = (await controller.Edit(new AnnualLeaveViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_ModelIsInvalid_AsksForEditView() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.EditAsync(It.IsAny<AnnualLeaveDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(new AnnualLeaveViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Post_ModelIsInvalid_RetrievesAnnualLeaveFromModel() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.EditAsync(It.IsAny<AnnualLeaveDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(new AnnualLeaveViewModel {
                Id = 7
            })) as ViewResult;

            AnnualLeaveViewModel model = result.ViewData.Model as AnnualLeaveViewModel;
            Assert.AreEqual(7, model.Id);
        }

        [Test]
        public async Task Edit_Post_ModelIsInvalid_SetViewBagEmployees() {
            Mock<AnnualLeaveService> almock = new Mock<AnnualLeaveService>();
            almock.Setup(m => m.EditAsync(It.IsAny<AnnualLeaveDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович", Post = new PostDTO { Department = new DepartmentDTO { DepartmentName = "IT" } } }
            });
            AnnualLeaveController controller = GetNewAnnualLeaveController(almock.Object, emock.Object);

            ViewResult result = (await controller.Edit(new AnnualLeaveViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович [IT]", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Details method
        /// </summary>
        [Test]
        public async Task Details_ModelIsValid_AsksForDetailsView() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }

        [Test]
        public async Task Details_ModelIsValid_RetrievesAnnualLeaveFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new AnnualLeaveDTO {
                Id = _id.Value
            });
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Details(7)) as ViewResult;

            AnnualLeaveViewModel model = result.ViewData.Model as AnnualLeaveViewModel;
            Assert.AreEqual(7, model.Id);
        }

        [Test]
        public async Task Details_ModelIsInvalid_AsksForErrorView() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Details_ModelIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // Delete_Get method
        /// </summary>
        [Test]
        public async Task Delete_Get_ModelIsValid_AsksForDeleteView() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelIsValid_RetrievesAnnualLeaveFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new AnnualLeaveDTO {
                Id = _id.Value
            });
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Delete(7)) as ViewResult;

            AnnualLeaveViewModel model = result.ViewData.Model as AnnualLeaveViewModel;
            Assert.AreEqual(7, model.Id);
        }

        [Test]
        public async Task Delete_Get_ModelIsInvalid_AsksForErrorView() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmed_Post_RedirectToIndex() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmed(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAllConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllConfirmed_Post_RedirectToIndex() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveController(mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteAllConfirmed()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // ExportJson method
        /// </summary>
        [Test]
        public async Task ExportJson_RedirectToIndex() {
            Mock<AnnualLeaveService> mock = new Mock<AnnualLeaveService>();
            AnnualLeaveController controller = GetNewAnnualLeaveControllerWithControllerContext(mock.Object, null);

            FilePathResult result = (await controller.ExportJson()) as FilePathResult;

            Assert.AreEqual("application/json", result.ContentType);
            Assert.AreEqual("AnnualLeaves.json", result.FileDownloadName);
            Assert.AreEqual("./DiplomMSSQLApp.WEB/Results/AnnualLeaves.json", result.FileName);
        }
    }
}
