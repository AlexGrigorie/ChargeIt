using System.ComponentModel.DataAnnotations;

namespace ChargeIt.Models
{
    public class CarOwnerViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        public ICollection<CarDbModel> Cars { get; set; }
    }
}
