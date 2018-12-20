using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class AdminControllerTests {
        protected AdminController GetNewAdminController(AppUserManager userManager) {
            return new AdminController(userManager);
        }

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public void Index_AsksForIndexView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.Users);
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = controller.Index() as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }

        [Test]
        public void Index_RetrievesEmailPropertyFromModel() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.Users).Returns(new AppUser[] { new AppUser { Email = "bob@mail.ru" } }.AsQueryable);
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = controller.Index() as ViewResult;

            IQueryable<AppUser> model = result.ViewData.Model as IQueryable<AppUser>;
            Assert.AreEqual(1, model.Count());
            Assert.AreEqual("bob@mail.ru", model.ToArray()[0].Email);
        }

        /// <summary>
        /// // Create_Get method
        /// </summary>
        [Test]
        public void Create_Get_AsksForCreateView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = controller.Create() as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        /// <summary>
        /// // Create_Post method
        /// </summary>
        [Test]
        public async Task Create_Post_ModelStateIsValidAndIdentityResultSucceeded_RedirectToIndex() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            AdminController controller = GetNewAdminController(userManager.Object);

            RedirectToRouteResult result = (await controller.Create(new CreateModel { })) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Create_Post_ModelStateIsInvalid_AsksForCreateView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            AdminController controller = GetNewAdminController(userManager.Object);
            controller.ModelState.AddModelError("", "Error");

            ViewResult result = (await controller.Create(new CreateModel { })) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelStateIsValidAndIdentityResultNotSucceeded_AsksForCreateView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed(""));
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Create(new CreateModel { })) as ViewResult;

            Assert.AreEqual("Create", result.ViewName);
        }

        [Test]
        public async Task Create_Post_ModelStateIsValidAndIdentityResultNotSucceeded_AddsErrorToModelState() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Failed("Custom Error"));
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Create(new CreateModel { })) as ViewResult;

            Assert.AreEqual(1, controller.ModelState.Count);
            Assert.AreEqual("Custom Error", controller.ModelState.Values.First().Errors.First().ErrorMessage);
        }

        /// <summary>
        /// // Edit_Get method
        /// </summary>
        [Test]
        public async Task Edit_Get_UserFound_AsksForEditView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { });
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Edit("")) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Get_UserFound_RetrievesEmailPropertyFromModel() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { Email = "bob@mail.ru" });
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Edit("")) as ViewResult;

            EditModel model = result.ViewData.Model as EditModel;
            Assert.AreEqual("bob@mail.ru", model.Email);
        }

        [Test]
        public async Task Edit_Get_UserNotFound_RedirectToIndex() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as AppUser);
            AdminController controller = GetNewAdminController(userManager.Object);

            RedirectToRouteResult result = (await controller.Edit("")) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        /// <summary>
        /// // Edit_Post method
        /// </summary>
        [Test]
        public async Task Edit_Post_UserFoundAndIdentityResultSucceeded_RedirectToIndex() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { });
            Mock<UserValidator<AppUser>> userValidator = new Mock<UserValidator<AppUser>>();
            userValidator.Setup(m => m.ValidateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
            AdminController controller = GetNewAdminController(userManager.Object);

            RedirectToRouteResult result = (await controller.Edit(
                new EditModel { Name = "Bob", Email = "bob@mail.ru", Password = "" })) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Edit_Post_UserFoundAndIdentityResultNotSucceeded_AsksForEditView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { });
            Mock<UserValidator<AppUser>> userValidator = new Mock<UserValidator<AppUser>>();
            userValidator.Setup(m => m.ValidateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.UpdateAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Edit(new EditModel { Password = "" })) as ViewResult;

            Assert.AreEqual("Edit", result.ViewName);
        }

        [Test]
        public async Task Edit_Post_UserNotFound_AsksForEditView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as AppUser);
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Edit(new EditModel { })) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        /// <summary>
        /// // Delete method
        /// </summary>
        [Test]
        public async Task Delete_UserFoundAndIdentityResultSucceeded_RedirectToIndex() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { });
            userManager.Setup(m => m.DeleteAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Success);
            AdminController controller = GetNewAdminController(userManager.Object);

            RedirectToRouteResult result = (await controller.Delete("")) as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
        }

        [Test]
        public async Task Delete_UserFoundAndIdentityResultNotSucceeded_AsksForErrorView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new AppUser { });
            userManager.Setup(m => m.DeleteAsync(It.IsAny<AppUser>())).ReturnsAsync(IdentityResult.Failed(""));
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Delete("")) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }

        [Test]
        public async Task Delete_UserNotFound_AsksForErrorView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as AppUser);
            AdminController controller = GetNewAdminController(userManager.Object);

            ViewResult result = (await controller.Delete("")) as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
        }
    }
}
