using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class RoleAdminControllerTests {
        protected RoleAdminController GetNewRoleAdminController(AppUserManager userManager, AppRoleManager roleManager) {
            return new RoleAdminController(userManager, roleManager);
        }

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public void Index_AsksForIndexView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            AppRoleManager roleManager = new AppRoleManager(roleStore.Object);
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager);

            ViewResult result = controller.Index() as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public void Index_RetrievesNamePropertyFromModel() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock <AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.Roles).Returns(new AppRole[] { new AppRole { Name = "Users" } }.AsQueryable());
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            ViewResult result = controller.Index() as ViewResult;

            IQueryable<AppRole> model = result.ViewData.Model as IQueryable<AppRole>;
            Assert.AreEqual(1, model.Count());
            Assert.AreEqual("Users", model.ToArray()[0].Name);
        }

        /// <summary>
        /// // Create_Get method
        /// </summary>
        [Test]
        public void Create_Get_AsksForCreateView() {
            RoleAdminController controller = GetNewRoleAdminController(null, null);

            ViewResult result = controller.Create() as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        /// <summary>
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task Create_Post_ModelStateIsValidAndIdentityResultSucceeded_RedirectToIndex() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.CreateAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Success);
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            RedirectToRouteResult result = (await controller.Create("")) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Create_Post_ModelStateIsValidAndIdentityResultNotSucceeded_AsksForCreateView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.CreateAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Failed(""));
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            ViewResult result = (await controller.Create("")) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelStateIsValidAndIdentityResultNotSucceeded_AddsErrorToModelState() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.CreateAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Failed("Custom Error"));
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            ViewResult result = (await controller.Create("")) as ViewResult;

            Assert.AreEqual(1, controller.ModelState.Count);
            Assert.AreEqual("Custom Error", controller.ModelState.Values.First().Errors.First().ErrorMessage);
        }

        [Test]
        public async Task Create_Post_ModelStateIsNotValid_AsksForCreateView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);
            controller.ModelState.AddModelError("", "Error");

            ViewResult result = (await controller.Create("")) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_RoleNotFound_AsksForErrorView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as AppRole);
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            ViewResult result = (await controller.Edit("")) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_RoleFound_AsksForEditView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppRole { });
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            RoleAdminController controller = GetNewRoleAdminController(userManager.Object, roleManager.Object);

            ViewResult result = (await controller.Edit("")) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_RoleFound_RetrievesRoleNamePropertyFromModel() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppRole { Name = "Users" });
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            RoleAdminController controller = GetNewRoleAdminController(userManager.Object, roleManager.Object);

            ViewResult result = (await controller.Edit("")) as ViewResult;

            RoleEditModel model = result.ViewData.Model as RoleEditModel;
            Assert.AreEqual("Users", model.Role.Name);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_ModelStateIsValid_RedirectToIndex() {
            RoleAdminController controller = GetNewRoleAdminController(null, null);

            RedirectToRouteResult result = (await controller.Edit(new RoleModificationModel { })) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsNotValid_AsksForErrorView() {
            RoleAdminController controller = GetNewRoleAdminController(null, null);
            controller.ModelState.AddModelError("", "Error");

            ViewResult result = (await controller.Edit(new RoleModificationModel { })) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Роль не найдена", model[0]);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsValidAndAddIdentityResultNotSucceeded_AsksForErrorView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.AddToRoleAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed("Add Error"));
            RoleAdminController controller = GetNewRoleAdminController(userManager.Object, null);

            ViewResult result = (await controller.Edit(new RoleModificationModel { IdsToAdd = new string[] { "1" } })) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Add Error", model[0]);
        }

        [Test]
        public async Task Edit_Post_ModelStateIsValidAndRemoveIdentityResultNotSucceeded_AsksForErrorView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed("Remove Error"));
            RoleAdminController controller = GetNewRoleAdminController(userManager.Object, null);

            ViewResult result = (await controller.Edit(new RoleModificationModel { IdsToDelete = new string[] { "1" } })) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Remove Error", model[0]);
        }

        /// <summary>
        /// // Delete method
        /// </summary>
        [Test]
        public async Task Delete_RoleFoundAndIdentityResultSucceeded_RedirectToIndex() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppRole { });
            roleManager.Setup(m => m.DeleteAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Success);
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            RedirectToRouteResult result = (await controller.Delete("")) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Delete_RoleFoundAndIdentityResultNotSucceeded_AsksForErrorView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppRole { });
            roleManager.Setup(m => m.DeleteAsync(It.IsAny<AppRole>())).ReturnsAsync(IdentityResult.Failed("Delete Error"));
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            ViewResult result = (await controller.Delete("")) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Delete Error", model[0]);
        }

        [Test]
        public async Task Delete_RoleNotFound_AsksForErrorView() {
            Mock<RoleStore<AppRole>> roleStore = new Mock<RoleStore<AppRole>>();
            Mock<AppRoleManager> roleManager = new Mock<AppRoleManager>(roleStore.Object);
            roleManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as AppRole);
            RoleAdminController controller = GetNewRoleAdminController(null, roleManager.Object);

            ViewResult result = (await controller.Delete("")) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Роль не найдена", model[0]);
        }
    }
}
