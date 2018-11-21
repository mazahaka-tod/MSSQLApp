using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models {
    public class DepartmentViewModel {
        public int Id { get; set; }
        [Required(ErrorMessage = "Требуется ввести код отдела")]
        [Display(Name = "Код отдела")]
        public int Code { get; set; }
        [Required(ErrorMessage = "Требуется ввести название отдела")]
        [Display(Name = "Название отдела")]
        public string DepartmentName { get; set; }
        [Display(Name = "Организация")]
        public int? OrganizationId { get; set; }
        public OrganizationViewModel Organization { get; set; }
        [Display(Name = "Начальник отдела")]
        public int? ManagerId { get; set; }
        public EmployeeViewModel Manager { get; set; }
        [Display(Name = "Список должностей")]
        public virtual ICollection<PostViewModel> Posts { get; set; }

        public DepartmentViewModel() {
            Posts = new List<PostViewModel>();
        }
    }
}
