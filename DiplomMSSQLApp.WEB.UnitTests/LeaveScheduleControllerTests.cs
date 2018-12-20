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
    public class LeaveScheduleControllerTests {
        protected LeaveScheduleController GetNewLeaveScheduleController(IService<LeaveScheduleDTO> ls) {
            return new LeaveScheduleController(ls);
        }

        protected LeaveScheduleController GetNewLeaveScheduleControllerWithControllerContext(IService<LeaveScheduleDTO> ls, bool isXRequestedWith = false) {
            return new LeaveScheduleController(ls) { ControllerContext = MockingControllerContext(isXRequestedWith) };
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
        public async Task Index_SyncRequest_AsksForIndexView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleControllerWithControllerContext(mock.Object);

            ViewResult result = (await controller.Index()) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public async Task Index_SyncRequest_RetrievesLeaveSchedulesPropertyFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<LeaveScheduleDTO>>(), It.IsAny<int>())).Returns(new LeaveScheduleDTO[] {
                new LeaveScheduleDTO {
                    Id = 7,
                    Year = 2018
                }
            });
            LeaveScheduleController controller = GetNewLeaveScheduleControllerWithControllerContext(mock.Object);

            ViewResult result = (await controller.Index()) as ViewResult;

            LeaveScheduleListViewModel model = result.ViewData.Model as LeaveScheduleListViewModel;
            Assert.AreEqual(1, model.LeaveSchedules.Count());
            Assert.AreEqual(7, model.LeaveSchedules.FirstOrDefault().Id);
            Assert.AreEqual(2018, model.LeaveSchedules.FirstOrDefault().Year);
        }

        [Test]
        public async Task Index_SyncRequest_RetrievesPageInfoPropertyFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            LeaveScheduleController controller = GetNewLeaveScheduleControllerWithControllerContext(mock.Object);

            ViewResult result = (await controller.Index()) as ViewResult;

            LeaveScheduleListViewModel model = result.ViewData.Model as LeaveScheduleListViewModel;
            Assert.AreEqual(9, model.PageInfo.TotalItems);
            Assert.AreEqual(3, model.PageInfo.PageSize);
            Assert.AreEqual(3, model.PageInfo.PageNumber);
            Assert.AreEqual(3, model.PageInfo.TotalPages);
        }

        [Test]
        public async Task Index_AsyncRequest_RetrievesLeaveSchedulesPropertyFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<LeaveScheduleDTO>>(), It.IsAny<int>())).Returns(new LeaveScheduleDTO[] {
                new LeaveScheduleDTO {
                    Id = 7,
                    Year = 2018
                }
            });
            LeaveScheduleController controller = GetNewLeaveScheduleControllerWithControllerContext(mock.Object, true);

            JsonResult result = (await controller.Index()) as JsonResult;
            object LeaveSchedule = (result.Data.GetType().GetProperty("LeaveSchedules").GetValue(result.Data) as object[])[0];
            int id = (int)LeaveSchedule.GetType().GetProperty("Id").GetValue(LeaveSchedule);
            int year = (int)LeaveSchedule.GetType().GetProperty("Year").GetValue(LeaveSchedule);

            Assert.AreEqual(7, id);
            Assert.AreEqual(2018, year);
        }

        [Test]
        public async Task Index_AsyncRequest_JsonRequestBehaviorEqualsAllowGet() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleControllerWithControllerContext(mock.Object, true);

            JsonResult result = (await controller.Index()) as JsonResult;

            Assert.AreEqual(JsonRequestBehavior.AllowGet, result.JsonRequestBehavior);
        }

        /// <summary>
        /// // Create_Get method
        /// </summary>
        [Test]
        public void Create_Get_AsksForCreateView() {
            LeaveScheduleController controller = GetNewLeaveScheduleController(null);

            ViewResult result = controller.Create() as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        /// <summary>
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task Create_Post_ModelIsValid_RedirectToIndex() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            RedirectToRouteResult result = (await controller.Create(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Create_Post_ModelIsInvalid_AsksForCreateView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<LeaveScheduleDTO>())).Throws(new ValidationException("", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Create(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelIsInvalid_RetrievesLeaveScheduleFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.CreateAsync(It.IsAny<LeaveScheduleDTO>())).Throws(new ValidationException("", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Create(new LeaveScheduleViewModel {
                Id = 7,
                Year = 2018
            })) as ViewResult;

            LeaveScheduleViewModel model = result.ViewData.Model as LeaveScheduleViewModel;
            Assert.AreEqual(7, model.Id);
            Assert.AreEqual(2018, model.Year);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_ModelIsValid_AsksForEditView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Edit(7)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelIsValid_RetrievesLeaveScheduleFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new LeaveScheduleDTO {
                Id = _id.Value,
                Year = 2018
            });
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Edit(7)) as ViewResult;

            LeaveScheduleViewModel model = result.ViewData.Model as LeaveScheduleViewModel;
            Assert.AreEqual(7, model.Id);
            Assert.AreEqual(2018, model.Year);
        }

        [Test]
        public async Task Edit_Get_ModelIsInvalid_AsksForErrorView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Edit(7)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelIsValid_RedirectToIndex() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            RedirectToRouteResult result = (await controller.Edit(new LeaveScheduleViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_ModelIsInvalid_AsksForEditView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.EditAsync(It.IsAny<LeaveScheduleDTO>())).Throws(new ValidationException("", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Edit(new LeaveScheduleViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Post_ModelIsInvalid_RetrievesLeaveScheduleFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.EditAsync(It.IsAny<LeaveScheduleDTO>())).Throws(new ValidationException("", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Edit(new LeaveScheduleViewModel {
                Id = 7,
                Year = 2018
            })) as ViewResult;

            LeaveScheduleViewModel model = result.ViewData.Model as LeaveScheduleViewModel;
            Assert.AreEqual(7, model.Id);
            Assert.AreEqual(2018, model.Year);
        }

        /// <summary>
        /// // Delete_Get method
        /// </summary>
        [Test]
        public async Task Delete_Get_ModelIsValid_AsksForDeleteView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Delete(7)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelIsValid_RetrievesLeaveScheduleFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new LeaveScheduleDTO {
                Id = _id.Value,
                Year = 2018
            });
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Delete(7)) as ViewResult;

            LeaveScheduleViewModel model = result.ViewData.Model as LeaveScheduleViewModel;
            Assert.AreEqual(7, model.Id);
            Assert.AreEqual(2018, model.Year);
        }

        [Test]
        public async Task Delete_Get_ModelIsInvalid_AsksForErrorView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Delete(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Delete_Get_ModelIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.Delete(7)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmed_Post_ModelIsValid_RedirectToIndex() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            RedirectToRouteResult result = (await controller.DeleteConfirmed(7)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteConfirmed_Post_DeleteAsyncMethodThrowsException_AsksForErrorView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new ValidationException("DeleteAsync method throws Exception", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.DeleteConfirmed(7)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteConfirmed_Post_DeleteAsyncMethodThrowsException_RetrievesExceptionMessageFromModel() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new ValidationException("DeleteAsync method throws Exception", ""));
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = (await controller.DeleteConfirmed(7)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("DeleteAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAllConfirmed_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllConfirmed_Post_RedirectToIndex() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleController(mock.Object);

            RedirectToRouteResult result = (await controller.DeleteAllConfirmed()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // ExportJson method
        /// </summary>
        [Test]
        public async Task ExportJson_RedirectToIndex() {
            Mock<LeaveScheduleService> mock = new Mock<LeaveScheduleService>();
            LeaveScheduleController controller = GetNewLeaveScheduleControllerWithControllerContext(mock.Object);

            FilePathResult result = (await controller.ExportJson()) as FilePathResult;

            Assert.AreEqual("application/json", result.ContentType);
            Assert.AreEqual("LeaveSchedules.json", result.FileDownloadName);
            Assert.AreEqual("./DiplomMSSQLApp.WEB/Results/LeaveSchedules.json", result.FileName);
        }
    }
}
