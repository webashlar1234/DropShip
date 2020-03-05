var orderDT = null;
var order = {
    init: function () {
        order.initCategoryTable();
    },
    change: function () {

    },

    initCategoryTable: function () {
        orderDT = $('#orderDT').DataTable({
            ajax: {
                "url": "/Order/getOrdersData",
                "type": "POST",
                "datatype": "json",
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: false,
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
                    width: "5%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 4,
                    sortable: true,
                    width: "5%",
                    "render": function (data, type, full) {
                        return data + '<label>$</label>';
                    }
                },
                {
                    targets: 5,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 6,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, row) {
                        return data;
                    }
                },
                {
                    targets: 7,
                    sortable: true,
                    width: "5%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 8,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, full) {
                        //return '<input data-role="switch" type="checkbox" data-toggle="toggle" data-on="Paid " data-off="Unpaid " />';
                        return data;
                    }
                },
                {
                    targets: 9,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, row) {
                        return data;
                    }
                },
                {
                    targets: 10,
                    sortable: true,
                    width: "5%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 11,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, row) {
                        if (data) {
                            return '<a class="btn btn-info btn-sm" href="#" onclick=updateStatus("' + row.OrignalProductLink + '",this)>' + 'Buy Now' + '</a>';
                        }
                    }
                },
            ],
            columns: [
                { "sTitle": 'Ali Express Order Number', "mData": 'AliExpressOrderNumber', sDefaultContent: "", className: "AliExpressOrderNumber" },
                { "sTitle": 'Orignal Product Id', "mData": 'OrignalProductId', sDefaultContent: "", className: "OrignalProductId" },
                { "sTitle": 'Orignal Product Link', "mData": 'OrignalProductLink', sDefaultContent: "", className: "OrignalProductLink" },
                { "sTitle": 'Product Title', "mData": 'ProductTitle', sDefaultContent: "", className: "ProductTitle" },
                { "sTitle": 'Order Amount(USD)', "mData": 'OrderAmount', sDefaultContent: "", className: "OrderAmount" },
                { "sTitle": 'Delevery Country', "mData": 'DeleveryCountry', sDefaultContent: "", className: "DeleveryCountry" },
                { "sTitle": 'Shipping Weight(KG)', "mData": 'ShippingWeight', sDefaultContent: "", className: "ShippingWeight" },
                { "sTitle": 'Order Status', "mData": 'OrderStatus', sDefaultContent: "", className: "OrderStatus" },
                { "sTitle": 'Payment Status', "mData": 'PaymentStatus', sDefaultContent: "", className: "PaymentStatus" },
                { "sTitle": 'Seller ID', "mData": 'SellerID', sDefaultContent: "", className: "SellerID" },
                { "sTitle": 'Seller Email', "mData": 'SellerEmail', sDefaultContent: "", className: "SellerEmail" },
                { "sTitle": 'Actions', "mData": 'productExist', sDefaultContent: "", className: "Actions" }
            ]
        });
    }
}

$(document).ready(function () {
    order.init();
    $('[data-role="switch"]').bootstrapToggle({
        on: 'Enabled',
        off: 'Disabled'
    });
});

function updateStatus(productLink,data) {
    if (data.text == "Buy Now") {
        data.text = "Ship Now";
        window.open(productLink);
    }
}