using System;
using MediatR;

namespace Renting.Application.Commands.ReturnVehicle
{
    public sealed class ReturnVehicleCommand : IRequest<Unit>
    {
        public Guid RentalId { get; init; }
        public DateTime ReturnDate { get; init; }

        public ReturnVehicleCommand(Guid rentalId, DateTime returnDate)
        {
            RentalId = rentalId;
            ReturnDate = returnDate;
        }
    }
}