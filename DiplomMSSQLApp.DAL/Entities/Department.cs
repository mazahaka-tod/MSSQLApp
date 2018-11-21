using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities {
    public class Department {
        public int Id { get; set; }
        public int Code { get; set; }
        public string DepartmentName { get; set; }
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int? ManagerId { get; set; }
        public Employee Manager { get; set; }
        public virtual ICollection<Post> Posts { get; set; }

        public Department() {
            Posts = new List<Post>();
        }
    }
}
