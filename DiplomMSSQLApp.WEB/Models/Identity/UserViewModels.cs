using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models.Identity {
    public class CreateModel {
        [Required(ErrorMessage = "Требуется ввести имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Требуется ввести Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Требуется ввести пароль")]
        public string Password { get; set; }
    }

    public class EditModel {
        [Required]
        public string Id { get; set; }
        [Required(ErrorMessage = "Требуется ввести имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Требуется ввести Email")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Требуется ввести пароль")]
        public string Password { get; set; }
    }

    public class LoginModel {
        [Required(ErrorMessage = "Требуется ввести имя")]
        [Display(Name = "Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Требуется ввести пароль")]
        [Display(Name = "Пароль")]
        public string Password { get; set; }
    }

    public class RoleEditModel {
        public AppRole Role { get; set; }
        public IEnumerable<AppUser> Members { get; set; }
        public IEnumerable<AppUser> NonMembers { get; set; }
    }

    public class RoleModificationModel {
        [Required(ErrorMessage = "Требуется ввести название роли")]
        public string RoleName { get; set; }
        public string[] IdsToAdd { get; set; }
        public string[] IdsToDelete { get; set; }
    }
}
