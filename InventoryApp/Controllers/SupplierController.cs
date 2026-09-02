using InventoryApp.DataAccess;
using InventoryApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace InventoryApp.Controllers
{
    public class SupplierController : Controller
    {
        private readonly InventoryDbContext _db;

        public SupplierController(InventoryDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 5;

            if (page < 1) page = 1;

            int totalSuppliers = await _db.Suppliers.CountAsync();
            int totalPages = (int)Math.Ceiling(totalSuppliers / (double)pageSize);

            var suppliers = _db.Suppliers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(suppliers);
        }
        public IActionResult Create()
        {
            Supplier sup = new Supplier();
            return View(sup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Supplier sup)
        {
            if (ModelState.IsValid)
            {
                await _db.Suppliers.AddAsync(sup);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sup);
        }
        public async Task<IActionResult> Details(Guid id)
        {
            var sup =  await _db.Suppliers.
                Include(s => s.Purchases).
                ThenInclude(p => p.PurchaseItems).
                ThenInclude(p => p.Product).
                FirstOrDefaultAsync(s => s.ID == id);
            
            if (sup == null)
            {
                return NotFound();
            }
            return View(sup);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            Supplier? sup = await _db.Suppliers.FirstOrDefaultAsync(s => s.ID == id);
            if (sup != null)
            {
                _db.Suppliers.Remove(sup);
                await _db.SaveChangesAsync();

                TempData["DeleteSuccess"] = "Delete Successful!";
            }
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(Guid id)
        {
            Supplier? sup = await _db.Suppliers.FirstOrDefaultAsync(s => s.ID == id);
            if (sup == null)
            {
                return NotFound();
            }
            return View(sup);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Supplier sup)
        {
            if (ModelState.IsValid)
            {
                _db.Suppliers.Update(sup);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(sup);
        }
    }
}