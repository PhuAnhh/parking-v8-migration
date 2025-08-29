using Microsoft.EntityFrameworkCore;
using Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CardEventDbContext>(opt =>
    opt.UseSqlServer("Server=14.160.26.45,9999;Database=MPARKINGEVENT_LETO;User=hungnn;Password=Kztek@123456;TrustServerCertificate=True;Connection Timeout=30;"));

builder.Services.AddDbContext<ResourceDbContext>(opt =>
    opt.UseNpgsql("User ID=postgres;Password=Pass1234!;Host=192.168.21.100;Port=5432;Database=resource;Pooling=true;"));

builder.Services.AddDbContext<EventDbContext>(opt =>
    opt.UseNpgsql("User ID=postgres;Password=Pass1234!;Host=192.168.21.100;Port=5432;Database=event;Pooling=true;"));

builder.Services.AddTransient<InsertEntriesService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var insertEntriesService = scope.ServiceProvider.GetRequiredService<InsertEntriesService>();
    await insertEntriesService.InsertEntries();
}

app.Run();