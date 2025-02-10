using CleanArchitecture.Infrastructure.Configurations;

namespace CleanArchitecture.Infrastructure.Contexts;

public class WriteDbContext(DbContextOptions<WriteDbContext> opt) : DbContext(opt)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("public", "citext");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEntityConfiguration).Assembly);
    }
}
