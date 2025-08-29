using Application.Entities.CardEvent;
using Microsoft.EntityFrameworkCore;
using Application.Entities.Lane;

namespace Application;

public class CardEventDbContext : DbContext
{
    public DbSet<CardEvent> CardEvents { get; set; }
    // public DbSet<Lane> Lanes { get; set; }
    
    public CardEventDbContext(DbContextOptions<CardEventDbContext> options) : base(options) { }
}
