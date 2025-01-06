using Microsoft.AspNetCore.Mvc;
using EBookPvtLtd.Data;
using EBookPvtLtd.Models;
using Microsoft.EntityFrameworkCore;

public class FeedbacksController : Controller
{
    private readonly EBookPvtLtdContext _context;

    public FeedbacksController(EBookPvtLtdContext context)
    {
        _context = context;
    }

    // GET: Feedback for a specific book
    [HttpGet]
    public IActionResult Index(int bookId)
    {
        // Retrieve feedback along with book details
        var feedbacks = _context.Feedback
            .Include(f => f.Book) // Include Book details
            .Where(f => f.BookId == bookId)
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        var book = _context.Book.FirstOrDefault(b => b.BookId == bookId);
        if (book == null)
        {
            return NotFound();
        }

        ViewBag.Book = book; // Pass the book to the view
        return View(feedbacks);
    }

    // GET: Add Feedback
    [HttpGet]
    public IActionResult Create(int bookId)
    {
        ViewBag.BookId = bookId;
        return View();
    }

    // POST: Add Feedback
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(int bookId, Feedback feedback)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("ModelState is invalid:");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Error: {error.ErrorMessage}");
            }

            ViewBag.BookId = bookId;
            return View(feedback);
        }

        try
        {
            feedback.BookId = bookId;
            _context.Feedback.Add(feedback);
            _context.SaveChanges();
            Console.WriteLine("Feedback added successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while adding feedback: {ex.Message}");
            ModelState.AddModelError("", "Failed to save feedback. Please try again.");
            ViewBag.BookId = bookId;
            return View(feedback);
        }

        return RedirectToAction("Index", new { bookId });
    }

    [HttpGet]
    public IActionResult GetFeedback(int bookId)
    {
        var feedbacks = _context.Feedback
            .Where(f => f.BookId == bookId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.CustomerName,
                f.Content,
                f.CreatedAt
            })
            .ToList();

        return Json(feedbacks);
    }

    [HttpPost]
    public IActionResult AddFeedback([FromBody] Feedback feedback)
    {
        if (ModelState.IsValid)
        {
            try
            {
                feedback.CreatedAt = DateTime.Now; // Ensure CreatedAt is set
                _context.Feedback.Add(feedback);
                _context.SaveChanges();
                return Ok(new { Success = true, Message = "Feedback added successfully!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = "Error saving feedback.", Error = ex.Message });
            }
        }

        return BadRequest(new { Success = false, Message = "Invalid feedback data.", Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
    }
}
