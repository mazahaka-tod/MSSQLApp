using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models {
    public class PostViewModel {
        public int Id { get; set; }
        [Required(ErrorMessage = "Требуется ввести название должности")]
        [Display(Name = "Название должности")]
        public string Title { get; set; }
        [Required(ErrorMessage = "Требуется ввести количество штатных единиц")]
        [Range(0, 10000, ErrorMessage = "Значение должно быть в диапазоне [0, 10000]")]
        [Display(Name = "Количество штатных единиц")]
        public int? NumberOfUnits { get; set; }
        [Required(ErrorMessage = "Требуется ввести оклад")]
        [Range(0, 1000000, ErrorMessage = "Оклад должен быть в диапазоне [0, 1000000]")]
        [Display(Name = "Оклад, руб.")]
        public double? Salary { get; set; }
        [Required(ErrorMessage = "Требуется ввести надбавку")]
        [Range(0, 100000, ErrorMessage = "Надбавка должна быть в диапазоне [0, 100000]")]
        [Display(Name = "Надбавка, руб.")]
        public double? Premium { get; set; }
        [Display(Name = "Всего, руб.")]
        public double TotalSalary {
            get { return (Salary.Value + Premium.Value) * NumberOfUnits.Value; }
        }
        [Display(Name = "Структурное подразделение")]
        public int? DepartmentId { get; set; }
        public DepartmentViewModel Department { get; set; }
        [Display(Name = "Список сотрудников")]
        public virtual ICollection<EmployeeViewModel> Employees { get; set; }

        public PostViewModel() {
            Employees = new List<EmployeeViewModel>();
        }
    }
}
