using InventoryApp.DataAccess;
using InventoryApp.Models;
using InventoryApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers
{
    public class SaleController : Controller
    {
        private readonly InventoryDbContext _db;

        public SaleController(InventoryDbContext db)
        {
            _db = db;
        }

        // Display all sales
        public async Task<IActionResult> Index()
        {
            var sales = await _db.Sales
                .Include(s => s.Sale_Items)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(s => s.Date)
                .ToListAsync();

            return View(sales);
        }

        // Show Create Sale page
        public async Task<IActionResult> Create()
        {
            ViewBag.Products = await _db.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(new SaleCreateViewModel());
        }

        // Save Sale
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleCreateViewModel model)
        {
            if (model.Items == null || model.Items.Count == 0)
            {
                ModelState.AddModelError(nameof(model.Items), "Add at least one product to the sale.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Products = await _db.Products
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                return View(model);
            }

            var sale = new Sale
            {
                ID = Guid.NewGuid(),
                Date = model.Date,
                CustomerInfo = model.CustomerInfo,
                Sale_Items = new List<Sale_Item>()
            };

            foreach (var item in model.Items)
            {
                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.ID == item.ProductID);

                if (product == null)
                {
                    ModelState.AddModelError(
                        "",
                        "One of the selected products was not found."
                    );

                    ViewBag.Products = await _db.Products
                        .OrderBy(p => p.Name)
                        .ToListAsync();

                    return View(model);
                }

                // Check available stock
                if (item.Quantity > product.StockQuantity)
                {
                    ModelState.AddModelError(
                        "",
                        $"Not enough stock for product: {product.Name}. " +
                        $"Available stock: {product.StockQuantity}"
                    );

                    ViewBag.Products = await _db.Products
                        .OrderBy(p => p.Name)
                        .ToListAsync();

                    return View(model);
                }

                var saleItem = new Sale_Item
                {
                    ID = Guid.NewGuid(),
                    SaleID = sale.ID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost
                };

                sale.Sale_Items.Add(saleItem);

                // Decrease stock
                product.StockQuantity -= item.Quantity;
            }

            _db.Sales.Add(sale);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Display sale details
        public async Task<IActionResult> Details(Guid id)
        {
            var sale = await _db.Sales
                .Include(s => s.Sale_Items)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(s => s.ID == id);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }
    }
}
