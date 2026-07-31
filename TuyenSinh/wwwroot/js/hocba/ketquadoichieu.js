$(document).ready(function () {
    let lastResultData = [];

    // Initialize DataTables calling AJAX API matching Preview mode
    const table = $('#doiChieuTable').DataTable({
        ajax: {
            url: `/admin/hoc-ba/lay-ket-qua-doi-chieu?hocBaFileId=${hocBaFileId}&nguyenVongFileId=${nguyenVongFileId}`,
            dataSrc: function (json) {
                if (!json.success) {
                    Swal.fire('Lỗi', json.message || 'Không thể tải dữ liệu đối chiếu.', 'error');
                    // Hide loaders and show empty content
                    $('#summaryLoading, #tableNganhLoading').addClass('d-none');
                    $('#summaryContent, #tableNganhContent').removeClass('d-none');
                    return [];
                }

                // Save data for client-side CSV export
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

                // Store category lists globally for Modal & CSV export
                window.groupLists = {
                    ThieuMoiToHop: th.danhSachThieuMoiToHop || th.DanhSachThieuMoiToHop || [],
                    KhongHocBa: th.danhSachKhongHocBa || th.DanhSachKhongHocBa || [],
                    KhongDiemCN: th.danhSachKhongDiemCN || th.DanhSachKhongDiemCN || [],
                    BoQua: th.danhSachBoQua || th.DanhSachBoQua || []
                };

                // 2. Render Table 2: Thống kê theo ngành
                $('#tableNganhLoading').addClass('d-none');
                $('#tableNganhContent').removeClass('d-none');

                const dsNganh = json.thongKeTheoNganh || json.ThongKeTheoNganh || [];
                if (dsNganh.length > 0) {
                    const rowsHtml = dsNganh.map(item => {
                        const ma = item.maXetTuyen || item.MaXetTuyen || '';
                        const ten = item.tenNganh || item.TenNganh || '';
                        const tongNV = (item.tongNV || item.TongNV || 0).toLocaleString();
                        const soThiSinh = (item.soThiSinh || item.SoThiSinh || 0).toLocaleString();
                        const nvCoToHopDu = (item.nvCoToHopDu || item.NVCoToHopDu || 0).toLocaleString();
                        const nvThieuMoiToHop = (item.nvThieuMoiToHop || item.NVThieuMoiToHop || 0).toLocaleString();
                        const nvKhongDiemCN = (item.nvKhongDiemCN || item.NVKhongDiemCN || 0).toLocaleString();
                        const nvKhongHocBa = (item.nvKhongHocBa || item.NVKhongHocBa || 0).toLocaleString();
                        const valTyLe = (item.tyLeThieu !== undefined ? item.tyLeThieu : item.TyLeThieu) || 0;
                        const tyLeThieu = valTyLe.toFixed(2) + '%';
                        const badgeClass = valTyLe > 0 ? 'text-danger fw-bold' : 'text-success';

                        return `<tr>
                            <td><code>${ma}</code></td>
                            <td>${ten}</td>
                            <td class="text-end fw-semibold">${tongNV}</td>
                            <td class="text-end">${soThiSinh}</td>
                            <td class="text-end text-success">${nvCoToHopDu}</td>
                            <td class="text-end text-danger">${nvThieuMoiToHop}</td>
                            <td class="text-end text-warning">${nvKhongDiemCN}</td>
                            <td class="text-end text-secondary">${nvKhongHocBa}</td>
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
            { data: 'Stt' },
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

    // Helper function for AJAX Excel download with SweetAlert loading modal using jQuery AJAX
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
            xhrFields: {
                responseType: 'blob'
            },
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

    // Export Excel button click handler
    $('#btnExportExcel').click(function () {
        const url = `/admin/hoc-ba/xuat-excel-ket-qua-doi-chieu?hocBaFileId=${hocBaFileId}&nguyenVongFileId=${nguyenVongFileId}`;
        downloadExcelWithLoading(url, 'DoiChieu_HocBa_NguyenVong.xlsx');
    });

    // Client-side CSV export scraping loaded rows from DataTable
    $('#btnExportCsv').click(function () {
        if (!lastResultData || lastResultData.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Số ĐDCN', 'Họ và Tên', 'TT NV', 'Mã Ngành', 'Tên Ngành', 'Mã Tổ Hợp', 'Năm Học', 'Môn Thiếu'];
        const rows = lastResultData.map(item => [
            item.Stt,
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

    // Modal Chi tiết nhóm lỗi (Top 50) & Xuất CSV nhóm
    let currentGroupType = '';
    let currentGroupData = [];
    let currentGroupTitle = '';

    function openGroupModal(type, title) {
        currentGroupType = type;
        currentGroupTitle = title;
        currentGroupData = (window.groupLists && window.groupLists[type]) || [];

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

    // Click handlers for the 4 error/skipped statistics in Table 1
    $('#statNVThieuMoiToHop').click(function () {
        openGroupModal('ThieuMoiToHop', 'DanhSachNguyenVongThieuDiemOMoiToHop');
    });

    $('#statNVKhongHocBa').click(function () {
        openGroupModal('KhongHocBa', 'DanhSachNguyenVongKhongCoHocBa');
    });

    $('#statNVKhongDiemCN').click(function () {
        openGroupModal('KhongDiemCN', 'DanhSachNguyenVongTrongDiemCN');
    });

    $('#statNguyenVongBoQua').click(function () {
        openGroupModal('BoQua', 'DanhSachNguyenVongBoQua');
    });

    // Export CSV button inside Modal
    $('#btnExportGroupCsv').click(function () {
        if (!currentGroupData || currentGroupData.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Số ĐDCN (CCCD)', 'Thứ Tự NV', 'Mã Ngành', 'Tên Ngành Đăng Ký'];
        const rows = currentGroupData.map((item, idx) => [
            idx + 1,
            `"${item.cccd || item.Cccd || ''}"`,
            item.thuTuNV || item.ThuTuNV || '',
            `"${item.maXetTuyen || item.MaXetTuyen || ''}"`,
            `"${(item.tenNganh || item.TenNganh || '').replace(/"/g, '""')}"`
        ]);

        const csvContent = '\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        const fileName = (currentGroupTitle || 'DanhSach_ChiTiet').replace(/[^a-zA-Z0-9_ -]/g, '') + '.csv';
        a.download = fileName;
        a.click();
        URL.revokeObjectURL(url);
    });
});
