using System;

namespace Renting.Api.DTOs.Rentals
{
    public sealed class RentalResponse
    {
        public Guid Id { get; init; }
        public Guid VehicleId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public bool Returned { get; init; }
    }
}