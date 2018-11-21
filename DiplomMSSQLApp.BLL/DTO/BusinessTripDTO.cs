using System.Collections.Generic;

namespace DiplomMSSQLApp.BLL.DTO {
    public class BusinessTripDTO : BaseBusinessTripDTO {
        public virtual ICollection<EmployeeDTO> Employees { get; set; }

        public BusinessTripDTO() {
            Employees = new List<EmployeeDTO>();
        }
    }
}
