using System;
using System.Collections.Generic;
using System.Linq;

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

        public override bool Equals(object obj)
        {
            return Id == (obj as BusinessTripDTO).Id && Name == (obj as BusinessTripDTO).Name &&
                DateStart == (obj as BusinessTripDTO).DateStart && DateEnd == (obj as BusinessTripDTO).DateEnd &&
                Destination == (obj as BusinessTripDTO).Destination && Purpose == (obj as BusinessTripDTO).Purpose;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
