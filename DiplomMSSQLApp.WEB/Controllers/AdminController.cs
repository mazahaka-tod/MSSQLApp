using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize(Roles = "Administrators")]
    public class AdminController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private AppUserManager _userManager;

        public AppUserManager UserManager {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>(); }
            private set { _userManager = value; }
        }

        public AdminController() { }

        public AdminController(AppUserManager userManager) {
            UserManager = userManager;
        }

        /// <summary>
        /// // Index method
        /// </summary>
        public ActionResult Index() {
            return View("Index", UserManager.Users);
        }

        /// <summary>
        /// // Create method
        /// </summary>
        public ActionResult Create() {
            return View("Create");
        }
        [HttpPost]
        public async Task<ActionResult> Create(CreateModel model) {
            if (ModelState.IsValid) {
                AppUser user = new AppUser { UserName = model.Name, Email = model.Email };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded) {
                    _logger.Info("Succeeded");
                    return RedirectToAction("Index");
                }
                else {
                    AddErrorsFromResult(result);
                }
            }
            _logger.Warn("ModelState is invalid");
            return View("Create", model);
        }

        private void AddErrorsFromResult(IdentityResult result) {
            foreach (string error in result.Errors) {
                _logger.Warn(error);
                ModelState.AddModelError("", error);
            }
        }

        /// <summary>
        /// // Edit method
        /// </summary>
        public async Task<ActionResult> Edit(string id) {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null) {
                return View("Edit", new EditModel { Id = user.Id, Name = user.UserName, Email = user.Email });
            }
            else {
                _logger.Warn("User not found");
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<ActionResult> Edit(EditModel model) {
            AppUser user = await UserManager.FindByIdAsync(model.Id);
            if (user != null) {
                user.UserName = model.Name;
                user.Email = model.Email;
                IdentityResult validUser = await UserManager.UserValidator.ValidateAsync(user);
                if (!validUser.Succeeded) {
                    AddErrorsFromResult(validUser);
                }
                IdentityResult validPass = null;
                if (model.Password != string.Empty) {
                    validPass = await UserManager.PasswordValidator.ValidateAsync(model.Password);
                    if (validPass.Succeeded) {
                        user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.Password);
                    }
                    else {
                        AddErrorsFromResult(validPass);
                    }
                }
                if ((validUser.Succeeded && validPass == null) || (validUser.Succeeded && model.Password != string.Empty && validPass.Succeeded)) {
                    IdentityResult result = await UserManager.UpdateAsync(user);
                    if (result.Succeeded) {
                        _logger.Info("Succeeded");
                        return RedirectToAction("Index");
                    }
                    else {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else {
                _logger.Warn("User not found");
                return View("Error", new string[] { "Пользователь не найден" });
            }
            return View("Edit", new EditModel { Id = user.Id, Name = user.UserName, Email = user.Email });
        }

        /// <summary>
        /// // Delete method
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Delete(string id) {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null) {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded) {
                    _logger.Info("Succeeded");
                    return RedirectToAction("Index");
                }
                else {
                    foreach (string error in result.Errors) {
                        _logger.Warn(error);
                    }
                    return View("Error", result.Errors);
                }
            }
            else {
                _logger.Warn("User not found");
                return View("Error", new string[] { "Пользователь не найден" });
            }
        }
    }
}
