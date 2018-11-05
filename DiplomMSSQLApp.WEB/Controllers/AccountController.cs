using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NLog;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize]
    public class AccountController : Controller {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private IAuthenticationManager AuthManager {
            get { return HttpContext.GetOwinContext().Authentication; }
        }

        private AppUserManager UserManager {
            get { return HttpContext.GetOwinContext().GetUserManager<AppUserManager>(); }
        }

        /// <summary>
        /// // Login method
        /// </summary>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl) {
            if (HttpContext.User.Identity.IsAuthenticated) {
                logger.Warn("Access Denied");
                return View("Error", new string[] { "Доступ закрыт" });
            }
            ViewBag.returnUrl = returnUrl;
            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl) {
            if (ModelState.IsValid) {
                AppUser user = await UserManager.FindAsync(model.Name, model.Password);
                if (user == null) {
                    logger.Warn("Invalid name or password");
                    ModelState.AddModelError("", "Неверное имя или пароль");
                }
                else {
                    ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    AuthManager.SignOut();
                    AuthManager.SignIn(new AuthenticationProperties { IsPersistent = false }, ident);
                    logger.Info("Succeeded");
                    return Redirect(returnUrl);
                }
            }
            logger.Warn("ModelState is invalid");
            ViewBag.returnUrl = returnUrl;
            return View(model);
        }

        /// <summary>
        /// // Logout method
        /// </summary>
        public ActionResult Logout() {
            AuthManager.SignOut();
            return RedirectToAction("Index", "Organization");
        }
    }
}
