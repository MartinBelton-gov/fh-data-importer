using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FamilyHubs.DataImporter.Infrastructure;

public static class DbStartupExtension
{
    public static IServiceCollection RegisterAppDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ApplicationDbContextInitialiser>();
        var connectionString = configuration.GetConnectionString("PostCodeConnection");
        ArgumentException.ThrowIfNullOrEmpty(connectionString);

        var connection = new SqliteConnectionStringBuilder(connectionString).ToString();

       services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(connection, mg =>
                    mg.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.ToString()));
        });

        return services;
    }

}