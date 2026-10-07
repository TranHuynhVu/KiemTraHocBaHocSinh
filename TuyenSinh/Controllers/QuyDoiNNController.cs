using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Services;

namespace TuyenSinh.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("admin/quy-doi-ngoai-ngu")]
    public class QuyDoiNNController(IQuyDoiNNService quyDoiNNService) : Controller
    {
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var dsQuyDoi = await quyDoiNNService.LayDanhSachQuyDoiAsync();
            ViewBag.DanhSachBac = await quyDoiNNService.LayDanhSachBacAsync();
            ViewBag.DanhSachLoai = await quyDoiNNService.LayDanhSachLoaiAsync();
            return View("Index", dsQuyDoi);
        }

        [HttpPost("diem-quy-doi/them")]
        public async Task<IActionResult> ThemDiemQuyDoi(int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi)
        {
            var result = await quyDoiNNService.ThemQuyDoiAsync(bacNgoaiNguId, loaiNgoaiNguId, diemNN, diemQuyDoi);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("diem-quy-doi/sua")]
        public async Task<IActionResult> SuaDiemQuyDoi(int id, int bacNgoaiNguId, int loaiNgoaiNguId, decimal diemNN, decimal diemQuyDoi)
        {
            var result = await quyDoiNNService.SuaQuyDoiAsync(id, bacNgoaiNguId, loaiNgoaiNguId, diemNN, diemQuyDoi);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("diem-quy-doi/xoa")]
        public async Task<IActionResult> XoaDiemQuyDoi(int id)
        {
            var result = await quyDoiNNService.XoaQuyDoiAsync(id);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("bac-ngoai-ngu")]
        public async Task<IActionResult> QuanLyBac()
        {
            var dsBac = await quyDoiNNService.LayDanhSachBacAsync();
            return View("BacNgoaiNgu", dsBac);
        }

        [HttpPost("bac-ngoai-ngu/them")]
        public async Task<IActionResult> ThemBac(string tenBac, string tenVietTat)
        {
            var result = await quyDoiNNService.ThemBacAsync(tenBac, tenVietTat);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(QuanLyBac));
        }

        [HttpPost("bac-ngoai-ngu/sua")]
        public async Task<IActionResult> SuaBac(int id, string tenBac, string tenVietTat)
        {
            var result = await quyDoiNNService.SuaBacAsync(id, tenBac, tenVietTat);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(QuanLyBac));
        }

        [HttpPost("bac-ngoai-ngu/xoa")]
        public async Task<IActionResult> XoaBac(int id)
        {
            var result = await quyDoiNNService.XoaBacAsync(id);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(QuanLyBac));
        }

        [HttpGet("loai-ngoai-ngu")]
        public async Task<IActionResult> QuanLyLoai()
        {
            var dsLoai = await quyDoiNNService.LayDanhSachLoaiAsync();
            return View("LoaiNgoaiNgu", dsLoai);
        }

        [HttpPost("loai-ngoai-ngu/them")]
        public async Task<IActionResult> ThemLoai(string tenLoai)
        {
            var result = await quyDoiNNService.ThemLoaiAsync(tenLoai);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(QuanLyLoai));
        }

        [HttpPost("loai-ngoai-ngu/sua")]
        public async Task<IActionResult> SuaLoai(int id, string tenLoai)
        {
            var result = await quyDoiNNService.SuaLoaiAsync(id, tenLoai);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(QuanLyLoai));
        }

        [HttpPost("loai-ngoai-ngu/xoa")]
        public async Task<IActionResult> XoaLoai(int id)
        {
            var result = await quyDoiNNService.XoaLoaiAsync(id);
            result.SetNotification(TempData);
            return RedirectToAction(nameof(QuanLyLoai));
        }
    }
}
