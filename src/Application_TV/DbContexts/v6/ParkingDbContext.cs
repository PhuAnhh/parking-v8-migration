using Application_TV.Configurations.v6;
using Application_TV.Entities.v6;
using Microsoft.EntityFrameworkCore;

namespace Application_TV.DbContexts.v6;

public class ParkingDbContext : DbContext
{
    public DbSet<Led> Leds { get; set; }
    public DbSet<Gate> Gates { get; set; }
    public DbSet<Camera> Cameras { get; set; }
    public DbSet<Computer> Computers { get; set; }
    public DbSet<ControlUnit> ControlUnits { get; set; }
    public DbSet<Identity> Identites { get; set; }
    public DbSet<IdentityGroup> IdentityGroups { get; set; }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<CustomerGroup> CustomerGroups { get; set; }
    public DbSet<Lane> Lanes { get; set; }
    public DbSet<EventIn> EventIns { get; set; }
    public DbSet<EventOut> EventOuts { get; set; }
    public DbSet<RegisteredVehicle> RegisteredVehicles { get; set; }
    public DbSet<RegisteredVehicleIdentityMap> RegisteredVehicleIdentityMaps { get; set; }

    public ParkingDbContext(DbContextOptions<ParkingDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GateConfiguration());
        modelBuilder.ApplyConfiguration(new LedConfiguration());
        modelBuilder.ApplyConfiguration(new CameraConfiguration());
        modelBuilder.ApplyConfiguration(new ComputerConfiguration());
        modelBuilder.ApplyConfiguration(new ControlUnitConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityConfiguration());
        modelBuilder.ApplyConfiguration(new IdentityGroupConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new CustomerGroupConfiguration());
        modelBuilder.ApplyConfiguration(new LaneConfiguration());
        modelBuilder.ApplyConfiguration(new EventInConfiguration());
        modelBuilder.ApplyConfiguration(new EventOutConfiguration());
        modelBuilder.ApplyConfiguration(new RegisteredVehicleConfiguration());
        modelBuilder.ApplyConfiguration(new RegisteredVehicleIdentityMapConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.LogTo(Console.WriteLine);
        // Add your database provider configuration, e.g.,
        // optionsBuilder.UseSqlServer("YourConnectionString");
    }
}