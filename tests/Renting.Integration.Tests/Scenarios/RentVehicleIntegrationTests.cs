using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Renting.Api.DTOs.Rentals;
using Renting.Api.DTOs.Vehicles;
using Renting.Domain.Entities;
using Renting.Infrastructure.Persistence;
using Renting.Integration.Tests.Fixtures;

namespace Renting.Integration.Tests.Scenarios
{
    public class RentVehicleIntegrationTests : IClassFixture<RentingWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly RentingWebApplicationFactory _factory;

        public RentVehicleIntegrationTests(RentingWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task RentVehicle_CompleteFlow_Success()
        {
            // Arrange - Crear un vehículo primero
            var createVehicleRequest = new CreateVehicleRequest
            {
                Make = "Toyota",
                Model = "Corolla",
                Year = 2023
            };

            var createVehicleResponse = await _client.PostAsJsonAsync(
                "/api/vehicles",
                createVehicleRequest
            );

            Assert.Equal(HttpStatusCode.Created, createVehicleResponse.StatusCode);
            var vehicleId = await createVehicleResponse.Content.ReadFromJsonAsync<Guid>();

            // Act - Alquilar el vehículo
            var rentRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var rentResponse = await _client.PostAsJsonAsync("/api/rentals", rentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, rentResponse.StatusCode);
            var rentalId = await rentResponse.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, rentalId);

            // Verificar que el vehículo ya no está disponible
            var availableResponse = await _client.GetAsync("/api/vehicles/available");
            var availableVehicles = await availableResponse.Content.ReadFromJsonAsync<VehicleResponse[]>();

            Assert.NotNull(availableVehicles);
            Assert.DoesNotContain(availableVehicles, v => v.Id == vehicleId);
        }

        [Fact]
        public async Task RentVehicle_VehicleNotAvailable_ReturnsConflict()
        {
            // Arrange - Crear vehículo
            var vehicleId = await CreateVehicleAsync("Honda", "Civic", 2022);

            // Primer alquiler (exitoso)
            var firstRentRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var firstRentResponse = await _client.PostAsJsonAsync("/api/rentals", firstRentRequest);
            Assert.Equal(HttpStatusCode.Created, firstRentResponse.StatusCode);

            // Act - Intentar alquilar el mismo vehículo (debe fallar)
            var secondRentRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-002",
                StartDate = DateTime.UtcNow.AddDays(2)
            };

            var secondRentResponse = await _client.PostAsJsonAsync("/api/rentals", secondRentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, secondRentResponse.StatusCode);
        }

        [Fact]
        public async Task RentVehicle_CustomerHasActiveRental_ReturnsConflict()
        {
            // Arrange - Crear dos vehículos
            var vehicle1Id = await CreateVehicleAsync("Toyota", "Camry", 2023);
            var vehicle2Id = await CreateVehicleAsync("Nissan", "Altima", 2023);

            // Primer alquiler
            var firstRentRequest = new CreateRentalRequest
            {
                VehicleId = vehicle1Id,
                CustomerId = "customer-exclusive",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var firstRentResponse = await _client.PostAsJsonAsync("/api/rentals", firstRentRequest);
            Assert.Equal(HttpStatusCode.Created, firstRentResponse.StatusCode);

            // Act - Mismo cliente intenta alquilar otro vehículo
            var secondRentRequest = new CreateRentalRequest
            {
                VehicleId = vehicle2Id,
                CustomerId = "customer-exclusive",
                StartDate = DateTime.UtcNow.AddDays(2)
            };

            var secondRentResponse = await _client.PostAsJsonAsync("/api/rentals", secondRentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, secondRentResponse.StatusCode);
        }

        [Fact]
        public async Task RentVehicle_VehicleNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistentVehicleId = Guid.NewGuid();
            var rentRequest = new CreateRentalRequest
            {
                VehicleId = nonExistentVehicleId,
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/rentals", rentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RentVehicle_StartDateInPast_ReturnsBadRequest()
        {
            // Arrange
            var vehicleId = await CreateVehicleAsync("Ford", "Focus", 2021);
            var rentRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(-1) // Fecha en el pasado
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/rentals", rentRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ReturnVehicle_Success_VehicleBecomesAvailable()
        {
            // Arrange - Crear y alquilar vehículo
            var vehicleId = await CreateVehicleAsync("Mazda", "CX-5", 2023);
            var rentalId = await RentVehicleAsync(vehicleId, "customer-return-test");

            // Act - Devolver el vehículo
            var returnResponse = await _client.PostAsync(
                $"/api/rentals/{rentalId}/return",
                null
            );

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, returnResponse.StatusCode);

            // Verificar que el vehículo está disponible nuevamente
            var availableResponse = await _client.GetAsync("/api/vehicles/available");
            var availableVehicles = await availableResponse.Content.ReadFromJsonAsync<VehicleResponse[]>();

            Assert.NotNull(availableVehicles);
            Assert.Contains(availableVehicles, v => v.Id == vehicleId);
        }

        [Fact]
        public async Task ReturnVehicle_RentalNotFound_ReturnsNotFound()
        {
            // Arrange
            var nonExistentRentalId = Guid.NewGuid();

            // Act
            var response = await _client.PostAsync(
                $"/api/rentals/{nonExistentRentalId}/return",
                null
            );

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ReturnVehicle_AlreadyReturned_ReturnsConflict()
        {
            // Arrange - Crear, alquilar y devolver vehículo
            var vehicleId = await CreateVehicleAsync("Subaru", "Outback", 2023);
            var rentalId = await RentVehicleAsync(vehicleId, "customer-double-return");

            var firstReturnResponse = await _client.PostAsync(
                $"/api/rentals/{rentalId}/return",
                null
            );
            Assert.Equal(HttpStatusCode.NoContent, firstReturnResponse.StatusCode);

            // Act - Intentar devolver nuevamente
            var secondReturnResponse = await _client.PostAsync(
                $"/api/rentals/{rentalId}/return",
                null
            );

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, secondReturnResponse.StatusCode);
        }

        [Fact]
        public async Task RentVehicle_AfterReturn_CustomerCanRentAgain()
        {
            // Arrange - Crear dos vehículos
            var vehicle1Id = await CreateVehicleAsync("Kia", "Sportage", 2023);
            var vehicle2Id = await CreateVehicleAsync("Hyundai", "Tucson", 2023);

            // Alquilar primer vehículo
            var rental1Id = await RentVehicleAsync(vehicle1Id, "customer-multi-rent");

            // Devolver primer vehículo
            var returnResponse = await _client.PostAsync(
                $"/api/rentals/{rental1Id}/return",
                null
            );
            Assert.Equal(HttpStatusCode.NoContent, returnResponse.StatusCode);

            // Act - Alquilar segundo vehículo (debe tener éxito)
            var secondRentRequest = new CreateRentalRequest
            {
                VehicleId = vehicle2Id,
                CustomerId = "customer-multi-rent",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var secondRentResponse = await _client.PostAsJsonAsync(
                "/api/rentals",
                secondRentRequest
            );

            // Assert
            Assert.Equal(HttpStatusCode.Created, secondRentResponse.StatusCode);
        }

        // Helper methods
        private async Task<Guid> CreateVehicleAsync(string make, string model, int year)
        {
            var request = new CreateVehicleRequest
            {
                Make = make,
                Model = model,
                Year = year
            };

            var response = await _client.PostAsJsonAsync("/api/vehicles", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        private async Task<Guid> RentVehicleAsync(Guid vehicleId, string customerId)
        {
            var request = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = customerId,
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            var response = await _client.PostAsJsonAsync("/api/rentals", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Guid>();
        }
    }
}