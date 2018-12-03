namespace DiplomMSSQLApp.BLL.BusinessModels {
    public class BusinessTripFilter {
        public string[] Code { get; set; }
        public string DateStart { get; set; }
        public string DateEnd { get; set; }
        public string[] Destination { get; set; }

        public bool IsAntiFilter { get; set; }
        public string SortField { get; set; }
        public string SortOrder { get; set; }
        public string[] Columns { get; set; }

        public BusinessTripFilter() {
            Columns = new string[] { "0", "1", "2", "3" };
        }
    }
}
