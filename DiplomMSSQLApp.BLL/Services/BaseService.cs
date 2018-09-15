using DiplomMSSQLApp.BLL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using DiplomMSSQLApp.BLL.BusinessModels;
using System.Threading.Tasks;

namespace DiplomMSSQLApp.BLL.Services {
    public abstract class BaseService<T> : IService<T> where T : class {
        public PageInfo PageInfo { get; set; }
        public int NumberOfObjectsPerPage { get; set; } = 8;

        public abstract Task CreateAsync(T item);
        public abstract Task DeleteAsync(int id);
        public abstract Task DeleteAllAsync();
        public abstract void Dispose();
        public abstract Task EditAsync(T item);
        public abstract Task<T> FindByIdAsync(int? id);
        public abstract IEnumerable<T> Get(EmployeeFilter filter);
        public abstract Task<IEnumerable<T>> GetAllAsync();
        public abstract Task TestCreateAsync(int num, string path);
        public abstract Task TestReadAsync(int num, string path, int val);
        public abstract Task TestUpdateAsync(int num, string path);
        public abstract Task TestDeleteAsync(int num, string path);
        // Paging
        public IEnumerable<T> GetPage(IEnumerable<T> col, int page) {
            PageInfo = new PageInfo { PageNumber = page, PageSize = NumberOfObjectsPerPage, TotalItems = col.Count() };
            if (page < 1) PageInfo.PageNumber = 1;
            if (page > PageInfo.TotalPages) PageInfo.PageNumber = PageInfo.TotalPages;
            IEnumerable<T> pageEntities = col.Skip((PageInfo.PageNumber - 1) * NumberOfObjectsPerPage).Take(NumberOfObjectsPerPage);
            return pageEntities;
        }
    }
}
