using Microsoft.EntityFrameworkCore;
using Renting.Domain.Entities;
using System;

namespace Renting.Infrastructure.Persistence
{
    public sealed class RentingDbContext : DbContext
    {
        public RentingDbContext(DbContextOptions<RentingDbContext> options)
            : base(options)
        {
        }

        public DbSet<Vehicle> Vehicles { get; set; } = null!;
        public DbSet<Rental> Rentals { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Vehicle>(b =>
            {
                b.HasKey(v => v.Id);
                b.Property(v => v.Make).HasMaxLength(100).IsRequired();
                b.Property(v => v.Model).HasMaxLength(100).IsRequired();
                b.Property(v => v.Year).IsRequired();
                b.Property(v => v.IsAvailable).IsRequired();

                // ✅ SEED DATA - Vehículos iniciales
                b.HasData(
                    new Vehicle
                    {
                        Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Make = "Toyota",
                        Model = "Corolla",
                        Year = 2023,
                        IsAvailable = true
                    },
                    new Vehicle
                    {
                        Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Make = "Honda",
                        Model = "Civic",
                        Year = 2024,
                        IsAvailable = true
                    },
                    new Vehicle
                    {
                        Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Make = "Ford",
                        Model = "Focus",
                        Year = 2022,
                        IsAvailable = true
                    },
                    new Vehicle
                    {
                        Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                        Make = "BMW",
                        Model = "X5",
                        Year = 2024,
                        IsAvailable = true
                    },
                    new Vehicle
                    {
                        Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                        Make = "Audi",
                        Model = "A4",
                        Year = 2023,
                        IsAvailable = true
                    }
                );
            });

            modelBuilder.Entity<Rental>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.CustomerId).HasMaxLength(100).IsRequired();
                
                b.Property(r => r.StartDate)
                    .IsRequired()
                    .HasColumnType("timestamp without time zone");
                
                b.Property(r => r.EndDate)
                    .HasColumnType("timestamp without time zone");
            });
        }
    }
}
