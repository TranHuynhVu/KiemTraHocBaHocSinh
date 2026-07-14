$(document).ready(function () {
    setupDropZone('dropZoneHocBa', 'fileHocBa', 'fileHocBaInfo', 'fileHocBaName');
    setupDropZone('dropZoneNguyenVong', 'fileNguyenVong', 'fileNguyenVongInfo', 'fileNguyenVongName');

    // Handle submission loading modal
    $('#doiChieuForm').on('submit', function () {
        Swal.fire({
            title: 'Đang xử lý đối chiếu...',
            text: 'Hệ thống đang đối chiếu dữ liệu học bạ & nguyện vọng. Vui lòng đợi trong giây lát...',
            allowOutsideClick: false,
            allowEscapeKey: false,
            showConfirmButton: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });
    });

    // Alert error message if passed from server-side
    if (typeof errorMessage !== 'undefined' && errorMessage) {
        Swal.fire('Lỗi', errorMessage, 'error');
    }
});

function setupDropZone(zoneId, inputId, infoId, nameId) {
    const $zone = $('#' + zoneId);
    const $input = $('#' + inputId);

    $zone.on('click', function () {
        $input.trigger('click');
    });

    $zone.on('dragover', function (e) {
        e.preventDefault();
        $zone.addClass('dragover');
    });

    $zone.on('dragleave', function () {
        $zone.removeClass('dragover');
    });

    $zone.on('drop', function (e) {
        e.preventDefault();
        $zone.removeClass('dragover');
        const files = e.originalEvent.dataTransfer.files;
        if (files.length > 0) {
            setFile($input, infoId, nameId, files[0]);
        }
    });

    $input.on('change', function () {
        const files = this.files;
        if (files.length > 0) {
            setFile($input, infoId, nameId, files[0]);
        }
    });
}

function setFile($input, infoId, nameId, file) {
    const ext = file.name.split('.').pop().toLowerCase();
    if (ext !== 'xlsx') {
        Swal.fire({
            icon: 'error',
            title: 'Định dạng tệp không hợp lệ',
            text: 'Hệ thống chỉ hỗ trợ tệp tin Excel định dạng .xlsx.',
            confirmButtonColor: '#ff3b30'
        });
        $input.val('');
        $('#' + nameId).text('');
        $('#' + infoId).addClass('d-none');
        checkBothFilesSelected();
        return;
    }

    const dt = new DataTransfer();
    dt.items.add(file);
    $input[0].files = dt.files;

    $('#' + nameId).text(file.name);
    $('#' + infoId).removeClass('d-none');

    checkBothFilesSelected();
}

function checkBothFilesSelected() {
    const f1 = $('#fileHocBa')[0].files.length > 0;
    const f2 = $('#fileNguyenVong')[0].files.length > 0;
    $('#btnDoiChieu').prop('disabled', !(f1 && f2));
}
