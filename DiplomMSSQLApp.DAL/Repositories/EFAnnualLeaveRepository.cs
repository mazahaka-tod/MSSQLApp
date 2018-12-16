using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFAnnualLeaveRepository : EFGenericRepository<AnnualLeave> {
        private DbSet<AnnualLeave> _dbSet;

        public EFAnnualLeaveRepository(DbContext context) : base(context) {
            _dbSet = context.Set<AnnualLeave>();
        }

        public override async Task<AnnualLeave> FindByIdAsync(int id) {
            return await _dbSet.Include(al => al.Employee).Include(al => al.Employee.Post).Include(al => al.Employee.Post.Department).Include(al => al.LeaveSchedule).FirstOrDefaultAsync(al => al.Id == id);
        }

        public override IEnumerable<AnnualLeave> Get(Func<AnnualLeave, bool> predicate) {
            return _dbSet.Include(al => al.Employee).Include(al => al.Employee.Post).Include(al => al.Employee.Post.Department).Include(al => al.LeaveSchedule).Where(predicate).ToList();
        }

        public override async Task<IEnumerable<AnnualLeave>> GetAllAsync() {
            return await _dbSet.Include(al => al.Employee).ToListAsync();
        }
    }
}
