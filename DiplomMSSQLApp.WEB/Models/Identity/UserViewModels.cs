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
}
