using DiplomMSSQLApp.BLL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using DiplomMSSQLApp.BLL.BusinessModels;

namespace DiplomMSSQLApp.BLL.Services
{
    public abstract class BaseService<T> : IService<T> where T : class
    {
        public PageInfo PageInfo { get; set; }

        public virtual void Create(T item) { }
        public abstract void Delete(int id);
        public abstract void DeleteAll();
        public abstract void Dispose();
        public virtual void Edit(T item) { }
        public abstract T FindById(int? id);
        public virtual IEnumerable<T> Get(EmployeeFilter f, string path, ref int cnt) { return null; }
        public abstract IEnumerable<T> GetAll();
        public virtual void TestCreate(int num, string path) { }
        public virtual void TestRead(int num, string path, int val) { }
        public virtual void TestUpdate(int num, string path) { }
        public virtual void TestDelete(int num, string path) { }

        public IEnumerable<T> GetPage(IEnumerable<T> col, int page, int cnt)
        {
            // Пагинация (paging)
            int pageSize = 8; // количество объектов на страницу
            PageInfo = new PageInfo { PageNumber = page, PageSize = pageSize, TotalItems = col.Count(), RealPages = cnt };
            if (page < 1) PageInfo.PageNumber = 1;
            if (page > PageInfo.TotalPages) PageInfo.PageNumber = PageInfo.TotalPages;
            IEnumerable<T> pageEntities = col.Skip((PageInfo.PageNumber - 1) * pageSize).Take(pageSize);
            return pageEntities;
        }
    }
}
