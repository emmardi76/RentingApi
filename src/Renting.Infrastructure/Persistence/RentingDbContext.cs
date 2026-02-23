using Microsoft.EntityFrameworkCore;
using Renting.Domain.Entities;

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
            });

            modelBuilder.Entity<Rental>(b =>
            {
                b.HasKey(r => r.Id);
                b.Property(r => r.CustomerId).HasMaxLength(100).IsRequired();
                
                // ✅ Forzar timestamp sin zona horaria
                b.Property(r => r.StartDate)
                    .IsRequired()
                    .HasColumnType("timestamp without time zone");
                
                b.Property(r => r.EndDate)
                    .HasColumnType("timestamp without time zone");
            });
        }
    }
}
