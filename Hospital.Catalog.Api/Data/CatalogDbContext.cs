using Hospital.Catalog.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.Catalog.Api.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
}