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

        // Добавление элемента
        public void Create(TEntity item)
        {
            _dbSet.Add(item);
        }
        // Добавление последовательности элементов
        public void Create(IEnumerable<TEntity> items)
        {
            _dbSet.AddRange(items);
        }
        // Поиск по id
        public virtual TEntity FindById(int id)
        {
            return _dbSet.Find(id);
        }
        // Получение всех элементов
        public IEnumerable<TEntity> Get()
        {
            return _dbSet.ToList();
        }
        //// Получение элементов по условию
        //public virtual IEnumerable<TEntity> Get(int salary)
        //{
        //    return _dbSet.ToList();
        //}
        //// Получение элементов по условию
        //public virtual IEnumerable<TEntity> Get(bool flag)
        //{
        //    return _dbSet.ToList();
        //}
        // Получение элементов, удовлетворяющих предикату
        public virtual IEnumerable<TEntity> Get(Func<TEntity, bool> predicate)
        {
            return _dbSet.Where(predicate).ToList();
        }
        // Получение первого элемента
        public virtual TEntity GetFirst()
        {
            return _dbSet.FirstOrDefault();
        }
        // Удаление элемента
        public void Remove(TEntity item)
        {
            _dbSet.Remove(item);
        }
        // Удаление последовательности элементов
        public void RemoveSeries(IEnumerable<TEntity> items)
        {
            _dbSet.RemoveRange(items);
        }
        // Удаление всех элементов
        public void RemoveAll()
        {
            _dbSet.RemoveRange(Get());
        }
        // Обновление элемента
        public void Update(TEntity item)
        {
            _context.Entry(item).State = EntityState.Modified;
        }

        // Нереализованные методы
        public virtual IEnumerable<TEntity> Get(int salary)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<TEntity> Get(bool flag)
        {
            throw new NotImplementedException();
        }
    }
}
