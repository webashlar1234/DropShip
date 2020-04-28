$(document).ready(function () {
    RegistrationValidation();
    $(document).on('change', '#LstCountry', function () {
        $('#TxtPhone').val($('#LstCountry option:selected').data('code'));
    })
});

function RegisterUser() {
    var Isvalid = $("#frmRegistration").valid();
    if (Isvalid === true) {
        console.log('valid');
    }
}

function RegistrationValidation() {
    $("#frmRegistration").validate({
        rules:
        {
            UserName: {
                required: true,
                minlength: 4
            },
            Email: {
                required: true,
                email: true
            },
            ConfirmPassword: {
                equalTo: "#TxtPassword",
                required: true
            },
            Password: {
                minlength: 6,
                required: true
            },
            Country: { required: true }
        },
        messages:
        {
            ConfirmPassword: { equalTo: "Password and Confirm password should match" },
            Password: { minlength: "minimum 6 chararctes Required" },
            UserName: { minlength: "minimum 4 chararctes Required" }
        }
    });
}