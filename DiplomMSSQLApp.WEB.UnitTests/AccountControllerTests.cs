using DiplomMSSQLApp.WEB.Controllers;
using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Moq;
using NUnit.Framework;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests {
    [TestFixture]
    public class AccountControllerTests {
        protected AccountController GetNewAccountController(IAuthenticationManager authManager, AppUserManager userManager) {
            return new AccountController(authManager, userManager);
        }

        protected AccountController GetNewAccountControllerWithHttpContext(IAuthenticationManager authManager, AppUserManager userManager, bool isAuthenticated) {
            Mock<HttpContextBase> mockHttpContext = new Mock<HttpContextBase>();
            mockHttpContext.Setup(m => m.User.Identity.IsAuthenticated).Returns(isAuthenticated);
            return new AccountController(authManager, userManager) { ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object } };
        }

        /// <summary>
        /// // Login_Get method
        /// </summary>
        [Test]
        public void Login_Get_IsAuthenticatedPropertyIsTrue_AsksForErrorView() {
            AccountController controller = GetNewAccountControllerWithHttpContext(null, null, true);

            ViewResult result = controller.Login("") as ViewResult;

            Assert.AreEqual("Error", result.ViewName);
            string[] model = result.ViewData.Model as string[];
            Assert.AreEqual("Доступ закрыт", model[0]);
        }

        [Test]
        public void Login_Get_IsAuthenticatedPropertyIsFalse_AsksForLoginView() {
            AccountController controller = GetNewAccountControllerWithHttpContext(null, null, false);

            ViewResult result = controller.Login("") as ViewResult;

            Assert.AreEqual("Login", result.ViewName);
        }

        /// <summary>
        /// // Login_Post method
        /// </summary>
        [Test]
        public async Task Login_Post_ModelStateIsValidAndUserNotFound_AsksForLoginView() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(null as AppUser);
            AccountController controller = GetNewAccountController(null, userManager.Object);

            ViewResult result = (await controller.Login(new LoginModel { }, "")) as ViewResult;

            Assert.AreEqual("Login", result.ViewName);
        }

        [Test]
        public async Task Login_Post_ModelStateIsValidAndUserFound_RedirectToReturnUrl() {
            Mock<IUserStore<AppUser>> userStore = new Mock<IUserStore<AppUser>>();
            Mock<AppUserManager> userManager = new Mock<AppUserManager>(userStore.Object);
            userManager.Setup(m => m.FindAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new AppUser { });
            Mock<IAuthenticationManager> authManager = new Mock<IAuthenticationManager>();
            AccountController controller = GetNewAccountController(authManager.Object, userManager.Object);

            RedirectResult result = (await controller.Login(new LoginModel { }, "/")) as RedirectResult;

            Assert.AreEqual("/", result.Url);
        }

        [Test]
        public async Task Login_Post_ModelStateIsInvalid_AsksForLoginView() {
            AccountController controller = GetNewAccountController(null, null);
            controller.ModelState.AddModelError("", "Error");

            ViewResult result = (await controller.Login(new LoginModel { }, "")) as ViewResult;

            Assert.AreEqual("Login", result.ViewName);
        }

        /// <summary>
        /// // Logout method
        /// </summary>
        [Test]
        public void Logout_RedirectToIndex() {
            Mock<IAuthenticationManager> authManager = new Mock<IAuthenticationManager>();
            AccountController controller = GetNewAccountController(authManager.Object, null);

            RedirectToRouteResult result = controller.Logout() as RedirectToRouteResult;

            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("Organization", result.RouteValues["controller"]);
        }
    }
}
