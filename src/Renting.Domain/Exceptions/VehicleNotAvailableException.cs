using System;

namespace Renting.Domain.Exceptions
{
    public sealed class VehicleNotAvailableException : Exception
    {
        public Guid VehicleId { get; }

        public VehicleNotAvailableException(Guid vehicleId)
            : base($"Vehicle with ID '{vehicleId}' is not available for rent.")
        {
            VehicleId = vehicleId;
        }
    }
}