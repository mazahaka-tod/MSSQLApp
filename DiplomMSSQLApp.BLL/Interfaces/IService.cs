using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Interfaces
{
    public interface IService<TEntity> where TEntity : class
    {
        PageInfo PageInfo { get; set; }

        Task CreateAsync(TEntity item);
        Task DeleteAsync(int id);
        Task DeleteAllAsync();
        void Dispose();
        Task EditAsync(TEntity item);
        Task<TEntity> FindByIdAsync(int? id);
        IEnumerable<TEntity> Get(EmployeeFilter filter);
        Task<IEnumerable<TEntity> > GetAllAsync();
        Task<TEntity> GetFirstAsync();
        IEnumerable<TEntity> GetPage(IEnumerable<TEntity> col, int page);
        Task TestCreateAsync(int num);
        Task TestReadAsync(int num, int salary);
        Task TestUpdateAsync(int num);
        Task TestDeleteAsync(int num);
    }
}
