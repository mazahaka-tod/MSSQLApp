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
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class DepartmentControllerTests {
        protected DepartmentController GetNewDepartmentController(IService<EmployeeDTO> es, IService<DepartmentDTO> ds) {
            return new DepartmentController(es, ds);
        }

        /// <summary>
        /// // IndexAsync method
        /// </summary>
        [Test]
        public async Task IndexAsync_AsksForIndexView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }
        
        [Test]
        public async Task IndexAsync_RetrievesDepartmentsPropertyFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.GetPage(It.IsAny<IEnumerable<DepartmentDTO>>(), It.IsAny<int>())).Returns(new DepartmentDTO[] {
                new DepartmentDTO {
                    Id = 1,
                    DepartmentName = "IT",
                    Manager = "Brown"
                }
            });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.IndexAsync()) as ViewResult;

            DepartmentListViewModel model = result.ViewData.Model as DepartmentListViewModel;
            Assert.AreEqual(1, model.Departments.Count());
            Assert.AreEqual(1, model.Departments.FirstOrDefault().Id);
            Assert.AreEqual("IT", model.Departments.FirstOrDefault().DepartmentName);
            Assert.AreEqual("Brown", model.Departments.FirstOrDefault().Manager);
        }
        
        [Test]
        public async Task IndexAsync_RetrievesPageInfoPropertyFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.PageInfo).Returns(new PageInfo() { TotalItems = 9, PageSize = 3, PageNumber = 3 });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

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
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            DepartmentController controller = GetNewDepartmentController(mock.Object, null);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }
        
        [Test]
        public async Task CreateAsync_Get_SetViewBagEmployees() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            mock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Brown", FirstName = "Daniel" }
            });
            DepartmentController controller = GetNewDepartmentController(mock.Object, null);

            ViewResult result = (await controller.CreateAsync()) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("Brown Daniel", item.Value);
        }
        
        /// <summary>
        /// // CreateAsync_Post method
        /// </summary>
        [Test]
        public async Task CreateAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            RedirectToRouteResult result = (await controller.CreateAsync(null)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }
        
        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_RetrievesDepartmentFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.CreateAsync(new DepartmentViewModel {
                Id = 2,
                DepartmentName = "IT",
                Manager = "Brown"
            })) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("IT", model.DepartmentName);
            Assert.AreEqual("Brown", model.Manager);
        }
        
        [Test]
        public async Task CreateAsync_Post_ModelStateIsNotValid_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Brown", FirstName = "Daniel" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.CreateAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.CreateAsync(null)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("Brown Daniel", item.Value);
        }
        
        /// <summary>
        /// // EditAsync_Get method
        /// </summary>
        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_AsksForEditView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task EditAsync_Get_ModelStateIsValid_RetrievesDepartmentFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new DepartmentDTO {
                Id = _id.Value,
                DepartmentName = "IT",
                Manager = "Brown"
            });
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(2)) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("IT", model.DepartmentName);
            Assert.AreEqual("Brown", model.Manager);
        }
        
        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }
        
        [Test]
        public async Task EditAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }
        
        [Test]
        public async Task EditAsync_Get_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Brown", FirstName = "Daniel" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(1)) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("Brown Daniel", item.Value);
        }

        /// <summary>
        /// // EditAsync_Post method
        /// </summary>
        [Test]
        public async Task EditAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            RedirectToRouteResult result = (await controller.EditAsync(new DepartmentViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }
        
        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.EditAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(new DepartmentViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }
        
        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_RetrievesDepartmentFromModel() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.EditAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(new DepartmentViewModel {
                Id = 2,
                DepartmentName = "IT",
                Manager = "Brown"
            })) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("IT", model.DepartmentName);
            Assert.AreEqual("Brown", model.Manager);
        }
        
        [Test]
        public async Task EditAsync_Post_ModelStateIsNotValid_SetViewBagEmployees() {
            Mock<EmployeeService> emock = new Mock<EmployeeService>();
            emock.Setup(m => m.GetAllAsync()).ReturnsAsync(new EmployeeDTO[] {
                new EmployeeDTO() { Id = 1, LastName = "Brown", FirstName = "Daniel" }
            });
            Mock<DepartmentService> dmock = new Mock<DepartmentService>();
            dmock.Setup(m => m.EditAsync(It.IsAny<DepartmentDTO>())).Throws(new ValidationException("", ""));
            DepartmentController controller = GetNewDepartmentController(emock.Object, dmock.Object);

            ViewResult result = (await controller.EditAsync(new DepartmentViewModel())) as ViewResult;

            SelectListItem item = (result.ViewBag.Employees as SelectList).FirstOrDefault();
            Assert.AreEqual("Brown Daniel", item.Text);
            Assert.AreEqual("Brown Daniel", item.Value);
        }

        /// <summary>
        /// // DetailsAsync method
        /// </summary>
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_AsksForDetailsView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("Details", result.ViewName);
        }
        
        [Test]
        public async Task DetailsAsync_ModelStateIsValid_RetrievesDepartmentFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new DepartmentDTO {
                Id = _id.Value,
                DepartmentName = "IT",
                Manager = "Brown"
            });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DetailsAsync(2)) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("IT", model.DepartmentName);
            Assert.AreEqual("Brown", model.Manager);
        }
        
        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }
        
        [Test]
        public async Task DetailsAsync_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DetailsAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }
        
        /// <summary>
        /// // DeleteAsync_Get method
        /// </summary>
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_AsksForDeleteView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("Delete", result.ViewName);
        }
        
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsValid_RetrievesDepartmentFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new DepartmentDTO {
                Id = _id.Value,
                DepartmentName = "IT",
                Manager = "Brown"
            });
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteAsync(2)) as ViewResult;

            DepartmentViewModel model = result.ViewData.Model as DepartmentViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("IT", model.DepartmentName);
            Assert.AreEqual("Brown", model.Manager);
        }
        
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }
        
        [Test]
        public async Task DeleteAsync_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        /// <summary>
        /// // DeleteConfirmedAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            RedirectToRouteResult result = (await controller.DeleteConfirmedAsync(1)) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new Exception(""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteConfirmedAsync(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task DeleteConfirmedAsync_Post_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAsync(It.IsAny<int>())).Throws(new Exception(""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteConfirmedAsync(1)) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("Нельзя удалить отдел, пока в нем работает хотя бы один сотрудник.", model);
        }
        
        /// <summary>
        /// // DeleteAll_Get method
        /// </summary>
        [Test]
        public void DeleteAll_Get_AsksForDeleteAllView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = controller.DeleteAll() as ViewResult;

            Assert.AreEqual("DeleteAll", result.ViewName);
        }

        /// <summary>
        /// // DeleteAllAsync_Post method
        /// </summary>
        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            RedirectToRouteResult result = (await controller.DeleteAllAsync()) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAllAsync()).Throws(new Exception(""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteAllAsync()) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task DeleteAllAsync_Post_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<DepartmentService> mock = new Mock<DepartmentService>();
            mock.Setup(m => m.DeleteAllAsync()).Throws(new Exception(""));
            DepartmentController controller = GetNewDepartmentController(null, mock.Object);

            ViewResult result = (await controller.DeleteAllAsync()) as ViewResult;

            string model = result.ViewData.Model as string;
            Assert.AreEqual("Нельзя удалить отдел, пока в нем работает хотя бы один сотрудник.", model);
        }
    }
}
