using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;

namespace Renting.Application.Commands.CreateVehicle
{
    public sealed class CreateVehicleCommandHandler : IRequestHandler<CreateVehicleCommand, Guid>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public CreateVehicleCommandHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        }

        public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
        {
            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                Make = request.Make,
                Model = request.Model,
                Year = request.Year,
                IsAvailable = true
            };

            await _vehicleRepository.AddAsync(vehicle, cancellationToken);

            return vehicle.Id;
        }
    }
}