using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO {
    public class OrganizationDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LegalAddress { get; set; }
        public string ActualAddress { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public virtual ICollection<DepartmentDTO> Departments { get; set; }

        public OrganizationDTO() {
            Departments = new List<DepartmentDTO>();
        }
    }
}
