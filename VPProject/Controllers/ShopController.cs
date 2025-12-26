using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VPProject.Data;
using VPProject.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace VPProject.Controllers
{
    public class ShopController : Controller
    {
        private readonly VPProjectContext _context;

        public ShopController(VPProjectContext context)
        {
            _context = context;
        }

        // GET: Shop
        public async Task<IActionResult> Index(int? categoryId, string search)
        {
            var products = _context.Product.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.ProductName.Contains(search));
            }

            ViewBag.Categories = await _context.Category.ToListAsync();
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = search;

            return View(await products.ToListAsync());
        }

        // GET: Shop/ProductDetails/5
        public async Task<IActionResult> ProductDetails(int? id)
        {
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

        // POST: Shop/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin", "Shop");
            }

            var product = await _context.Product.FindAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            var existingCart = await _context.Cart
                .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.ProductId == productId);

            if (existingCart != null)
            {
                existingCart.Quantity += quantity;
                _context.Update(existingCart);
            }
            else
            {
                var cart = new Cart
                {
                    CustomerId = customerId.Value,
                    ProductId = productId,
                    Quantity = quantity,
                    Price = product.Price
                };
                _context.Cart.Add(cart);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Product added to cart successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Shop/Cart
        public async Task<IActionResult> Cart()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin");
            }

            var cartItems = await _context.Cart
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            ViewBag.Total = cartItems.Sum(c => c.Price * c.Quantity);

            return View(cartItems);
        }

        // POST: Shop/UpdateCart
        [HttpPost]
        public async Task<IActionResult> UpdateCart(int cartId, int quantity)
        {
            var cart = await _context.Cart.FindAsync(cartId);
            if (cart == null)
            {
                return NotFound();
            }

            if (quantity <= 0)
            {
                _context.Cart.Remove(cart);
            }
            else
            {
                cart.Quantity = quantity;
                _context.Update(cart);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));
        }

        // POST: Shop/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            var cart = await _context.Cart.FindAsync(cartId);
            if (cart != null)
            {
                _context.Cart.Remove(cart);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Cart));
        }

        // GET: Shop/Checkout
        public async Task<IActionResult> Checkout()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin");
            }

            var cartItems = await _context.Cart
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction(nameof(Cart));
            }

            ViewBag.CartItems = cartItems;
            ViewBag.Total = cartItems.Sum(c => c.Price * c.Quantity);

            var customer = await _context.Customer.FindAsync(customerId.Value);
            return View(customer);
        }

        // POST: Shop/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin");
            }

            var cartItems = await _context.Cart
                .Include(c => c.Product)
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();

            if (!cartItems.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction(nameof(Cart));
            }

            var totalAmount = cartItems.Sum(c => c.Price * c.Quantity);

            // Create order
            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                TotalAmount = totalAmount
            };

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            // Create order details
            foreach (var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Price
                };
                _context.OrderDetail.Add(orderDetail);
            }

            // Clear cart
            _context.Cart.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Order placed successfully!";
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }

        // GET: Shop/OrderConfirmation
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Shop/CustomerLogin
        public IActionResult CustomerLogin()
        {
            return View();
        }

        // POST: Shop/CustomerLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerLogin(string phone)
        {
            if (string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "Phone number is required";
                return View();
            }

            var customer = await _context.Customer
                .FirstOrDefaultAsync(c => c.Phone == phone);

            if (customer != null)
            {
                HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
                HttpContext.Session.SetString("CustomerName", customer.CustomerName);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Error = "Customer not found. Please register first.";
            return View();
        }

        // GET: Shop/CustomerRegister
        public IActionResult CustomerRegister()
        {
            return View();
        }

        // POST: Shop/CustomerRegister
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CustomerRegister(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
                HttpContext.Session.SetString("CustomerName", customer.CustomerName);

                TempData["Success"] = "Registration successful!";
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

        // GET: Shop/CustomerLogout
        public IActionResult CustomerLogout()
        {
            HttpContext.Session.Remove("CustomerId");
            HttpContext.Session.Remove("CustomerName");
            return RedirectToAction(nameof(Index));
        }

        // GET: Shop/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("CustomerLogin");
            }

            var orders = await _context.Order
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }
    }
}