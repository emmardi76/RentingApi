using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Renting.Api.Controllers;
using Renting.Api.DTOs.Vehicles;
using Renting.Application.Commands.CreateVehicle;
using Renting.Application.Queries.GetAvailableVehicles;
using Renting.Domain.Entities;

namespace Renting.Api.Tests.Controllers
{
    public class VehiclesControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly VehiclesController _controller;

        public VehiclesControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _mapperMock = new Mock<IMapper>();
            _controller = new VehiclesController(_mediatorMock.Object, _mapperMock.Object);
        }

        [Fact]
        public async Task CreateVehicle_ValidRequest_ReturnsCreatedWithVehicleId()
        {
            // Arrange
            var request = new CreateVehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023
            };

            var command = new CreateVehicleCommand(request.Make, request.Model, request.Year);
            var expectedId = Guid.NewGuid();

            _mapperMock
                .Setup(x => x.Map<CreateVehicleCommand>(request))
                .Returns(command);

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<CreateVehicleCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedId);

            // Act
            var result = await _controller.CreateVehicle(request, CancellationToken.None);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(expectedId, createdResult.Value);
            Assert.Equal(nameof(VehiclesController.CreateVehicle), createdResult.ActionName);
        }

        [Fact]
        public async Task GetAvailableVehicles_ReturnsOkWithVehiclesList()
        {
            // Arrange
            var vehicles = new List<Vehicle>
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

            var vehicleResponses = vehicles.Select(v => new VehicleResponse
            {
                Id = v.Id,
                Make = v.Make,
                Model = v.Model,
                Year = v.Year,
                IsAvailable = v.IsAvailable
            }).ToList();

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAvailableVehiclesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(vehicles);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<VehicleResponse>>(vehicles))
                .Returns(vehicleResponses);

            // Act
            var result = await _controller.GetAvailableVehicles(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVehicles = Assert.IsAssignableFrom<IEnumerable<VehicleResponse>>(okResult.Value);
            Assert.Equal(2, returnedVehicles.Count());
        }

        [Fact]
        public async Task GetAvailableVehicles_NoVehicles_ReturnsEmptyList()
        {
            // Arrange
            var emptyList = new List<Vehicle>();
            var emptyResponseList = new List<VehicleResponse>();

            _mediatorMock
                .Setup(x => x.Send(It.IsAny<GetAvailableVehiclesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(emptyList);

            _mapperMock
                .Setup(x => x.Map<IEnumerable<VehicleResponse>>(emptyList))
                .Returns(emptyResponseList);

            // Act
            var result = await _controller.GetAvailableVehicles(CancellationToken.None);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedVehicles = Assert.IsAssignableFrom<IEnumerable<VehicleResponse>>(okResult.Value);
            Assert.Empty(returnedVehicles);
        }
    }
}