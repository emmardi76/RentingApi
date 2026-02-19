using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Renting.Infrastructure.Persistence
{
    public sealed class RentingDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RentingDbContext>
    {
        public RentingDbContext CreateDbContext(string[] args)
        {
            // Cadena de conexión usada por las herramientas en tiempo de diseño.
            // Usa localhost para migraciones locales; puede leerse desde una variable de entorno si se desea.
            var envConnection = Environment.GetEnvironmentVariable("RENTING_CONNECTION");
            var connectionString = !string.IsNullOrWhiteSpace(envConnection)
                ? envConnection
                : "Host=localhost;Database=renting_db;Username=renting_user;Password=renting_pass";

            var optionsBuilder = new DbContextOptionsBuilder<RentingDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new RentingDbContext(optionsBuilder.Options);
        }
    }
}