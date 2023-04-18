using FamilyHubs.DataImporter.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FamilyHubs.DataImporter.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<PostCodeCache> PostCodeCache => Set<PostCodeCache>();
}
