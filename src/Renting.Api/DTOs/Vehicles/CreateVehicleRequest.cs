using System.ComponentModel.DataAnnotations;

namespace Renting.Api.DTOs.Vehicles
{
    public sealed class CreateVehicleRequest
    {
        [Required(ErrorMessage = "Make is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Make must be between 1 and 100 characters")]
        public string Make { get; init; } = string.Empty;

        [Required(ErrorMessage = "Model is required")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Model must be between 1 and 100 characters")]
        public string Model { get; init; } = string.Empty;

        [Range(1900, 2100, ErrorMessage = "Year must be between 1900 and 2100")]
        public int Year { get; init; }
    }
}