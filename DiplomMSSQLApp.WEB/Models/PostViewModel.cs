using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models
{
    public class PostViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Название должности")]
        public string Title { get; set; }
        [Display(Name = "Минимальная зарплата")]
        public int MinSalary { get; set; }
        [Display(Name = "Максимальная зарплата")]
        public int MaxSalary { get; set; }
        [Display(Name = "Список сотрудников")]
        public virtual ICollection<EmployeeViewModel> Employees { get; set; }

        public PostViewModel()
        {
            Employees = new List<EmployeeViewModel>();
        }
    }
}