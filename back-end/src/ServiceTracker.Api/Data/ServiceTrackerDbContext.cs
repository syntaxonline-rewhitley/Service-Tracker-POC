using Microsoft.EntityFrameworkCore;
using ServiceTracker.Api.Entities;

namespace ServiceTracker.Api.Data;

public class ServiceTrackerDbContext(DbContextOptions<ServiceTrackerDbContext> options) : DbContext(options)
{
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyContact> CompanyContacts => Set<CompanyContact>();

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
    }
}
