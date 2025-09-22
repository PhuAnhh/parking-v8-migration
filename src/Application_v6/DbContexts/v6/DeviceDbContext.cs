using Microsoft.EntityFrameworkCore;
using Application_v6.Configurations.v6.Device;
using Application_v6.Entities.v6.Device;

namespace Application_v6.DbContexts.v6;

public class DeviceDbContext : DbContext
{
    public DbSet<Gate> Gates { get; set; }
    public DbSet<Computer> Computers { get; set; }
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<ControlUnit> ControlUnits { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<Led> Leds { get; set; }

    public DeviceDbContext(DbContextOptions<DeviceDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GateConfiguration());
        modelBuilder.ApplyConfiguration(new ComputerConfiguration());
        modelBuilder.ApplyConfiguration(new CameraConfiguration());
        modelBuilder.ApplyConfiguration(new ControlUnitConfiguration());
        modelBuilder.ApplyConfiguration(new LaneConfiguration());
        modelBuilder.ApplyConfiguration(new LedConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
        // Add your database provider configuration, e.g.,
        // optionsBuilder.UseSqlServer("YourConnectionString");
    }
}