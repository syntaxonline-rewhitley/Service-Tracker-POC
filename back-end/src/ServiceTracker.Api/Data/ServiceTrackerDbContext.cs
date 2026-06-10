using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Data;

public class ServiceTrackerDbContext(DbContextOptions<ServiceTrackerDbContext> options) : IdentityDbContext<IdentityUser>(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyContact> CompanyContacts => Set<CompanyContact>();
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<ServiceTicket> ServiceTickets => Set<ServiceTicket>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(254);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.Website).HasMaxLength(2048);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<CompanyContact>(entity =>
        {
            entity.HasKey(e => new { e.CompanyId, e.ContactId });
            entity.Property(e => e.LinkedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Company)
                  .WithMany(c => c.CompanyContacts)
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Contact)
                  .WithMany(c => c.CompanyContacts)
                  .HasForeignKey(e => e.ContactId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Specialization).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        modelBuilder.Entity<ServiceTicket>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TicketNumber).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.TicketNumber).IsUnique();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ResolutionNotes).HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            entity.HasOne(e => e.Company)
                  .WithMany()
                  .HasForeignKey(e => e.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Contact)
                  .WithMany()
                  .HasForeignKey(e => e.ContactId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Technician)
                  .WithMany()
                  .HasForeignKey(e => e.TechnicianId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
