using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFDepartmentRepository : EFGenericRepository<Department> {
        private DbSet<Department> _dbSet;

        public EFDepartmentRepository(DbContext context) : base(context) {
            _dbSet = context.Set<Department>();
        }

        public override async Task<Department> FindByIdAsync(int id) {
            return await _dbSet.Include(d => d.Organization).Include(d => d.Posts).FirstOrDefaultAsync(d => d.Id == id);
        }

        public override IEnumerable<Department> Get(Func<Department, bool> predicate) {
            return _dbSet.Include(d => d.Posts).Where(predicate).ToList();
        }

        public override async Task<IEnumerable<Department>> GetAllAsync() {
            return await _dbSet.Include(d => d.Organization).ToListAsync();
        }
    }
}
