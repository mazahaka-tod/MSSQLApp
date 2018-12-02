using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models {
    public class PostListViewModel {
        public IEnumerable<PostViewModel> Posts { get; set; }
        public PostFilter Filter { get; set; }
        public PageInfo PageInfo { get; set; }
        public int NumberOfUnitsOnPage { get; set; }
        public double SalaryOnPage { get; set; }
        public int TotalNumberOfUnits { get; set; }
        public double TotalSalary { get; set; }
    }
}
