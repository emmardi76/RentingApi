using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Renting.Application.Commands.CreateVehicle;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;

namespace Renting.Tests.Unit.Commands
{
    public class CreateVehicleCommandHandlerTests
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly CreateVehicleCommandHandler _handler;

        public CreateVehicleCommandHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _handler = new CreateVehicleCommandHandler(_vehicleRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ValidCommand_CreatesVehicleAndReturnsId()
        {
            // Arrange
            var command = new CreateVehicleCommand("Toyota", "Corolla", 2023);

            Vehicle? capturedVehicle = null;
            _vehicleRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
                .Callback<Vehicle, CancellationToken>((v, ct) => capturedVehicle = v)
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(Guid.Empty, result);
            Assert.NotNull(capturedVehicle);
            Assert.Equal("Toyota", capturedVehicle.Make);
            Assert.Equal("Corolla", capturedVehicle.Model);
            Assert.Equal(2023, capturedVehicle.Year);
            Assert.True(capturedVehicle.IsAvailable);

            _vehicleRepositoryMock.Verify(
                x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task Handle_ValidCommand_VehicleIsAvailableByDefault()
        {
            // Arrange
            var command = new CreateVehicleCommand("Honda", "Civic", 2022);

            Vehicle? capturedVehicle = null;
            _vehicleRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
                .Callback<Vehicle, CancellationToken>((v, ct) => capturedVehicle = v)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(capturedVehicle);
            Assert.True(capturedVehicle.IsAvailable);
        }

        [Fact]
        public async Task Handle_ValidCommand_GeneratesUniqueId()
        {
            // Arrange
            var command = new CreateVehicleCommand("Ford", "Focus", 2021);

            var ids = new System.Collections.Generic.List<Guid>();
            _vehicleRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Vehicle>(), It.IsAny<CancellationToken>()))
                .Callback<Vehicle, CancellationToken>((v, ct) => ids.Add(v.Id))
                .Returns(Task.CompletedTask);

            // Act
            var result1 = await _handler.Handle(command, CancellationToken.None);
            var result2 = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(result1, result2);
            Assert.NotEqual(ids[0], ids[1]);
        }
    }
}