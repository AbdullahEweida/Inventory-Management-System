using InventoryApp.DataAccess;
using InventoryApp.Models;
using InventoryApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace InventoryApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly InventoryDbContext _context;

        public HomeController(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var recentSales = await _context.Sales
                .Include(s => s.Sale_Items)
                .OrderByDescending(s => s.Date)
                .Take(5)
                .Select(s => new RecentActivityViewModel
                {
                    Type = "Sale",
                    Date = s.Date,
                    Details = $"Sale to {s.CustomerInfo}",
                    Amount = s.Sale_Items.Sum(item => item.Quantity * item.UnitCost)
                })
                .ToListAsync();

            var recentPurchases = await _context.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                .OrderByDescending(p => p.Date)
                .Take(5)
                .Select(p => new RecentActivityViewModel
                {
                    Type = "Purchase",
                    Date = p.Date,
                    Details = $"Purchase from {p.Supplier.Name}",
                    Amount = p.PurchaseItems.Sum(item => item.Quantity * item.UnitPrice)
                })
                .ToListAsync();

            var recentActivities = recentSales
                .Concat(recentPurchases)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList();

            var mostSoldProducts = await _context.Sales_Items
                .Include(item => item.Product)
                .GroupBy(item => item.Product.Name)
                .Select(group => new MostSoldProductViewModel
                {
                    ProductName = group.Key,
                    TotalQuantitySold = group.Sum(item => item.Quantity),
                    TotalRevenue = group.Sum(item => item.Quantity * item.UnitCost)
                })
                .OrderByDescending(product => product.TotalQuantitySold)
                .Take(5)
                .ToListAsync();

            var dashboard = new DashboardViewModel
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalSuppliers = await _context.Suppliers.CountAsync(),

                TotalStockQuantity = await _context.Products
                    .SumAsync(product => (int?)product.StockQuantity) ?? 0,

                LowStockProductsCount = await _context.Products
                    .CountAsync(product => product.StockQuantity <= product.LowStockThreshold),

                TotalPurchasesCount = await _context.Purchases.CountAsync(),
                TotalPurchasesAmount = await _context.Purchases_Items
                    .SumAsync(item => (decimal?)(item.Quantity * item.UnitPrice)) ?? 0,

                TotalSalesCount = await _context.Sales.CountAsync(),
                TotalSalesRevenue = await _context.Sales_Items
                    .SumAsync(item => (decimal?)(item.Quantity * item.UnitCost)) ?? 0,

                RecentActivities = recentActivities,
                MostSoldProducts = mostSoldProducts
            };

            return View(dashboard);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        [Route("/Home/Error")]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode.HasValue && statusCode.Value == 404)
            {
                return View("NotFound");
            }

            return View("Error");
        }
    }
}