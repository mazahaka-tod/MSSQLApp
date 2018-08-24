using DiplomMSSQLApp.DAL.Entities;
using System;

namespace DiplomMSSQLApp.DAL.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<BusinessTrip> BusinessTrips { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Employee> Employees { get; }
        IGenericRepository<Post> Posts { get; }
        void Save();
    }
}
