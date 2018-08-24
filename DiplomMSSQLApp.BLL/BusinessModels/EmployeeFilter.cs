namespace DiplomMSSQLApp.BLL.BusinessModels
{
    // бизнес-модель для реализации фильтра выборки сотрудников
    public class EmployeeFilter
    {
        public string[] LastName { get; set; }
        public bool Or { get; set; }
        public string Email { get; set; }
        public bool PhoneNumber { get; set; }
        public string HireDate { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public double?[] Bonus { get; set; }
        public bool BonusExists { get; set; }
        public string Post { get; set; }
        public string DepartmentName { get; set; }
        public bool Not { get; set; }
        public string Sort { get; set; }
        public string Asc { get; set; }
        public string[] Columns { get; set; }

        public EmployeeFilter()
        {
            Columns = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8" };
        }
    }
}
