using System;
using System.ComponentModel.DataAnnotations;

namespace Renting.Api.DTOs.Rentals
{
    public sealed class CreateRentalRequest
    {
        [Required(ErrorMessage = "VehicleId is required")]
        public Guid VehicleId { get; init; }

        [Required(ErrorMessage = "CustomerId is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "CustomerId must be between 1 and 100 characters")]
        public string CustomerId { get; init; } = string.Empty;

        [Required(ErrorMessage = "StartDate is required")]
        public DateTime StartDate { get; init; }
    }
}