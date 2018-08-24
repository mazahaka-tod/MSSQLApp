using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO
{
    public class EmployeeDTO
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public DateTime HireDate { get; set; }
        public double? Salary { get; set; }
        public double? Bonus { get; set; }
        public int? PostId { get; set; }
        public PostDTO Post { get; set; }
        public int? DepartmentId { get; set; }
        public DepartmentDTO Department { get; set; }
        public virtual ICollection<BusinessTripDTO> BusinessTrips { get; set; }

        public EmployeeDTO()
        {
            BusinessTrips = new List<BusinessTripDTO>();
        }
    }
}
