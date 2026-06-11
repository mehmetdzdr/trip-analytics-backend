using Microsoft.EntityFrameworkCore;
using TripAnalytics.API.Domain.Entities;

namespace TripAnalytics.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}

        public DbSet<TripSummary> TripSummaries { get; set; } = null!;
        public DbSet<ZipZone> ZipZones { get; set; } = null!;

        public DbSet<ZonePairSummary> ZonePairSummaries { get; set; }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ZipZone
            modelBuilder.Entity<ZipZone>(entity =>
            {
                entity.HasKey(z => z.PostalCode);
                entity.Property(z => z.PostalCode).HasMaxLength(10);
                entity.Property(z => z.Borough).HasMaxLength(50);
                entity.Property(z => z.Name).HasMaxLength(100);
            });

            // TripSummary
            modelBuilder.Entity<TripSummary>(entity =>
            {
                entity.HasKey(t => t.PostalCode);

                entity.HasOne(t => t.ZipZone)
                      .WithOne()
                      .HasForeignKey<TripSummary>(t => t.PostalCode);

                entity.Property(t => t.PickupsByHour)
                       .HasColumnType("integer[]");

                entity.Property(t => t.DropoffsByHour)
                      .HasColumnType("integer[]");
            });

            modelBuilder.Entity<ZonePairSummary>(entity =>
            {
                entity.HasKey(z => new { z.PickupZip, z.DropoffZip }); // composite PK
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.username).IsUnique();
                entity.HasIndex(u => u.email).IsUnique();   
                entity.Property(u => u.username).HasMaxLength(50);
                entity.Property(u => u.email).HasMaxLength(100);
            });
        }
    }
}
