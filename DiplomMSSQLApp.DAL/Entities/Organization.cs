using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities {
    public class Organization {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LegalAddress { get; set; }
        public string ActualAddress { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public virtual ICollection<Department> Departments { get; set; }

        public Organization() {
            Departments = new List<Department>();
        }
    }
}
