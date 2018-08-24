using DiplomMSSQLApp.BLL.BusinessModels;
using System.Collections.Generic;

namespace DiplomMSSQLApp.WEB.Models
{
    public class BusinessTripListViewModel
    {
        public IEnumerable<BusinessTripViewModel> BusinessTrips { get; set; }
        public PageInfo PageInfo { get; set; }
    }
}