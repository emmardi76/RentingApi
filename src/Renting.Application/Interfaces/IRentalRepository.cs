using System;
using System.Threading;
using System.Threading.Tasks;
using Renting.Domain.Entities;

namespace Renting.Application.Interfaces
{
    public interface IRentalRepository
    {
        Task AddAsync(Rental rental, CancellationToken cancellationToken = default);
        Task<Rental?> GetActiveByCustomerAsync(string customerId, CancellationToken cancellationToken = default);
        Task<Rental?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task UpdateAsync(Rental rental, CancellationToken cancellationToken = default);
    }
}   