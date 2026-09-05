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


        // Index
        public IActionResult Index(string searchString, Guid? categoryId, string stockStatus, int page = 1)
        {
            int pageSize = 10;
            var query = context.Products.Include(p => p.Category).AsQueryable();

            // filter
            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(p => p.Name.Contains(searchString) || p.SKU.Contains(searchString));
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

        
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentStockStatus = stockStatus;
            ViewBag.Categories = new SelectList(context.Categories, "ID", "Name", categoryId);

      
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
                TotalPages = totalPages
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




        //Add
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(context.Categories.ToList(), "ID", "Name");
            return View("AddProduct");
        }

        [HttpPost]
        public IActionResult Add(ProductViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(context.Categories.ToList(), "ID", "Name", viewModel.CategoryID);
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
                    StockQuantity = viewModel.StockQuantity,
                    LowStockThreshold = viewModel.LowStockThreshold,
                    CategoryID = viewModel.CategoryID.Value
                };

                context.Products.Add(product);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while saving the product.");
                ViewBag.Categories = new SelectList(context.Categories.ToList(), "ID", "Name", viewModel.CategoryID);
                return View("AddProduct", viewModel);
            }
        }

        // Edit
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
                CategoryID = product.CategoryID
            };

            ViewBag.Categories = new SelectList(context.Categories.ToList(), "ID", "Name", viewModel.CategoryID);
            return View("EditProduct", viewModel);
        }

      
        [HttpPost]
        public IActionResult Edit(ProductViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(context.Categories.ToList(), "ID", "Name", viewModel.CategoryID);
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
                product.CategoryID = viewModel.CategoryID.Value;

                context.Products.Update(product);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while updating the product.");
                ViewBag.Categories = new SelectList(context.Categories.ToList(), "ID", "Name", viewModel.CategoryID);
                return View("EditProduct", viewModel);
            }
        }

        // Delete
        [HttpGet]
        public IActionResult Delete(Guid id)
        {
            var product = context.Products.Find(id);
            if (product != null)
            {
                context.Products.Remove(product);
                context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

     
        [HttpPost]
        [ActionName("Delete")]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var product = context.Products.Find(id);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                context.Products.Remove(product);
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                ModelState.AddModelError("", "An error occurred while deleting the product.");
                return View("DeleteProduct");
            }
        }
    }
}