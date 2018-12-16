using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities {
    public class Post {
        public int Id { get; set; }
        public string Title { get; set; }
        public int NumberOfUnits { get; set; }
        public double Salary { get; set; }
        public double Premium { get; set; }
        public int NumberOfDaysOfLeave { get; set; }

        public int? DepartmentId { get; set; }
        public Department Department { get; set; }

        public virtual ICollection<Employee> Employees { get; set; }
        public Post() {
            Employees = new List<Employee>();
        }
    }
}
