using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities {
    public class BusinessTrip {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Destination { get; set; }
        public string Purpose { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }

        public BusinessTrip() {
            Employees = new List<Employee>();
        }
    }
}
