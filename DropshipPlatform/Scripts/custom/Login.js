$(document).ready(function () {
    LoginValidation();
});

function LoginUser() {
    var Isvalid = $("#frmLogin").valid();
    if (Isvalid === true) {
        console.log('valid');
    }
}

function LoginValidation() {
    $("#frmLogin").validate({
        rules:
        {
            UserName: {
                required: true
            },
            Email: {
                required: true,
                email: true
            },
            Password: {
                required: true
            }
        },
    });
}