using System;
using MediatR;

namespace Renting.Application.Commands.RentVehicle
{
    public sealed class RentVehicleCommand : IRequest<Guid>
    {
        public Guid VehicleId { get; init; }
        public string CustomerId { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }

        public RentVehicleCommand(Guid vehicleId, string customerId, DateTime startDate)
        {
            VehicleId = vehicleId;
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
            StartDate = startDate;
        }
    }
}