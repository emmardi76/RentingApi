using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renting.Api.DTOs.Vehicles;
using Renting.Application.Commands.CreateVehicle;
using Renting.Application.Queries.GetAvailableVehicles;

namespace Renting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VehiclesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public VehiclesController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Creates a new vehicle
        /// </summary>
        /// <param name="request">Vehicle creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The ID of the created vehicle</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> CreateVehicle(
            [FromBody] CreateVehicleRequest request,
            CancellationToken cancellationToken)
        {
            var command = _mapper.Map<CreateVehicleCommand>(request);
            var vehicleId = await _mediator.Send(command, cancellationToken);

            return CreatedAtAction(
                nameof(CreateVehicle),
                new { id = vehicleId },
                vehicleId
            );
        }

        /// <summary>
        /// Gets all available vehicles for rent
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available vehicles</returns>
        [HttpGet("available")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VehicleResponse>>> GetAvailableVehicles(
            CancellationToken cancellationToken)
        {
            var query = new GetAvailableVehiclesQuery();
            var vehicles = await _mediator.Send(query, cancellationToken);

            var response = _mapper.Map<IEnumerable<VehicleResponse>>(vehicles);

            return Ok(response);
        }
    }
}