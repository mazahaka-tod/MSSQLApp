using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double? MinSalary { get; set; }
        public double? MaxSalary { get; set; }
        public virtual ICollection<Employee> Employees { get; set; }

        public Post()
        {
            Employees = new List<Employee>();
        }
    }
}