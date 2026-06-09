using Microsoft.EntityFrameworkCore;

namespace ServiceTracker.Api.Data;

public class ServiceTrackerDbContext(DbContextOptions<ServiceTrackerDbContext> options) : DbContext(options)
{
    // DbSet properties will be added here as entities are created
    // Example: public DbSet<ServiceRecord> ServiceRecords => Set<ServiceRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
