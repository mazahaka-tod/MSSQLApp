using System;

namespace DiplomMSSQLApp.BLL.BusinessModels
{
    // бизнес-модель для реализации Paging (постраничный вывод)
    public class PageInfo
    {
        public int PageNumber { get; set; }     // номер текущей страницы
        public int PageSize { get; set; }       // кол-во объектов на странице
        public int TotalItems { get; set; }     // всего объектов
        public int TotalPages                   // всего страниц
        {
            get { return (int)Math.Ceiling((decimal)TotalItems / PageSize); }
        }
        public int RealPages { get; set; }
    }
}
