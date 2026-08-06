$(document).ready(function () {
    const dropZone = $('#dropZoneDiemSan');
    const fileInput = $('#fileDiemSan');
    const fileInfo = $('#fileDiemSanInfo');
    const fileNameDisplay = $('#fileDiemSanName');
    const btnSubmit = $('#btnThucHienKiemTra');

    // Initialize Select2 for quick searching
    if ($.fn.select2) {
        $('#selectMaNganh').select2({
            theme: 'bootstrap-5',
            placeholder: '-- Tất cả các ngành trong tệp --',
            allowClear: true,
            width: '100%'
        });
    }

    dropZone.on('click', function () {
        fileInput.click();
    });

    dropZone.on('dragover', function (e) {
        e.preventDefault();
        e.stopPropagation();
        dropZone.css('border-color', '#007aff').css('background-color', '#f0f7ff');
    });

    dropZone.on('dragleave', function (e) {
        e.preventDefault();
        e.stopPropagation();
        dropZone.css('border-color', '#d1d1d6').css('background-color', '#fafafa');
    });

    dropZone.on('drop', function (e) {
        e.preventDefault();
        e.stopPropagation();
        dropZone.css('border-color', '#d1d1d6').css('background-color', '#fafafa');

        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            fileInput[0].files = files;
            updateFileInfo(files[0]);
        }
    });

    fileInput.on('change', function () {
        if (this.files.length > 0) {
            updateFileInfo(this.files[0]);
        }
    });

    function updateFileInfo(file) {
        if (!file.name.toLowerCase().endsWith('.xlsx')) {
            Swal.fire('Lỗi định dạng', 'Chỉ chấp nhận tệp tin Excel .xlsx', 'warning');
            fileInput.val('');
            fileInfo.addClass('d-none');
            btnSubmit.prop('disabled', true);
            return;
        }

        fileNameDisplay.text(file.name);
        fileInfo.removeClass('d-none');
        btnSubmit.prop('disabled', false);
    }
});
