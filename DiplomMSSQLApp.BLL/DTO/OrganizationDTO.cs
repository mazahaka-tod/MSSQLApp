namespace DiplomMSSQLApp.BLL.DTO {
    public class OrganizationDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LegalAddress { get; set; }
        public string ActualAddress { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string WWW { get; set; }
        public Requisites Requisites { get; set; }
        public Bank Bank { get; set; }
    }

    public class Requisites {
        public string OKPO { get; set; }
        public string INN { get; set; }
        public string OGRN { get; set; }
        public string OKATO { get; set; }
        public string OKOPF { get; set; }
        public string OKFS { get; set; }
        public string OKVED { get; set; }
        public string PFR { get; set; }
    }

    public class Bank {
        public string Name { get; set; }
        public string BankAccount { get; set; }
        public string BankCorrespondentAccount { get; set; }
        public string KPP { get; set; }
        public string BIK { get; set; }
    }
}
