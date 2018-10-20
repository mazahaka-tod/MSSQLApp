using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models {
    public class OrganizationViewModel {
        public int Id { get; set; }
        [Required(ErrorMessage = "Требуется ввести наименование организации")]
        [Display(Name = "Наименование")]
        public string Name { get; set; }
        [Display(Name = "Юридический адрес")]
        public string LegalAddress { get; set; }
        [Display(Name = "Фактический адрес")]
        public string ActualAddress { get; set; }
        [Display(Name = "Телефон")]
        public string Phone { get; set; }
        [Display(Name = "Факс")]
        public string Fax { get; set; }
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }
    }
}
