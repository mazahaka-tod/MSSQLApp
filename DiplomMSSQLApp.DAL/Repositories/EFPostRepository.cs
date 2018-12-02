using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFPostRepository : EFGenericRepository<Post> {
        private DbSet<Post> _dbSet;

        public EFPostRepository(DbContext context) : base(context) {
            _dbSet = context.Set<Post>();
        }

        public override async Task<Post> FindByIdAsync(int id) {
            return await _dbSet.Include(p => p.Department).FirstOrDefaultAsync(p => p.Id == id);
        }

        public override IEnumerable<Post> Get(Func<Post, bool> predicate) {
            return _dbSet.Include(p => p.Department).Where(predicate).ToList();
        }

        public override async Task<IEnumerable<Post>> GetAllAsync() {
            return await _dbSet.Include(p => p.Department).ToListAsync();
        }

        public override async Task<Post> GetFirstAsync() {
            return await _dbSet.Include(p => p.Department).FirstOrDefaultAsync();
        }
    }
}
