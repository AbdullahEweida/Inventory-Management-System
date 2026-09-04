using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.DataAccess
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<Sale_Item> Sales_Items { get; set;}
        public DbSet<Purchase_Item> Purchases_Items { get; set; }
    }
}
