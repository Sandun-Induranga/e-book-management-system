using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EBookPvtLtd.Data;
using EBookPvtLtd.Models;

namespace EBookPvtLtd.Controllers
{
    public class OrdersController : Controller
    {
        private readonly EBookPvtLtdContext _context;
        private readonly ILogger<EBookPvtLtdContext> _logger;

        public OrdersController(EBookPvtLtdContext context, ILogger<EBookPvtLtdContext> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Orders
        public IActionResult Index()
        {
            var role = TempData["Role"] as string;
            var userId = TempData["CustomerId"] as int? ?? 0;

            IEnumerable<Order> orders;

            if (role == "Customer")
            {
                orders = _context.Order
                    .Include(o => o.Customer)
                    .Where(o => o.Customer.CustomerId == userId) // Fetch orders for the logged-in customer
                    .ToList();
                ViewBag.Title = "My Orders";
            }
            else
            {
                orders = _context.Order
                    .Include(o => o.Customer)
                    .ToList();
                ViewBag.Title = "Manage Orders";
            }

            ViewBag.Role = role; // Pass the role to the view
            return View(orders);
        }

        // GET: Orders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "CustomerId");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderId,CustomerId,OrderDate,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // GET: Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,CustomerId,OrderDate,Status")] Order order)
        {
            if (id != order.OrderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.OrderId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "CustomerId", order.CustomerId);
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout([FromBody] List<CartItem> cartItems)
        {
            _logger.LogInformation("Checkout initiated.");

            // Assuming customer is logged in and we have their ID
            int customerId = TempData["CustomerId"] as int? ?? 0;
            _logger.LogInformation("Customer ID: {0}", customerId);

            // Create a new order
            var order = new Order
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
            };

            _logger.LogInformation("Order created.");

            _context.Order.Add(order);
            await _context.SaveChangesAsync();

            // Add order items
            foreach (var item in cartItems)
            {
                var book = await _context.Book.FindAsync(item.BookId);
                if (book != null)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        Price = book.Price * item.Quantity,
                    };

                    _context.OrderItem.Add(orderItem);
                }
            }

            _logger.LogInformation("Order items added.");

            await _context.SaveChangesAsync();

            _logger.LogInformation("Checkout completed.");

            return Json(new { Success = true, order.OrderId });
        }

        // GET: Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Order
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order != null)
            {
                _context.Order.Remove(order);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(int id)
        {
            return _context.Order.Any(e => e.OrderId == id);
        }
    }
}

public class CartItem
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
