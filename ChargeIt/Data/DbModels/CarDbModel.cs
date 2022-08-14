using ChargeIt.Data.DbModels;

namespace ChargeIt.Models
{
    public class CarDbModel
    {
        public int Id { get; set; }
        public string PlateNumber { get; set; }
        public CarOwnerDbModel Owner { get; set; }
        public int? OwnerId { get; set; }
    }
}
