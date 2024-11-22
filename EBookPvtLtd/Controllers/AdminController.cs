using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EBookPvtLtd.Controllers
{
    public class AdminController : Controller
    {
        // GET: AdminController
        public ActionResult Index()
        {
            return View();
        }
    }
}
