using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Renting.Api.DTOs.Vehicles;
using Renting.Tests.Host.Fixtures;

namespace Renting.Tests.Host.Controllers
{
    public class VehiclesControllerTests
    {
        [Fact]
        public async Task PostVehicles_ValidRequest_ReturnsCreated()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request = new CreateVehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/vehicles", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var vehicleId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, vehicleId);
        }

        [Fact]
        public async Task PostVehicles_ValidRequest_ReturnsValidGuid()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request = new CreateVehicleRequest
            {
                Make = "Honda",
                Model = "Civic",
                Year = 2022
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/vehicles", request);
            var vehicleId = await response.Content.ReadFromJsonAsync<Guid>();

            // Assert
            Assert.NotEqual(Guid.Empty, vehicleId);
            Assert.IsType<Guid>(vehicleId);
        }

        [Fact]
        public async Task PostVehicles_InvalidRequest_MissingMake_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request = new
            {
                Model = "Civic",
                Year = 2022
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/vehicles", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostVehicles_InvalidRequest_EmptyMake_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request = new CreateVehicleRequest
            {
                Make = "",
                Model = "Civic",
                Year = 2022
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/vehicles", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostVehicles_InvalidRequest_YearTooLow_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request = new CreateVehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 1800
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/vehicles", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostVehicles_InvalidRequest_YearTooHigh_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request = new CreateVehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2150
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/vehicles", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetVehiclesAvailable_NoVehicles_ReturnsEmptyArray()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/vehicles/available");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var vehicles = await response.Content.ReadFromJsonAsync<VehicleResponse[]>();
            Assert.NotNull(vehicles);
            Assert.Empty(vehicles);
        }

        [Fact]
        public async Task GetVehiclesAvailable_AfterCreatingVehicle_ReturnsVehicle()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var createRequest = new CreateVehicleRequest
            {
                Make = "Ford",
                Model = "Focus",
                Year = 2021
            };

            var createResponse = await client.PostAsJsonAsync("/api/vehicles", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // Act
            var getResponse = await client.GetAsync("/api/vehicles/available");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var vehicles = await getResponse.Content.ReadFromJsonAsync<VehicleResponse[]>();
            Assert.NotNull(vehicles);
            Assert.NotEmpty(vehicles);
            Assert.Contains(vehicles, v =>
                v.Make == "Ford" &&
                v.Model == "Focus" &&
                v.Year == 2021 &&
                v.IsAvailable
            );
        }

        [Fact]
        public async Task PostVehicles_MultipleRequests_CreatesMultipleVehicles()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var request1 = new CreateVehicleRequest
            {
                Make = "Toyota",
                Model = "Camry",
                Year = 2023
            };

            var request2 = new CreateVehicleRequest
            {
                Make = "Nissan",
                Model = "Altima",
                Year = 2022
            };

            // Act
            var response1 = await client.PostAsJsonAsync("/api/vehicles", request1);
            var response2 = await client.PostAsJsonAsync("/api/vehicles", request2);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response1.StatusCode);
            Assert.Equal(HttpStatusCode.Created, response2.StatusCode);

            var id1 = await response1.Content.ReadFromJsonAsync<Guid>();
            var id2 = await response2.Content.ReadFromJsonAsync<Guid>();

            Assert.NotEqual(id1, id2);
        }
    }
}