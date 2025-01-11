using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EBookPvtLtd.Data;
using EBookPvtLtd.Models;
using EBookPvtLtd.Areas.Identity.Pages.Account;

namespace EBookPvtLtd.Controllers
{
    public class OrdersController : Controller
    {
        private readonly EBookPvtLtdContext _context;
        private readonly ILogger<EBookPvtLtdContext> _logger;
        static int customerId = 0;

        public OrdersController(EBookPvtLtdContext context, ILogger<EBookPvtLtdContext> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Orders
        public IActionResult Index()
        {
            IEnumerable<Order> orders = _context.Order
                    .Include(o => o.Customer)
                    .ToList();
                ViewBag.Title = "Manage Orders";
          
            return View(orders);
        }

        public IActionResult CustomerIndex()
        {
            var userId = TempData["CustomerId"] as int? ?? 0;

            if(customerId == 0)
            {
                customerId = userId;
            }

            IEnumerable<Order> orders = _context.Order
                    .Include(o => o.Customer)
                    .Where(o => o.Customer.CustomerId == customerId) // Fetch orders for the logged-in customer
                    .ToList();
                ViewBag.Title = "My Orders";
            return View(orders);
        }

        // GET: Orders/Details/id
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Order
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Prepare the data to return as JSON
            var orderDetails = new
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                Status = order.Status,
                TotalAmount = order.TotalAmount.ToString("C"),
                Customer = new
                {
                    order.Customer.FirstName,
                    order.Customer.LastName,
                    order.Customer.Email
                },
                Items = order.OrderItems.Select(oi => new
                {
                    oi.Book.Title,
                    oi.Price,
                    oi.Quantity,
                    TotalPrice = (oi.Price * oi.Quantity).ToString("C")
                })
            };

            return Json(orderDetails);
        }

        // GET: Orders/Create
        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "CustomerId");
            return RedirectToAction("CustomerIndex", "Orders");
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
            }
            ViewData["CustomerId"] = new SelectList(_context.Customer, "CustomerId", "CustomerId", order.CustomerId);
            return RedirectToAction("CustomerIndex", "Orders");
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
            decimal totalAmount = 0;
            
            foreach (var item in cartItems)
            {
                var book = await _context.Book.FindAsync(item.BookId);
                if (book != null)
                {
                    totalAmount += book.Price * item.Quantity;
                }
            }

            // Create a new order
            var order = new Order
            {
                CustomerId = LoginModel.customerId,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = totalAmount,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _context.Order.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.Status != "Pending")
            {
                return BadRequest("Only pending orders can be cancelled.");
            }

            order.Status = "Cancelled";
            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Order cancelled successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling order with ID {0}.", id);
                TempData["ErrorMessage"] = "An error occurred while cancelling the order.";
            }

            return RedirectToAction(nameof(CustomerIndex));
        }

        [HttpPost]
        public IActionResult ChangeStatus(int id, string status)
        {
            var order = _context.Order.FirstOrDefault(o => o.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _context.SaveChanges();

            return Json(new { success = true });
        }
    }
}

public class CartItem
{
    public int BookId { get; set; }
    public int Quantity { get; set; }
}
