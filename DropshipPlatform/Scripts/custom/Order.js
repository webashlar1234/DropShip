var orderDT = null;
var orderDTSeller = null;
var table = '#orderDT';
var jsonProducts = null;

var order = {
    init: function () {
        order.onclick();
        order.initCategoryTable();
    },
    initSeller: function () {
        order.initCategoryTableSeller();
    },
    change: function () {

    },
    onclick: function () {
        $(document).on('click', '.shipOrder', function () {
            order.ShipNow($(this).data('orderid'), $(this).data('isfullship'), $(this).parent('td').find('.tracking').val());
        });
    },
    initCategoryTable: function () {
        orderDT = $('#orderDT').DataTable({
            ajax: {
                "url": "/Order/getOrdersData",
                "type": "POST",
                "datatype": "json",
                "data": { orderStatus: $('#ddlOrderStatus').val(), sellerPaymentStatus: $('#ddlPaymentStatus').val() },
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: true,
            language: {
                sSearch: "",
                searchPlaceholder: "Search Order",
                loadingRecords: '&nbsp;',
                processing: '<div class="spinner"></div>'
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
                        });
                $('.dataTables_filter').append($searchButton, $clearButton);
            },
            columnDefs: [
                {
                    targets: 0,
                    sortable: true,
                    width: "3%",
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
                    width: "5%",
                    "render": function (data, type, full) {
                        return data ? data + '<label>$</label>' : '';
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
                        return data;
                    }
                },
                {
                    targets: 5,
                    sortable: true,
                    width: "15%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 6,
                    sortable: true,
                    width: "8%",
                    "render": function (data, type, full) {
                        //return '<input data-role="switch" type="checkbox" data-toggle="toggle" data-on="Paid " data-off="Unpaid " />';
                        return data == true ? 'Paid' : 'Unpaid';
                    }
                },
                {
                    targets: 7,
                    sortable: true,
                    width: "8%",
                    "render": function (data, type, row) {
                        return data;
                    }
                },
                {
                    targets: 8,
                    sortable: true,
                    width: "8%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 9,
                    sortable: true,
                    width: "8%",
                    "render": function (data, type, full) {
                        if (data) {
                            return "<a class='btn btn-info btn-sm BuyAll' data-full='" + global.build_full_attrData(full) + "' onclick=BuyAllProduct(this)>Buy All</a>";
                        }
                    }
                }
            ],
            columns: [
                { "sTitle": '', "mData": '', sDefaultContent: "", className: "details-control" },
                { "sTitle": 'Ali Express Order Number', "mData": 'AliExpressOrderNumber', sDefaultContent: "", className: "AliExpressOrderNumber" },
                { "sTitle": 'Order Amount(USD)', "mData": 'OrderAmount', sDefaultContent: "", className: "OrderAmount" },
                { "sTitle": 'Delevery Country', "mData": 'DeleveryCountry', sDefaultContent: "", className: "DeleveryCountry" },
                { "sTitle": 'Shipping Weight(KG)', "mData": 'ShippingWeight', sDefaultContent: "", className: "ShippingWeight" },
                { "sTitle": 'Order Status', "mData": 'OrderStatus', sDefaultContent: "", className: "OrderStatus" },
                { "sTitle": 'Payment Status', "mData": 'SellerPaymentStatus', sDefaultContent: "", className: "PaymentStatus" },
                { "sTitle": 'Seller ID', "mData": 'SellerID', sDefaultContent: "", className: "SellerID" },
                { "sTitle": 'Seller Email', "mData": 'SellerEmail', sDefaultContent: "", className: "SellerEmail" },
                { "sTitle": 'Actions', "mData": 'productExistAny', sDefaultContent: "", className: "Actions" }
            ],
            "createdRow": function (row, data, dataIndex) {
                if (data.ChildOrderItemList.length > 0) {
                    console.log(data.ChildOrderItemList);
                    $(row).find("td:eq(0)").addClass('details-control');
                } else {
                    $(row).find("td:eq(0)").removeClass('details-control');
                }
            }
        });
    },

    initCategoryTableSeller: function () {
        orderDTSeller = $('#orderDTSeller').DataTable({
            ajax: {
                "url": "/Order/getOrdersData",
                "type": "POST",
                "datatype": "json",
                "data": { orderStatus: $('#ddlOrderStatus').val(), sellerPaymentStatus: $('#ddlPaymentStatus').val() },
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: true,
            language: {
                sSearch: "",
                searchPlaceholder: "Search Order",
                loadingRecords: '&nbsp;',
                processing: '<div class="spinner"></div>'
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
                    width: "5%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 1,
                    sortable: true,
                    width: "12%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 2,
                    sortable: true,
                    width: "10%",
                    "render": function (data, type, row) {
                        return data + '<label>$</label>';
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
                        return data;
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
                }
            ],
            columns: [
                { "sTitle": '', "mData": '', sDefaultContent: "", className: "details-control" },
                { "sTitle": 'Ali Express Order Number', "mData": 'AliExpressOrderNumber', sDefaultContent: "", className: "AliExpressOrderNumber" },
                { "sTitle": 'Order Amount(USD)', "mData": 'OrderAmount', sDefaultContent: "", className: "OrderAmount" },
                { "sTitle": 'Delevery Country', "mData": 'DeleveryCountry', sDefaultContent: "", className: "DeleveryCountry" },
                { "sTitle": 'Shipping Weight(KG)', "mData": 'ShippingWeight', sDefaultContent: "", className: "ShippingWeight" },
                { "sTitle": 'Status', "mData": 'OrderStatus', sDefaultContent: "", className: "OrderStatus" },
                { "sTitle": 'Tracking No', "mData": 'TrackingNo', sDefaultContent: "", className: "TrackingNo" },
                { "sTitle": 'Payment Status', "mData": 'PaymentStatus', sDefaultContent: "", className: "PaymentStatus" },
            ],
            "createdRow": function (row, data, dataIndex) {
                if (data.ChildOrderItemList.length > 0) {
                    console.log(data.ChildOrderItemList);
                    $(row).find("td:eq(0)").addClass('details-control');
                } else {
                    $(row).find("td:eq(0)").removeClass('details-control');
                }
            }
        });
    },
    ShipNow: function (OrderId, isFullShip, TrackingNumber) {
        ShowLoader();
        obj = {
            AliExpressOrderNumber: OrderId, TrackingNumber: TrackingNumber
        }
        $.ajax({
            type: "POST",
            url: "/Order/FullFillAliExpressOrder",
            dataType: "json",
            async: false,
            data: { orderData: obj, isFullShip: isFullShip },
            success: function (data) {
                if (data) {
                    SuccessMessage("Product Status Updated successfully");
                }
                else {
                    ErrorMessage("Order Fullfilment faild for order " + OrderId + ", please try again later");
                }
                HideLoader();
            },
            error: function (err) {
                HideLoader();
                ErrorMessage("Order Fullfilment faild for order " + OrderId + ", please try again later");
            }
        });
    }
};

