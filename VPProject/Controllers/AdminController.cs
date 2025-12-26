using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VPProject.Data;
using VPProject.Models;
using System.Linq;
using System.Threading.Tasks;

namespace VPProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly VPProjectContext _context;

        public AdminController(VPProjectContext context)
        {
            _context = context;
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Username and password are required";
                return View();
            }

            var admin = await _context.Admin
                .FirstOrDefaultAsync(a => a.Username == username && a.Password == password);

            if (admin != null)
            {
                // Store admin info in session
                HttpContext.Session.SetString("AdminId", admin.AdminId.ToString());
                HttpContext.Session.SetString("AdminName", admin.FullName);
                HttpContext.Session.SetString("AdminRole", admin.Role);

                return RedirectToAction(nameof(Dashboard));
            }

            ViewBag.Error = "Invalid username or password";
            return View();
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            // Check if admin is logged in
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminId")))
            {
                return RedirectToAction(nameof(Login));
            }

            // Get statistics
            ViewBag.TotalProducts = await _context.Product.CountAsync();
            ViewBag.TotalCategories = await _context.Category.CountAsync();
            ViewBag.TotalCustomers = await _context.Customer.CountAsync();
            ViewBag.TotalOrders = await _context.Order.CountAsync();
            ViewBag.TotalRevenue = await _context.Order
                .SumAsync(o => (double)o.TotalAmount);

            // Get recent orders
            var recentOrders = await _context.Order
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            ViewBag.RecentOrders = recentOrders;

            // Get low stock products (assuming quantity management)
            var topProducts = await _context.Product
                .Include(p => p.Category)
                .Take(5)
                .ToListAsync();

            ViewBag.TopProducts = topProducts;

            return View();
        }

        // GET: Admin/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }

        // GET: Admin/Profile
        public async Task<IActionResult> Profile()
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction(nameof(Login));
            }

            var admin = await _context.Admin.FindAsync(int.Parse(adminId));
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admin/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(Admin admin)
        {
            var adminId = HttpContext.Session.GetString("AdminId");
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(admin);
                    await _context.SaveChangesAsync();
                    HttpContext.Session.SetString("AdminName", admin.FullName);
                    ViewBag.Success = "Profile updated successfully";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.AdminId))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(admin);
        }

        private bool AdminExists(int id)
        {
            return _context.Admin.Any(e => e.AdminId == id);
        }
    }
}