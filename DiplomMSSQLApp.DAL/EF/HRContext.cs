using System.Data.Entity;
using DiplomMSSQLApp.DAL.Entities;

namespace DiplomMSSQLApp.DAL.EF {
    public class HRContext : DbContext {
        public DbSet<BusinessTrip> BusinessTrips { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Post> Posts { get; set; }

        public HRContext(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            // Создаем таблицу для связи "многие-ко-многим"
            modelBuilder.Entity<Employee>()
                        .HasMany(e => e.BusinessTrips)
                        .WithMany(bt => bt.Employees)
                        .Map(t => t.MapLeftKey("EmployeeId")
                        .MapRightKey("BusinessTripId")
                        .ToTable("EmployeeBusinessTrip"));
        }

        static HRContext() {
            Database.SetInitializer(new HRContextInitializer());
        }
    }

    public class HRContextInitializer : DropCreateDatabaseIfModelChanges<HRContext> {
        protected override void Seed(HRContext db) {
            Organization organization = new Organization {
                Id = 1,
                Name = "Отделение ПФР",
                LegalAddress = "194214, Санкт-Петербург, пр. Энгельса, дом 73",
                Phone = "+7 (812) 292-85-92",
                Fax = "+7 (812) 292-81-54",
                Requisites = new Requisites { },
                Bank = new Bank { }
            };
            db.Organizations.Add(organization);
            db.SaveChanges();
        }
    }
}
