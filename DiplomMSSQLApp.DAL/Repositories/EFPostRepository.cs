using DiplomMSSQLApp.DAL.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DiplomMSSQLApp.DAL.Repositories
{
    public class EFPostRepository : EFGenericRepository<Post>
    {
        private DbSet<Post> _dbSet;

        public EFPostRepository(DbContext context) : base(context)
        {
            _dbSet = context.Set<Post>();
        }

        public override IEnumerable<Post> Get(int val)
        {
            return _dbSet.AsQueryable().Where(p => p.MaxSalary >= val).AsNoTracking().ToList();
        }

        public override IEnumerable<Post> Get(bool f)
        {
            if (f)
                return _dbSet.AsQueryable().Where(p => p.MaxSalary == 60000).AsNoTracking().ToList();
            else
                return _dbSet.Where(p => p.MaxSalary != 75000).ToList();
        }

        public override Post GetFirst()
        {
            return _dbSet.FirstOrDefault(p => p.MaxSalary != 75000);
        }
    }
}
