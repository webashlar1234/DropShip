var ProductsDt = null;
var table = '#ProductsDt';
var product = {
    init: function () {
        product.initCheckSelection();
        product.change();
        product.initProductDT();
    },
    change: function () {
        $(document).on('change', '#ddlProductCat, #ddlPickupFilter', function () {
            product.initProductDT();
        });
        $(document).on('change', '#ddlProductBulkAction', function () {
            if (Number($(this).val()) === 1) {
                product.AddPickedProducts();
            }
        });
    },
    AddPickedProducts: function () {
        var selectedItems = global.getSelectedCheckboxList('.chkProducts', 'productid');
        var pickedProducts = [];
        $.each(selectedItems, function (index, item) {
            pickedProducts.push({ key: item, value: $('.chkProducts[productid=' + item + ']').parents('tr').find('.SellerPriceInp').val() })
        });
        $.ajax({
            type: "POST",
            url: "/Products/pickSellerProducts",
            dataType: "json",
            async: false,
            data: { products: pickedProducts },
            success: function (data) {
                //$('#ddlProductBulkAction').val(0);
                //$('#ddlProductBulkAction').selectpicker('refresh');
                SuccessMessage("Picked successfully");
                window.location.href = "/Products/MyProduct";
            }
        });
    },
    initCheckSelection:function(){
        $(document).on('change', '#chk_prod_All', function () {
            $(".chkProducts", table).prop("checked", $(this).is(":checked"));
        });
        $(document).on('change', '.chkProducts', function () {
            $("#chk_prod_All", table).prop("checked", $(".chkProducts:checked", table).length > 0);
        });
    },
    initProductDT: function () {
        if (ProductsDt) {
            ProductsDt.destroy();
        }
        ProductsDt = $(table).DataTable({
            "ajax": {
                "url": "/Products/getProductManagementDT",
                "type": "POST",
                "datatype": "json",
                "data": function (d) { d.category = $('#ddlProductCat').val(), d.filterOptions = $('#ddlPickupFilter').val() }
                //"data": { category: $('#ddlProductCat').val() }
            },
            //processing: true,
            //serverSide: true,
            sort: true,
            filter: true,
            search: {
                //search: "Search"
            },
            language: {
                sSearch: "",
                searchPlaceholder: "Search product"
            },
            preDrawCallback: function () {
                $('#ProductsDt_filter input').addClass('form-control');
            },
            fnDrawCallback: function (oSettings) {
                $('.selectpicker').selectpicker('refresh');
            },
            initComplete: function (setting, json) {

            },
            columnDefs: [
                {
                    targets: 0,
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, full) {
                        var rawHTML = "";
                        if (full.SellerPrice > 0) {
                            rawHTML = "Picked";
                        }
                        else {
                            rawHTML = '<label class="mt-checkbox mt-checkbox-outline"><input type="checkbox" productid=' + full.ProductID + ' id="chk_prod_' + full.ProductID + '" class="chkProducts"><span></span></label>';
                        }
                        return rawHTML;
                    }
                },
                {
                    targets: 1,
                    sortable: true,
                    width: "25%",
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
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 4,
                    sortable: true,
                    width: "10%",
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
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, full) {
                        return '<input class="SellerPriceInp" ' + (full.SellerPrice > 0 ? 'Disabled' : '') + ' placeholder="Your Price" value="' + (full.SellerPrice > 0 ? full.SellerPrice : '') + '" >';
                    }
                },
                 {
                     targets: 7,
                     sortable: false,
                     width: "15%",
                     "render": function (data, type, full) {
                         return full.SellerPickedCount > 0 ? ("Picked by " + full.SellerPickedCount + " sellers") : (full.IsActive ? "Online" : "Offline");
                     }
                 }
            ],
            columns: [
                { "title": '<label class="mt-checkbox mt-checkbox-outline"><input type="checkbox" id="chk_prod_All" class="chkAllProducts"><span></span></label>Check All', "mData": '', sDefaultContent: "", className: "checkAll" },
                { "title": 'Product Title', "mData": 'Title', sDefaultContent: "", className: "Title" },
                { "title": 'Category', "mData": 'CategoryName', sDefaultContent: "", className: "CategoryName" },
                { "title": 'Cost(USD)', "mData": 'Cost', sDefaultContent: "", className: "Cost" },
                { "title": 'Inventory', "mData": 'Inventory', sDefaultContent: "", className: "Inventory" },
                { "title": 'Shipping Weight(KG)', "mData": 'ShippingWeight', sDefaultContent: "", className: "ShippingWeight" },
                { "title": 'Your Price', "mData": '', sDefaultContent: "", className: "YourPrice" },
                { "title": 'Product Status', "mData": '', sDefaultContent: "", className: "" }
            ]
        });
    }
}
$(document).ready(function () {
    product.init();
});