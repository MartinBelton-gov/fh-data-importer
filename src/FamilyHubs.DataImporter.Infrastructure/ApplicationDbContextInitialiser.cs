using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FamilyHubs.DataImporter.Infrastructure;

public class ApplicationDbContextInitialiser
{
    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task InitialiseAsync(bool shouldClearDatabaseOnRestart)
    {
        try
        {
            if (shouldClearDatabaseOnRestart)
                await _context.Database.EnsureDeletedAsync();

            if (_context.Database.IsSqlServer())
                await _context.Database.MigrateAsync();
            else
                await _context.Database.EnsureCreatedAsync();
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }
}
