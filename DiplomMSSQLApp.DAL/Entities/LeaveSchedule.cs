using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities {
    public class LeaveSchedule {
        public int Id { get; set; }
        public int Number { get; set; }
        public int Year { get; set; }
        public DateTime? DateOfPreparation { get; set; }
        public DateTime? DateOfApproval { get; set; }

        public virtual ICollection<AnnualLeave> AnnualLeaves { get; set; }
        public LeaveSchedule() {
            AnnualLeaves = new List<AnnualLeave>();
        }
    }
}
