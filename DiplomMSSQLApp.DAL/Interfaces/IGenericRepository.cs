using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Create(TEntity item);
        void Create(IEnumerable<TEntity> items);
        TEntity FindById(int id);
        IEnumerable<TEntity> Get();
        IEnumerable<TEntity> Get(int val);
        IEnumerable<TEntity> Get(bool f);
        IEnumerable<TEntity> Get(Func<TEntity, bool> predicate);
        TEntity GetFirst();
        void Remove(TEntity item);
        void RemoveSeries(IEnumerable<TEntity> items);
        void RemoveAll();
        void Update(TEntity item);
    }
}
