using InventoryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.DataAccess
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        DbSet<Product> Products { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Supplier> Suppliers { get; set; }
        DbSet<Purchase> Purchases { get; set; }
        DbSet<Sale> Sales { get; set; }
        DbSet<Sale_Item> Sales_Items { get; set;}
        DbSet<Purchase_Item> Purchases_Items { get; set; }
    }
}
