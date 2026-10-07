using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public interface IHocBaDiemCalculator
    {
        decimal? GetScore(HocBaTHPTImport record, string fieldName);
        string LayTenHienThiMonHocCN(string fieldName);
        decimal? GetNgoaiNguScore(HocBaTHPTImport record, string code);
    }
}
