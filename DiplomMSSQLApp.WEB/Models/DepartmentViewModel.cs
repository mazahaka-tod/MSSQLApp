using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models
{
    public class DepartmentViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Название отдела")]
        public string DepartmentName { get; set; }
        [Display(Name = "Начальник отдела")]
        public string Manager { get; set; }
        [Display(Name = "Список сотрудников")]
        public virtual ICollection<EmployeeViewModel> Employees { get; set; }

        public DepartmentViewModel()
        {
            Employees = new List<EmployeeViewModel>();
        }
    }
}