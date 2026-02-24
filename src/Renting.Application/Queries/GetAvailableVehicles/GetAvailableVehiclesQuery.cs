using System.Collections.Generic;
using MediatR;
using Renting.Domain.Entities;

namespace Renting.Application.Queries.GetAvailableVehicles
{
    public sealed class GetAvailableVehiclesQuery : IRequest<IEnumerable<Vehicle>>
    {
    }
}