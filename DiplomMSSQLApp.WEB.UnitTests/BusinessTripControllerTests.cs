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
    public class BusinessTripControllerTests {
        protected BusinessTripController GetNewBusinessTripController(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es) {
            return new BusinessTripController(bs, es);
        }

        protected BusinessTripController GetNewBusinessTripControllerWithControllerContext(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es, bool isXRequestedWith = false) {
            return new BusinessTripController(bs, es) { ControllerContext = MockingControllerContext(isXRequestedWith) };
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
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripControllerWithControllerContext(mock.Object, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public void Index_SyncRequest_RetrievesBusinessTripsPropertyFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<BusinessTripDTO>>(), It.IsAny<int>())).Returns(new BusinessTripDTO[] {
                new BusinessTripDTO {
                    Id = 1,
                    Name = "02.09.2018_026"
                }
            });
            BusinessTripController controller = GetNewBusinessTripControllerWithControllerContext(mock.Object, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            BusinessTripListViewModel model = result.ViewData.Model as BusinessTripListViewModel;
            Assert.AreEqual(1, model.BusinessTrips.Count());
            Assert.AreEqual(1, model.BusinessTrips.FirstOrDefault().Id);
            Assert.AreEqual("02.09.2018_026", model.BusinessTrips.FirstOrDefault().Name);
        }

        [Test]
        public void Index_SyncRequest_RetrievesPageInfoPropertyFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            BusinessTripController controller = GetNewBusinessTripControllerWithControllerContext(mock.Object, null);

            ViewResult result = controller.Index(null, null) as ViewResult;

            BusinessTripListViewModel model = result.ViewData.Model as BusinessTripListViewModel;
            Assert.AreEqual(9, model.PageInfo.TotalItems);
            Assert.AreEqual(3, model.PageInfo.PageSize);
            Assert.AreEqual(3, model.PageInfo.PageNumber);
            Assert.AreEqual(3, model.PageInfo.TotalPages);
        }

        [Test]
        public void Index_AsyncRequest_RetrievesBusinessTripsPropertyFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<BusinessTripDTO>>(), It.IsAny<int>())).Returns(new BusinessTripDTO[] {
                new BusinessTripDTO {
                    Id = 1,
                    Name = "02.09.2018_026"
                }
            });
            BusinessTripController controller = GetNewBusinessTripControllerWithControllerContext(mock.Object, null, true);

            JsonResult result = controller.Index(null, null) as JsonResult;
            object businessTrip = (result.Data.GetType().GetProperty("BusinessTrips").GetValue(result.Data) as object[])[0];
            int id = (int)businessTrip.GetType().GetProperty("Id").GetValue(businessTrip);
            string code = businessTrip.GetType().GetProperty("Code").GetValue(businessTrip).ToString();

            Assert.AreEqual(1, id);
            Assert.AreEqual("02.09.2018_026", code);
        }

        [Test]
        public void Index_AsyncRequest_JsonRequestBehaviorEqualsAllowGet() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripControllerWithControllerContext(mock.Object, null, true);

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
            BusinessTripController controller = GetNewBusinessTripController(null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Get_ExistsEmployees_SetViewBagEmployees() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CountAsync()).ReturnsAsync(1);
            mock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            BusinessTripController controller = GetNewBusinessTripController(null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        [Test]
        public async Task Create_Get_NoEmployees_AsksForErrorView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.CountAsync()).ReturnsAsync(0);
            BusinessTripController controller = GetNewBusinessTripController(null, mock.Object);

            ViewResult result = (await controller.Create()) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        /// <summary>
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task Create_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.Create(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Create_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.CreateAsync(It.IsAny<BusinessTripDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelStateIsNotValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.CreateAsync(It.IsAny<BusinessTripDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Create(new BusinessTripViewModel {
                Id = 2,
                Name = "02.09.2018_026"
            })) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task Create_Post_ModelStateIsInvalid_SetViewBagEmployees() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.CreateAsync(It.IsAny<BusinessTripDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_ModelStateIsValid_AsksForEditView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new BusinessTripDTO {
                Id = _id.Value,
                Name = "02.09.2018_026"
            });
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(2)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        [Test]
        public async Task Edit_Get_SetViewBagEmployees() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.Edit(new BusinessTripViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.EditAsync(It.IsAny<BusinessTripDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(new BusinessTripViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.EditAsync(It.IsAny<BusinessTripDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(new BusinessTripViewModel {
                Id = 2,
                Name = "02.09.2018_026"
            })) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_SetViewBagEmployees() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.EditAsync(It.IsAny<BusinessTripDTO>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.Edit(new BusinessTripViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Петров Петр Петрович", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // Details method
        /// </summary>
        [Test]
        public async Task Details_ModelStateIsValid_AsksForDetailsView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }

        [Test]
        public async Task Details_ModelStateIsValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new BusinessTripDTO {
                Id = _id.Value,
                Name = "02.09.2018_026",
            });
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Details(2)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task Details_ModelStateIsNotValid_AsksForErrorView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Details_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Details(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // Delete_Get method
        /// </summary>
        [Test]
        public async Task Delete_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new BusinessTripDTO {
                Id = _id.Value,
                Name = "02.09.2018_026"
            });
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Delete(2)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmed_Post_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmed(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAllConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllConfirmed_Post_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteAllConfirmed()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // ExportJson method
        /// </summary>
        [Test]
        public async Task ExportJson_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripControllerWithControllerContext(mock.Object, null);

            FilePathResult result = (await controller.ExportJson()) as FilePathResult;

            Assert.AreEqual("application/json", result.ContentType);
            Assert.AreEqual("BusinessTrips.json", result.FileDownloadName);
            Assert.AreEqual("./DiplomMSSQLApp.WEB/Results/BusinessTrips.json", result.FileName);
        }
    }
}
