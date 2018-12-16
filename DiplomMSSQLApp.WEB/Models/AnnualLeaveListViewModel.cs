using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models {
    public class AnnualLeaveListViewModel {
        public IEnumerable<AnnualLeaveViewModel> AnnualLeaves { get; set; }
        public AnnualLeaveFilter Filter { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
