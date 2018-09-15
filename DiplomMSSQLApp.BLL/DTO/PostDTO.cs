using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO
{
    public class PostDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public virtual ICollection<EmployeeDTO> Employees { get; set; }

        public PostDTO()
        {
            Employees = new List<EmployeeDTO>();
        }
    }
}
