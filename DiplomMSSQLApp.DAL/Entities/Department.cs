﻿using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities {
    public class Department {
        public int Id { get; set; }
        public string DepartmentName { get; set; }
        public string Manager { get; set; }
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }
        public virtual ICollection<Post> Posts { get; set; }
        
        public Department() {
            Employees = new List<Employee>();
            Posts = new List<Post>();
        }
    }
}
