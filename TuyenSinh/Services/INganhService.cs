using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using TuyenSinh.Common;
using TuyenSinh.Models;

namespace TuyenSinh.Services
{
    public interface INganhService
    {
        Task<List<Nganh>> LayDanhSachNganhAsync();
        Task<ServiceResult> NhapNganhTuExcelAsync(IFormFile file);
    }
}
