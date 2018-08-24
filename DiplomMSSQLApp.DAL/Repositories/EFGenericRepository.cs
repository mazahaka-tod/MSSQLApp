using DiplomMSSQLApp.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DiplomMSSQLApp.DAL.Repositories
{
    public class EFGenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private DbContext _context;
        private DbSet<TEntity> _dbSet;

        public EFGenericRepository(DbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public IEnumerable<TEntity> Get()
        {
            return _dbSet.ToList();
        }

        public virtual IEnumerable<TEntity> Get(Func<TEntity, bool> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }
        public virtual TEntity FindById(int id)
        {
            return _dbSet.Find(id);
        }

        public void Create(TEntity item)
        {
            _dbSet.Add(item);
        }

        public void Create(IEnumerable<TEntity> items)
        {
            _dbSet.AddRange(items);
        }

        public void Update(TEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }
        public void Remove(TEntity item)
        {
            _dbSet.Remove(item);
        }
        public void RemoveAll()
        {
            _dbSet.RemoveRange(Get());
        }

        public virtual IEnumerable<TEntity> Get(int val)
        {
            return _dbSet.ToList();
        }

        public virtual IEnumerable<TEntity> Get(bool f)
        {
            return _dbSet.ToList();
        }

        public virtual TEntity GetFirst()
        {
            return _dbSet.SingleOrDefault();
        }

        public void RemoveSeries(IEnumerable<TEntity> items)
        {
            _dbSet.RemoveRange(items);
        }
    }
}
