using DiplomMSSQLApp.WEB.Infrastructure;
using DiplomMSSQLApp.WEB.Models.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace DiplomMSSQLApp.WEB.Controllers {
    [HandleError]
    [Authorize(Roles = "Administrators")]
    public class RoleAdminController : Controller {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private AppUserManager _userManager;
        private AppRoleManager _roleManager;

        private AppUserManager UserManager {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>(); }
            set { _userManager = value; }
        }

        private AppRoleManager RoleManager {
            get { return _roleManager ?? HttpContext.GetOwinContext().GetUserManager<AppRoleManager>(); }
            set { _roleManager = value; }
        }

        public RoleAdminController() { }

        public RoleAdminController(AppUserManager userManager, AppRoleManager roleManager) {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        /// <summary>
        /// // Index method
        /// </summary>
        public ActionResult Index() {
            return View("Index", RoleManager.Roles);
        }

        /// <summary>
        /// // Create method
        /// </summary>
        public ActionResult Create() {
            return View("Create");
        }
        [HttpPost]
        public async Task<ActionResult> Create([Required]string name) {
            if (ModelState.IsValid) {
                IdentityResult result = await RoleManager.CreateAsync(new AppRole(name));
                if (result.Succeeded) {
                    _logger.Info("Succeeded");
                    return RedirectToAction("Index");
                }
                else {
                    foreach (string error in result.Errors) {
                        _logger.Warn(error);
                        ModelState.AddModelError("", error);
                    }
                    return View("Create", name);
                }
            }
            _logger.Warn("ModelState is invalid");
            return View("Create", name);
        }

        /// <summary>
        /// // Edit method
        /// </summary>
        public async Task<ActionResult> Edit(string id) {
            AppRole role = await RoleManager.FindByIdAsync(id);
            if (role == null) {
                _logger.Warn("Role not found");
                return View("Error", new string[] { "Роль не найдена" });
            }
            string[] memberIDs = role.Users.Select(x => x.UserId).ToArray();
            IEnumerable<AppUser> members = UserManager.Users.Where(x => memberIDs.Any(y => y == x.Id));
            IEnumerable<AppUser> nonMembers = UserManager.Users.Except(members);
            return View("Edit", new RoleEditModel {
                Role = role,
                Members = members,
                NonMembers = nonMembers
            });
        }
        [HttpPost]
        public async Task<ActionResult> Edit(RoleModificationModel model) {
            IdentityResult result;
            if (ModelState.IsValid) {
                foreach (string userId in model.IdsToAdd ?? new string[] { }) {
                    result = await UserManager.AddToRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded) {
                        foreach (string error in result.Errors) {
                            _logger.Warn(error);
                        }
                        return View("Error", result.Errors);
                    }
                }
                foreach (string userId in model.IdsToDelete ?? new string[] { }) {
                    result = await UserManager.RemoveFromRoleAsync(userId, model.RoleName);
                    if (!result.Succeeded) {
                        foreach (string error in result.Errors) {
                            _logger.Warn(error);
                        }
                        return View("Error", result.Errors);
                    }
                }
                _logger.Info("Succeeded");
                return RedirectToAction("Index");
            }
            _logger.Warn("Role not found");
            return View("Error", new string[] { "Роль не найдена" });
        }

        /// <summary>
        /// // Delete method
        /// </summary>
        [HttpPost]
        public async Task<ActionResult> Delete(string id) {
            AppRole role = await RoleManager.FindByIdAsync(id);
            if (role != null) {
                IdentityResult result = await RoleManager.DeleteAsync(role);
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
                _logger.Warn("Role not found");
                return View("Error", new string[] { "Роль не найдена" });
            }
        }
    }
}
