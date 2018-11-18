using DiplomMSSQLApp.DAL.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFPostRepository : EFGenericRepository<Post> {
        private DbSet<Post> _dbSet;

        public EFPostRepository(DbContext context) : base(context) {
            _dbSet = context.Set<Post>();
        }

        //public override IEnumerable<Post> Get(int salary) {
        //    return _dbSet.Include(p => p.Department).Where(p => p.Salary >= salary).ToList();
        //}

        //public override IEnumerable<Post> Get(bool flag) {
        //    if (flag)
        //        return _dbSet.Include(p => p.Department).Where(p => p.Salary > 50000).ToList();
        //    else
        //        return _dbSet.Include(p => p.Department).Where(p => p.Salary <= 50000).ToList();
        //}

        public override async Task<Post> FindByIdAsync(int id) {
            return await _dbSet.Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == id);
        }

        public override async Task<IEnumerable<Post>> GetAllAsync() {
            return await _dbSet.Include(p => p.Department).ToListAsync();
        }

        public override async Task<Post> GetFirstAsync() {
            return await _dbSet.Include(p => p.Department).FirstOrDefaultAsync();
        }
    }
}
