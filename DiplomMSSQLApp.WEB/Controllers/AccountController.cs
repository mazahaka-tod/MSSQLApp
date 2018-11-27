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
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private IAuthenticationManager _authManager;
        private AppUserManager _userManager;

        public IAuthenticationManager AuthManager {
            get { return _authManager ?? HttpContext.GetOwinContext().Authentication; }
            set { _authManager = value; }
        }

        public AppUserManager UserManager {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>(); }
            set { _userManager = value; }
        }

        public AccountController() { }

        public AccountController(IAuthenticationManager authManager, AppUserManager userManager) {
            AuthManager = authManager;
            UserManager = userManager;
        }

        /// <summary>
        /// // Login method
        /// </summary>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl) {
            if (HttpContext.User.Identity.IsAuthenticated) {
                _logger.Warn("Access Denied");
                return View("Error", new string[] { "Доступ закрыт" });
            }
            ViewBag.returnUrl = returnUrl;
            return View("Login");
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model, string returnUrl) {
            if (ModelState.IsValid) {
                AppUser user = await UserManager.FindAsync(model.Name, model.Password);
                if (user == null) {
                    _logger.Warn("Invalid name or password");
                    ModelState.AddModelError("", "Неверное имя или пароль");
                }
                else {
                    ClaimsIdentity ident = await UserManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                    AuthManager.SignOut();
                    AuthManager.SignIn(new AuthenticationProperties { IsPersistent = false }, ident);
                    _logger.Info("Succeeded");
                    return Redirect(returnUrl);
                }
            }
            _logger.Warn("ModelState is invalid");
            ViewBag.returnUrl = returnUrl;
            return View("Login", model);
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
