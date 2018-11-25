using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models {
    public class EmployeeListViewModel {
        public IEnumerable<EmployeeViewModel> Employees { get; set; }
        public EmployeeFilter Filter { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
