using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ComputerConfigContext : DbContext
{
    public ComputerConfigContext(DbContextOptions<ComputerConfigContext> options)
        : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Worker> Workers => Set<Worker>();
    public DbSet<Admin> Admins => Set<Admin>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();  

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ----- Customers -----
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(c => c.PhoneNumber).IsUnique();
            entity.HasIndex(c => c.Email).IsUnique();
            entity.HasIndex(c => c.PersonalId).IsUnique();

            entity.Property(c => c.FullName).HasMaxLength(150).IsRequired();
            entity.Property(c => c.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.PersonalId).HasMaxLength(30).IsRequired();
            entity.Property(c => c.PasswordHash).IsRequired();
        });

        // ----- Workers -----
        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasIndex(w => w.PhoneNumber).IsUnique();
            entity.HasIndex(w => w.Email).IsUnique();
            entity.HasIndex(w => w.PersonalId).IsUnique();

            entity.Property(w => w.FullName).HasMaxLength(150).IsRequired();
            entity.Property(w => w.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(w => w.Email).HasMaxLength(200);
            entity.Property(w => w.PersonalId).HasMaxLength(30).IsRequired();
            entity.Property(w => w.PasswordHash).IsRequired();
        });

        // ----- Admins -----
        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasIndex(a => a.PhoneNumber).IsUnique();
            entity.HasIndex(a => a.Email).IsUnique();
            entity.HasIndex(a => a.PersonalId).IsUnique();

            entity.Property(a => a.FullName).HasMaxLength(150).IsRequired();
            entity.Property(a => a.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(a => a.Email).HasMaxLength(200);
            entity.Property(a => a.PersonalId).HasMaxLength(30).IsRequired();
            entity.Property(a => a.PasswordHash).IsRequired();
        });

        // ----- Tickets -----
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasIndex(t => t.TrackingCode).IsUnique();

            entity.HasOne(t => t.Customer)
                .WithMany()
                .HasForeignKey(t => t.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(t => t.Worker)
                .WithMany(w => w.Tickets)
                .HasForeignKey(t => t.WorkerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ----- AuditLogs -----
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasIndex(a => a.Timestamp);
            entity.HasIndex(a => a.UserId);

            entity.Property(a => a.UserId).HasMaxLength(50);
            entity.Property(a => a.UserRole).HasMaxLength(30);
            entity.Property(a => a.UserName).HasMaxLength(150);
            entity.Property(a => a.HttpMethod).HasMaxLength(10);
            entity.Property(a => a.Path).HasMaxLength(300);
            entity.Property(a => a.IpAddress).HasMaxLength(45);
        });
    }
}