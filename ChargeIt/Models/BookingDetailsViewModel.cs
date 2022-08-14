namespace ChargeIt.Models
{
    public class BookingDetailsViewModel
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public Guid BookingCode { get; set; }
        public int ChargeMachineId { get; set; }
        public int CarId { get; set; }
        public string PlateNumber { get; set; }
        public Guid StationCode { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public int? Number { get; set; }
    }
}
