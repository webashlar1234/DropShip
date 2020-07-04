var SellerDT = null;
var seller = {
    init: function () {
        seller.initSellerTable();
        seller.click();
    },
    click: function () {
        $(document).on('click', '#updateUserStatus', function () {
            ShowLoader();
            var userID = $(this).attr('data-userid');
            $.ajax({
                type: "POST",
                url: "/User/updateUserStatus",
                dataType: "json",
                async: false,
                data: { UserID: userID },
                success: function (data) {
                    if (data) {
                        SellerDT.clear().draw();
                        SuccessMessage("Saved successfully");
                    }
                    else {
                        ErrorMessage("Something went wrong please try again later");
                    }
                    HideLoader();
                },
                error: function (err) {
                    HideLoader();
                    ErrorMessage("Something went wrong please try again later");
                }
            });
        });
    },
    change: function () {
        
    },
    initSellerTable: function () {
        SellerDT = $('#SellerDT').DataTable({
            ajax: {
                "url": "/User/getUserDatatable",
                "type": "POST",
                "datatype": "json"
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: true,
            language: {
                sSearch: "",
                searchPlaceholder: "Search Seller",
                loadingRecords: '&nbsp;',
                processing: '<div class="spinner"></div>'
            },
            preDrawCallback: function () {
                //$('#categoryMappingDT_filter input').addClass('col-lg-2');
            },
            fnDrawCallback: function (oSettings) {
                $('.selectpicker').selectpicker('refresh');
            },
            initComplete: function (setting, json) {
                var input = $('.dataTables_filter input').unbind(),
                    self = this.api(),
                    $searchButton = $('<button class="btn btn-sm btn-black mr-2 ml-2">')
                        .text('Search')
                        .click(function () {
                            self.search(input.val()).draw();
                        }),
                    $clearButton = $('<button class="btn btn-sm  btn-white">')
                        .text('Clear')
                        .click(function () {
                            input.val('');
                            $searchButton.click();
                        })
                $('.dataTables_filter').append($searchButton, $clearButton);
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
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 3,
                    sortable: true,
                    width: "12%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 4,
                    sortable: true,
                    width: "12%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 5,
                    sortable: false,
                    width: "8%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 6,
                    sortable: false,
                    width: "12%",
                    "render": function (data, type, full) {
                        if (data) {
                            var pattern = /Date\(([^)]+)\)/;
                            var results = pattern.exec(data);
                            var dt = new Date(parseFloat(results[1]));
                            var day = dt.getDate() > 9 ? dt.getDate() : "0" + dt.getDate();
                            return day + "/" + (dt.getMonth() + 1) + "/" + dt.getFullYear();
                        }
                        else {
                            return "";
                        }
                    }
                },
                {
                    targets: 7,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 8,
                    sortable: true,
                    width: "15%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 9,
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, full) {
                        var text = full.IsActive == true ? "InActive" : "Active";
                        return '<button class="btn btn-info btn-sm" id="updateUserStatus" data-userid="' + full.UserID + '">' + text + '</button>';
                    }
                }
            ],
            columns: [
                { "title": 'Seller Name', "mData": 'Name', sDefaultContent: "", className: "Name" },
                { "sTitle": 'Seller Email ID', "mData": 'EmailID', sDefaultContent: "", className: "EmailID" },
                { "sTitle": 'Phone', "mData": 'Phone', sDefaultContent: "", className: "Phone" },
                { "sTitle": 'AliExpress Seller ID', "mData": 'AliExpressLoginID', sDefaultContent: "", className: "AliExpressLoginID" },
                { "sTitle": 'Stripe Customer ID', "mData": 'StripeCustomerID', sDefaultContent: "", className: "StripeCustomerID" },
                { "sTitle": 'IsPolicyAccepted', "mData": 'IsPolicyAccepted', sDefaultContent: "", className: "IsPolicyAccepted" },
                { "sTitle": 'Date', "mData": 'ItemCreatedWhen', sDefaultContent: "", className: "ItemCreatedWhen" },
                { "sTitle": 'Membership', "mData": 'Membership', sDefaultContent: "", className: "Membership" },
                { "sTitle": 'PickedProducts', "mData": 'NoOfPickedProducts', sDefaultContent: "", className: "NoOfPickedProducts" },
                { "sTitle": '', "mData": '', sDefaultContent: "", className: "Actions" }
            ]
        });
    }
}

$(document).ready(function () {
    seller.init();
});


