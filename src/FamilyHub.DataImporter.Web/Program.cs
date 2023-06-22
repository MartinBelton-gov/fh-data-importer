using FamilyHub.DataImporter.Web;
using Serilog;

Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.ConfigureHost();

    builder.Services.RegisterApplicationComponents(builder.Configuration);

    // Add services to the container.
    builder.Services.RegisterApplicationComponents(builder.Configuration);


    var app = builder.Build();

    await app.ConfigureDb(builder.Configuration, Log.Logger);

    app.ConfigureWebApplication();

    await app.RunAsync();
}
catch (Exception e)
{
    if (e.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
    {
        //this error only occurs when DB migration is running on its own
        throw;
    }

    Log.Fatal(e, "An unhandled exception occurred during bootstrapping");
}
finally
{
    Log.CloseAndFlush();
}

