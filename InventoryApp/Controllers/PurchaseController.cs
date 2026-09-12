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
            if (model.Items == null || model.Items.Count == 0)
            {
                ModelState.AddModelError(nameof(model.Items), "Add at least one product to the purchase.");
            }

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




        // Show Order Product page
        [HttpGet]
        public async Task<IActionResult> OrderProduct(Guid productId)
        {
            var product = await _db.Products.Include(p=> p.Category)
                .FirstOrDefaultAsync(p => p.ID == productId);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Suppliers = await _db.Suppliers
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(product);
        }


        // Confirm Order
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OrderProduct(
       Guid productId,
       Guid supplierId,
       int quantity,
       decimal unitprice)
        {
            // Get product
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.ID == productId);

            if (product == null)
            {
                return NotFound();
            }

            // Get supplier
            var supplier = await _db.Suppliers
                .FirstOrDefaultAsync(s => s.ID == supplierId);

            if (supplier == null)
            {
                return NotFound();
            }

            // Validate quantity
            if (quantity <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Quantity must be greater than 0."
                );

                ViewBag.Suppliers = await _db.Suppliers
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return View(product);
            }

            // Validate unit price
            if (unitprice <= 0)
            {
                ModelState.AddModelError(
                    "",
                    "Unit Price must be greater than 0."
                );

                ViewBag.Suppliers = await _db.Suppliers
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                return View(product);
            }

            // Create Purchase
            var purchase = new Purchase
            {
                Id = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                SupplierID = supplierId,
                PurchaseItems = new List<Purchase_Item>()
            };

            // Add product to purchase
            var purchaseItem = new Purchase_Item
            {
                ID = Guid.NewGuid(),
                PurchaseID = purchase.Id,
                ProductID = product.ID,
                Quantity = quantity,
                UnitPrice = unitprice
            };

            purchase.PurchaseItems.Add(purchaseItem);

            // Increase stock
            product.StockQuantity += quantity;

            // Update product price
            product.UnitPrice = unitprice;

            // Save
            _db.Purchases.Add(purchase);

            await _db.SaveChangesAsync();

            return RedirectToAction(
                nameof(Details),
                new { id = purchase.Id }
            );
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
