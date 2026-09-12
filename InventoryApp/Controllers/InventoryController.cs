using InventoryApp.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryApp.Controllers
{
    public class InventoryController : Controller
    {
        private readonly InventoryDbContext _db;

        public InventoryController(InventoryDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(string? searchString, int page = 1)
        {
            int pageSize = 10;
            if (page < 1) page = 1;

            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || (p.SKU != null && p.SKU.Contains(searchString)));
            }

            int totalProducts = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var stock = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchString = searchString;

            ViewBag.LowStockCount = await _db.Products.CountAsync(p => p.StockQuantity <= p.LowStockThreshold);

            return View(stock);
        }

        [HttpGet]
        public async Task<IActionResult> Notifications()
        {
            var products = await _db.Products
                .Where(p => p.StockQuantity <= p.LowStockThreshold)
                .OrderBy(p => p.StockQuantity)
                .ThenBy(p => p.Name)
                .Take(6)
                .Select(p => new
                {
                    id = p.ID,
                    name = p.Name,
                    sku = p.SKU,
                    stock = p.StockQuantity,
                    threshold = p.LowStockThreshold,
                    severity = p.StockQuantity <= 0 ? "out" : "low"
                })
                .ToListAsync();

            var totalAlerts = await _db.Products
                .CountAsync(p => p.StockQuantity <= p.LowStockThreshold);

            return Json(new
            {
                count = totalAlerts,
                items = products
            });
        }

        public async Task<IActionResult> LowStockAlerts(int page = 1)
        {
            int pageSize = 6;
            if (page < 1) page = 1;

            var query = _db.Products.Where(p => p.StockQuantity <= p.LowStockThreshold);

            int totalAlerts = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalAlerts / (double)pageSize);

            var alerts = await query
                .OrderBy(p => p.StockQuantity)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(alerts);
        }
    }
}