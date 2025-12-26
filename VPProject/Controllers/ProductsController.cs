using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VPProject.Data;
using VPProject.Models;

namespace VPProject.Controllers
{
    public class ProductsController : Controller
    {
        private readonly VPProjectContext _context;

        public ProductsController(VPProjectContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            var products = _context.Product.Include(p => p.Category);
            return View(await products.ToListAsync());
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Price,ImageUrl,CategoryId")] Product product)
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            // Remove ProductId from ModelState validation as it's auto-generated
            ModelState.Remove("ProductId");

            // Remove Category navigation property from validation
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Error: " + ex.Message);
                }
            }

            // Repopulate dropdown if validation fails
            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,ImageUrl,CategoryId")] Product product)
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id != product.ProductId)
            {
                return NotFound();
            }

            // Remove Category navigation property from validation
            ModelState.Remove("Category");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Unable to save changes. Error: " + ex.Message);
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Category, "CategoryId", "CategoryName", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction("Login", "Admin");
            }

            try
            {
                var product = await _context.Product.FindAsync(id);
                if (product != null)
                {
                    _context.Product.Remove(product);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Product not found!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Unable to delete product. Error: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.ProductId == id);
        }
    }
}