using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO
{
    public class BusinessTripDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Destination { get; set; }
        public string Purpose { get; set; }
        public virtual ICollection<EmployeeDTO> Employees { get; set; }

        public BusinessTripDTO()
        {
            Employees = new List<EmployeeDTO>();
        }
    }
}
