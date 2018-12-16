using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Interfaces {
    public interface IUnitOfWork : IDisposable {
        IGenericRepository<AnnualLeave> AnnualLeaves { get; }
        IGenericRepository<BusinessTrip> BusinessTrips { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Employee> Employees { get; }
        IGenericRepository<LeaveSchedule> LeaveSchedules { get; }
        IGenericRepository<Organization> Organizations { get; }
        IGenericRepository<Post> Posts { get; }
        Task SaveAsync();
    }
}
