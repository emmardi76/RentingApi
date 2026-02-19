using Renting.Domain.Entities;

namespace Renting.Application.Interfaces
{
    public interface IVehicleRepository
    {
        Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
        Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Vehicle>> GetAvailableAsync(CancellationToken cancellationToken = default);
        Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default);
    }
}