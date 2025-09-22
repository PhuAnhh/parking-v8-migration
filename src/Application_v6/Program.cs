using Application_v6.DbContexts.v6;
using Application_v6.DbContexts.v8;
using Application_v6.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application_v6;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        services.AddDbContext<ParkingDbContext>(opt =>
            opt.UseSqlServer(
                    config.GetConnectionString("Parking"),
                    sqlOptions =>
                    {
                        sqlOptions.CommandTimeout(300); // tăng timeout
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                    })
                .LogTo(s => System.Diagnostics.Debug.WriteLine(s))
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging());

        services.AddDbContext<DeviceDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("Device")));

        services.AddDbContext<EventDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Event")));

        services.AddDbContext<ResourceDbContext>(opt =>
            opt.UseNpgsql(config.GetConnectionString("Resource")));

        // Service
        services.AddTransient<AccessKeyCollectionService>();
        services.AddTransient<AccessKeyService>();
        services.AddTransient<CustomerCollectionService>();
        services.AddTransient<CustomerService>();
        services.AddTransient<DeviceService>();
        services.AddTransient<EntryService>();
        services.AddTransient<ExitService>();

        // Form
        services.AddSingleton<Main>();

        var serviceProvider = services.BuildServiceProvider();

        Application.Run(serviceProvider.GetRequiredService<Main>());
    }
}