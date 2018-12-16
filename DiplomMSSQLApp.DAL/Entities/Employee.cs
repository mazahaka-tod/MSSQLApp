using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomMSSQLApp.DAL.Entities {
    public enum Gender { Мужской, Женский };

    public class Employee {
        public int Id { get; set; }
        public int PersonnelNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public Gender Gender { get; set; }
        public int Age { get; set; }

        public Birth Birth { get; set; }
        public Passport Passport { get; set; }
        public Contacts Contacts { get; set; }
        public Education Education { get; set; }
        public DateTime HireDate { get; set; }

        public int? PostId { get; set; }
        public Post Post { get; set; }

        public virtual ICollection<AnnualLeave> AnnualLeaves { get; set; }
        public virtual ICollection<BusinessTrip> BusinessTrips { get; set; }
        public Employee() {
            AnnualLeaves = new List<AnnualLeave>();
            BusinessTrips = new List<BusinessTrip>();
        }
    }

    [ComplexType]
    public class Birth {
        public DateTime BirthDate { get; set; }
        public string BirthPlace { get; set; }
    }

    [ComplexType]
    public class Passport {
        public string Series { get; set; }
        public string Number { get; set; }
        public string WhoIssued { get; set; }
        public DateTime? DateOfIssue { get; set; }
    }

    [ComplexType]
    public class Contacts {
        public string Address { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string Email { get; set; }
    }

    public enum Level { Нет, Высшее_Магистр, Высшее_Бакалавр, Высшее_Неоконченное, Среднее_Специальное, Среднее };

    [ComplexType]
    public class Education {
        public Level Level { get; set; }
        public string EducationalInstitution { get; set; }
        public string Faculty { get; set; }
        public string Specialization { get; set; }
        public int? YearEnd { get; set; }
        public string Qualification { get; set; }
        public string EducationDocument { get; set; }
    }
}
