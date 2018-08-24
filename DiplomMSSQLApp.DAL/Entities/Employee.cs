using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities
{
    public class Employee
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
        public Post Post { get; set; }
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }
        public virtual ICollection<BusinessTrip> BusinessTrips { get; set; }

        public Employee()
        {
            BusinessTrips = new List<BusinessTrip>();
        }
    }
}