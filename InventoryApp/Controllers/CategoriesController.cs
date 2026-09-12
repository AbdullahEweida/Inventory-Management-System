
using InventoryApp.DataAccess;
using InventoryApp.Models;
using InventoryApp.ViewModels;
using InventoryApp.ViewModels.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace InventoryApp.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly InventoryDbContext context;

        public CategoriesController(InventoryDbContext context)
        {
            this.context = context;
        }




        // Index
        public IActionResult Index()
        {
            var categoriesList = context.Categories
                .Include(c => c.Products)
                .ToList();

            var model = new CategoryIndexViewModel
            {
                Categories = categoriesList.Select(c => new CategoryViewModel
                {
                    Id = c.ID,
                    Name = c.Name,
                    Description = c.Description,
                    ProductsCount = c.Products != null ? c.Products.Count : 0
                }).ToList()
            };

            return View("CategoryIndex", model);
        }





        //Add
        public IActionResult Add()
        {
            return View("AddCategory");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(CategoryViewModel model)
        {
            // Check if category name already exists
            if (!string.IsNullOrWhiteSpace(model.Name) && context.Categories.Any(c => c.Name.ToLower() == model.Name.ToLower()))
            {
                ModelState.AddModelError(
                    "Name",
                    "A category with this name already exists."
                );
            }

            if (ModelState.IsValid)
            {
                var category = new Category
                {
                    ID = Guid.NewGuid(),
                    Name = model.Name,
                    Description = model.Description
                };

                context.Categories.Add(category);
                context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            return View("AddCategory", model);
        }




        // Edit
        [HttpGet]
        public IActionResult Edit(Guid id)
        {
            var category = context.Categories.Find(id);
            if (category == null) return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.ID,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        
        [ValidateAntiForgeryToken]
        [HttpPost]
        public IActionResult Edit(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var category = context.Categories.Find(model.Id);
                if (category == null) return NotFound();

                category.Name = model.Name;
                category.Description = model.Description;

                context.Categories.Update(category);
                context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }




        // Delete
       

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Guid id)
        {
            var category = context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.ID == id);

            if (category == null)
            {
                return NotFound();
            }

            // Check if category has products
            if (category.Products != null && category.Products.Any())
            {
                TempData["Error"] = "Cannot delete this category because it contains products.";
                return RedirectToAction(nameof(Index));
            }

            context.Categories.Remove(category);
            context.SaveChanges();

            TempData["Success"] = "Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }



        // Details
        // Details
        public IActionResult Details(Guid id)
        {
            var category = context.Categories
                .Include(c => c.Products)
                .FirstOrDefault(c => c.ID == id);

            if (category == null) return NotFound();

            var model = new CategoryViewModel
            {
                Id = category.ID,
                Name = category.Name,
                Description = category.Description,
                ProductsCount = category.Products != null ? category.Products.Count : 0,

                
                Products = category.Products?.Select(p => new ProductDetailsViewModel
                {
                    Id = p.ID,
                    Name = p.Name,
                    SKU = p.SKU,
                    UnitPrice = p.UnitPrice,
                    StockQuantity = p.StockQuantity
                }).ToList() ?? new List<ProductDetailsViewModel>()
            };

            return View(model);
        }
    }
}