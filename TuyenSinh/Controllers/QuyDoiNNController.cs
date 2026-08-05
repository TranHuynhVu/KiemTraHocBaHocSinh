using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/quy-doi-ngoai-ngu")]
    public class QuyDoiNNController : Controller
    {
        private readonly IQuyDoiNNService _quyDoiNNService;

        public QuyDoiNNController(IQuyDoiNNService quyDoiNNService)
        {
            _quyDoiNNService = quyDoiNNService;
        }

        #region Quản lý Điểm Quy Đổi Ngoại Ngữ (Trang chính & Matrix)

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var dsQuyDoi = await _quyDoiNNService.LayDanhSachQuyDoiAsync();
            ViewBag.DanhSachBac = await _quyDoiNNService.LayDanhSachBacAsync();
            ViewBag.DanhSachLoai = await _quyDoiNNService.LayDanhSachLoaiAsync();
            return View("Index", dsQuyDoi);
        }

        [HttpPost("diem-quy-doi/them")]
        public async Task<IActionResult> ThemDiemQuyDoi(int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal? diemNNDen, decimal diemQuyDoi)
        {
            var result = await _quyDoiNNService.ThemQuyDoiAsync(bacNgoaiNguId, loaiNgoaiNguId, diemNN, diemNNDen, diemQuyDoi);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("diem-quy-doi/sua")]
        public async Task<IActionResult> SuaDiemQuyDoi(int id, int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal? diemNNDen, decimal diemQuyDoi)
        {
            var result = await _quyDoiNNService.SuaQuyDoiAsync(id, bacNgoaiNguId, loaiNgoaiNguId, diemNN, diemNNDen, diemQuyDoi);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("diem-quy-doi/xoa")]
        public async Task<IActionResult> XoaDiemQuyDoi(int id)
        {
            var result = await _quyDoiNNService.XoaQuyDoiAsync(id);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Quản lý Bậc Ngoại Ngữ

        [HttpGet("bac-ngoai-ngu")]
        public async Task<IActionResult> QuanLyBac()
        {
            var dsBac = await _quyDoiNNService.LayDanhSachBacAsync();
            return View("BacNgoaiNgu", dsBac);
        }

        [HttpPost("bac-ngoai-ngu/them")]
        public async Task<IActionResult> ThemBac(string tenBac, string tenVietTat)
        {
            var result = await _quyDoiNNService.ThemBacAsync(tenBac, tenVietTat);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(QuanLyBac));
        }

        [HttpPost("bac-ngoai-ngu/sua")]
        public async Task<IActionResult> SuaBac(int id, string tenBac, string tenVietTat)
        {
            var result = await _quyDoiNNService.SuaBacAsync(id, tenBac, tenVietTat);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(QuanLyBac));
        }

        [HttpPost("bac-ngoai-ngu/xoa")]
        public async Task<IActionResult> XoaBac(int id)
        {
            var result = await _quyDoiNNService.XoaBacAsync(id);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(QuanLyBac));
        }

        #endregion

        #region Quản lý Loại Ngoại Ngữ

        [HttpGet("loai-ngoai-ngu")]
        public async Task<IActionResult> QuanLyLoai()
        {
            var dsLoai = await _quyDoiNNService.LayDanhSachLoaiAsync();
            return View("LoaiNgoaiNgu", dsLoai);
        }

        [HttpPost("loai-ngoai-ngu/them")]
        public async Task<IActionResult> ThemLoai(string tenLoai)
        {
            var result = await _quyDoiNNService.ThemLoaiAsync(tenLoai);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(QuanLyLoai));
        }

        [HttpPost("loai-ngoai-ngu/sua")]
        public async Task<IActionResult> SuaLoai(int id, string tenLoai)
        {
            var result = await _quyDoiNNService.SuaLoaiAsync(id, tenLoai);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(QuanLyLoai));
        }

        [HttpPost("loai-ngoai-ngu/xoa")]
        public async Task<IActionResult> XoaLoai(int id)
        {
            var result = await _quyDoiNNService.XoaLoaiAsync(id);
            if (result.Success)
            {
                TempData["Success"] = result.Message;
            }
            else
            {
                TempData["Error"] = result.Message;
            }
            return RedirectToAction(nameof(QuanLyLoai));
        }

        #endregion
    }
}
