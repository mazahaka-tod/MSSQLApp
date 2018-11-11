using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models {
    public class OrganizationViewModel {
        public int Id { get; set; }
        [Required(ErrorMessage = "Требуется ввести наименование организации")]
        [Display(Name = "Наименование организации")]
        public string Name { get; set; }
        [Display(Name = "Юридический адрес")]
        public string LegalAddress { get; set; }
        [Display(Name = "Фактический адрес")]
        public string ActualAddress { get; set; }
        [Display(Name = "Телефон")]
        public string Phone { get; set; }
        [Display(Name = "Факс")]
        public string Fax { get; set; }
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; }
        public string WWW { get; set; }
        public Requisites Requisites { get; set; }
        public Bank Bank { get; set; }
    }

    public class Requisites {
        public int Id { get; set; }
        [Display(Name = "ОКПО")]
        public string OKPO { get; set; }
        [Display(Name = "ИНН")]
        public string INN { get; set; }
        [Display(Name = "ОГРН")]
        public string OGRN { get; set; }
        [Display(Name = "ОКАТО")]
        public string OKATO { get; set; }
        [Display(Name = "ОКОПФ")]
        public string OKOPF { get; set; }
        [Display(Name = "ОКФС")]
        public string OKFS { get; set; }
        [Display(Name = "ОКВЭД")]
        public string OKVED { get; set; }
        [Display(Name = "ПФР")]
        public string PFR { get; set; }
    }

    public class Bank {
        [Display(Name = "Наименование банка")]
        public string Name { get; set; }
        [Display(Name = "р/с")]
        public string BankAccount { get; set; }
        [Display(Name = "к/с")]
        public string BankCorrespondentAccount { get; set; }
        [Display(Name = "КПП")]
        public string KPP { get; set; }
        [Display(Name = "БИК")]
        public string BIK { get; set; }
    }
}
