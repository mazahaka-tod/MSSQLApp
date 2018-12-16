using System.Collections.Generic;
using System.Data.Entity;
using DiplomMSSQLApp.DAL.Entities;

namespace DiplomMSSQLApp.DAL.EF {
    public class HRContext : DbContext {
        public DbSet<AnnualLeave> AnnualLeaves { get; set; }
        public DbSet<BusinessTrip> BusinessTrips { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<LeaveSchedule> LeaveSchedules { get; set; }
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
            base.OnModelCreating(modelBuilder);
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
                Requisites = new Requisites {
                    INN = "7801014571",
                    OGRN = "1038808196162",
                    OKATO = "43755397000",
                    OKPO = "13038635"
                },
                Bank = new Bank {
                    Name = "ПАО Сбербанк",
                    BankAccount = "40702810264000002749",
                    BankCorrespondentAccount = "30101275200000000963",
                    BIK = "044014803"
                }
            };
            db.Organizations.Add(organization);
            db.SaveChanges();

            IEnumerable<Department> departments = new List<Department> {
                new Department { Id = 1, Code = 101, DepartmentName = "Administration", OrganizationId = 1 },
                new Department { Id = 2, Code = 102, DepartmentName = "Finance", OrganizationId = 1 },
                new Department { Id = 3, Code = 103, DepartmentName = "HR", OrganizationId = 1 },
                new Department { Id = 4, Code = 104, DepartmentName = "IT", OrganizationId = 1 },
                new Department { Id = 5, Code = 105, DepartmentName = "Logistics", OrganizationId = 1 },
                new Department { Id = 6, Code = 106, DepartmentName = "Management", OrganizationId = 1 },
                new Department { Id = 7, Code = 107, DepartmentName = "Production", OrganizationId = 1 },
                new Department { Id = 8, Code = 108, DepartmentName = "Purchasing", OrganizationId = 1 }
            };
            db.Departments.AddRange(departments);
            db.SaveChanges();
        }
    }
}
