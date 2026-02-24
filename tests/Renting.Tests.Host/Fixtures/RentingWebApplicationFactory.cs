using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Renting.Infrastructure.Persistence;

namespace Renting.Tests.Host.Fixtures
{
    public class RentingWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName;
        private IServiceScope? _scope;

        public RentingWebApplicationFactory()
        {
            // Generar un nombre único de base de datos para cada instancia de factory
            _databaseName = "TestDatabase_" + Guid.NewGuid();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remover el DbContext de PostgreSQL existente
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RentingDbContext>)
                );

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Agregar DbContext con base de datos en memoria con nombre único
                services.AddDbContext<RentingDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_databaseName);
                });
            });

            builder.UseEnvironment("Testing");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scope?.Dispose();
                _scope = null;
            }

            base.Dispose(disposing);
        }
    }
}