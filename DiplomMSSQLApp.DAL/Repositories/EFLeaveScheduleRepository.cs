using DiplomMSSQLApp.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DiplomMSSQLApp.DAL.Repositories {
    public class EFLeaveScheduleRepository : EFGenericRepository<LeaveSchedule> {
        private DbSet<LeaveSchedule> _dbSet;

        public EFLeaveScheduleRepository(DbContext context) : base(context) {
            _dbSet = context.Set<LeaveSchedule>();
        }

        public override IEnumerable<LeaveSchedule> Get(Func<LeaveSchedule, bool> predicate) {
            return _dbSet.Include(ls => ls.AnnualLeaves).Where(predicate).ToList();
        }
    }
}