$(document).ready(function () {

    $('#orderDT').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = orderDT.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');

        } else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');
        }
    });

    $('#orderDTSeller').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = orderDTSeller.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');

        } else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');
        }
    });

    adminTable = $("#orderDT");
    sellerTable = $("#orderDTSeller");

    if (adminTable) {
        order.init();
    }
    if (sellerTable) {
        order.initSeller();
    }

    $('[data-role="switch"]').bootstrapToggle({
        on: 'Enabled',
        off: 'Disabled'
    });
});

function BuyProduct(productLink, data, OrderId, TrackingNumber) {
    //data.text = "Ship Now";
    //data.outerHTML += '<input class="form-control tracking" name="txtTracking" type="text" style="margin-top:5px" value="' + TrackingNumber + '">'
    window.open("http://" + productLink);

    $('<a class="btn btn-info btn-sm shipOrder" data-orderid="' + OrderId + '" data-isfullship="false">Ship Now</a><input class="form-control tracking" name="txtTracking" type="text" style="margin-top:5px" value="' + TrackingNumber + '">').insertAfter(data)
    $(data).remove();
    UpdateOrderStatus(OrderId);
}

function BuyAllProduct(thisitem) {
    var Order = $(thisitem).data('full');
    $.each(Order.ChildOrderItemList, function (key, item) {
        window.open("http://" + item.OrignalProductLink);
    });

    $('<a class="btn btn-info btn-sm shipOrder" data-orderid="' + Order.AliExpressOrderID + '" data-isfullship="true">Ship Now</a><input class="form-control tracking" name="txtTracking" type="text" style="margin-top:5px" value="' + Order.TrackingNumber + '">').insertAfter(thisitem);
    $(thisitem).remove();
    UpdateOrderStatus(Order.AliExpressOrderID);
}

function UpdateOrderStatus(OrderId) {
    $.ajax({
        type: "POST",
        url: "/Order/BuyOrderFromSourceWebsite",
        dataType: "json",
        async: false,
        data: { OrderID: OrderId },
        success: function (data) {
            if (data) {
                //SuccessMessage("Product Status Updated successfully");
            }
            HideLoader();
        },
        error: function (err) {
            HideLoader();
        }
    });
}



function format(data) {
    console.log(data.ChildOrderItemList);
    var trs = '';
    $.each($(data.ChildOrderItemList), function (key, value) {

        var BuyNowLink = value.productExist ? '<a class="btn btn-info btn-sm" href="#" onclick=BuyProduct("' + value.OrignalProductLink + '",this,"' + data.AliExpressOrderNumber + '","' + data.TrackingNumber + '")>' + 'Buy Now' + '</a>' : '';

        console.log(value);
        trs +=
            '<tr class="skuRow" data-for="' + value.AliExpressProductId + '"<td>' +
            '</td> <td>' + value.ProductName +
            '</td> <td>' + value.AliExpressProductId +
            '</td> <td>' + value.OrignalProductLink +
            '</td> <td>' + value.OrignalProductId +
            '</td><td>' + value.Price +
            '</td><td>' + value.Colour +
            '</td><td>' + value.Size +
            '</td><td>' + BuyNowLink +
            '</td></tr>';
    });
    // `data` is the original data object for the row
    return '<table class="table table-border table-hover innertable">' +
        '<thead>' +
        '<th style="width:15%">Product Name</th>' +
        '<th>AliExpress Product ID</th>' +
        '<th>Orignal Product Link</th>' +
        '<th>Orignal Product Id</th>' +
        '<th>Price</th>' +
        '<th>Colour</th>' +
        '<th>Size</th>' +
        '<th>Action</th>' +
        '</thead><tbody>' +
        trs +
        '</tbody></table>';
}
