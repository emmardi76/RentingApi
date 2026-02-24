namespace Renting.Domain.Entities
{
    public sealed class Vehicle
    {
        public Guid Id { get; set; }
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}