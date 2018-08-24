using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models
{
    public class BusinessTripViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Код")]
        public string Name { get; set; }
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата начала")]
        public DateTime DateStart { get; set; }
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата окончания")]
        public DateTime DateEnd { get; set; }
        [Display(Name = "Место назначения")]
        public string Destination { get; set; }
        [Display(Name = "Цель командировки")]
        public string Purpose { get; set; }
        [Display(Name = "Список сотрудников")]
        public virtual ICollection<EmployeeViewModel> Employees { get; set; }

        public BusinessTripViewModel()
        {
            Employees = new List<EmployeeViewModel>();
        }
    }
}