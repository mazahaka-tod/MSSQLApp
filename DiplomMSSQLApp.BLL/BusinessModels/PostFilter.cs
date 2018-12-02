namespace DiplomMSSQLApp.BLL.BusinessModels {
    public class PostFilter {
        public int?[] DepartmentCode { get; set; }
        public string[] DepartmentName { get; set; }
        public string[] PostTitle { get; set; }

        public int? MinNumberOfUnits { get; set; }
        public int? MaxNumberOfUnits { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public double? MinPremium { get; set; }
        public double? MaxPremium { get; set; }
        public double? MinTotalSalary { get; set; }
        public double? MaxTotalSalary { get; set; }

        public bool IsAntiFilter { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
        public string[] Columns { get; set; }

        public PostFilter() {
            Columns = new string[] { "0", "1", "2", "3", "4", "5", "6" };
        }
    }
}
