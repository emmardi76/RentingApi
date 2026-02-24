using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;

namespace Renting.Application.Queries.GetAvailableVehicles
{
    public sealed class GetAvailableVehiclesQueryHandler : IRequestHandler<GetAvailableVehiclesQuery, IEnumerable<Vehicle>>
    {
        private readonly IVehicleRepository _vehicleRepository;

        public GetAvailableVehiclesQueryHandler(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
        }

        public async Task<IEnumerable<Vehicle>> Handle(GetAvailableVehiclesQuery request, CancellationToken cancellationToken)
        {
            return await _vehicleRepository.GetAvailableAsync(cancellationToken);
        }
    }
}