using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface INganhService
    {
        Task<List<Nganh>> LayDanhSachNganhAsync();
        Task<(bool Success, string Message)> NhapNganhTuExcelAsync(IFormFile file);
    }
}
