using InventoryApp.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryDbContext _db;

        public InventoryController(InventoryDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var stock = await _db.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(stock);
        }

        public async Task<IActionResult> LowStockAlerts()
        {
            var alerts = await _db.Products
                .Where(p => p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ToListAsync();

            return View(alerts);
        }
    }
}