using InventoryApp.DataAccess;
using InventoryApp.Models;
using InventoryApp.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Controllers
{
    public class ProductsController : Controller
    {
        private readonly InventoryDbContext context;

        public ProductsController(InventoryDbContext context)
        {
            this.context = context;
        }

        // دالة مساعدة خاصة لجلب قائمة الأقسام وتجنب تكرار الكود
        private List<SelectListItem> GetCategoriesDropdown(Guid? selectedCategoryId = null)
        {
            return context.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.ID.ToString(),
                    Text = c.Name,
                    Selected = selectedCategoryId.HasValue && c.ID == selectedCategoryId.Value
                })
                .ToList();
        }

        // Index
        public IActionResult Index(string? searchString, Guid? categoryId, string? stockStatus, int page = 1)
        {
            int pageSize = 10;
            if (page < 1) page = 1;
            var query = context.Products.Include(p => p.Category).AsQueryable();

            // الفلترة
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || (p.SKU != null && p.SKU.Contains(searchString)));
            }

            if (categoryId.HasValue && categoryId.Value != Guid.Empty)
            {
                query = query.Where(p => p.CategoryID == categoryId);
            }

            if (!string.IsNullOrEmpty(stockStatus))
            {
                switch (stockStatus)
                {
                    case "low":
                        query = query.Where(p => p.StockQuantity <= p.LowStockThreshold && p.StockQuantity > 0);
                        break;
                    case "out":
                        query = query.Where(p => p.StockQuantity == 0);
                        break;
                    case "in":
                        query = query.Where(p => p.StockQuantity > p.LowStockThreshold);
                        break;
                }
            }

            int totalProducts = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalProducts / pageSize);

            var productsList = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var model = new ProductIndexViewModel
            {
                Products = productsList.Select(p => new ProductViewModel
                {
                    Id = p.ID,
                    Name = p.Name,
                    SKU = p.SKU,
                    CategoryName = p.Category != null ? p.Category.Name : "N/A",
                    UnitPrice = p.UnitPrice,
                    StockQuantity = p.StockQuantity,
                    LowStockThreshold = p.LowStockThreshold
                }).ToList(),
                CurrentPage = page,
                TotalPages = totalPages,
                CurrentSearch = searchString,
                CurrentCategory = categoryId,
                CurrentStockStatus = stockStatus,
                CategoriesList = GetCategoriesDropdown(categoryId)
            };

            return View("ProductIndex", model);
        }

        // Details
        public IActionResult Details(Guid id)
        {
            var product = context.Products
                .Include(p => p.Category)
                .FirstOrDefault(p => p.ID == id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductDetailsViewModel
            {
                Id = product.ID,
                Name = product.Name,
                SKU = product.SKU,
                UnitPrice = product.UnitPrice,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                CategoryID = product.CategoryID,
                CategoryName = product.Category?.Name
            };

            return View("ProductDetails", viewModel);
        }

        // Add - GET
        [HttpGet]
        public IActionResult Add()
        {
            var viewModel = new ProductViewModel
            {
                CategoriesList = GetCategoriesDropdown()
            };

            return View("AddProduct", viewModel);
        }

        // Add - POST
        // Add - POST
        [HttpPost]
        public IActionResult Add(ProductViewModel viewModel)
        {
            // Validate category selection
            if (!viewModel.CategoryID.HasValue || viewModel.CategoryID == Guid.Empty)
            {
                ModelState.AddModelError(nameof(viewModel.CategoryID), "Please select a category.");
            }

            // Check if product name already exists
            if (!string.IsNullOrWhiteSpace(viewModel.Name) && context.Products.Any(p => p.Name.ToLower() == viewModel.Name.ToLower()))
            {
                ModelState.AddModelError(
                    "Name",
                    "A product with this name already exists."
                );
            }

            // Check if SKU already exists (only when supplied)
            if (!string.IsNullOrWhiteSpace(viewModel.SKU) && context.Products.Any(p => p.SKU != null && p.SKU.ToLower() == viewModel.SKU.ToLower()))
            {
                ModelState.AddModelError(
                    "SKU",
                    "A product with this SKU already exists."
                );
            }

            if (!ModelState.IsValid)
            {
                viewModel.CategoriesList = GetCategoriesDropdown(viewModel.CategoryID);
                return View("AddProduct", viewModel);
            }

            try
            {
                Product product = new Product
                {
                    ID = Guid.NewGuid(),
                    Name = viewModel.Name,
                    SKU = viewModel.SKU,
                    UnitPrice = viewModel.UnitPrice,
                    LowStockThreshold = viewModel.LowStockThreshold,
                    CategoryID = viewModel.CategoryID!.Value
                };

                context.Products.Add(product);
                context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError(
                    "",
                    "An error occurred while saving the product."
                );

                viewModel.CategoriesList = GetCategoriesDropdown(viewModel.CategoryID);
                return View("AddProduct", viewModel);
            }
        }

        // Edit - GET
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            var viewModel = new ProductViewModel
            {
                Id = product.ID,
                Name = product.Name,
                SKU = product.SKU,
                UnitPrice = product.UnitPrice,
                StockQuantity = product.StockQuantity,
                LowStockThreshold = product.LowStockThreshold,
                CategoryID = product.CategoryID,
                CategoriesList = GetCategoriesDropdown(product.CategoryID)
            };

            return View("EditProduct", viewModel);
        }

        // Edit - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductViewModel viewModel)
        {
            if (!viewModel.CategoryID.HasValue || viewModel.CategoryID == Guid.Empty)
            {
                ModelState.AddModelError(nameof(viewModel.CategoryID), "Please select a category.");
            }

            if (!string.IsNullOrWhiteSpace(viewModel.Name) && context.Products.Any(p => p.Name.ToLower() == viewModel.Name.ToLower() && p.ID != viewModel.Id))
            {
                ModelState.AddModelError(nameof(viewModel.Name), "A product with this name already exists.");
            }

            if (!string.IsNullOrWhiteSpace(viewModel.SKU) && context.Products.Any(p => p.SKU != null && p.SKU.ToLower() == viewModel.SKU.ToLower() && p.ID != viewModel.Id))
            {
                ModelState.AddModelError(nameof(viewModel.SKU), "A product with this SKU already exists.");
            }

            if (!ModelState.IsValid)
            {
                viewModel.CategoriesList = GetCategoriesDropdown(viewModel.CategoryID);
                return View("EditProduct", viewModel);
            }

            try
            {
                var product = context.Products.Find(viewModel.Id);
                if (product == null) return NotFound();

                product.Name = viewModel.Name;
                product.SKU = viewModel.SKU;
                product.UnitPrice = viewModel.UnitPrice;
                product.StockQuantity = viewModel.StockQuantity;
                product.LowStockThreshold = viewModel.LowStockThreshold;
                product.CategoryID = viewModel.CategoryID!.Value;

                context.Products.Update(product);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the product.");
                viewModel.CategoriesList = GetCategoriesDropdown(viewModel.CategoryID);
                return View("EditProduct", viewModel);
            }
        }

        // Delete - GET (عادة يفضل استدعاء صفحة تأكيد الحذف بدلاً من الحذف المباشر في GET)
        
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }
            bool isInPurchase = context.Purchases_Items
                           .Any(x => x.ProductID == id);

            // Check if product is used in any sale
            bool isInSale = context.Sales_Items
                .Any(x => x.ProductID == id);

            if (isInPurchase || isInSale)
            {
                TempData["Error"] =
                    "Cannot delete this product because it is  already used .";

                return RedirectToAction(nameof(Index));
            }

            context.Products.Remove(product);
            context.SaveChanges();


            TempData["Success"] =
                "Product deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}