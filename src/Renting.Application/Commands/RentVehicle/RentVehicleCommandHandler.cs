using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;
using Renting.Domain.Exceptions;

namespace Renting.Application.Commands.RentVehicle
{
    public sealed class RentVehicleCommandHandler : IRequestHandler<RentVehicleCommand, Guid>
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IRentalRepository _rentalRepository;

        public RentVehicleCommandHandler(
            IVehicleRepository vehicleRepository,
            IRentalRepository rentalRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _rentalRepository = rentalRepository ?? throw new ArgumentNullException(nameof(rentalRepository));
        }

        public async Task<Guid> Handle(RentVehicleCommand request, CancellationToken cancellationToken)
        {
            // 1. Validar que el vehículo existe
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken);
            if (vehicle == null)
            {
                throw new VehicleNotFoundException(request.VehicleId);
            }

            // 2. Validar que el vehículo está disponible
            if (!vehicle.IsAvailable)
            {
                throw new VehicleNotAvailableException(request.VehicleId);
            }

            // 3. Validar que el cliente no tiene alquileres activos
            var activeRental = await _rentalRepository.GetActiveByCustomerAsync(
                request.CustomerId,
                cancellationToken
            );

            if (activeRental != null)
            {
                throw new CustomerHasActiveRentalException(request.CustomerId);
            }

            // 4. Validar que la fecha de inicio no está muy en el pasado (permitir el día actual)
            if (request.StartDate.Date < DateTime.UtcNow.Date)
            {
                throw new InvalidRentalDateException("Start date cannot be before today.");
            }

            // 5. Crear el alquiler
            var rental = new Rental
            {
                Id = Guid.NewGuid(),
                VehicleId = request.VehicleId,
                CustomerId = request.CustomerId,
                StartDate = request.StartDate,
                Returned = false
            };

            await _rentalRepository.AddAsync(rental, cancellationToken);

            // 6. Marcar el vehículo como no disponible
            vehicle.IsAvailable = false;
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

            return rental.Id;
        }
    }
}
