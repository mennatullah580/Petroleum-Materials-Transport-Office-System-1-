using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using Petroleum_Materials_Transport_Office_System.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Petroleum_Materials_Transport_Office_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Existing table
        public DbSet<Invoice> Invoice { get; set; }

        // NEW TABLES (Required for Reports)
        public DbSet<Fuel_Type> Fuel_Type { get; set; }
        public DbSet<Provider> Provider { get; set; }
        public DbSet<Company> Company { get; set; }
        public DbSet<Location> Location { get; set; }
    }
}