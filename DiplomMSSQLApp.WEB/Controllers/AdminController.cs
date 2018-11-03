using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    public class AdminController : Controller {
        private AppUserManager UserManager {
            get { return HttpContext.GetOwinContext().GetUserManager<AppUserManager>(); }
        }

        // Index
        public ActionResult Index() {
            return View(UserManager.Users);
        }

        // Create
        public ActionResult Create() {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(CreateModel model) {
            if (ModelState.IsValid) {
                AppUser user = new AppUser { UserName = model.Name, Email = model.Email };
                IdentityResult result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded) {
                    return RedirectToAction("Index");
                }
                else {
                    AddErrorsFromResult(result);
                }
            }
            return View(model);
        }

        private void AddErrorsFromResult(IdentityResult result) {
            foreach (string error in result.Errors) {
                ModelState.AddModelError("", error);
            }
        }

        // Edit
        public async Task<ActionResult> Edit(string id) {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null) {
                return View(new EditModel { Id = user.Id, Name = user.UserName, Email = user.Email });
            }
            else {
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
                        return RedirectToAction("Index");
                    }
                    else {
                        AddErrorsFromResult(result);
                    }
                }
            }
            else {
                ModelState.AddModelError("", "Пользователь не найден");
            }
            return View(new EditModel { Id = user.Id, Name = user.UserName, Email = user.Email });
        }
        // Delete
        [HttpPost]
        public async Task<ActionResult> Delete(string id) {
            AppUser user = await UserManager.FindByIdAsync(id);
            if (user != null) {
                IdentityResult result = await UserManager.DeleteAsync(user);
                if (result.Succeeded) {
                    return RedirectToAction("Index");
                }
                else {
                    return View("Error", result.Errors);
                }
            }
            else {
                return View("Error", new string[] { "Пользователь не найден" });
            }
        }
    }
}
