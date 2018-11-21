using System;

namespace DiplomMSSQLApp.BLL.DTO {
    public class BaseBusinessTripDTO {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public string Destination { get; set; }
        public string Purpose { get; set; }

        public override bool Equals(object obj) {
            return Id == (obj as BaseBusinessTripDTO).Id && Name == (obj as BaseBusinessTripDTO).Name &&
                DateStart == (obj as BaseBusinessTripDTO).DateStart && DateEnd == (obj as BaseBusinessTripDTO).DateEnd &&
                Destination == (obj as BaseBusinessTripDTO).Destination && Purpose == (obj as BaseBusinessTripDTO).Purpose;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}
