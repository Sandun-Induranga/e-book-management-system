using EBookPvtLtd.Data;
using EBookPvtLtd.Models;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

namespace EBookPvtLtd.Controllers
{
    public class AdminController : Controller
    {
        private readonly EBookPvtLtdContext _context;

        public AdminController(EBookPvtLtdContext context)
        {
            _context = context;
        }

        // GET: Admin Dashboard
        public IActionResult Index()
        {
            var totalBooks = _context.Book.Count();
            var totalUsers = _context.Users.Count();
            var totalOrders = _context.Order.Count();

            // Aggregate sales data by year and month
            var salesData = _context.Order
                .Where(o => o.Status == "Completed")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    OrderDate = g.Key.Month,
                    Month = g.Key.Month,
                    Total = g.Sum(o => o.TotalAmount)
                })
                .OrderBy(o => o.Month)
                .ToList();

            // Aggregate genre data, limiting to top 5 genres
            var genreData = _context.Book
                .GroupBy(b => b.Genre)
                .Select(g => new { Genre = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            // Pass data to the view
            var dashboardData = new
            {
                TotalBooks = totalBooks,
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                SalesData = salesData,
                GenreData = genreData
            };

            ViewBag.DashboardData = dashboardData;

            return View();
        }

        // View for selecting report options
        public IActionResult Reports()
        {
            return View();
        }

        // Generate report based on admin input
        [HttpPost]
        public IActionResult GenerateReport(string reportType, DateTime startDate, DateTime endDate)
        {
            var reportData = new List<Order>();

            switch (reportType)
            {
                case "Sales":
                    reportData = _context.Order
                        .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                        .ToList();
                    break;
                default:
                    ModelState.AddModelError("", "Invalid report type selected.");
                    return View("Reports");
            }

            TempData["ReportData"] = System.Text.Json.JsonSerializer.Serialize(reportData);

            return View("ReportResults", reportData);
        }

        // Export report to Excel
        [HttpPost]
        public IActionResult ExportToExcel()
        {
            var reportDataJson = TempData["ReportData"]?.ToString();

            if (string.IsNullOrEmpty(reportDataJson))
            {
                ViewBag.ErrorMessage = "No report data available for export.";
                return View("Reports");
            }

            List<Order> reportData;
            try
            {
                reportData = System.Text.Json.JsonSerializer.Deserialize<List<Order>>(reportDataJson);
            }
            catch
            {
                ViewBag.ErrorMessage = "Failed to deserialize report data.";
                return View("Reports");
            }

            using (var package = new ExcelPackage())
            {
                // Create a worksheet
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Add headers
                worksheet.Cells["A1"].Value = "Order ID";
                worksheet.Cells["B1"].Value = "Customer ID";
                worksheet.Cells["C1"].Value = "Order Date";
                worksheet.Cells["D1"].Value = "Status";
                worksheet.Cells["E1"].Value = "Total Amount";

                // Style headers
                using (var headerRange = worksheet.Cells["A1:E1"])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Populate data
                int rowIndex = 2;
                decimal totalIncome = 0;

                foreach (var order in reportData)
                {

                    worksheet.Cells[rowIndex, 1].Value = order.OrderId;
                    worksheet.Cells[rowIndex, 2].Value = order.CustomerId;
                    worksheet.Cells[rowIndex, 3].Value = order.OrderDate.ToString("yyyy-MM-dd");
                    worksheet.Cells[rowIndex, 4].Value = order.Status;
                    worksheet.Cells[rowIndex, 5].Value = order.TotalAmount;
                    worksheet.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0.00"; // Format as currency

                    // Highlight completed orders in green, pending in red
                    worksheet.Cells[rowIndex, 4].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[rowIndex, 4].Style.Fill.BackgroundColor.SetColor(order.Status == "Completed" ? System.Drawing.Color.LightGreen : order.Status == "Processing" ? System.Drawing.Color.Blue :  System.Drawing.Color.LightSalmon);

                    totalIncome += order.TotalAmount;
                    rowIndex++;
                }

                // Add total income row
                worksheet.Cells[rowIndex, 4].Value = "Total Income:";
                worksheet.Cells[rowIndex, 4].Style.Font.Bold = true;
                worksheet.Cells[rowIndex, 5].Value = totalIncome;
                worksheet.Cells[rowIndex, 5].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[rowIndex, 5].Style.Font.Bold = true;

                // Adjust column widths
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Create a memory stream and write the Excel package to it
                var stream = new MemoryStream(package.GetAsByteArray());

                // Add a timestamp to the file name for better organization
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Report{timestamp}.xlsx");
            }
        }
    }
}
