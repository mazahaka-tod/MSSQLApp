using System.Data.Entity;
using DiplomMSSQLApp.DAL.Entities;

namespace DiplomMSSQLApp.DAL.EF
{
    public class HRContext : DbContext
    {
        public DbSet<BusinessTrip> BusinessTrips { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Post> Posts { get; set; }

        public HRContext(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Создаем таблицу для связи "многие-ко-многим"
            modelBuilder.Entity<Employee>()
                        .HasMany(e => e.BusinessTrips)
                        .WithMany(bt => bt.Employees)
                        .Map(t => t.MapLeftKey("EmployeeId")
                        .MapRightKey("BusinessTripId")
                        .ToTable("EmployeeBusinessTrip"));
        }
    }
}
