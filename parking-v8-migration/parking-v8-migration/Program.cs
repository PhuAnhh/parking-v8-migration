using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.DbContexts.v3;
using Application.DbContexts.v8;
using Application.Service;

namespace parking_v8_migration;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var services = new ServiceCollection();

        services.AddDbContext<MParkingDbContext>(opt =>
            opt.UseSqlServer(
                "Server=14.160.26.45,9999;Database=MPARKING_LETO;User=hungnn;Password=Kztek@123456;TrustServerCertificate=True;Connection Timeout=30;"));
        services.AddDbContext<MParkingEventDbContext>(opt =>
            opt.UseSqlServer(
                "Server=14.160.26.45,9999;Database=MPARKINGEVENT_LETO;User=hungnn;Password=Kztek@123456;TrustServerCertificate=True;Connection Timeout=30;"));
        services.AddDbContext<ResourceDbContext>(opt =>
            opt.UseNpgsql(
                "User ID=postgres;Password=Pass1234!;Host=14.160.26.45;Port=5432;Database=resource;Pooling=true;"));
        services.AddDbContext<EventDbContext>(opt =>
            opt.UseNpgsql(
                "User ID=postgres;Password=Pass1234!;Host=14.160.26.45;Port=5432;Database=event;Pooling=true;"));

        // Service
        services.AddTransient<InsertEntriesService>();
        services.AddTransient<InsertExitsService>();

        // Form
        services.AddSingleton<Main>();

        var serviceProvider = services.BuildServiceProvider();

        // Run WinForms
        System.Windows.Forms.Application.Run(serviceProvider.GetRequiredService<Main>());
    }
}