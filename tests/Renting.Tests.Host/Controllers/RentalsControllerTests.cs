using System;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Renting.Api.DTOs.Vehicles;
using Renting.Api.DTOs.Rentals;
using Renting.Tests.Host.Fixtures;

namespace Renting.Tests.Host.Controllers
{
    public class RentalsControllerTests
    {
        [Fact]
        public async Task PostRentals_ValidRequest_ReturnsCreated()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicleId = await CreateVehicleAsync(client, "Toyota", "Corolla", 2023);

            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/rentals", rentalRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var rentalId = await response.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, rentalId);
        }

        [Fact]
        public async Task PostRentals_VehicleNotFound_ReturnsNotFound()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = Guid.NewGuid(),
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/rentals", rentalRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostRentals_VehicleNotAvailable_ReturnsConflict()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicleId = await CreateVehicleAsync(client, "Honda", "Civic", 2022);
            await RentVehicleAsync(client, vehicleId, "customer-001");

            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-002",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/rentals", rentalRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PostRentals_CustomerHasActiveRental_ReturnsConflict()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicle1Id = await CreateVehicleAsync(client, "Toyota", "Camry", 2023);
            var vehicle2Id = await CreateVehicleAsync(client, "Nissan", "Altima", 2023);

            await RentVehicleAsync(client, vehicle1Id, "customer-exclusive");

            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicle2Id,
                CustomerId = "customer-exclusive",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/rentals", rentalRequest);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task PostRentals_StartDateInPast_ReturnsBadRequest()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicleId = await CreateVehicleAsync(client, "Ford", "Focus", 2021);

            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-001",
                StartDate = DateTime.UtcNow.AddDays(-1)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/rentals", rentalRequest);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task PostRentalsReturn_ValidRequest_ReturnsNoContent()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicleId = await CreateVehicleAsync(client, "Mazda", "CX-5", 2023);
            
            // Crear rental con fecha de inicio HOY
            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-return",
                StartDate = DateTime.UtcNow.Date
            };
            
            var createResponse = await client.PostAsJsonAsync("/api/rentals", rentalRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var rentalId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            // Act
            var response = await client.PostAsync($"/api/rentals/{rentalId}/return", null);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task PostRentalsReturn_RentalNotFound_ReturnsNotFound()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var nonExistentRentalId = Guid.NewGuid();

            // Act
            var response = await client.PostAsync($"/api/rentals/{nonExistentRentalId}/return", null);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostRentalsReturn_AlreadyReturned_ReturnsConflict()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicleId = await CreateVehicleAsync(client, "Subaru", "Outback", 2023);
            
            // Crear rental con fecha de inicio HOY
            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-double",
                StartDate = DateTime.UtcNow.Date
            };
            
            var createResponse = await client.PostAsJsonAsync("/api/rentals", rentalRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var rentalId = await createResponse.Content.ReadFromJsonAsync<Guid>();
            Assert.NotEqual(Guid.Empty, rentalId);

            // Primera devolución
            var firstReturn = await client.PostAsync($"/api/rentals/{rentalId}/return", null);
            Assert.Equal(HttpStatusCode.NoContent, firstReturn.StatusCode);

            // Act - Segunda devolución
            var secondReturn = await client.PostAsync($"/api/rentals/{rentalId}/return", null);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, secondReturn.StatusCode);
        }

        [Fact]
        public async Task PostRentalsReturn_MakesVehicleAvailableAgain()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicleId = await CreateVehicleAsync(client, "Kia", "Sportage", 2023);
            
            // Crear rental con fecha de inicio HOY
            var rentalRequest = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = "customer-cycle",
                StartDate = DateTime.UtcNow.Date
            };
            
            var createResponse = await client.PostAsJsonAsync("/api/rentals", rentalRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            
            var rentalId = await createResponse.Content.ReadFromJsonAsync<Guid>();

            var availableBefore = await client.GetFromJsonAsync<VehicleResponse[]>("/api/vehicles/available");
            Assert.NotNull(availableBefore);
            Assert.DoesNotContain(availableBefore, v => v.Id == vehicleId);

            // Act
            await client.PostAsync($"/api/rentals/{rentalId}/return", null);

            // Assert
            var availableAfter = await client.GetFromJsonAsync<VehicleResponse[]>("/api/vehicles/available");
            Assert.NotNull(availableAfter);
            Assert.Contains(availableAfter, v => v.Id == vehicleId);
        }

        [Fact]
        public async Task PostRentals_AfterReturn_CustomerCanRentAgain()
        {
            // Arrange
            await using var factory = new RentingWebApplicationFactory();
            var client = factory.CreateClient();
            
            var vehicle1Id = await CreateVehicleAsync(client, "Hyundai", "Tucson", 2023);
            var vehicle2Id = await CreateVehicleAsync(client, "Volkswagen", "Tiguan", 2023);

            var rental1Request = new CreateRentalRequest
            {
                VehicleId = vehicle1Id,
                CustomerId = "customer-multi",
                StartDate = DateTime.UtcNow.Date
            };

            var rental1Response = await client.PostAsJsonAsync("/api/rentals", rental1Request);
            Assert.Equal(HttpStatusCode.Created, rental1Response.StatusCode);
            
            var rental1Id = await rental1Response.Content.ReadFromJsonAsync<Guid>();
            
            var returnResponse = await client.PostAsync($"/api/rentals/{rental1Id}/return", null);
            Assert.Equal(HttpStatusCode.NoContent, returnResponse.StatusCode);

            var rental2Request = new CreateRentalRequest
            {
                VehicleId = vehicle2Id,
                CustomerId = "customer-multi",
                StartDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/rentals", rental2Request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Helper methods
        private static async Task<Guid> CreateVehicleAsync(HttpClient client, string make, string model, int year)
        {
            var request = new CreateVehicleRequest
            {
                Make = make,
                Model = model,
                Year = year
            };

            var response = await client.PostAsJsonAsync("/api/vehicles", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Guid>();
        }

        private static async Task<Guid> RentVehicleAsync(HttpClient client, Guid vehicleId, string customerId, DateTime? startDate = null)
        {
            var request = new CreateRentalRequest
            {
                VehicleId = vehicleId,
                CustomerId = customerId,
                StartDate = startDate ?? DateTime.UtcNow.AddDays(1)
            };

            var response = await client.PostAsJsonAsync("/api/rentals", request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<Guid>();
        }
    }
}