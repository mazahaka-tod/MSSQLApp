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
            return await _dbSet.Include(e => e.Post).Include(e => e.Post.Department).FirstOrDefaultAsync(e => e.Id == id);
        }

        public override IEnumerable<Employee> Get(Func<Employee, bool> predicate) {
            return _dbSet.Include(e => e.Post).Include(e => e.Post.Department).Where(predicate).ToList();
        }

        public override async Task<IEnumerable<Employee>> GetAllAsync() {
            return await _dbSet.Include(e => e.Post).Include(e => e.Post.Department).ToListAsync();
        }

        public override async Task<Employee> GetFirstAsync() {
            return await _dbSet.Include(e => e.Post).Include(e => e.Post.Department).FirstOrDefaultAsync();
        }
    }
}
