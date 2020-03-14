var operationalUserDT = null;
var operationalUser = {
    init: function () {
        operationalUser.initCategoryTable();
    },
    change: function () {

    },

    initCategoryTable: function () {
        operationalUserDT = $('#operationalUserDT').DataTable({
            ajax: {
                "url": "/Registration/getOperationalUsers",
                "type": "POST",
                "datatype": "json",
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: true,
            language: {
                loadingRecords: '&nbsp;',
                processing: '<div class="spinner"></div>'
            },
            columnDefs: [
                {
                    targets: 0,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 1,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 2,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, row) {
                        return data;
                    }
                },
                {
                    targets: 3,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 4,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, row) {
                        return '<a class="" href="#" onclick=deleteUser("' + row.UserID + '")><i class="fa fa-trash-o fa-fw fa-2x" style="color:red"></i></a>';
                    }
                },
            ],
            columns: [
                { "sTitle": 'Name', "mData": 'Username', sDefaultContent: "", className: "Name" },
                { "sTitle": 'Email', "mData": 'Email', sDefaultContent: "", className: "EmailID" },
                { "sTitle": 'Cell Phone', "mData": 'Phone', sDefaultContent: "", className: "Phone" },
                { "sTitle": 'Country', "mData": 'Country', sDefaultContent: "", className: "Country" },
                { "sTitle": 'Actions', "mData": 'productExist', sDefaultContent: "", className: "Actions" }
            ]
        });
    }
}

$(document).ready(function () {
    operationalUser.init();
    OperationalRegistrationValidation();
})

function OperationalRegistrationValidation() {
    $("#frmOperationalRegistration").validate({
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

function operationalRegisterUser() {
    var Isvalid = $("#frmOperationalRegistration").valid();
    if (Isvalid === true) {
        console.log('valid');
    }
}

function deleteUser(UserID) {
    ShowLoader();
    $.ajax({
        type: "POST",
        url: "/Registration/deleteOperationalManager",
        dataType: "json",
        async: false,
        data: { UserID: UserID},
        success: function (data) {
            if (data) {
                SuccessMessage("Operational Manager Deleted Successfully");
                operationalUserDT.clear().draw();
            }
            HideLoader();
        },
        error: function (err) {
            HideLoader();
        }
    });
}
