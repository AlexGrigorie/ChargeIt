using ChargeIt.Models;

namespace ChargeIt.Data.DbModels
{
    public class CarOwnerDbModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public ICollection<CarDbModel> Cars { get; set; }
    }
}
