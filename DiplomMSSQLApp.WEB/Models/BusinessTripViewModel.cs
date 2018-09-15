using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models
{
    public class BusinessTripViewModel : BaseBusinessTripViewModel {
        [Display(Name = "Список сотрудников")]
        public virtual ICollection<EmployeeViewModel> Employees { get; set; }

        public BusinessTripViewModel()
        {
            Employees = new List<EmployeeViewModel>();
        }
    }
}