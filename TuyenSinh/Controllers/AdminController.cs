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
        private readonly INganhService _nganhService;

        public AdminController(
            IMonHocService monHocService,
            IToHopMonService toHopMonService,
            INganhService nganhService)
        {
            _monHocService = monHocService;
            _toHopMonService = toHopMonService;
            _nganhService = nganhService;
        }

        [HttpGet("")]
        public async Task<IActionResult> TongQuan()
        {
            var subjects = await _monHocService.LayDanhSachMonHocAsync();
            var combinations = await _toHopMonService.LayDanhSachToHopAsync();
            var majors = await _nganhService.LayDanhSachNganhAsync();

            ViewBag.CountSubjects = subjects.Count;
            ViewBag.CountCombinations = combinations.Count;
            ViewBag.CountMajors = majors.Count;
            return View("Index");
        }
    }
}
