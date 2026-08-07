using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly IMonHocService _monHocService;
        private readonly IToHopMonService _toHopMonService;

        public AdminController(
            IMonHocService monHocService,
            IToHopMonService toHopMonService)
        {
            _monHocService = monHocService;
            _toHopMonService = toHopMonService;
        }

        [HttpGet("")]
        public async Task<IActionResult> TongQuan()
        {
            var subjects = await _monHocService.LayDanhSachMonHocAsync();
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            ViewBag.CountSubjects = subjects.Count;
            ViewBag.CountCombinations = combinations.Count;
            return View("Index");
        }
    }
}
