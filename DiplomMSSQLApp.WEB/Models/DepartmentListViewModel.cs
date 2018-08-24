using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models
{
    public class DepartmentListViewModel
    {
        public IEnumerable<DepartmentViewModel> Departments { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}