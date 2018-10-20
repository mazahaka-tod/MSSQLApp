﻿using DiplomMSSQLApp.BLL.DTO;
using DiplomMSSQLApp.BLL.Infrastructure;
using DiplomMSSQLApp.BLL.Interfaces;
using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Models;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class OrganizationControllerTests {
        protected OrganizationController GetNewOrganizationController(IService<OrganizationDTO> os) {
            return new OrganizationController(os);
        }

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public async Task Index_AsksForIndexView() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Index()) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public async Task Index_RetrievesIdAndNamePropertyFromModel() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            mock.Setup(m => m.GetFirstAsync()).ReturnsAsync(new OrganizationDTO() {
                Id = 1,
                Name = "GazProm"
            });
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Index()) as ViewResult;

            OrganizationViewModel model = result.ViewData.Model as OrganizationViewModel;
            Assert.AreEqual(1, model.Id);
            Assert.AreEqual("GazProm", model.Name);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_ModelStateIsValid_AsksForEditView() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsValid_RetrievesIdAndNameFromModel() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).ReturnsAsync((int? _id) => new OrganizationDTO {
                Id = _id.Value,
                Name = "GazProm"
            });
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Edit(2)) as ViewResult;

            OrganizationViewModel model = result.ViewData.Model as OrganizationViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("GazProm", model.Name);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsNotValid_AsksForCustomErrorView() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            Assert.AreEqual("CustomError", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_ModelStateIsNotValid_RetrievesExceptionMessageFromModel() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            mock.Setup(m => m.FindByIdAsync(It.IsAny<int?>())).Throws(new ValidationException("FindByIdAsync method throws Exception", ""));
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Edit(1)) as ViewResult;

            string model = result.ViewData.Model.ToString();
            Assert.AreEqual("FindByIdAsync method throws Exception", model);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelStateIsValid_RedirectToIndex() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            RedirectToRouteResult result = (await controller.Edit(new OrganizationViewModel())) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_AsksForEditView() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            mock.Setup(m => m.EditAsync(It.IsAny<OrganizationDTO>())).Throws(new ValidationException("", ""));
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Edit(new OrganizationViewModel())) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_RetrievesIdAndNameFromModel() {
            Mock<OrganizationService> mock = new Mock<OrganizationService>();
            mock.Setup(m => m.EditAsync(It.IsAny<OrganizationDTO>())).Throws(new ValidationException("", ""));
            OrganizationController controller = GetNewOrganizationController(mock.Object);

            ViewResult result = (await controller.Edit(new OrganizationViewModel {
                Id = 2,
                Name = "GazProm"
            })) as ViewResult;

            OrganizationViewModel model = result.ViewData.Model as OrganizationViewModel;
            Assert.AreEqual(2, model.Id);
            Assert.AreEqual("GazProm", model.Name);
        }
    }
}