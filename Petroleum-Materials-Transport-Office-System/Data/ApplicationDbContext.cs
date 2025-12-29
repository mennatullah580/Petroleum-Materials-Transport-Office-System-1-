using Microsoft.EntityFrameworkCore;
using Petroleum_Materials_Transport_Office_System.Models;

namespace Petroleum_Materials_Transport_Office_System.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // --- 1. Basic Operational Tables ---
        public DbSet<Company> Company { get; set; }
        public DbSet<Provider> Provider { get; set; }
        public DbSet<Fuel_Type> Fuel_Type { get; set; }
        public DbSet<Location> Location { get; set; }
        public DbSet<Driver> Driver { get; set; }
        public DbSet<Vehicle> Vehicle { get; set; }

        // --- 2. Core Transactions ---
        public DbSet<Order> Orders { get; set; }
        public DbSet<Invoice> Invoice { get; set; }
        public DbSet<Financials> Financials { get; set; }
        public DbSet<Payment_Transaction> Payment_Transaction { get; set; }

        // --- 3. Accounting & Finance (NOW ADDED!) ---
        public DbSet<Treasury_Bank> Treasury_Bank { get; set; }
        public DbSet<Expense_Item> Expense_Item { get; set; }
        public DbSet<Cost_Center> Cost_Center { get; set; }
    }
}