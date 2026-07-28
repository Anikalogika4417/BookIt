using BookIt.Models;
using Microsoft.EntityFrameworkCore;

namespace BookIt.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Business>(entity =>
        {
            entity.HasIndex(b => b.OwnerId);
            entity.HasIndex(b => b.Category);

            // 1:1 — a Business is owned by exactly one User, an owner has at most one Business.
            entity.HasOne(b => b.Owner)
                .WithOne(u => u.OwnedBusiness)
                .HasForeignKey<Business>(b => b.OwnerId);
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasIndex(s => s.BusinessId);
            entity.Property(s => s.Price).HasPrecision(10, 2);

            entity.HasOne(s => s.Business)
                .WithMany(b => b.Services)
                .HasForeignKey(s => s.BusinessId);
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasIndex(a => new { a.ServiceId, a.StartTime });
            entity.HasIndex(a => new { a.ClientId, a.StartTime });

            entity.HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId);

            entity.HasOne(a => a.Client)
                .WithMany(u => u.Appointments)
                .HasForeignKey(a => a.ClientId);
        });
    }
}
