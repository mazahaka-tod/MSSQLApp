using DiplomMSSQLApp.BLL.BusinessModels;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Interfaces {
    public interface IService<TEntity> where TEntity : class {
        PageInfo PageInfo { get; set; }

        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task CreateAsync(TEntity item);
        Task DeleteAsync(int id);
        Task DeleteAllAsync();
        void Dispose();
        Task EditAsync(TEntity item);
        Task<TEntity> FindByIdAsync(int? id);
        Task<IEnumerable<TEntity>> GetAllAsync();
        IEnumerable<TEntity> GetPage(IEnumerable<TEntity> col, int page);
    }
}
