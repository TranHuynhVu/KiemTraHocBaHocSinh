$(document).ready(function () {
    let lastResultData = [];

    const table = $('#doiChieuTable').DataTable({
        ajax: {
            url: `/admin/hoc-ba/lay-ket-qua-doi-chieu?hocBaFileId=${hocBaFileId}&nguyenVongFileId=${nguyenVongFileId}`,
            dataSrc: function (json) {
                if (!json.success) {
                    Swal.fire('Lỗi', json.message || 'Không thể tải dữ liệu đối chiếu.', 'error');
                    $('#summaryLoading, #tableNganhLoading').addClass('d-none');
                    $('#summaryContent, #tableNganhContent').removeClass('d-none');
                    return [];
                }

                lastResultData = json.data || [];
                const tongDong = lastResultData.length || 0;
                const uniqueThiSinh = new Set(lastResultData.map(item => item.SoDDCN).filter(cccd => cccd)).size;

                $('#statTongDongThieuDiem').text(tongDong.toLocaleString() + ' dòng');
                $('#statTongThiSinhThieuDiem').text(uniqueThiSinh.toLocaleString() + ' thí sinh');

                // 1. Render Table 1: Thống kê tổng hợp
                $('#summaryLoading').addClass('d-none');
                $('#summaryContent').removeClass('d-none');

                const th = json.thongKeTongHop || {};
                $('#statTongDongNV').text((th.TongDongNguyenVong || 0).toLocaleString());
                $('#statTongThiSinh').text((th.TongThiSinhDuyNhat || 0).toLocaleString());
                $('#statTongThiSinhBiAnhHuong').text((th.TongThiSinhBiAnhHuong || 0).toLocaleString());
                $('#statTongNganhBiAnhHuong').text((th.TongNganhBiAnhHuong || 0).toLocaleString());
                $('#statNVCoToHopDu').text((th.NguyenVongCoToHopDu || 0).toLocaleString());
                $('#statNVThieuMoiToHop').text((th.NguyenVongThieuMoiToHop || 0).toLocaleString());
                $('#statNVKhongHocBa').text((th.NguyenVongKhongHocBa || 0).toLocaleString());
                $('#statNVKhongDiemCN').text((th.NguyenVongKhongDiemCN || 0).toLocaleString());
                $('#statNguyenVongBoQua').text((th.NguyenVongBoQua || 0).toLocaleString());

                window.groupLists = {
                    ThieuMoiToHop: th.DanhSachThieuMoiToHop || [],
                    KhongHocBa: th.DanhSachKhongHocBa || [],
                    KhongDiemCN: th.DanhSachKhongDiemCN || [],
                    BoQua: th.DanhSachBoQua || [],
                    ThiSinhBiAnhHuong: th.DanhSachThiSinhBiAnhHuong || [],
                    NganhBiAnhHuong: th.DanhSachNganhBiAnhHuong || []
                };

                // 2. Render Table 2: Thống kê theo ngành
                $('#tableNganhLoading').addClass('d-none');
                $('#tableNganhContent').removeClass('d-none');

                const dsNganh = json.thongKeTheoNganh || [];

                window.nganhGroupLists = {};
                dsNganh.forEach(function (item) {
                    const ma = item.MaXetTuyen || '';
                    window.nganhGroupLists[ma] = {
                        ThieuMoiToHop: item.DanhSachThieuMoiToHop || [],
                        KhongHocBa: item.DanhSachKhongHocBa || [],
                        KhongDiemCN: item.DanhSachKhongDiemCN || []
                    };
                });

                if (dsNganh.length > 0) {
                    const rowsHtml = dsNganh.map(function (item) {
                        const ma = item.MaXetTuyen || '';
                        const ten = item.TenNganh || '';
                        const tongNV = (item.TongNV || 0).toLocaleString();
                        const soThiSinh = (item.SoThiSinh || 0).toLocaleString();
                        const nvCoToHopDu = (item.NVCoToHopDu || 0).toLocaleString();

                        const rawThieu = item.NVThieuMoiToHop || 0;
                        const rawDiemCN = item.NVKhongDiemCN || 0;
                        const rawHocBa = item.NVKhongHocBa || 0;

                        const valTyLe = item.TyLeThieu || 0;
                        const tyLeThieu = valTyLe.toFixed(2) + '%';
                        const badgeClass = valTyLe > 0 ? 'text-danger fw-bold' : 'text-success';

                        // Render clickable links for 3 error columns (only if count > 0)
                        const tenEsc = ten.replace(/"/g, '&quot;');
                        const cellThieu = rawThieu > 0
                            ? `<a href="javascript:void(0)" class="nganh-group-link text-danger text-decoration-underline fw-semibold"
                                data-ma="${ma}" data-ten="${tenEsc}" data-loai="ThieuMoiToHop">${rawThieu.toLocaleString()}</a>`
                            : `<span class="text-muted">0</span>`;

                        const cellDiemCN = rawDiemCN > 0
                            ? `<a href="javascript:void(0)" class="nganh-group-link text-warning text-decoration-underline fw-semibold"
                                data-ma="${ma}" data-ten="${tenEsc}" data-loai="KhongDiemCN">${rawDiemCN.toLocaleString()}</a>`
                            : `<span class="text-muted">0</span>`;

                        const cellHocBa = rawHocBa > 0
                            ? `<a href="javascript:void(0)" class="nganh-group-link text-secondary text-decoration-underline fw-semibold"
                                data-ma="${ma}" data-ten="${tenEsc}" data-loai="KhongHocBa">${rawHocBa.toLocaleString()}</a>`
                            : `<span class="text-muted">0</span>`;

                        return `<tr>
                            <td><code>${ma}</code></td>
                            <td>${ten}</td>
                            <td class="text-end fw-semibold">${tongNV}</td>
                            <td class="text-end">${soThiSinh}</td>
                            <td class="text-end text-success">${nvCoToHopDu}</td>
                            <td class="text-end">${cellThieu}</td>
                            <td class="text-end">${cellDiemCN}</td>
                            <td class="text-end">${cellHocBa}</td>
                            <td class="text-end ${badgeClass}">${tyLeThieu}</td>
                        </tr>`;
                    }).join('');

                    $('#tbodyThongKeTheoNganh').html(rowsHtml);
                } else {
                    $('#tbodyThongKeTheoNganh').html('<tr><td colspan="9" class="text-center text-muted py-3">Không có dữ liệu ngành</td></tr>');
                }

                // Update Warnings Alert
                const alertBox = $('#alertNganhNotFound');
                if (json.tongLoiKhongTimThayNganh > 0) {
                    $('#alertNganhNotFoundText').html(
                        `Các mã xét tuyển sau đây xuất hiện trong file nguyện vọng nhưng không được định nghĩa trong danh mục ngành học của hệ thống: ` +
                        `<strong>${json.danhSachMaNganhKhongTim.join(', ')}</strong>`
                    );
                    alertBox.removeClass('d-none');
                } else {
                    alertBox.addClass('d-none');
                }

                return lastResultData;
            }
        },
        processing: true,
        deferRender: true,
        columns: [
            {
                data: null,
                render: function (data, type, row, meta) {
                    return meta.row + 1;
                }
            },
            { data: 'SoDDCN', render: function (data) { return `<span class="cccd-link">${data || ''}</span>`; } },
            { data: 'HoVaTen', render: function (data) { return `<strong class="text-dark">${data || ''}</strong>`; } },
            { data: 'ThuTuNV', render: function (data) { return `<span class="badge bg-secondary badge-combo-major">${data}</span>`; } },
            {
                data: 'MaNganh',
                render: function (data, type, row) {
                    return `<span class="badge-gray">${data || ''}</span> ${row.TenNganh || ''}`;
                }
            },
            { data: 'MaToHop', render: function (data) { return `<span class="badge bg-secondary badge-combo-major">${data || ''}</span>`; } },
            { data: 'NamHoc', render: function (data) { return `<span class="badge-gray">${data || ''}</span>`; } },
            { data: 'MonThieu', render: function (data) { return `<span class="text-error-subjects">${data || ''}</span>`; } }
        ],
        scrollX: false,
        scrollY: "450px",
        scrollCollapse: true,
        paging: true,
        pageLength: 25,
        lengthMenu: [10, 25, 50, 100],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json'
        },
        order: [[0, 'asc']]
    });

    function downloadExcelWithLoading(url, fileName) {
        Swal.fire({
            title: 'Đang xuất tệp Excel...',
            text: 'Hệ thống đang chuẩn bị tệp báo cáo Excel, vui lòng đợi trong giây lát...',
            allowOutsideClick: false,
            allowEscapeKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: url,
            method: 'GET',
            xhrFields: { responseType: 'blob' },
            success: function (blob) {
                Swal.close();
                const downloadUrl = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = downloadUrl;
                a.download = fileName;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(downloadUrl);
            },
            error: function (xhr, status, error) {
                Swal.close();
                Swal.fire('Lỗi', 'Không thể xuất tệp Excel: ' + error, 'error');
            }
        });
    }

    // Export main table Excel
    $('#btnExportExcel').click(function () {
        const url = `/admin/hoc-ba/xuat-excel-ket-qua-doi-chieu?hocBaFileId=${hocBaFileId}&nguyenVongFileId=${nguyenVongFileId}`;
        downloadExcelWithLoading(url, 'DoiChieu_HocBa_NguyenVong.xlsx');
    });

    // Export main table CSV
    $('#btnExportCsv').click(function () {
        if (!lastResultData || lastResultData.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Số ĐDCN', 'Họ và Tên', 'TT NV', 'Mã Ngành', 'Tên Ngành', 'Mã Tổ Hợp', 'Năm Học', 'Môn Thiếu'];
        const rows = lastResultData.map((item, idx) => [
            idx + 1,
            `"${item.SoDDCN || ''}"`,
            `"${item.HoVaTen || ''}"`,
            item.ThuTuNV,
            `"${item.MaNganh || ''}"`,
            `"${item.TenNganh || ''}"`,
            `"${item.MaToHop || ''}"`,
            `"${item.NamHoc || ''}"`,
            `"${(item.MonThieu || '').replace(/"/g, '""')}"`
        ]);

        const csvContent = '\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'DoiChieu_HocBa_NguyenVong.csv';
        a.click();
        URL.revokeObjectURL(url);
    });

    // Modal Chi tiết nhóm lỗi (Top 50) & Xuất Excel nhóm
    let currentGroupType = '';
    let currentGroupData = [];
    let currentGroupTitle = '';

    const LABEL_MAP = {
        ThieuMoiToHop: 'NV thiếu điểm ở mọi tổ hợp',
        KhongHocBa: 'NV không có học bạ',
        KhongDiemCN: 'NV trống điểm CN',
        BoQua: 'NV bị bỏ qua (hệ số = 0)'
    };

    function openGroupModal(type, title, data) {
        currentGroupType = type;
        currentGroupTitle = title;
        currentGroupData = data || [];

        $('#modalGroupTitle').text(title);
        const topCount = Math.min(50, currentGroupData.length);
        $('#badgeModalGroupCount').text(`Top ${topCount} / ${currentGroupData.length} dòng`);

        const top50 = currentGroupData.slice(0, 50);
        if (top50.length === 0) {
            $('#tbodyChiTietNhomLoi').html(`
                <tr>
                    <td colspan="5" class="text-center text-muted py-4">Không có dữ liệu trong nhóm này.</td>
                </tr>
            `);
        } else {
            const html = top50.map((item, index) => `
                <tr>
                    <td class="text-center text-muted">${index + 1}</td>
                    <td class="fw-semibold text-dark">${item.Cccd || ''}</td>
                    <td class="text-center fw-bold text-primary">${item.ThuTuNV || ''}</td>
                    <td><span class="badge bg-light text-dark border">${item.MaXetTuyen || ''}</span></td>
                    <td class="text-truncate" style="max-width: 250px;" title="${item.TenNganh || ''}">${item.TenNganh || ''}</td>
                </tr>
            `).join('');
            $('#tbodyChiTietNhomLoi').html(html);
        }

        const modal = new bootstrap.Modal(document.getElementById('modalChiTietNhomLoi'));
        modal.show();
    }

    // Modal 2: Chi tiết Thí sinh bị ảnh hưởng (Không trùng CCCD)
    function openThiSinhBiAnhHuongModal() {
        const list = (window.groupLists || {}).ThiSinhBiAnhHuong || [];
        const topCount = Math.min(50, list.length);
        $('#badgeModalThiSinhCount').text(`Top ${topCount} / ${list.length} thí sinh`);

        const top50 = list.slice(0, 50);
        if (top50.length === 0) {
            $('#tbodyThiSinhBiAnhHuong').html(`
                <tr>
                    <td colspan="5" class="text-center text-muted py-4">Không có thí sinh bị ảnh hưởng.</td>
                </tr>
            `);
        } else {
            const html = top50.map((item, index) => `
                <tr>
                    <td class="text-center text-muted">${item.Stt || (index + 1)}</td>
                    <td class="fw-semibold text-dark">${item.Cccd || ''}</td>
                    <td class="fw-semibold text-dark">${item.HoVaTen || ''}</td>
                    <td class="text-center"><span class="badge bg-light text-dark border">${item.SoNVLoi || 0} NV</span></td>
                    <td class="text-truncate small text-muted" style="max-width: 300px;" title="${item.ChiTietNVLoi || ''}">${item.ChiTietNVLoi || ''}</td>
                </tr>
            `).join('');
            $('#tbodyThiSinhBiAnhHuong').html(html);
        }

        const modal = new bootstrap.Modal(document.getElementById('modalThiSinhBiAnhHuong'));
        modal.show();
    }

    // Modal 3: Chi tiết Ngành bị ảnh hưởng (Không trùng Ngành)
    function openNganhBiAnhHuongModal() {
        const list = (window.groupLists || {}).NganhBiAnhHuong || [];
        const topCount = Math.min(50, list.length);
        $('#badgeModalNganhCount').text(`Top ${topCount} / ${list.length} ngành`);

        const top50 = list.slice(0, 50);
        if (top50.length === 0) {
            $('#tbodyNganhBiAnhHuong').html(`
                <tr>
                    <td colspan="5" class="text-center text-muted py-4">Không có ngành bị ảnh hưởng.</td>
                </tr>
            `);
        } else {
            const html = top50.map((item, index) => `
                <tr>
                    <td class="text-center text-muted">${item.Stt || (index + 1)}</td>
                    <td><span class="badge bg-light text-dark border">${item.MaXetTuyen || ''}</span></td>
                    <td class="fw-semibold text-dark">${item.TenNganh || ''}</td>
                    <td class="text-center fw-semibold text-dark">${item.SoThiSinhBiAnhHuong || 0} thí sinh</td>
                    <td class="text-center"><span class="badge bg-light text-dark border">${item.SoNVLoi || 0} NV</span></td>
                </tr>
            `).join('');
            $('#tbodyNganhBiAnhHuong').html(html);
        }

        const modal = new bootstrap.Modal(document.getElementById('modalNganhBiAnhHuong'));
        modal.show();
    }

    // ---- Bảng Thống kê tổng hợp – click handlers ----
    $('#statNVThieuMoiToHop').click(function () {
        openGroupModal('ThieuMoiToHop', 'Danh sách nguyện vọng thiếu môn trong tổ hợp',
            (window.groupLists || {}).ThieuMoiToHop || []);
    });
    $('#statNVKhongHocBa').click(function () {
        openGroupModal('KhongHocBa', 'Danh sách nguyện vọng không có học bạ',
            (window.groupLists || {}).KhongHocBa || []);
    });
    $('#statNVKhongDiemCN').click(function () {
        openGroupModal('KhongDiemCN', 'Danh sách nguyện vọng có điểm CN NULL',
            (window.groupLists || {}).KhongDiemCN || []);
    });
    $('#statTongThiSinhBiAnhHuong').click(function () {
        openThiSinhBiAnhHuongModal();
    });
    $('#statTongNganhBiAnhHuong').click(function () {
        openNganhBiAnhHuongModal();
    });
    $('#statNguyenVongBoQua').click(function () {
        openGroupModal('BoQua', 'Danh sách nguyện vọng bị bỏ qua',
            (window.groupLists || {}).BoQua || []);
    });

    // ---- Bảng Thống kê theo ngành – delegated click handler ----
    $(document).on('click', '.nganh-group-link', function () {
        const ma = $(this).data('ma');
        const ten = $(this).data('ten');
        const loai = $(this).data('loai');

        const nganhData = (window.nganhGroupLists || {})[ma] || {};
        const list = nganhData[loai] || [];

        const loaiLabel = LABEL_MAP[loai] || loai;
        const title = `[${ma}] ${ten} – ${loaiLabel}`;

        openGroupModal(loai, title, list);
    });

    // ---- Export Excel cho Modal Thí sinh bị ảnh hưởng ----
    $('#btnExportThiSinhExcel').click(function () {
        const list = (window.groupLists || {}).ThiSinhBiAnhHuong || [];
        if (!list || list.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu thí sinh để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Số ĐDCN (CCCD)', 'Họ và Tên', 'Số NV Lỗi', 'Chi Tiết Nguyện Vọng & Ngành Lỗi'];
        const rows = list.map((item, idx) => [
            idx + 1,
            item.Cccd || '',
            item.HoVaTen || '',
            item.SoNVLoi || 0,
            item.ChiTietNVLoi || ''
        ]);

        if (typeof XLSX !== 'undefined') {
            const ws = XLSX.utils.aoa_to_sheet([headers, ...rows]);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'ThiSinhBiAnhHuong');
            XLSX.writeFile(wb, 'DanhSach_ThiSinh_BiAnhHuong.xlsx');
        }
    });

    // ---- Export Excel cho Modal Ngành bị ảnh hưởng ----
    $('#btnExportNganhExcel').click(function () {
        const list = (window.groupLists || {}).NganhBiAnhHuong || [];
        if (!list || list.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu ngành để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Mã Xét Tuyển', 'Tên Ngành Xét Tuyển', 'Số Thí Sinh Bị Ảnh Hưởng', 'Số NV Bị Lỗi'];
        const rows = list.map((item, idx) => [
            idx + 1,
            item.MaXetTuyen || '',
            item.TenNganh || '',
            item.SoThiSinhBiAnhHuong || 0,
            item.SoNVLoi || 0
        ]);

        if (typeof XLSX !== 'undefined') {
            const ws = XLSX.utils.aoa_to_sheet([headers, ...rows]);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'NganhBiAnhHuong');
            XLSX.writeFile(wb, 'DanhSach_Nganh_BiAnhHuong.xlsx');
        }
    });

    // ---- Export Excel trong Modal Nóm lỗi chung ----
    $('#btnExportGroupCsv').click(function () {
        if (!currentGroupData || currentGroupData.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Số ĐDCN (CCCD)', 'Thứ Tự NV', 'Mã Ngành', 'Tên Ngành Đăng Ký'];
        const rows = currentGroupData.map((item, idx) => [
            idx + 1,
            item.Cccd || '',
            item.ThuTuNV || '',
            item.MaXetTuyen || '',
            item.TenNganh || ''
        ]);

        const fileName = currentGroupTitle || 'DanhSach_ChiTiet';

        if (typeof XLSX !== 'undefined') {
            const sheetData = [headers, ...rows];
            const ws = XLSX.utils.aoa_to_sheet(sheetData);
            const wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'DanhSach');
            XLSX.writeFile(wb, fileName + '.xlsx');
        } else {
            // Fallback CSV with UTF-8 BOM
            const csvRows = rows.map(r => r.map(c => `"${String(c).replace(/"/g, '""')}"`).join(','));
            const csvContent = '\uFEFF' + [headers.join(','), ...csvRows].join('\n');
            const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = fileName + '.csv';
            a.click();
            URL.revokeObjectURL(url);
        }
    });
});
