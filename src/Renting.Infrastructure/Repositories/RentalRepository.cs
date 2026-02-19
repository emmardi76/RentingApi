using Microsoft.EntityFrameworkCore;
using Renting.Application.Interfaces;
using Renting.Domain.Entities;
using Renting.Infrastructure.Persistence;

namespace Renting.Infrastructure.Repositories
{
    public sealed class RentalRepository : IRentalRepository
    {
        private readonly RentingDbContext _dbContext;

        public RentalRepository(RentingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(Rental rental, CancellationToken cancellationToken = default)
        {
            await _dbContext.Rentals.AddAsync(rental, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<Rental?> GetActiveByCustomerAsync(string customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Rentals
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId && !r.Returned)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Rentals.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default)
        {
            _dbContext.Rentals.Update(rental);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}