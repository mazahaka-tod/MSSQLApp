using DiplomMSSQLApp.BLL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using DiplomMSSQLApp.BLL.BusinessModels;
using System.Threading.Tasks;
using System.Linq.Expressions;
using System;

namespace DiplomMSSQLApp.BLL.Services {
    public abstract class BaseService<T> : IService<T> where T : class {
        public virtual PageInfo PageInfo { get; set; }
        public int NumberOfObjectsPerPage { get; set; } = 10;

        public abstract Task<int> CountAsync();
        public abstract Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        public abstract Task CreateAsync(T item);
        public abstract Task DeleteAsync(int id);
        public abstract Task DeleteAllAsync();
        public abstract void Dispose();
        public abstract Task EditAsync(T item);
        public abstract Task ExportJsonAsync(string fullPath);
        public abstract Task<T> FindByIdAsync(int? id);
        public abstract Task<IEnumerable<T>> GetAllAsync();
        
        // Paging
        public virtual IEnumerable<T> GetPage(IEnumerable<T> col, int page) {
            PageInfo = new PageInfo { PageNumber = page, PageSize = NumberOfObjectsPerPage, TotalItems = col.Count() };
            if (page < 1) PageInfo.PageNumber = 1;
            if (page > PageInfo.TotalPages) PageInfo.PageNumber = PageInfo.TotalPages;
            IEnumerable<T> pageEntities = col.Skip((PageInfo.PageNumber - 1) * NumberOfObjectsPerPage).Take(NumberOfObjectsPerPage);
            return pageEntities;
        }
    }
}
