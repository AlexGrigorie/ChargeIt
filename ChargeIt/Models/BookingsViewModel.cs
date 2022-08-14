using System.ComponentModel.DataAnnotations;

namespace ChargeIt.Models
{
    public class BookingsViewModel
    {
        public List<DropDownViewModel> ChargeMachines { get; set; }
        public List<DropDownViewModel> Cars { get; set; }
        [Display(Name = "Charge Machine")]
        [Required(ErrorMessage = "Please select a valid station")]
        public int? ChargeMachineId { get; set; }
        [Display(Name = "Car")]
        [Required(ErrorMessage = "Please select a valid car")]
        public int? CarId { get; set; }
        public DateTime? Date { get; set; }
        [Required(ErrorMessage = "Please select a valid interval")]
        [Display(Name = "Available Intervals")]
        public int? IntervalHour { get; set; }
    }
}
