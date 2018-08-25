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
        TEntity FindById(int? id);
        IEnumerable<TEntity> Get(EmployeeFilter f, string path, ref int cnt);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetPage(IEnumerable<TEntity> col, int page, int cnt);
        Task TestCreateAsync(int num, string path);
        void TestRead(int num, string path, int val);
        Task TestUpdateAsync(int num, string path);
        Task TestDeleteAsync(int num, string path);
    }
}
