using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Renting.Application.Commands.RentVehicle;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;
using Renting.Domain.Exceptions;

namespace Renting.Tests.Unit.Commands
{
    public class RentVehicleCommandHandlerTests
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IRentalRepository> _rentalRepositoryMock;
        private readonly RentVehicleCommandHandler _handler;

        public RentVehicleCommandHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _rentalRepositoryMock = new Mock<IRentalRepository>();
            _handler = new RentVehicleCommandHandler(
                _vehicleRepositoryMock.Object,
                _rentalRepositoryMock.Object
            );
        }

        [Fact]
        public void Constructor_NullVehicleRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new RentVehicleCommandHandler(null!, _rentalRepositoryMock.Object)
            );

            Assert.Equal("vehicleRepository", exception.ParamName);
        }

        [Fact]
        public void Constructor_NullRentalRepository_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(
                () => new RentVehicleCommandHandler(_vehicleRepositoryMock.Object, null!)
            );

            Assert.Equal("rentalRepository", exception.ParamName);
        }

        [Fact]
        public async Task Handle_VehicleNotFound_ThrowsVehicleNotFoundException()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var command = new RentVehicleCommand(
                vehicleId,
                "customer-123",
                DateTime.UtcNow.AddDays(1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vehicle?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<VehicleNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal(vehicleId, exception.VehicleId);
        }

        [Fact]
        public async Task Handle_VehicleNotAvailable_ThrowsVehicleNotAvailableException()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = false
            };

            var command = new RentVehicleCommand(
                vehicleId,
                "customer-123",
                DateTime.UtcNow.AddDays(1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<VehicleNotAvailableException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal(vehicleId, exception.VehicleId);
        }

        [Fact]
        public async Task Handle_CustomerHasActiveRental_ThrowsCustomerHasActiveRentalException()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-123";
            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = true
            };

            var activeRental = new Rental
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                CustomerId = customerId,
                StartDate = DateTime.UtcNow.AddDays(-2),
                Returned = false
            };

            var command = new RentVehicleCommand(
                vehicleId,
                customerId,
                DateTime.UtcNow.AddDays(1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRental);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<CustomerHasActiveRentalException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal(customerId, exception.CustomerId);
        }

        [Fact]
        public async Task Handle_StartDateInPast_ThrowsInvalidRentalDateException()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(
                vehicleId,
                "customer-123",
                DateTime.UtcNow.AddDays(-1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidRentalDateException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            Assert.Equal("Start date cannot be before today.", exception.Message);
        }

        [Fact]
        public async Task Handle_StartDateIsToday_CreatesRentalSuccessfully()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-123";
            var startDate = DateTime.UtcNow.Date;

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(vehicleId, customerId, startDate);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            Rental? capturedRental = null;
            _rentalRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
                .Callback<Rental, CancellationToken>((r, ct) => capturedRental = r)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.NotNull(capturedRental);
            Assert.Equal(vehicleId, capturedRental.VehicleId);
            Assert.Equal(customerId, capturedRental.CustomerId);
            Assert.Equal(startDate, capturedRental.StartDate);
            Assert.False(capturedRental.Returned);
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesRentalAndMarksVehicleUnavailable()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-123";
            var startDate = DateTime.UtcNow.AddDays(1);

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(vehicleId, customerId, startDate);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            Rental? capturedRental = null;
            _rentalRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
                .Callback<Rental, CancellationToken>((r, ct) => capturedRental = r)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.NotNull(capturedRental);
            Assert.Equal(vehicleId, capturedRental.VehicleId);
            Assert.Equal(customerId, capturedRental.CustomerId);
            Assert.Equal(startDate, capturedRental.StartDate);
            Assert.False(capturedRental.Returned);
            Assert.False(vehicle.IsAvailable);

            _vehicleRepositoryMock.Verify(
                x => x.UpdateAsync(vehicle, It.IsAny<CancellationToken>()),
                Times.Once
            );

            _rentalRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

       

        [Fact]
        public async Task Handle_ValidRequest_SetsReturnedToFalse()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-789";
            var startDate = DateTime.UtcNow.AddDays(2);

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Ford",
                Model = "Focus",
                Year = 2022,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(vehicleId, customerId, startDate);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            Rental? capturedRental = null;
            _rentalRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
                .Callback<Rental, CancellationToken>((r, ct) => capturedRental = r)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRental);
            Assert.False(capturedRental.Returned);
            Assert.Null(capturedRental.EndDate);
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsRepositoriesInCorrectOrder()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-order-test";
            var startDate = DateTime.UtcNow.AddDays(3);

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Mazda",
                Model = "3",
                Year = 2023,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(vehicleId, customerId, startDate);

            var callOrder = new System.Collections.Generic.List<string>();

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle)
                .Callback(() => callOrder.Add("GetVehicle"));

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null)
                .Callback(() => callOrder.Add("CheckActiveRental"));

            _rentalRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => callOrder.Add("AddRental"));

            _vehicleRepositoryMock
                .Setup(x => x.UpdateAsync(vehicle, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Callback(() => callOrder.Add("UpdateVehicle"));

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(4, callOrder.Count);
            Assert.Equal("GetVehicle", callOrder[0]);
            Assert.Equal("CheckActiveRental", callOrder[1]);
            Assert.Equal("AddRental", callOrder[2]);
            Assert.Equal("UpdateVehicle", callOrder[3]);
        }

        [Fact]
        public async Task Handle_VehicleNotFound_DoesNotCallOtherRepositories()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var command = new RentVehicleCommand(
                vehicleId,
                "customer-123",
                DateTime.UtcNow.AddDays(1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Vehicle?)null);

            // Act & Assert
            await Assert.ThrowsAsync<VehicleNotFoundException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            _rentalRepositoryMock.Verify(
                x => x.GetActiveByCustomerAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            _rentalRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            _vehicleRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_VehicleNotAvailable_DoesNotCreateRental()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = false
            };

            var command = new RentVehicleCommand(
                vehicleId,
                "customer-123",
                DateTime.UtcNow.AddDays(1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            // Act & Assert
            await Assert.ThrowsAsync<VehicleNotAvailableException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            _rentalRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            _vehicleRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_CustomerHasActiveRental_DoesNotCreateNewRental()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-123";
            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023,
                IsAvailable = true
            };

            var activeRental = new Rental
            {
                Id = Guid.NewGuid(),
                VehicleId = Guid.NewGuid(),
                CustomerId = customerId,
                StartDate = DateTime.UtcNow.AddDays(-2),
                Returned = false
            };

            var command = new RentVehicleCommand(
                vehicleId,
                customerId,
                DateTime.UtcNow.AddDays(1)
            );

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(activeRental);

            // Act & Assert
            await Assert.ThrowsAsync<CustomerHasActiveRentalException>(
                () => _handler.Handle(command, CancellationToken.None)
            );

            _rentalRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()),
                Times.Never
            );

            _vehicleRepositoryMock.Verify(
                x => x.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesRentalWithCorrectData()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-data-test";
            var startDate = DateTime.UtcNow.AddDays(7);

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "BMW",
                Model = "X5",
                Year = 2024,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(vehicleId, customerId, startDate);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            Rental? capturedRental = null;
            _rentalRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Rental>(), It.IsAny<CancellationToken>()))
                .Callback<Rental, CancellationToken>((r, ct) => capturedRental = r)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedRental);
            Assert.NotEqual(Guid.Empty, capturedRental.Id);
            Assert.Equal(vehicleId, capturedRental.VehicleId);
            Assert.Equal(customerId, capturedRental.CustomerId);
            Assert.Equal(startDate, capturedRental.StartDate);
            Assert.False(capturedRental.Returned);
            Assert.Null(capturedRental.EndDate);
        }

        [Fact]
        public async Task Handle_ValidRequest_UpdatesVehicleAvailability()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var customerId = "customer-availability-test";
            var startDate = DateTime.UtcNow.AddDays(1);

            var vehicle = new Vehicle
            {
                Id = vehicleId,
                Make = "Audi",
                Model = "A4",
                Year = 2023,
                IsAvailable = true
            };

            var command = new RentVehicleCommand(vehicleId, customerId, startDate);

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicle);

            _rentalRepositoryMock
                .Setup(x => x.GetActiveByCustomerAsync(customerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Rental?)null);

            Vehicle? capturedVehicle = null;
            _vehicleRepositoryMock
                .Setup(x => x.UpdateAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
                .Callback<Vehicle, CancellationToken>((v, ct) => capturedVehicle = v)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedVehicle);
            Assert.False(capturedVehicle.IsAvailable);
            Assert.Equal(vehicleId, capturedVehicle.Id);

            _vehicleRepositoryMock.Verify(
                x => x.UpdateAsync(vehicle, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_CancellationRequested_PropagatesCancellation()
        {
            // Arrange
            var vehicleId = Guid.NewGuid();
            var command = new RentVehicleCommand(
                vehicleId,
                "customer-123",
                DateTime.UtcNow.AddDays(1)
            );

            var cts = new CancellationTokenSource();
            cts.Cancel();

            _vehicleRepositoryMock
                .Setup(x => x.GetByIdAsync(vehicleId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _handler.Handle(command, cts.Token)
            );
        }
    }
}
