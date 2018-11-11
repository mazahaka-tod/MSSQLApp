using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO
{
    public class DepartmentDTO
    {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string Manager { get; set; }
        public int? EmployeeId { get; set; }
        public EmployeeDTO Employee { get; set; }
        public virtual ICollection<EmployeeDTO> Employees { get; set; }

        public DepartmentDTO()
        {
            Employees = new List<EmployeeDTO>();
        }
    }
}
