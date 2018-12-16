using System;

namespace DiplomMSSQLApp.DAL.Entities {
    public class AnnualLeave {
        public int Id { get; set; }
        public int? ScheduledNumberOfDays { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int? ActualNumberOfDays { get; set; }
        public DateTime? ActualDate { get; set; }

        public int? EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int? LeaveScheduleId { get; set; }
        public LeaveSchedule LeaveSchedule { get; set; }
    }
}
