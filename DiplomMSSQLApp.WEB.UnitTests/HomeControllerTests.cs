using DiplomMSSQLApp.BLL.Services;
using DiplomMSSQLApp.WEB.Controllers;
using Moq;
using NUnit.Framework;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.UnitTests
{
    [TestFixture]
    public class HomeControllerTests {

        /// <summary>
        /// // Index method
        /// </summary>
        [Test]
        public void Index_Get_AsksForIndexView() {
            Mock<EmployeeService> mock = new Mock<EmployeeService>();
            Mock<HttpServerUtilityBase> serverMock = new Mock<HttpServerUtilityBase>();
            serverMock.Setup(m => m.MapPath(It.IsAny<string>())).Returns("/");
            Mock<HttpContextBase> httpCtxStub = new Mock<HttpContextBase>();
            httpCtxStub.Setup(m => m.Server).Returns(serverMock.Object);
            ControllerContext controllerCtx = new ControllerContext {
                HttpContext = httpCtxStub.Object    // mocking Server.MapPath method
            };
            HomeController controller = new HomeController(mock.Object, null, null) { ControllerContext = controllerCtx };

            ViewResult result = controller.Index(null, null) as ViewResult;

            Assert.AreEqual("Index", result.ViewName);
        }
    }
}
