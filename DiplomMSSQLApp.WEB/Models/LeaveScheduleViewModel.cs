using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models {
    public class LeaveScheduleViewModel {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется ввести номер графика")]
        [Display(Name = "Номер")]
        public int Number { get; set; }

        [Required(ErrorMessage = "Требуется ввести год графика")]
        [Range(1900, 2100, ErrorMessage = "Некорректное значение")]
        [Display(Name = "Год")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Требуется ввести дату составления графика")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата составления графика")]
        public DateTime DateOfPreparation { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Дата утверждения графика")]
        public DateTime? DateOfApproval { get; set; }

        public virtual ICollection<AnnualLeaveViewModel> AnnualLeaves { get; set; }
        public LeaveScheduleViewModel() {
            AnnualLeaves = new List<AnnualLeaveViewModel>();
        }
    }
}
