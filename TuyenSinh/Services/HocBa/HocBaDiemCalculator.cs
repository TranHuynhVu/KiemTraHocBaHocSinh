using System;
using TuyenSinh.Enums;
using TuyenSinh.ViewModels.HocBa;

namespace TuyenSinh.Services.HocBa
{
    public sealed class HocBaDiemCalculator : IHocBaDiemCalculator
    {
        public decimal? GetScore(HocBaTHPTImport record, string fieldName)
        {
            if (record == null || string.IsNullOrWhiteSpace(fieldName)) return null;

            var code = fieldName.Trim().ToUpper();
            if (Enum.TryParse<MaMonHoc>(code, out var maMon))
            {
                return maMon switch
                {
                    MaMonHoc.TO => record.ToanCN,
                    MaMonHoc.VA => record.VanCN,
                    MaMonHoc.LI => record.VatLyCN,
                    MaMonHoc.HO => record.HoaHocCN,
                    MaMonHoc.SI => record.SinhHocCN,
                    MaMonHoc.SU => record.LichSuCN,
                    MaMonHoc.DI => record.DiaLyCN,
                    MaMonHoc.GD => record.GDCDCN,
                    MaMonHoc.TI => record.TinHocCN,
                    MaMonHoc.CNCN => record.CNCNCN,
                    MaMonHoc.N1 or MaMonHoc.N2 or MaMonHoc.N3 or MaMonHoc.N4 or MaMonHoc.N5 or MaMonHoc.N6 => GetNgoaiNguScore(record, code),
                    _ => null
                };
            }

            return code switch
            {
                "KTPL" => record.KTPLCN,
                "CNNN" => record.CNNNCN,
                "CNCN" => record.CNCNCN,
                _ => null
            };
        }

        public string LayTenHienThiMonHocCN(string fieldName)
        {
            if (Enum.TryParse<MaMonHoc>(fieldName.ToUpper(), out var maMon))
            {
                return maMon switch
                {
                    MaMonHoc.TO => "Toán CN",
                    MaMonHoc.VA => "Văn CN",
                    MaMonHoc.LI => "Vật lí CN",
                    MaMonHoc.HO => "Hóa học CN",
                    MaMonHoc.SI => "Sinh học CN",
                    MaMonHoc.SU => "Lịch sử CN",
                    MaMonHoc.DI => "Địa lí CN",
                    MaMonHoc.GD => "GDCD CN",
                    MaMonHoc.TI => "Tin học CN",
                    MaMonHoc.CNCN => "CNCN CN",
                    MaMonHoc.NN => "Ngoại ngữ CN",
                    _ => fieldName + " CN"
                };
            }
            return fieldName + " CN";
        }

        public decimal? GetNgoaiNguScore(HocBaTHPTImport record, string code)
        {
            if (record == null) return null;

            var mon1 = record.MonNgoaiNgu?.Trim().ToUpper();
            var mon2 = record.MonNgoaiNgu2?.Trim().ToUpper();

            // 1. Nếu Môn ngoại ngữ 1 khớp mã yêu cầu -> lấy điểm Ngoại ngữ CN (dự phòng Ngoại ngữ 2 CN)
            if (!string.IsNullOrEmpty(mon1) && mon1.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return record.NgoaiNguCN ?? record.NgoaiNgu2CN;
            }

            // 2. Nếu Môn ngoại ngữ 2 khớp mã yêu cầu -> lấy điểm Ngoại ngữ 2 CN (dự phòng Ngoại ngữ CN)
            if (!string.IsNullOrEmpty(mon2) && mon2.Equals(code, StringComparison.OrdinalIgnoreCase))
            {
                return record.NgoaiNgu2CN ?? record.NgoaiNguCN;
            }

            return null;
        }
    }
}
