using System;
using System.Collections.Generic;

namespace DiplomMSSQLApp.DAL.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Create(TEntity item);                                      // Добавление элемента
        void Create(IEnumerable<TEntity> items);                        // Добавление последовательности элементов
        TEntity FindById(int id);                                       // Поиск по id
        IEnumerable<TEntity> Get();                                     // Получение всех элементов
        IEnumerable<TEntity> Get(int salary);                           // Получение элементов по условию
        IEnumerable<TEntity> Get(bool flag);                            // Получение элементов по условию
        IEnumerable<TEntity> Get(Func<TEntity, bool> predicate);        // Получение элементов, удовлетворяющих предикату
        TEntity GetFirst();                                             // Получение первого элемента
        void Remove(TEntity item);                                      // Удаление элемента
        void RemoveSeries(IEnumerable<TEntity> items);                  // Удаление последовательности элементов
        void RemoveAll();                                               // Удаление всех элементов
        void Update(TEntity item);                                      // Обновление элемента
    }
}
