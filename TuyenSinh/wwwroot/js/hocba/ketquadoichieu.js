$(document).ready(function () {
    let lastResultData = [];

    // Initialize DataTables calling AJAX API matching Preview mode
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

                // Save data for client-side export
                lastResultData = json.data || [];
                $('#badgeTongDongChiTietLoi').text((lastResultData.length || 0).toLocaleString() + ' dòng');

                // 1. Render Table 1: Thống kê tổng hợp
                $('#summaryLoading').addClass('d-none');
                $('#summaryContent').removeClass('d-none');

                const th = json.thongKeTongHop || json.ThongKeTongHop || {};
                $('#statTongDongNV').text((th.tongDongNguyenVong || th.TongDongNguyenVong || 0).toLocaleString());
                $('#statTongThiSinh').text((th.tongThiSinhDuyNhat || th.TongThiSinhDuyNhat || 0).toLocaleString());
                $('#statNVCoToHopDu').text((th.nguyenVongCoToHopDu || th.NguyenVongCoToHopDu || 0).toLocaleString());
                $('#statNVThieuMoiToHop').text((th.nguyenVongThieuMoiToHop || th.NguyenVongThieuMoiToHop || 0).toLocaleString());
                $('#statNVKhongHocBa').text((th.nguyenVongKhongHocBa || th.NguyenVongKhongHocBa || 0).toLocaleString());
                $('#statNVKhongDiemCN').text((th.nguyenVongKhongDiemCN || th.NguyenVongKhongDiemCN || 0).toLocaleString());
                $('#statNguyenVongBoQua').text((th.nguyenVongBoQua || th.NguyenVongBoQua || 0).toLocaleString());

                // Store category lists globally for Modal & export
                window.groupLists = {
                    ThieuMoiToHop: th.danhSachThieuMoiToHop || th.DanhSachThieuMoiToHop || [],
                    KhongHocBa:    th.danhSachKhongHocBa    || th.DanhSachKhongHocBa    || [],
                    KhongDiemCN:   th.danhSachKhongDiemCN   || th.DanhSachKhongDiemCN   || [],
                    BoQua:         th.danhSachBoQua          || th.DanhSachBoQua          || []
                };

                // 2. Render Table 2: Thống kê theo ngành
                $('#tableNganhLoading').addClass('d-none');
                $('#tableNganhContent').removeClass('d-none');

                const dsNganh = json.thongKeTheoNganh || json.ThongKeTheoNganh || [];

                // Store per-ngành detail lists for modal (keyed by maXetTuyen)
                window.nganhGroupLists = {};
                dsNganh.forEach(function (item) {
                    const ma = item.maXetTuyen || item.MaXetTuyen || '';
                    window.nganhGroupLists[ma] = {
                        ThieuMoiToHop: item.danhSachThieuMoiToHop || item.DanhSachThieuMoiToHop || [],
                        KhongHocBa:    item.danhSachKhongHocBa    || item.DanhSachKhongHocBa    || [],
                        KhongDiemCN:   item.danhSachKhongDiemCN   || item.DanhSachKhongDiemCN   || []
                    };
                });

                if (dsNganh.length > 0) {
                    const rowsHtml = dsNganh.map(function (item) {
                        const ma = item.maXetTuyen || item.MaXetTuyen || '';
                        const ten = item.tenNganh || item.TenNganh || '';
                        const tongNV = (item.tongNV || item.TongNV || 0).toLocaleString();
                        const soThiSinh = (item.soThiSinh || item.SoThiSinh || 0).toLocaleString();
                        const nvCoToHopDu = (item.nvCoToHopDu || item.NVCoToHopDu || 0).toLocaleString();

                        const rawThieu  = item.nvThieuMoiToHop || item.NVThieuMoiToHop || 0;
                        const rawDiemCN = item.nvKhongDiemCN   || item.NVKhongDiemCN   || 0;
                        const rawHocBa  = item.nvKhongHocBa    || item.NVKhongHocBa    || 0;

                        const valTyLe = (item.tyLeThieu !== undefined ? item.tyLeThieu : item.TyLeThieu) || 0;
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

    // ============================================================
    // Helper: AJAX Excel download with SweetAlert loading
    // ============================================================
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

    // ============================================================
    // Modal Chi tiết nhóm lỗi
    // ============================================================
    let currentGroupType  = '';
    let currentGroupData  = [];
    let currentGroupTitle = '';

    const LABEL_MAP = {
        ThieuMoiToHop: 'NV thiếu điểm ở mọi tổ hợp',
        KhongHocBa:    'NV không có học bạ',
        KhongDiemCN:   'NV trống điểm CN',
        BoQua:         'NV bị bỏ qua (hệ số = 0)'
    };

    function openGroupModal(type, title, data) {
        currentGroupType  = type;
        currentGroupTitle = title;
        currentGroupData  = data || [];

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
                    <td class="fw-semibold text-dark">${item.cccd || item.Cccd || ''}</td>
                    <td class="text-center fw-bold text-primary">${item.thuTuNV || item.ThuTuNV || ''}</td>
                    <td><span class="badge bg-light text-dark border">${item.maXetTuyen || item.MaXetTuyen || ''}</span></td>
                    <td class="text-truncate" style="max-width: 250px;" title="${item.tenNganh || item.TenNganh || ''}">${item.tenNganh || item.TenNganh || ''}</td>
                </tr>
            `).join('');
            $('#tbodyChiTietNhomLoi').html(html);
        }

        const modal = new bootstrap.Modal(document.getElementById('modalChiTietNhomLoi'));
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
    $('#statNguyenVongBoQua').click(function () {
        openGroupModal('BoQua', 'Danh sách nguyện vọng bị bỏ qua',
            (window.groupLists || {}).BoQua || []);
    });

    // ---- Bảng Thống kê theo ngành – delegated click handler ----
    $(document).on('click', '.nganh-group-link', function () {
        const ma   = $(this).data('ma');
        const ten  = $(this).data('ten');
        const loai = $(this).data('loai');

        const nganhData = (window.nganhGroupLists || {})[ma] || {};
        const list = nganhData[loai] || [];

        const loaiLabel = LABEL_MAP[loai] || loai;
        const title = `[${ma}] ${ten} – ${loaiLabel}`;

        openGroupModal(loai, title, list);
    });

    // ---- Export Excel trong Modal ----
    $('#btnExportGroupCsv').click(function () {
        if (!currentGroupData || currentGroupData.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Số ĐDCN (CCCD)', 'Thứ Tự NV', 'Mã Ngành', 'Tên Ngành Đăng Ký'];
        const rows = currentGroupData.map((item, idx) => [
            idx + 1,
            item.cccd       || item.Cccd       || '',
            item.thuTuNV    || item.ThuTuNV    || '',
            item.maXetTuyen || item.MaXetTuyen || '',
            item.tenNganh   || item.TenNganh   || ''
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
