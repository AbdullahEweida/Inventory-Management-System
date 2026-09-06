using InventoryApp.DataAccess;
using InventoryApp.Models;
using InventoryApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers
{
    public class PurchaseController : Controller
    {
        private readonly InventoryDbContext _db;

        public PurchaseController(InventoryDbContext db)
        {
            _db = db;
        }

        // Display all purchases
        public async Task<IActionResult> Index()
        {
            var purchases = await _db.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return View(purchases);
        }

        // Show Create Purchase page
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = await _db.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Products = await _db.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(new PurchaseCreateViewModel());
        }

        // Save Purchase
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = await _db.Suppliers
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                ViewBag.Products = await _db.Products
                    .OrderBy(p => p.Name)
                    .ToListAsync();

                return View(model);
            }

            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                SupplierID = model.SupplierID,
                Date = model.Date,
                PurchaseItems = new List<Purchase_Item>()
            };

            foreach (var item in model.Items)
            {
                var product = await _db.Products
                    .FirstOrDefaultAsync(p => p.ID == item.ProductID);

                if (product == null)
                {
                    ModelState.AddModelError("", "One of the selected products was not found.");

                    ViewBag.Suppliers = await _db.Suppliers
                        .OrderBy(s => s.Name)
                        .ToListAsync();

                    ViewBag.Products = await _db.Products
                        .OrderBy(p => p.Name)
                        .ToListAsync();

                    return View(model);
                }

                var purchaseItem = new Purchase_Item
                {
                    ID = Guid.NewGuid(),
                    PurchaseID = purchase.Id,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };

                purchase.PurchaseItems.Add(purchaseItem);

                // Increase stock
                product.StockQuantity += item.Quantity;
                product.UnitPrice = item.UnitPrice;
            }

            _db.Purchases.Add(purchase);

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Display purchase details
        public async Task<IActionResult> Details(Guid id)
        {
            var purchase = await _db.Purchases
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseItems)
                    .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }
    }
}
