using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models
{
    public class PostListViewModel
    {
        public IEnumerable<PostViewModel> Posts { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}