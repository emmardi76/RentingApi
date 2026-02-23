using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Renting.Api.Controllers;
using Renting.Api.DTOs.Rentals;
using Renting.Application.Commands.RentVehicle;
using Renting.Application.Commands.ReturnVehicle;
using Renting.Domain.Exceptions;

namespace Renting.Api.Tests.Controllers
{
    public class RentalsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly RentalsController _controller;

        public RentalsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _mapperMock = new Mock<IMapper>();
            _controller = new RentalsController(_mediatorMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateRental_ValidRequest_ReturnsCreatedWithRentalId()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var request = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-123",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var command = new RentVehicleCommand(
                request.VehicleId,
                request.CustomerId,
                request.StartDate
            );

            var expectedRentalId = Guid.NewGuid();

            _mapperMock
                .Setup(x => x.Map<RentVehicleCommand>(request))
                .Returns(command);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<RentVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedRentalId);

            // Act
            var result = await _controller.CreateRental(request, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(expectedRentalId, createdResult.Value);
        }

        [Fact]
        public async Task CreateRental_VehicleNotFound_ReturnsNotFound()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var request = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-123",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var command = new RentVehicleCommand(
                request.VehicleId,
                request.CustomerId,
                request.StartDate
            );

            _mapperMock
                .Setup(x => x.Map<RentVehicleCommand>(request))
                .Returns(command);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<RentVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new VehicleNotFoundException(vehicleId));

            // Act
            var result = await _controller.CreateRental(request, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task CreateRental_VehicleNotAvailable_ReturnsConflict()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var request = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-123",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var command = new RentVehicleCommand(
                request.VehicleId,
                request.CustomerId,
                request.StartDate
            );

            _mapperMock
                .Setup(x => x.Map<RentVehicleCommand>(request))
                .Returns(command);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<RentVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new VehicleNotAvailableException(vehicleId));

            // Act
            var result = await _controller.CreateRental(request, CancellationToken.None);

            // Assert
            Assert.IsType<ConflictObjectResult>(result.Result);
        }

        [Fact]
        public async Task ReturnVehicle_ValidRequest_ReturnsNoContent()
        {
            // Arrange
            var rentalId = Guid.NewGuid();

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<ReturnVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Unit.Value);

            // Act
            var result = await _controller.ReturnVehicle(rentalId, CancellationToken.None);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task ReturnVehicle_RentalNotFound_ReturnsNotFound()
        {
            // Arrange
            var rentalId = Guid.NewGuid();

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<ReturnVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RentalNotFoundException(rentalId));

            // Act
            var result = await _controller.ReturnVehicle(rentalId, CancellationToken.None);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task ReturnVehicle_AlreadyReturned_ReturnsConflict()
        {
            // Arrange
            var rentalId = Guid.NewGuid();

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<ReturnVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new RentalAlreadyReturnedException(rentalId));

            // Act
            var result = await _controller.ReturnVehicle(rentalId, CancellationToken.None);

            // Assert
            Assert.IsType<ConflictObjectResult>(result);
        }
    }
}