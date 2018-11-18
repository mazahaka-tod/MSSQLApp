﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.DAL.Interfaces {
    public interface IGenericRepository<TEntity> where TEntity : class {
        void Create(TEntity item);                                      // Добавление элемента
        void CreateSeries(IEnumerable<TEntity> items);                  // Добавление последовательности элементов
        Task<TEntity> FindByIdAsync(int id);                            // Поиск по id
        Task<IEnumerable<TEntity>> GetAllAsync();                       // Получение всех элементов
        //IEnumerable<TEntity> Get(int salary);                         // Получение элементов по условию
        //IEnumerable<TEntity> Get(bool flag);                          // Получение элементов по условию
        IEnumerable<TEntity> Get(Func<TEntity, bool> predicate);        // Получение элементов, удовлетворяющих предикату
        Task<TEntity> GetFirstAsync();                                  // Получение первого элемента
        void Remove(TEntity item);                                      // Удаление элемента
        void RemoveSeries(IEnumerable<TEntity> items);                  // Удаление последовательности элементов
        Task RemoveAllAsync();                                          // Удаление всех элементов
        void Update(TEntity item);                                      // Обновление элемента
    }
}
