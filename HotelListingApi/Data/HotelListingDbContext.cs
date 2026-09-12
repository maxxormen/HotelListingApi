using Microsoft.EntityFrameworkCore;
namespace HotelListingApi.Data;

public class HotelListingDbContext : DbContext
{
    public HotelListingDbContext(DbContextOptions<HotelListingDbContext> options) : base(options)
    {
    }

    public HotelListingDbContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Replace with your actual connection string
            optionsBuilder.UseSqlServer("Server=localhost,1433;Database=HotelListingDb;User Id=sa;Password=764560ntkO@;TrustServerCertificate=True;");
        }
    }
    
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<Country> Countries { get; set; }
}