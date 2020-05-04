var tSuccess = 'success';
var tError = 'error';
var tWarning = 'warning';
var tInfo = 'info';

function showToast(Message, Type) {
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "progressBar": true,
        "preventDuplicates": false,
        "positionClass": "toast-top-center",
        "onclick": null,
        "showDuration": "400",
        "hideDuration": "2000",
        "timeOut": "0",
        "extendedTimeOut": "0",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    }
    toastr[Type](Message);
}
function SuccessMessage(Message) {
    showToast(Message, tSuccess);
}
function ErrorMessage(Message) {
    showToast(Message, tError);
}