using System;

namespace Renting.Domain.Exceptions
{
    public sealed class VehicleNotFoundException : Exception
    {
        public Guid VehicleId { get; }

        public VehicleNotFoundException(Guid vehicleId)
            : base($"Vehicle with ID '{vehicleId}' was not found.")
        {
            VehicleId = vehicleId;
        }
    }
}