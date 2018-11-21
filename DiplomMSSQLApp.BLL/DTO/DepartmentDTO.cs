using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO {
    public class DepartmentDTO {
        public int Id { get; set; }
        public int Code { get; set; }
        public string DepartmentName { get; set; }
        public int? OrganizationId { get; set; }
        public OrganizationDTO Organization { get; set; }
        public int? ManagerId { get; set; }
        public EmployeeDTO Manager { get; set; }
        public virtual ICollection<PostDTO> Posts { get; set; }

        public DepartmentDTO() {
            Posts = new List<PostDTO>();
        }
    }
}
