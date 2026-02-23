using System;
using MediatR;

namespace Renting.Application.Commands.CreateVehicle
{
    public sealed class CreateVehicleCommand : IRequest<Guid>
    {
        public string Make { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public int Year { get; init; }

        public CreateVehicleCommand(string make, string model, int year)
        {
            Make = make ?? throw new ArgumentNullException(nameof(make));
            Model = model ?? throw new ArgumentNullException(nameof(model));
            Year = year;
        }
    }
}