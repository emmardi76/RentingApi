using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Renting.Application.Interfaces;
using Renting.Domain.Exceptions;

namespace Renting.Application.Commands.ReturnVehicle
{
    public sealed class ReturnVehicleCommandHandler : IRequestHandler<ReturnVehicleCommand, Unit>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public ReturnVehicleCommandHandler(
            IRentalRepository rentalRepository,
            IVehicleRepository vehicleRepository)
        {
            _rentalRepository = rentalRepository ?? throw new ArgumentNullException(nameof(rentalRepository));
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        }

        public async Task<Unit> Handle(ReturnVehicleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que el alquiler existe
            var rental = await _rentalRepository.GetByIdAsync(request.RentalId, cancellationToken);
            if (rental == null)
            {
                throw new RentalNotFoundException(request.RentalId);
            }

            // 2. Validar que el alquiler no ha sido devuelto previamente
            if (rental.Returned)
            {
                throw new RentalAlreadyReturnedException(request.RentalId);
            }

            // 3. Validar que la fecha de devolución no es anterior a la fecha de inicio
            if (request.ReturnDate < rental.StartDate)
            {
                throw new InvalidRentalDateException("Return date cannot be before start date.");
            }

            // 4. Marcar el alquiler como devuelto
            rental.EndDate = request.ReturnDate;
            rental.Returned = true;
            await _rentalRepository.UpdateAsync(rental, cancellationToken);

            // 5. Marcar el vehículo como disponible
            var vehicle = await _vehicleRepository.GetByIdAsync(rental.VehicleId, cancellationToken);
            if (vehicle != null)
            {
                vehicle.IsAvailable = true;
                await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
            }

            return Unit.Value;
        }
    }
}
