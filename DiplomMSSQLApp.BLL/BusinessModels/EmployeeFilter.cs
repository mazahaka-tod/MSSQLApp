namespace DiplomMSSQLApp.BLL.BusinessModels {
    // бизнес-модель для реализации фильтра выборки сотрудников
    public class EmployeeFilter {
        public string[] LastName { get; set; }
        public bool IsMatchAnyLastName { get; set; }
        public string Email { get; set; }
        public bool IsPhoneNumber { get; set; }
        public string HireDate { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public double?[] Bonus { get; set; }
        public bool IsBonus { get; set; }
        public string PostTitle { get; set; }
        public string DepartmentName { get; set; }
        public bool IsAntiFilter { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
        public string[] Columns { get; set; }

        public EmployeeFilter() {
            Columns = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8" };
        }
    }
}
