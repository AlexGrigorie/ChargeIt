namespace ChargeIt.Models
{
    public class BookingDbModel
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid Code { get; set; }
        public int CarId { get; set; }
        public CarDbModel Car { get; set; }
        public int ChargeMachineId { get; set; }
        public ChargeMachineDbModel ChargeMachine { get; set; }

    }
}
