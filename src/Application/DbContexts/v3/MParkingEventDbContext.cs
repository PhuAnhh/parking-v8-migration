using Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.DbContexts.v3;

public class MParkingEventDbContext : DbContext
{
    public DbSet<CardEvent> CardEvents { get; set; }
    // public DbSet<Lane> Lanes { get; set; }
    
    public MParkingEventDbContext(DbContextOptions<MParkingEventDbContext> options) : base(options) { }
}
