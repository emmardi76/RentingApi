namespace Renting.Domain.Entities
{
    public sealed class Rental
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Returned { get; set; } = false;
    }
}