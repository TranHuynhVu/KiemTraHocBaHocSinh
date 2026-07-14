using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TuyenSinh.Models;
using Microsoft.Extensions.Logging;

namespace TuyenSinh.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("TongQuan", "Admin");
            }
            return RedirectToAction("Login", "Account");
        }

        [HttpGet("chinh-sach-bao-mat")]
        public IActionResult ChinhSachBaoMat()
        {
            return View("Privacy");
        }

        [HttpGet("loi")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult BaoLoi()
        {
            return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
