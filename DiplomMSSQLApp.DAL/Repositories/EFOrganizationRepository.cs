using DiplomMSSQLApp.DAL.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFOrganizationRepository : EFGenericRepository<Organization> {
        private DbSet<Organization> _dbSet;

        public EFOrganizationRepository(DbContext context) : base(context) {
            _dbSet = context.Set<Organization>();
        }

        public override async Task<Organization> FindByIdAsync(int id) {
            return await _dbSet.FirstOrDefaultAsync(o => o.Id == id);
        }

        public override async Task<IEnumerable<Organization>> GetAsync() {
            return await _dbSet.ToListAsync();
        }

        public override async Task<Organization> GetFirstAsync() {
            return await _dbSet.FirstOrDefaultAsync();
        }
    }
}
