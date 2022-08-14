namespace ChargeIt.Models
{
    public class ChargeMachineDbModel
    {
        public int Id { get; set; }
        public Guid Code { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public int Number { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }

    }
}
