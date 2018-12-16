using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models {
    public class LeaveScheduleListViewModel {
        public IEnumerable<LeaveScheduleViewModel> LeaveSchedules { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}
