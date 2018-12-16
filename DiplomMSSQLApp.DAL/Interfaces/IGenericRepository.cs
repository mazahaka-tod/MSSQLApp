using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Interfaces {
    public interface IGenericRepository<TEntity> where TEntity : class {
        Task<int> CountAsync();                                                     // Количество элементов
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);            // Количество элементов, удовлетворяющих предикату
        void Create(TEntity item);                                                  // Добавление элемента
        Task<TEntity> FindByIdAsync(int id);                                        // Поиск по id
        Task<IEnumerable<TEntity>> GetAllAsync();                                   // Получение всех элементов
        IEnumerable<TEntity> Get(Func<TEntity, bool> predicate);                    // Получение элементов, удовлетворяющих предикату
        Task<TEntity> GetFirstAsync();                                              // Получение первого элемента
        Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate);     // Получение первого элемента, удовлетворяющего предикату
        void Remove(TEntity item);                                                  // Удаление элемента
        void RemoveSeries(IEnumerable<TEntity> items);                              // Удаление последовательности элементов
        Task RemoveAllAsync();                                                      // Удаление всех элементов
        void Update(TEntity item);                                                  // Обновление элемента
    }
}
