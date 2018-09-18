using DiplomMSSQLApp.DAL.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFPostRepository : EFGenericRepository<Post> {
        private DbSet<Post> _dbSet;

        public EFPostRepository(DbContext context) : base(context) {
            _dbSet = context.Set<Post>();
        }

        public override IEnumerable<Post> Get(int salary) {
            return _dbSet.Where(p => p.MaxSalary >= salary).ToList();
        }

        public override IEnumerable<Post> Get(bool flag) {
            if (flag)
                return _dbSet.Where(p => p.MaxSalary > 50000).ToList();
            else
                return _dbSet.Where(p => p.MaxSalary <= 50000).ToList();
        }

        public override Post GetFirst() {
            return _dbSet.FirstOrDefault();
        }
    }
}
