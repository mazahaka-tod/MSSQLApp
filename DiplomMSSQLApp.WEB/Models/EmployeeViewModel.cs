using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomMSSQLApp.WEB.Models
{
    public class EmployeeViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }
        [Display(Name = "Имя")]
        public string FirstName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Телефон")]
        public string PhoneNumber { get; set; }
        [Display(Name = "Адрес")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Требуется ввести дату приема на работу")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата приёма на работу")]
        public DateTime HireDate { get; set; }
        [Display(Name = "Зарплата")]
        public double? Salary { get; set; }
        [Display(Name = "Премия")]
        public double? Bonus { get; set; }
        [Display(Name = "Название должности")]
        public int? PostId { get; set; }
        public PostViewModel Post { get; set; }
        [Display(Name = "Название отдела")]
        public int? DepartmentId { get; set; }
        public DepartmentViewModel Department { get; set; }
        [Display(Name = "Список командировок")]
        public virtual ICollection<BusinessTripViewModel> BusinessTrips { get; set; }

        public EmployeeViewModel()
        {
            BusinessTrips = new List<BusinessTripViewModel>();
        }
    }
}