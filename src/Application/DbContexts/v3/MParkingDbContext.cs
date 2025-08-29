using Application.Entities.v3;
using Microsoft.EntityFrameworkCore;

namespace Application.DbContexts.v3;

public class MParkingDbContext : DbContext
{
    public DbSet<Card> Cards { get; set; }
}