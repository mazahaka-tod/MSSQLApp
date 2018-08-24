using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.Interfaces
{
    public interface IService<TEntity> where TEntity : class
    {
        PageInfo PageInfo { get; set; }

        void Create(TEntity item);
        void Delete(int id);
        void DeleteAll();
        void Dispose();
        void Edit(TEntity item);
        TEntity FindById(int? id);
        IEnumerable<TEntity> Get(EmployeeFilter f, string path, ref int cnt);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetPage(IEnumerable<TEntity> col, int page, int cnt);
        void TestCreate(int num, string path);
        void TestRead(int num, string path, int val);
        void TestUpdate(int num, string path);
        void TestDelete(int num, string path);
    }
}
