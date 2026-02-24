using AutoMapper;
using Renting.Api.DTOs.Rentals;
using Renting.Api.DTOs.Vehicles;
using Renting.Application.Commands.CreateVehicle;
using Renting.Application.Commands.RentVehicle;
using Renting.Domain.Entities;

namespace Renting.Api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Vehicle mappings
            CreateMap<CreateVehicleRequest, CreateVehicleCommand>();
            CreateMap<Vehicle, VehicleResponse>();
            
            // Rental mappings
            CreateMap<CreateRentalRequest, RentVehicleCommand>();
        }
    }
}
