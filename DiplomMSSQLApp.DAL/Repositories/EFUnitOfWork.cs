using DiplomMSSQLApp.DAL.EF;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories
{
    public class EFUnitOfWork : IUnitOfWork
    {
        private HRContext db;
        private EFGenericRepository<BusinessTrip> businessTripRepository;
        private EFGenericRepository<Department> departmentRepository;
        private EFEmployeeRepository employeeRepository;
        private EFPostRepository postRepository;

        public EFUnitOfWork(string connectionString)
        {
            db = new HRContext(connectionString);
        }

        public IGenericRepository<BusinessTrip> BusinessTrips
        {
            get
            {
                if (businessTripRepository == null)
                    businessTripRepository = new EFGenericRepository<BusinessTrip>(db);
                return businessTripRepository;
            }
        }

        public IGenericRepository<Department> Departments
        {
            get
            {
                if (departmentRepository == null)
                    departmentRepository = new EFGenericRepository<Department>(db);
                return departmentRepository;
            }
        }

        public IGenericRepository<Employee> Employees
        {
            get
            {
                if (employeeRepository == null)
                    employeeRepository = new EFEmployeeRepository(db);
                return employeeRepository;
            }
        }

        public IGenericRepository<Post> Posts
        {
            get
            {
                if (postRepository == null)
                    postRepository = new EFPostRepository(db);
                return postRepository;
            }
        }

        public async Task SaveAsync()
        {
            await db.SaveChangesAsync();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    db.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
