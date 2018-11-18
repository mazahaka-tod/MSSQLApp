using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Interfaces {
    public interface IService<TEntity> where TEntity : class {
        PageInfo PageInfo { get; set; }

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
