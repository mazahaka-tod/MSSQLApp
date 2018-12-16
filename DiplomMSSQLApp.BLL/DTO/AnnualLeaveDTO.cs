using System;

namespace DiplomMSSQLApp.BLL.DTO {
    public class AnnualLeaveDTO {
        public int Id { get; set; }
        public int? ScheduledNumberOfDays { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public int? ActualNumberOfDays { get; set; }
        public DateTime? ActualDate { get; set; }

        public int? EmployeeId { get; set; }
        public EmployeeDTO Employee { get; set; }

        public int? LeaveScheduleId { get; set; }
        public LeaveScheduleDTO LeaveSchedule { get; set; }
    }
}
