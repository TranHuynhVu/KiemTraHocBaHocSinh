$(document).ready(function () {
    let lastResultData = [];

    const table = $('#diemSanTable').DataTable({
        ajax: {
            url: `/admin/hoc-ba/lay-ket-qua-kiem-tra-diem-san?maNganh=${encodeURIComponent(maNganh)}&fileId=${encodeURIComponent(fileId)}`,
            dataSrc: function (json) {
                if (!json.success) {
                    Swal.fire('Lỗi', json.message || 'Không thể tải dữ liệu kiểm tra điểm sàn.', 'error');
                    $('#statTongSo').text('0');
                    $('#statTongDat').text('0');
                    $('#statTongLoi').text('0');
                    return [];
                }

                lastResultData = json.data || [];
                $('#statTongSo').text(json.tongSoThiSinh || 0);
                $('#statTongDat').text(json.soThiSinhDat || 0);
                $('#statTongLoi').text(json.soThiSinhKhongDat || 0);

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
            { data: 'HoTen', render: function (data) { return `<strong class="text-dark">${data || ''}</strong>`; } },
            { data: 'CCCD', render: function (data) { return `<span class="cccd-link">${data || ''}</span>`; } },
            { data: 'MaNganh', render: function (data) { return `<span class="badge bg-primary-subtle text-primary">${data || ''}</span>`; } },
            { data: 'ToHop', render: function (data) { return `<span class="badge bg-secondary">${data || ''}</span>`; } },
            { data: 'DiemXetTuyen', render: function (data) { return `<span class="fw-bold text-dark">${data !== null && data !== undefined ? data : ''}</span>`; } },
            { data: 'DiemSan', render: function (data) { return `<span class="text-secondary">${data !== null && data !== undefined ? data : ''}</span>`; } },
            { data: 'DiemSanToan', render: function (data) { return `<span class="text-secondary">${data !== null && data !== undefined ? data : ''}</span>`; } },
            { data: 'GhiChu', render: function (data) { return `<span class="text-danger small">${data || ''}</span>`; } }
        ],
        scrollX: true,
        scrollY: "450px",
        scrollCollapse: true,
        paging: true,
        pageLength: 25,
        lengthMenu: [10, 25, 50, 100],
        language: {
            url: 'https://cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json'
        }
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

    $('#btnExportExcel').click(function () {
        const url = `/admin/hoc-ba/xuat-excel-kiem-tra-diem-san?maNganh=${encodeURIComponent(maNganh)}&fileId=${encodeURIComponent(fileId)}`;
        downloadExcelWithLoading(url, 'KetQua_KiemTra_DiemSan.xlsx');
    });

    $('#btnExportCsv').click(function () {
        if (!lastResultData || lastResultData.length === 0) {
            Swal.fire('Chưa có dữ liệu', 'Không có dữ liệu để xuất.', 'warning');
            return;
        }

        const headers = ['STT', 'Họ và Tên', 'Số ĐDCN', 'Mã Ngành', 'Tổ Hợp', 'Điểm Xét Tuyển', 'Điểm Sàn Ngành', 'Điểm Sàn Toán', 'Ghi Chú Lỗi'];
        const rows = lastResultData.map((item, index) => [
            index + 1,
            `"${item.HoTen || ''}"`,
            `"${item.CCCD || ''}"`,
            `"${item.MaNganh || ''}"`,
            `"${item.ToHop || ''}"`,
            item.DiemXetTuyen,
            item.DiemSan,
            item.DiemSanToan,
            `"${(item.GhiChu || '').replace(/"/g, '""')}"`
        ]);

        const csvContent = '\uFEFF' + [headers.join(','), ...rows.map(r => r.join(','))].join('\n');
        const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'KetQua_KiemTra_DiemSan.csv';
        a.click();
        URL.revokeObjectURL(url);
    });
});
