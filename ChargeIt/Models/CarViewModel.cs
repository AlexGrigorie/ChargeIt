using System.ComponentModel.DataAnnotations;

namespace ChargeIt.Models
{
    public class CarViewModel
    {
        public int Id { get; set; }
        public List<DropDownViewModel> Owners { get; set; }
        [Display(Name = "Owner Name")]
        public string OwnerName { get; set; }

        [Display(Name = "Car Owner")]
        [Required(ErrorMessage = "The owner is required.")]
        public int? CarOwnerId { get; set; }

        [RegularExpression(@"(AB|AG|AR|B|BC|BH|BN|BR|BT|BV|BZ|CJ|CL|CS|CT|CV|DB|DJ|GJ|GL|GR|HD|HR|IF|IL|IS|MH|MM|MS|NT|OT|PH|SB
                           |SJ|SM|SV|TL|TM|TR|VL|VN|VS)(?!(0))[0-9]{1,3}[A-Z]{3}",
                           ErrorMessage = "The plate number is invalid!")]
        [Display(Name = "Plate Number")]
        [Required]
        public string PlateNumber { get; set; }

    }
}
