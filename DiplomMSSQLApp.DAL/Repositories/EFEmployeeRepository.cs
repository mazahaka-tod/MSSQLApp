using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFEmployeeRepository : EFGenericRepository<Employee> {
        private DbSet<Employee> _dbSet;

        public EFEmployeeRepository(DbContext context) : base(context) {
            _dbSet = context.Set<Employee>();
        }

        public override async Task<Employee> FindByIdAsync(int id) {
            return await _dbSet.Include(e => e.Department).Include(e => e.Post).FirstOrDefaultAsync(e => e.Id == id);
        }

        public override IEnumerable<Employee> Get(int salary) {
            return _dbSet.Include(e => e.Department).Include(e => e.Post).Where(e => e.Salary >= salary).ToList();
        }

        public override IEnumerable<Employee> Get(bool flag) {
            if (flag)
                return _dbSet.Include(e => e.Department).Include(e => e.Post).Where(e => e.Salary > 50000).ToList();
            else
                return _dbSet.Include(e => e.Department).Include(e => e.Post).Where(e => e.Salary <= 50000).ToList();
        }

        public override IEnumerable<Employee> Get(Func<Employee, bool> predicate) {
            return _dbSet.Include(e => e.Department).Include(e => e.Post).Where(predicate).ToList();
        }

        public override async Task<IEnumerable<Employee>> GetAsync() {
            return await _dbSet.Include(e => e.Department).Include(e => e.Post).ToListAsync();
        }

        public override async Task<Employee> GetFirstAsync() {
            return await _dbSet.Include(e => e.Department).Include(e => e.Post).FirstOrDefaultAsync();
        }
    }
}
