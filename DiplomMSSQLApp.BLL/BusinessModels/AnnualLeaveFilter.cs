namespace DiplomMSSQLApp.BLL.BusinessModels {
    public class AnnualLeaveFilter {
        public string[] Name { get; set; }
        public string[] PostTitle { get; set; }
        public string[] DepartmentName { get; set; }

        public string ScheduledDate { get; set; }
        public string ActualDate { get; set; }

        public int? MinNumberOfDaysOfLeave { get; set; }
        public int? MaxNumberOfDaysOfLeave { get; set; }
        public int? MinScheduledNumberOfDays { get; set; }
        public int? MaxScheduledNumberOfDays { get; set; }
        public int? MinActualNumberOfDays { get; set; }
        public int? MaxActualNumberOfDays { get; set; }

        public bool IsAntiFilter { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
        public string[] Columns { get; set; }

        public AnnualLeaveFilter() {
            Columns = new string[] { "0", "1", "2", "3", "4", "5", "6", "7" };
        }
    }
}
