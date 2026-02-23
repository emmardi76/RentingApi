using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Renting.Api.DTOs.Rentals;
using Renting.Application.Commands.RentVehicle;
using Renting.Application.Commands.ReturnVehicle;
using Renting.Domain.Exceptions;

namespace Renting.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RentalsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public RentalsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Creates a new rental (rent a vehicle)
        /// </summary>
        /// <param name="request">Rental creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The ID of the created rental</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<Guid>> CreateRental(
            [FromBody] CreateRentalRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = _mapper.Map<RentVehicleCommand>(request);
                
                // AÑADIDO: Log para ver el request completo
                Console.WriteLine($"🔍 DEBUG CONTROLLER - Request completo: {request}");

                var rentalId = await _mediator.Send(command, cancellationToken);

                return CreatedAtAction(
                    nameof(CreateRental),
                    new { id = rentalId },
                    rentalId
                );
            }
            catch (VehicleNotFoundException ex)
            {
                return NotFound(new { error = ex.Message, vehicleId = ex.VehicleId });
            }
            catch (VehicleNotAvailableException ex)
            {
                return Conflict(new { error = ex.Message, vehicleId = ex.VehicleId });
            }
            catch (CustomerHasActiveRentalException ex)
            {
                return Conflict(new { error = ex.Message, customerId = ex.CustomerId });
            }
            catch (InvalidRentalDateException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Returns a rented vehicle
        /// </summary>
        /// <param name="id">Rental ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content on success</returns>
        [HttpPost("{id}/return")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReturnVehicle(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new ReturnVehicleCommand(id, DateTime.UtcNow);
                await _mediator.Send(command, cancellationToken);

                return NoContent();
            }
            catch (RentalNotFoundException ex)
            {
                return NotFound(new { error = ex.Message, rentalId = ex.RentalId });
            }
            catch (RentalAlreadyReturnedException ex)
            {
                return Conflict(new { error = ex.Message, rentalId = ex.RentalId });
            }
            catch (InvalidRentalDateException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
