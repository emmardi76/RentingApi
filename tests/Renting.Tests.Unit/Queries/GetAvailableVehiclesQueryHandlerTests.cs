using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Renting.Application.Queries.GetAvailableVehicles;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;

namespace Renting.Tests.Unit.Queries
{
    public class GetAvailableVehiclesQueryHandlerTests
    {
        private readonly Mock<IVehicleRepository> _vehicleRepositoryMock;
        private readonly GetAvailableVehiclesQueryHandler _handler;

        public GetAvailableVehiclesQueryHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IVehicleRepository>();
            _handler = new GetAvailableVehiclesQueryHandler(_vehicleRepositoryMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsAvailableVehicles()
        {
            // Arrange
            var availableVehicles = new List<Vehicle>
            {
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Make = "Toyota",
                    Model = "Corolla",
                    Year = 2023,
                    IsAvailable = true
                },
                new Vehicle
                {
                    Id = Guid.NewGuid(),
                    Make = "Honda",
                    Model = "Civic",
                    Year = 2022,
                    IsAvailable = true
                }
            };

            _vehicleRepositoryMock
                .Setup(x => x.GetAvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(availableVehicles);

            var query = new GetAvailableVehiclesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.All(result, v => Assert.True(v.IsAvailable));
        }

        [Fact]
        public async Task Handle_NoAvailableVehicles_ReturnsEmptyList()
        {
            // Arrange
            _vehicleRepositoryMock
                .Setup(x => x.GetAvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Vehicle>());

            var query = new GetAvailableVehiclesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_CallsRepositoryOnce()
        {
            // Arrange
            _vehicleRepositoryMock
                .Setup(x => x.GetAvailableAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Vehicle>());

            var query = new GetAvailableVehiclesQuery();

            // Act
            await _handler.Handle(query, CancellationToken.None);

            // Assert
            _vehicleRepositoryMock.Verify(
                x => x.GetAvailableAsync(It.IsAny<CancellationToken>()),
                Times.Once
            );
        }
    }
}