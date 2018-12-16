using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO {
    public class LeaveScheduleDTO {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }
        public DateTime? DateOfPreparation { get; set; }
        public DateTime? DateOfApproval { get; set; }

        public virtual ICollection<AnnualLeaveDTO> AnnualLeaves { get; set; }
        public LeaveScheduleDTO() {
            AnnualLeaves = new List<AnnualLeaveDTO>();
        }
    }
}
