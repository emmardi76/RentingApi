using System;

namespace Renting.Api.DTOs.Vehicles
{
    public sealed class VehicleResponse
    {
        public Guid Id { get; init; }
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }
        public bool IsAvailable { get; init; }
    }
}