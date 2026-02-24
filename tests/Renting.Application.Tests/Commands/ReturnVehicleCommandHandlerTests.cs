using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Renting.Application.Commands.ReturnVehicle;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;
using Renting.Domain.Exceptions;

namespace Renting.Application.Tests.Commands
{
    public class ReturnVehicleCommandHandlerTests
    {
        private readonly Mock<IRentalRepository> _rentalRepositoryMock;
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly ReturnVehicleCommandHandler _handler;

        public ReturnVehicleCommandHandlerTests()
        {
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _handler = new ReturnVehicleCommandHandler(
                _rentalRepositoryMock.Object,
                _vehicleRepositoryMock.Object
            );
        }

        [Fact]
        public async Task Handle_RentalNotFound_ThrowsRentalNotFoundException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var command = new ReturnVehicleCommand(rentalId, DateTime.UtcNow);

            _rentalRepositoryMock
                .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            // Act & Assert
            await Assert.ThrowsAsync<RentalNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_RentalAlreadyReturned_ThrowsRentalAlreadyReturnedException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var rental = new Rental
            {
                Id = rentalId,
                VehicleId = Guid.NewGuid(),
                CustomerId = "customer-123",
                StartDate = DateTime.UtcNow.AddDays(-5),
                EndDate = DateTime.UtcNow.AddDays(-1),
                Returned = true
            };

            var command = new ReturnVehicleCommand(rentalId, DateTime.UtcNow);

            _rentalRepositoryMock
                .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rental);

            // Act & Assert
            await Assert.ThrowsAsync<RentalAlreadyReturnedException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_ReturnDateBeforeStartDate_ThrowsInvalidRentalDateException()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-5);
            var rental = new Rental
            {
                Id = rentalId,
                VehicleId = Guid.NewGuid(),
                CustomerId = "customer-123",
                StartDate = startDate,
                Returned = false
            };

            var returnDate = startDate.AddDays(-1); // Fecha anterior al inicio
            var command = new ReturnVehicleCommand(rentalId, returnDate);

            _rentalRepositoryMock
                .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rental);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidRentalDateException>(
                () => _handler.Handle(command, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Handle_ValidReturn_MarksRentalAsReturnedAndVehicleAsAvailable()
        {
            // Arrange
            var rentalId = Guid.NewGuid();
            var vehicleId = Guid.NewGuid();
            var startDate = DateTime.UtcNow.AddDays(-5);
            var returnDate = DateTime.UtcNow;

            var rental = new Rental
            {
                Id = rentalId,
                VehicleId = vehicleId,
                CustomerId = "customer-123",
                StartDate = startDate,
                Returned = false
            };

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = false
            };

            var command = new ReturnVehicleCommand(rentalId, returnDate);

            _rentalRepositoryMock
                .Setup(x => x.GetByIdAsync(rentalId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(rental);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(rental.Returned);
            Assert.Equal(returnDate, rental.EndDate);
            Assert.True(vehicle.IsAvailable);

            _rentalRepositoryMock.Verify(
                x => x.UpdateAsync(rental, It.IsAny<CancellationToken>()),
                Times.Once
            );

            _vehicleRepositoryMock.Verify(
                x => x.UpdateAsync(vehicle, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}