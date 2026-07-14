let rawTable, missingGradesTable, missingScoresTable;

$(document).ready(function () {
    rawTable = $('#rawPreviewTable').DataTable({
        ajax: {
            url: `/admin/hoc-ba/lay-du-lieu-xem-truoc?excelId=${excelId}`,
            dataSrc: 'data'
        },
        processing: true,
        deferRender: true,
        columns: [
            { data: 'STT' },
            { data: 'SoDDCN', render: function(data) { return `<span class="cccd-link">${data || ''}</span>`; } },
            { data: 'HoVaTen', render: function(data) { return `<strong class="text-dark">${data || ''}</strong>`; } },
            { data: 'NgaySinh', render: function(data) {
                if (!data) return '';
                let d = new Date(data);
                return d.toLocaleDateString('vi-VN');
            }},
            { data: 'GioiTinh' },
            { data: 'Lop', render: function(data) { return data ? `<span class="badge bg-secondary bg-opacity-10 text-dark" style="border-radius:3px; font-size:0.75rem;">Lớp ${data}</span>` : ''; } },
            { data: 'ChuongTrinhHoc' },
            { data: 'DiemTrungBinhNam', render: function(data) { return data !== null ? data.toFixed(2) : ''; } },
            { data: 'DiemTongKetHKI', render: function(data) { return data !== null ? data.toFixed(2) : ''; } },
            { data: 'DiemTongKetHKII', render: function(data) { return data !== null ? data.toFixed(2) : ''; } },
            { data: 'DiemTongKetCN', render: function(data) { return data !== null ? data.toFixed(2) : ''; } },
            { data: 'HocLucHKI' }, { data: 'HocLucHKII' }, { data: 'HocLucCN' },
            { data: 'HanhKiemHKI' }, { data: 'HanhKiemHKII' }, { data: 'HanhKiemCN' },
            { data: 'KetQuaHocTapHKI' }, { data: 'KetQuaHocTapHKII' }, { data: 'KetQuaHocTapCN' },
            { data: 'KetQuaRenLuyenHKI' }, { data: 'KetQuaRenLuyenHKII' }, { data: 'KetQuaRenLuyenCN' },
            { data: 'ToanHKI' }, { data: 'ToanHKII' }, { data: 'ToanCN' },
            { data: 'VanHKI' }, { data: 'VanHKII' }, { data: 'VanCN' },
            { data: 'VatLyHKI' }, { data: 'VatLyHKII' }, { data: 'VatLyCN' },
            { data: 'HoaHocHKI' }, { data: 'HoaHocHKII' }, { data: 'HoaHocCN' },
            { data: 'SinhHocHKI' }, { data: 'SinhHocHKII' }, { data: 'SinhHocCN' },
            { data: 'LichSuHKI' }, { data: 'LichSuHKII' }, { data: 'LichSuCN' },
            { data: 'DiaLyHKI' }, { data: 'DiaLyHKII' }, { data: 'DiaLyCN' },
            { data: 'GDCDHKI' }, { data: 'GDCDHKII' }, { data: 'GDCDCN' },
            { data: 'KTPLHKI' }, { data: 'KTPLHKII' }, { data: 'KTPLCN' },
            { data: 'TinHocHKI' }, { data: 'TinHocHKII' }, { data: 'TinHocCN' },
            { data: 'CNCNHKI' }, { data: 'CNCNHKII' }, { data: 'CNCNCN' },
            { data: 'CNNNHKI' }, { data: 'CNNNHKII' }, { data: 'CNNNCN' },
            { data: 'NgoaiNguHKI' }, { data: 'NgoaiNguHKII' }, { data: 'NgoaiNguCN' }, { data: 'MonNgoaiNgu' },
            { data: 'TuChonSongNguHKI' }, { data: 'TuChonSongNguHKII' }, { data: 'TuChonSongNguCN' },
            { data: 'QPANHKI' }, { data: 'QPANHKII' }, { data: 'QPANCN' },
            { data: 'TiengDanTocHKI' }, { data: 'TiengDanTocHKII' }, { data: 'TiengDanTocCN' },
            { data: 'NgoaiNgu2HKI' }, { data: 'NgoaiNgu2HKII' }, { data: 'NgoaiNgu2CN' }, { data: 'MonNgoaiNgu2' },
            { data: 'ToanPhapHKI' }, { data: 'ToanPhapHKII' }, { data: 'ToanPhapCN' }
        ],
        columnDefs: [
            { defaultContent: "", targets: "_all" }
        ],
        scrollX: true,
        scrollY: "380px",
        scrollCollapse: true,
        paging: true,
        pageLength: 10,
        lengthMenu: [5, 10, 25, 50],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });

    // Handle run checks
    $('#btnRunCheck').click(function () {
        // Show loading SweetAlert
        Swal.fire({
            title: 'Đang kiểm tra học bạ...',
            text: 'Hệ thống đang thực hiện nhóm thí sinh theo CCCD và tính toán tính đầy đủ học lực cùng tổ hợp môn.',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: '/admin/hoc-ba/thuc-hien-kiem-tra',
            type: 'POST',
            data: { excelId: excelId },
            success: function (res) {
                Swal.close();
                if (res.success) {
                    Swal.fire({
                        icon: 'success',
                        title: 'Thành công',
                        text: 'Đã hoàn tất đối chiếu logic dữ liệu học bạ thí sinh!',
                        confirmButtonColor: '#007aff'
                    });

                    $('#resultsWrapper').removeClass('d-none');
                    renderResults(res);

                    // Smooth scroll down to results
                    $('html, body').animate({
                        scrollTop: $("#resultsWrapper").offset().top - 20
                    }, 500);
                } else {
                    Swal.fire({
                        icon: 'error',
                        title: 'Thất bại',
                        text: res.message,
                        confirmButtonColor: '#ff3b30'
                    });
                }
            },
            error: function (xhr, status, error) {
                Swal.close();
                Swal.fire({
                    icon: 'error',
                    title: 'Lỗi',
                    text: 'Gặp lỗi trong quá trình chạy kiểm tra học sinh: ' + error,
                    confirmButtonColor: '#ff3b30'
                });
            }
        });
    });

    // Handle Export Excel button click with AJAX loading dialog
    $('#btnExportMissingScores').click(function () {
        const url = `/admin/hoc-ba/xuat-excel-thieu-diem?excelId=${excelId}`;
        downloadExcelWithLoading(url, 'ThiSinh_ThieuDiem_ToHop.xlsx');
    });
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

    fetch(url)
        .then(response => {
            if (!response.ok) throw new Error('Yêu cầu xuất tệp thất bại.');
            return response.blob();
        })
        .then(blob => {
            Swal.close();
            const downloadUrl = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = downloadUrl;
            a.download = fileName;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(downloadUrl);
        })
        .catch(error => {
            Swal.close();
            Swal.fire('Lỗi', error.message || 'Không thể xuất tệp Excel.', 'error');
        });
}

function renderResults(res) {
    // Update badges
    $('#missingGradesBadge').text(res.danhSachThieuNamHoc.length + ' bản ghi lỗi');
    $('#missingScoresBadge').text(res.danhSachThieuDiem.length + ' bản ghi lỗi');

    // Show/hide Export Excel button based on error list size
    if (res.danhSachThieuDiem && res.danhSachThieuDiem.length > 0) {
        $('#btnExportMissingScores').removeClass('d-none');
    } else {
        $('#btnExportMissingScores').addClass('d-none');
    }

    // Destroy existing DataTables to refresh
    if ($.fn.DataTable.isDataTable('#missingGradesTable')) {
        $('#missingGradesTable').DataTable().destroy();
    }
    if ($.fn.DataTable.isDataTable('#missingScoresTable')) {
        $('#missingScoresTable').DataTable().destroy();
    }

    // Draw Missing Grades Table
    missingGradesTable = $('#missingGradesTable').DataTable({
        data: res.danhSachThieuNamHoc,
        columns: [
            { data: 'Stt' },
            { data: 'Cccd', render: function(data) { return `<span class="cccd-link">${data || ''}</span>`; } },
            { data: 'HoVaTen', render: function(data) { return `<strong class="text-dark">${data || ''}</strong>`; } },
            { data: 'NamHienCo', render: function(data) {
                return data.split(', ').map(yr => `<span class="badge-gray me-1">${yr}</span>`).join('');
            }},
            { data: 'NamThieu', render: function(data) {
                return data.split(', ').map(yr => `<span class="badge-red me-1">${yr}</span>`).join('');
            }}
        ],
        scrollX: true,
        scrollY: "300px",
        scrollCollapse: true,
        paging: true,
        pageLength: 5,
        lengthMenu: [5, 10, 20, 50],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });

    // Draw Missing Scores Table
    missingScoresTable = $('#missingScoresTable').DataTable({
        data: res.danhSachThieuDiem,
        columns: [
            { data: 'Stt' },
            { data: 'Cccd', render: function(data) { return `<span class="cccd-link">${data || ''}</span>`; } },
            { data: 'HoVaTen', render: function(data) { return `<strong class="text-dark">${data || ''}</strong>`; } },
            { data: 'NamLoi', render: function(data) { return `<span>${data}</span>`; } },
            { data: 'ToHop', render: function(data) { return `<span>${data}</span>`; } },
            { data: 'MonThieu', render: function(data) { return `<span class="text-error-subjects">${data}</span>`; } }
        ],
        scrollX: true,
        scrollY: "300px",
        scrollCollapse: true,
        paging: true,
        pageLength: 5,
        lengthMenu: [5, 10, 20, 50],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.13.6/i18n/vi.json"
        }
    });
}
