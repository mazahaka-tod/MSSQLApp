using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFBusinessTripRepository : EFGenericRepository<BusinessTrip> {
        private DbSet<BusinessTrip> _dbSet;

        public EFBusinessTripRepository(DbContext context) : base(context) {
            _dbSet = context.Set<BusinessTrip>();
        }

        public override async Task<BusinessTrip> FindByIdAsync(int id) {
            return await _dbSet.Include(bt => bt.Employees).FirstOrDefaultAsync(d => d.Id == id);
        }

        public override IEnumerable<BusinessTrip> Get(Func<BusinessTrip, bool> predicate) {
            return _dbSet.Where(predicate).ToList();
        }

        public override async Task<IEnumerable<BusinessTrip>> GetAllAsync() {
            return await _dbSet.Include(bt => bt.Employees).ToListAsync();
        }
    }
}
