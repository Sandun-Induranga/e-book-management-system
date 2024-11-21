using EBookPvtLtd.Data;
using EBookPvtLtd.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EBookPvtLtd.Controllers
{
    public class BooksController : Controller
    {
        private readonly EBookPvtLtdContext _context;
        private readonly ILogger<EBookPvtLtdContext> _logger;

        public BooksController(EBookPvtLtdContext context, ILogger<EBookPvtLtdContext> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            return View(await _context.Book.ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // GET: Cart
        public IActionResult Cart()
        {
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookId,Title,Author,Genre,Price,Quantity,Description")] Book book, IFormFile Image)
        {
            // if (ModelState.IsValid)
            // {
            if (Image != null && Image.Length > 0)
            {
                try
                {
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    book.ImagePath = "/images/" + uniqueFileName;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error uploading image. Please try again.");
                    return View(book);
                }
            }

            _context.Add(book);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

            // Log ModelState errors
            /* foreach (var modelStateKey in ModelState.Keys)
             {
                 var modelStateVal = ModelState[modelStateKey];
                 foreach (var error in modelStateVal.Errors)
                 {
                     _logger.LogError($"Key: {modelStateKey}, Error: {error.ErrorMessage}");
                 }
             }*/

            // return View(book);
        }

        [HttpPost]
        public JsonResult GetCartBooks([FromBody] List<CartItem> cartItems)
        {
            try
            {
                if (cartItems == null || cartItems.Count == 0)
                {
                    return Json(new List<object>()); // Return an empty list if the cart is empty
                }

                // Extract BookIds from cartItems
                var bookIds = cartItems.Select(ci => ci.BookId).ToList();

                // Fetch books from the database
                var books = _context.Book
                    .Where(b => bookIds.Contains(b.BookId))
                    .Select(b => new
                    {
                        bookId = b.BookId,
                        title = b.Title,
                        price = b.Price,
                        imagePath = b.ImagePath
                    }).ToList();

                return Json(books);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetCartBooks: {ex.Message}");
                return Json(new { error = "Failed to load cart items." });
            }
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookId,Title,Author,Genre,Price,Quantity,Description,ImagePath")] Book book, IFormFile Image)
        {
            if (id != book.BookId)
            {
                return NotFound();
            }
            try
            {
                // Handle new image upload, if provided
                if (Image != null && Image.Length > 0)
                {
                    // Set the path to the wwwroot/images folder
                    string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

                    // Create a unique file name for the image
                    string uniqueFileName = Guid.NewGuid().ToString() + "_" + Image.FileName;

                    // Set the full file path
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Save the new image to the folder
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    // Delete the old image file, if a new one is uploaded and exists
                    if (!string.IsNullOrEmpty(book.ImagePath))
                    {
                        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", book.ImagePath.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Update the ImagePath with the new image's relative path
                    book.ImagePath = "/images/" + uniqueFileName;
                }

                // Update the book in the database
                _context.Update(book);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(book.BookId))
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

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Book
                .FirstOrDefaultAsync(m => m.BookId == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Book.FindAsync(id);
            if (book != null)
            {
                _context.Book.Remove(book);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Book.Any(e => e.BookId == id);
        }
    }
}
