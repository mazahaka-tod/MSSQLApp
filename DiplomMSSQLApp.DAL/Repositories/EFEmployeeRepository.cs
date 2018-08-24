using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DiplomMSSQLApp.DAL.Repositories
{
    public class EFEmployeeRepository : EFGenericRepository<Employee>
    {
        private DbSet<Employee> _dbSet;

        public EFEmployeeRepository(DbContext context) : base(context)
        {
            _dbSet = context.Set<Employee>();
        }

        public override IEnumerable<Employee> Get(Func<Employee, bool> predicate)
        {
            return _dbSet.Include(e => e.Department).Include(e => e.Post).Where(predicate).ToList();
        }

        public override Employee FindById(int id)
        {
            return _dbSet.Include(e => e.Department).Include(e => e.Post).FirstOrDefault(e => e.Id == id);
        }

        public override IEnumerable<Employee> Get(int val)
        {
            return _dbSet.Include(e => e.Department).Include(e => e.Post).Where(e => e.Salary >= val).ToList();
        }

        public override IEnumerable<Employee> Get(bool f)
        {
            if (f)
                return _dbSet.Where(e => e.Salary > 50000).ToList();
            else
                return _dbSet.Where(e => e.Salary <= 50000).ToList();
        }

        public override Employee GetFirst()
        {
            return _dbSet.FirstOrDefault(e => e.Salary < 100000);
        }
    }
}
