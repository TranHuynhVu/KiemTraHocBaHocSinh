$(document).ready(function () {
    // Drag and drop events
    let dropZone = $('#dropZone');
    let fileInput = $('#excelFile');

    dropZone.on('click', function(e) {
        // Prevent trigger click loop if clicking the hidden file input itself
        if (e.target !== fileInput[0] && !$(e.target).closest('#btnSelectFile').length === 0) {
            fileInput.trigger('click');
        }
    });

    // Make select button trigger file input click explicitly
    $('#btnSelectFile').on('click', function(e) {
        e.stopPropagation(); // Prevent bubbling up to dropZone click event
        fileInput.trigger('click');
    });

    dropZone.on('dragover', function(e) {
        e.preventDefault();
        dropZone.addClass('dragover');
    });

    dropZone.on('dragleave', function(e) {
        e.preventDefault();
        dropZone.removeClass('dragover');
    });

    dropZone.on('drop', function(e) {
        e.preventDefault();
        dropZone.removeClass('dragover');
        let files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            fileInput[0].files = files;
            fileInput.trigger('change');
        }
    });

    // Handle file input change event
    fileInput.on('change', function () {
        let files = this.files;
        if (files && files.length > 0) {
            let file = files[0];
            let ext = file.name.split('.').pop().toLowerCase();
            if (ext !== 'xlsx') {
                Swal.fire({
                    icon: 'error',
                    title: 'Định dạng tệp không hợp lệ',
                    text: 'Hệ thống chỉ hỗ trợ tệp tin Excel định dạng .xlsx.',
                    confirmButtonColor: '#ff3b30'
                });
                fileInput.val('');
                $('#fileInfoWrapper').addClass('d-none');
                $('#selectFileBtnWrapper').removeClass('d-none');
                $('#btnUpload').prop('disabled', true);
                return;
            }
            $('#selectedFileName').text(file.name);
            $('#fileInfoWrapper').removeClass('d-none');
            $('#selectFileBtnWrapper').addClass('d-none');
            $('#btnUpload').prop('disabled', false);
        } else {
            $('#fileInfoWrapper').addClass('d-none');
            $('#selectFileBtnWrapper').removeClass('d-none');
            $('#btnUpload').prop('disabled', true);
        }
    });

    // Click upload button
    $('#btnUpload').click(function () {
        if (fileInput[0].files.length === 0) {
            Swal.fire({
                icon: 'warning',
                title: 'Thiếu tệp tin',
                text: 'Vui lòng chọn tệp Excel trước khi tiếp tục.',
                confirmButtonColor: '#007aff'
            });
            return;
        }

        let formData = new FormData($('#uploadForm')[0]);

        // Show SweetAlert Loading Modal
        Swal.fire({
            title: 'Đang tải tệp tin...',
            text: 'Hệ thống đang xử lý và nạp cấu trúc tệp Excel, vui lòng đợi trong giây lát.',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        $.ajax({
            url: '/admin/hoc-ba/tai-len',
            type: 'POST',
            data: formData,
            contentType: false,
            processData: false,
            success: function (res) {
                Swal.close();
                if (res.success) {
                    // Redirect to Preview page with ExcelId
                    window.location.href = `/admin/hoc-ba/xem-truoc?excelId=${res.excelId}`;
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
                    title: 'Lỗi hệ thống',
                    text: 'Có lỗi xảy ra khi gửi dữ liệu lên server: ' + error,
                    confirmButtonColor: '#ff3b30'
                });
            }
        });
    });
});
