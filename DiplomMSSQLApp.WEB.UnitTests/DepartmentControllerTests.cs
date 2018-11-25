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
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class DepartmentControllerTests {
        protected DepartmentController GetNewDepartmentController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds, IService<OrganizationDTO> os) {
            return new DepartmentController(es, ds, os);
        }

        /// <summary>
        /// // IndexAsync method
        /// </summary>
        [Test]
        public async Task IndexAsync_AsksForIndexView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public async Task IndexAsync_RetrievesDepartmentsPropertyFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<DepartmentDTO>>(), It.IsAny<int>())).Returns(new DepartmentDTO[] {
                new DepartmentDTO {
                    Id = 1,
                    Code = 101,
                    DepartmentName = "IT"
                }
            });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            DepartmentListViewModel model = result.ViewData.Model as DepartmentListViewModel;
            Assert.AreEqual(1, model.Departments.Count());
            Assert.AreEqual(1, model.Departments.FirstOrDefault().Id);
            Assert.AreEqual(101, model.Departments.FirstOrDefault().Code);
            Assert.AreEqual("IT", model.Departments.FirstOrDefault().DepartmentName);
        }

        [Test]
        public async Task IndexAsync_RetrievesPageInfoPropertyFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            DepartmentListViewModel model = result.ViewData.Model as DepartmentListViewModel;
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
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            DepartmentController controller = GetNewDepartmentController(emock.Object, null, omock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagOrganizations() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            omock.Setup(m => m.GetAllAsync()).ReturnsAsync(new OrganizationDTO[] {
                new OrganizationDTO() { Id = 1, Name = "PFR" }
            });
            DepartmentController controller = GetNewDepartmentController(emock.Object, null, omock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Organizations as SelectList).FirstOrDefault();
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("PFR", item.Text);
        }

        [Test]
        public async Task CreateAsync_Get_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            DepartmentController controller = GetNewDepartmentController(emock.Object, null, omock.Object);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("Петров Петр Петрович", item.Text);
        }

        /// <summary>
        /// // CreateAsync_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            RedirectToRouteResult result = (await controller.CreateAsync(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsInvalid_AsksForCreateView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsInvalid_RetrievesDepartmentFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.CreateAsync(new DepartmentViewModel {
                Id = 2,
                Code = 101,
                DepartmentName = "IT"
            })) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual(101, model.Code);
            Assert.AreEqual("IT", model.DepartmentName);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsInvalid_SetViewBagOrganizations() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            omock.Setup(m => m.GetAllAsync()).ReturnsAsync(new OrganizationDTO[] {
                new OrganizationDTO() { Id = 1, Name = "PFR" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Organizations as SelectList).FirstOrDefault();
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("PFR", item.Text);
        }

        [Test]
        public async Task CreateAsync_Post_ModelStateIsInvalid_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("Петров Петр Петрович", item.Text);
        }

        /// <summary>
        /// // EditAsync_Get method
        /// </summary>
        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_AsksForEditView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_RetrievesDepartmentFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new DepartmentDTO {
                Id = _id.Value,
                Code = 101,
                DepartmentName = "IT"
            });
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(2)) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual(101, model.Code);
            Assert.AreEqual("IT", model.DepartmentName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        [Test]
        public async Task EditAsync_Get_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("Петров Петр Петрович", item.Text);
        }

        /// <summary>
        /// // EditAsync_Post method
        /// </summary>
        [Test]
        public async Task EditAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            RedirectToRouteResult result = (await controller.EditAsync(new DepartmentViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.EditAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(new DepartmentViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_RetrievesDepartmentFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            dmock.Setup(m => m.EditAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(new DepartmentViewModel {
                Id = 2,
                Code = 101,
                DepartmentName = "IT"
            })) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual(101, model.Code);
            Assert.AreEqual("IT", model.DepartmentName);
        }

        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<OrganizationService> omock = new Mock<OrganizationService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Петров", FirstName = "Петр", Patronymic = "Петрович" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.EditAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object, omock.Object);

            ViewResult result = (await controller.EditAsync(new DepartmentViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("1", item.Value);
            Assert.AreEqual("Петров Петр Петрович", item.Text);
        }

        /// <summary>
        /// // DetailsAsync method
        /// </summary>
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_AsksForDetailsView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsValid_RetrievesDepartmentFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new DepartmentDTO {
                Id = _id.Value,
                Code = 101,
                DepartmentName = "IT"
            });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(2)) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual(101, model.Code);
            Assert.AreEqual("IT", model.DepartmentName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_AsksForErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteAsync_Get method
        /// </summary>
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_RetrievesDepartmentFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new DepartmentDTO {
                Id = _id.Value,
                Code = 101,
                DepartmentName = "IT"
            });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(2)) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual(101, model.Code);
            Assert.AreEqual("IT", model.DepartmentName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_AsksForErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("FindByIdAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteConfirmedAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteConfirmedAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsInvalid_AsksForErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new ValidationException("DeleteAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteConfirmedAsync(1)) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new ValidationException("DeleteAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteConfirmedAsync(1)) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("DeleteAsync method throws Exception", model[0]);
        }

        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAllAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            RedirectToRouteResult result = (await controller.DeleteAllAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsNotValid_AsksForErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAllAsync()).Throws(new Exception(""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteAllAsync()) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsInvalid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAllAsync()).Throws(new Exception(""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object, null);

            ViewResult result = (await controller.DeleteAllAsync()) as ViewResult;

            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Нельзя удалить отдел, пока в нем есть хотя бы одна должность", model[0]);
        }
    }
}
