using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomMSSQLApp.WEB.Models {
    public enum Gender { Мужской, Женский };

    public class EmployeeViewModel {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется ввести табельный номер")]
        [Display(Name = "Табельный номер")]
        public int PersonnelNumber { get; set; }

        [Required(ErrorMessage = "Требуется ввести фамилию")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Требуется ввести имя")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; }

        [Display(Name = "Отчество")]
        public string Patronymic { get; set; }

        [Required(ErrorMessage = "Требуется ввести пол")]
        [Display(Name = "Пол")]
        public Gender Gender { get; set; }

        [Display(Name = "Возраст")]
        public int Age { get; set; }

        public Birth Birth { get; set; }
        public Passport Passport { get; set; }
        public Contacts Contacts { get; set; }
        public Education Education { get; set; }

        [Required(ErrorMessage = "Требуется ввести дату приема на работу")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата приёма на работу")]
        public DateTime HireDate { get; set; }

        [Display(Name = "Название должности")]
        public int? PostId { get; set; }
        public PostViewModel Post { get; set; }

        [Display(Name = "Список командировок")]
        public virtual ICollection<BaseBusinessTripViewModel> BusinessTrips { get; set; }

        public EmployeeViewModel() {
            BusinessTrips = new List<BaseBusinessTripViewModel>();
        }
    }

    [ComplexType]
    public class Birth {
        [Required(ErrorMessage = "Требуется ввести дату рождения")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата рождения")]
        public DateTime BirthDate { get; set; }
        [Display(Name = "Место рождения")]
        public string BirthPlace { get; set; }
    }

    [ComplexType]
    public class Passport {
        [Display(Name = "Серия")]
        public string Series { get; set; }
        [Display(Name = "Номер")]
        public string Number { get; set; }
        [Display(Name = "Кем выдан")]
        public string WhoIssued { get; set; }
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата выдачи")]
        public DateTime? DateOfIssue { get; set; }
    }

    [ComplexType]
    public class Contacts {
        [Display(Name = "Адрес")]
        public string Address { get; set; }
        [Display(Name = "Домашний телефон")]
        public string HomePhone { get; set; }
        [Display(Name = "Мобильный телефон")]
        public string MobilePhone { get; set; }
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }
    }

    public enum Level { Нет, Высшее_Магистр, Высшее_Бакалавр, Высшее_Неоконченное, Среднее_Специальное, Среднее };

    [ComplexType]
    public class Education {
        [Display(Name = "Уровень")]
        public Level Level { get; set; }
        [Display(Name = "Образовательное учреждение")]
        public string EducationalInstitution { get; set; }
        [Display(Name = "Факультет")]
        public string Faculty { get; set; }
        [Display(Name = "Специализация")]
        public string Specialization { get; set; }
        [Display(Name = "Год окончания")]
        public int? YearEnd { get; set; }
        [Display(Name = "Квалификация")]
        public string Qualification { get; set; }
        [Display(Name = "Документ об образовании")]
        public string EducationDocument { get; set; }
    }
}
