using Application.Entities;
using Application.DbContexts.v3;
using Application.DbContexts.v8;
using Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        var ipAddress = config["Server:IpAddress"];

        // DbContext
        services.AddDbContext<MParkingDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("MParking")));

        services.AddDbContext<MParkingEventDbContext>(opt =>
            opt.UseSqlServer(config.GetConnectionString("MParkingEvent")));

        services.AddDbContext<ResourceDbContext>(opt =>
        {
            var conn = config.GetConnectionString("Resource").Replace("{IpAddress}", ipAddress);
            opt.UseNpgsql(conn);
        });

        services.AddDbContext<EventDbContext>(opt =>
        {
            var conn = config.GetConnectionString("Event").Replace("{IpAddress}", ipAddress);
            opt.UseNpgsql(conn);
        });

        // Service
        services.AddTransient<InsertEntriesService>();
        services.AddTransient<InsertExitsService>();

        // Form
        services.AddSingleton<Main>();
        services.AddSingleton<Event>();
        services.AddSingleton<Excel>();

        var minioSettings = new MinioSettings();
        config.GetSection("Minio").Bind(minioSettings);
        minioSettings.Endpoint = minioSettings.Endpoint.Replace("{IpAddress}", ipAddress);
        services.AddSingleton(minioSettings);

        var serviceProvider = services.BuildServiceProvider();

        // Run WinForms
        System.Windows.Forms.Application.Run(serviceProvider.GetRequiredService<Main>());
    }
}