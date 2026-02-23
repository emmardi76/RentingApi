using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Renting.Infrastructure.Persistence;

namespace Renting.Api.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly RentingDbContext _context;

        public DatabaseHealthCheck(RentingDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (_context.Database.IsInMemory())
                {
                    return HealthCheckResult.Healthy("In-Memory database");
                }

                await _context.Database.CanConnectAsync(cancellationToken);
                return HealthCheckResult.Healthy("PostgreSQL connected");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "Database unavailable",
                    exception: ex
                );
            }
        }
    }
}