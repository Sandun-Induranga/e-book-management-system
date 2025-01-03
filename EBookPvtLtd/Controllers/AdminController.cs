using EBookPvtLtd.Data;
using EBookPvtLtd.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace EBookPvtLtd.Controllers
{
    public class AdminController : Controller
    {

        private readonly EBookPvtLtdContext _context;
        private List<Order> orders = [];

        public AdminController(EBookPvtLtdContext context)
        {
            _context = context;
        }


        // GET: AdminController
        public ActionResult Index()
        {
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
                    .Select(o => o)
                    .ToList();
                break;
            default:
                ViewBag.ErrorMessage = "Invalid report type selected.";
                return View("Reports");
        }

        orders = reportData;
        TempData["ReportData"] = System.Text.Json.JsonSerializer.Serialize(reportData);
        TempData["ReportType"] = reportType;

        return View("ReportResults", reportData);
    }

        [HttpPost]
        public IActionResult ExportReport()
        {
            var reportData = TempData["ReportData"] as List<object>;
            var reportType = TempData["ReportType"]?.ToString();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");
                worksheet.Cells.LoadFromCollection(reportData, true);
                var stream = new MemoryStream(package.GetAsByteArray());
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{reportType}_Report.xlsx");
            }
        }

        [HttpPost]
        public IActionResult ExportToExcel()
        {
            // Enable non-commercial use of EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Report");

                // Add headers
                worksheet.Cells[1, 1].Value = "Order ID";
                worksheet.Cells[1, 2].Value = "Customer ID";
                worksheet.Cells[1, 3].Value = "Total Amount";
                worksheet.Cells[1, 4].Value = "Order Date";

                // Add data (replace with your actual data)
                var orders = _context.Order.ToList(); // Example: Fetch orders from database
                for (int i = 0; i < orders.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = orders[i].OrderId;
                    worksheet.Cells[i + 2, 2].Value = orders[i].CustomerId;
                    worksheet.Cells[i + 2, 3].Value = orders[i].Status;
                    worksheet.Cells[i + 2, 4].Value = orders[i].OrderDate.ToString("yyyy-MM-dd");
                }

                // Auto-fit columns
                worksheet.Cells.AutoFitColumns();

                // Generate Excel file in memory
                var stream = new MemoryStream(package.GetAsByteArray());

                // Return file as downloadable response
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
            }
        }
    }
}
