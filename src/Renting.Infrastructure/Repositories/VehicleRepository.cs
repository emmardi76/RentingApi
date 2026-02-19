using Microsoft.EntityFrameworkCore;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;
using Renting.Infrastructure.Persistence;

namespace Renting.Infrastructure.Repositories
{
    public sealed class VehicleRepository : IVehicleRepository
    {
        private readonly RentingDbContext _dbContext;

        public VehicleRepository(RentingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            await _dbContext.Vehicles.AddAsync(vehicle, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Vehicle?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Vehicles.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<IEnumerable<Vehicle>> GetAvailableAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.Vehicles
                .AsNoTracking()
                .Where(v => v.IsAvailable)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateAsync(Vehicle vehicle, CancellationToken cancellationToken = default)
        {
            _dbContext.Vehicles.Update(vehicle);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}