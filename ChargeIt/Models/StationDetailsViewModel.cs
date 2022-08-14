namespace ChargeIt.Models
{
    public class StationDetailsViewModel
    {
        public ChargeMachineViewModel ChargeMachine { get; set; }
        public List<BookingDetailsViewModel> Bookings { get; set; }
    }
}
