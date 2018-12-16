using System;
using System.ComponentModel.DataAnnotations;

namespace DiplomMSSQLApp.WEB.Models {
    public class AnnualLeaveViewModel {
        public int Id { get; set; }

        [Required(ErrorMessage = "Требуется ввести запланированную дату отпуска")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Запланированная дата отпуска")]
        public DateTime ScheduledDate { get; set; }

        [Required(ErrorMessage = "Требуется ввести запланированное количество дней отпуска")]
        [Range(1, 1000, ErrorMessage = "Некорректное значение")]
        [Display(Name = "Запланированное количество дней отпуска")]
        public int ScheduledNumberOfDays { get; set; }

        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Фактическая дата отпуска")]
        public DateTime? ActualDate { get; set; }

        [Range(1, 1000, ErrorMessage = "Некорректное значение")]
        [Display(Name = "Фактическое количество дней отпуска")]
        public int? ActualNumberOfDays { get; set; }

        public int? EmployeeId { get; set; }
        public EmployeeViewModel Employee { get; set; }

        public int? LeaveScheduleId { get; set; }
        public LeaveScheduleViewModel LeaveSchedule { get; set; }
    }
}
