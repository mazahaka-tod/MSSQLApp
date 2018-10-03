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
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests
{
    [TestFixture]
    public class BusinessTripControllerTests {
        protected BusinessTripController GetNewBusinessTripController(IService<BusinessTripDTO> bs, IService<EmployeeDTO> es) {
            return new BusinessTripController(bs, es);
        }

        /// <summary>
        /// // IndexAsync method
        /// </summary>
        [Test]
        public async Task IndexAsync_AsksForIndexView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public async Task IndexAsync_RetrievesBusinessTripsPropertyFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<BusinessTripDTO>>(), It.IsAny<int>())).Returns(new BusinessTripDTO[] {
                new BusinessTripDTO {
                    Id = 1,
                    Name = "02.09.2018_026"
                }
            });
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            BusinessTripListViewModel model = result.ViewData.Model as BusinessTripListViewModel;
            Assert.AreEqual(1, model.BusinessTrips.Count());
            Assert.AreEqual(1, model.BusinessTrips.FirstOrDefault().Id);
            Assert.AreEqual("02.09.2018_026", model.BusinessTrips.FirstOrDefault().Name);
        }

        [Test]
        public async Task IndexAsync_RetrievesPageInfoPropertyFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            BusinessTripListViewModel model = result.ViewData.Model as BusinessTripListViewModel;
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
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(null, mock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagEmployees() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Brown", FirstName = "Daniel" }
            });
            BusinessTripController controller = GetNewBusinessTripController(null, mock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // CreateAsync_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.CreateAsync(null, null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.CreateAsync(It.IsAny<BusinessTripDTO>(), It.IsAny<int[]>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.CreateAsync(null, null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.CreateAsync(It.IsAny<BusinessTripDTO>(), It.IsAny<int[]>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.CreateAsync(new BusinessTripViewModel {
                Id = 2,
                Name = "02.09.2018_026"
            }, null)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_SetViewBagEmployees() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.CreateAsync(It.IsAny<BusinessTripDTO>(), It.IsAny<int[]>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Brown", FirstName = "Daniel" }
            });
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.CreateAsync(null, null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // EditAsync_Get method
        /// </summary>
        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_AsksForEditView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new BusinessTripDTO {
                Id = _id.Value,
                Name = "02.09.2018_026"
            });
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(2)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        [Test]
        public async Task EditAsync_Get_SetViewBagEmployees() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
			emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Brown", FirstName = "Daniel" }
            });
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // EditAsync_Post method
        /// </summary>
        [Test]
        public async Task EditAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.EditAsync(new BusinessTripViewModel(), null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.EditAsync(It.IsAny<BusinessTripDTO>(), It.IsAny<int[]>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(new BusinessTripViewModel(), null)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.EditAsync(It.IsAny<BusinessTripDTO>(), It.IsAny<int[]>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(new BusinessTripViewModel {
                Id = 2,
                Name = "02.09.2018_026"
            }, null)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagEmployees() {
            Mock<BusinessTripService> bmock = new Mock<BusinessTripService>();
            bmock.Setup(m => m.EditAsync(It.IsAny<BusinessTripDTO>(), It.IsAny<int[]>())).Throws(new ValidationException("", ""));
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 2, LastName = "Brown", FirstName = "Daniel" }
            });
            BusinessTripController controller = GetNewBusinessTripController(bmock.Object, emock.Object);

            ViewResult result = (await controller.EditAsync(new BusinessTripViewModel(), null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("2", item.Value);
        }

        /// <summary>
        /// // DetailsAsync method
        /// </summary>
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_AsksForDetailsView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new BusinessTripDTO {
                Id = _id.Value,
                Name = "02.09.2018_026",
            });
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(2)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        /// <summary>
        /// // DeleteAsync_Get method
        /// </summary>
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_RetrievesBusinessTripFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new BusinessTripDTO {
                Id = _id.Value,
                Name = "02.09.2018_026"
            });
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(2)) as ViewResult;

            BusinessTripViewModel model = result.ViewData.Model as BusinessTripViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("02.09.2018_026", model.Name);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        /// <summary>
        /// // DeleteConfirmedAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmedAsync_Post_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmedAsync(1)) as RedirectToRouteResult;

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
        /// // DeleteAllAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Post_RedirectToIndex() {
            Mock<BusinessTripService> mock = new Mock<BusinessTripService>();
            BusinessTripController controller = GetNewBusinessTripController(mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteAllAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
    }
}
