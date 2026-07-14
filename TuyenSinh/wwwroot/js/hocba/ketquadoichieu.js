$(document).ready(function () {
    let lastResultData = [];

    // Initialize DataTables calling AJAX API matching Preview mode
    const table = $('#doiChieuTable').DataTable({
        ajax: {
            url: `/admin/hoc-ba/lay-ket-qua-doi-chieu?hocBaFileId=${hocBaFileId}&nguyenVongFileId=${nguyenVongFileId}`,
            dataSrc: function (json) {
                if (!json.success) {
                    Swal.fire('Lỗi', json.message || 'Không thể tải dữ liệu đối chiếu.', 'error');
                    // Clear spinners
                    $('#statTongNV').text('0');
                    $('#statTongLoi').text('0');
                    $('#statNganhKhongTim').text('0');
                    return [];
                }

                // Save data for client-side CSV export
                lastResultData = json.data || [];

                // Update Stats Cards
                $('#statTongNV').text(json.tongNguyenVong || 0);
                $('#statTongLoi').text(lastResultData.length);
                $('#statNganhKhongTim').text(json.tongLoiKhongTimThayNganh || 0);

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
        scrollX: true,
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
});
