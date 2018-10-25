using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO {
    public class PostDTO {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? NumberOfUnits { get; set; }
        public double? Salary { get; set; }
        public double? Premium { get; set; }
        public double TotalSalary {
            get { return (Salary.Value + Premium.Value) * NumberOfUnits.Value; }
        }
        public int? DepartmentId { get; set; }
        public DepartmentDTO Department { get; set; }
        public virtual ICollection<EmployeeDTO> Employees { get; set; }

        public PostDTO() {
            Employees = new List<EmployeeDTO>();
        }
    }
}
