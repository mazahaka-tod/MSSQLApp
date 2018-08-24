using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO
{
    public class PostDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int MinSalary { get; set; }
        public int MaxSalary { get; set; }
        public virtual ICollection<EmployeeDTO> Employees { get; set; }

        public PostDTO()
        {
            Employees = new List<EmployeeDTO>();
        }
    }
}
