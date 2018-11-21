using DiplomMSSQLApp.DAL.EF;
using DiplomMSSQLApp.DAL.Entities;
using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFUnitOfWork : IUnitOfWork {
        private HRContext db;
        private IGenericRepository<BusinessTrip>    _businessTripRepository;
        private IGenericRepository<Department>      _departmentRepository;
        private IGenericRepository<Employee>        _employeeRepository;
        private IGenericRepository<Organization>    _organizationRepository;
        private IGenericRepository<Post>            _postRepository;

        public EFUnitOfWork(string connectionString) {
            db = new HRContext(connectionString);
        }

        public IGenericRepository<BusinessTrip> BusinessTrips {
            get {
                if (_businessTripRepository == null)
                    _businessTripRepository = new EFGenericRepository<BusinessTrip>(db);
                return _businessTripRepository;
            }
        }

        public IGenericRepository<Department> Departments {
            get {
                if (_departmentRepository == null)
                    _departmentRepository = new EFDepartmentRepository(db);
                return _departmentRepository;
            }
        }

        public IGenericRepository<Employee> Employees {
            get {
                if (_employeeRepository == null)
                    _employeeRepository = new EFEmployeeRepository(db);
                return _employeeRepository;
            }
        }

        public IGenericRepository<Organization> Organizations {
            get {
                if (_organizationRepository == null)
                    _organizationRepository = new EFGenericRepository<Organization>(db);
                return _organizationRepository;
            }
        }

        public IGenericRepository<Post> Posts {
            get {
                if (_postRepository == null)
                    _postRepository = new EFPostRepository(db);
                return _postRepository;
            }
        }

        public async Task SaveAsync() {
            await db.SaveChangesAsync();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing) {
            if (!disposed) {
                if (disposing) {
                    db.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
