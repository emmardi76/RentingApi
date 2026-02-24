using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Renting.Infrastructure.Persistence
{
    public class RentingDbContextFactory : IDesignTimeDbContextFactory<RentingDbContext>
    {
        public RentingDbContext CreateDbContext(string[] args)
        {
            // Leer desde variable de entorno o usar default
            var envConnection = Environment.GetEnvironmentVariable("RENTING_CONNECTION");
            var connectionString = !string.IsNullOrWhiteSpace(envConnection)
                ? envConnection
                : "Host=localhost;Port=5433;Database=postgres;Username=renting_user;Password=renting_pass";

            var optionsBuilder = new DbContextOptionsBuilder<RentingDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new RentingDbContext(optionsBuilder.Options);
        }
    }
}
