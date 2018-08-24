using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DiplomMSSQLApp.DAL.Repositories
{
    public class EFEmployeeRepository : EFGenericRepository<Employee>
    {
        //DbContext _context;
        private DbSet<Employee> _dbSet;

        public EFEmployeeRepository(DbContext context) : base(context)
        {
            //_context = context;
            _dbSet = context.Set<Employee>();
        }

        public override IEnumerable<Employee> Get(Func<Employee, bool> predicate)
        {
            return _dbSet.Include(e => e.Department).Include(e => e.Post)
                .Where(predicate).ToList();
        }

        public override Employee FindById(int id)
        {
            return _dbSet.Include(e => e.Department).Include(e => e.Post).FirstOrDefault(e => e.Id == id);
        }

        public override IEnumerable<Employee> Get(int val)
        {
            var emps = _dbSet.AsQueryable().Where(e => e.Salary >= val)
                .Include(e => e.Department).Include(e => e.Post)
                .AsNoTracking();
            return emps.ToList();
        }

        public override IEnumerable<Employee> Get(bool f)
        {
            return _dbSet.Where(e => e.Salary != 50000).ToList();
        }

        public override Employee GetFirst()
        {
            return _dbSet.FirstOrDefault(e => e.Salary < 100000);
        }
    }
}
