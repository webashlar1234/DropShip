var ProductsDt = null;
var table = '#ProductsDt';
var product = {
    init: function () {
        product.initCheckSelection();
        product.change();
        product.initProductDT();
    },
    change: function () {
        $(document).on('change', '#ddlProductCat', function () {
            product.initProductDT();
        });
        $(document).on('change', '#ddlProductBulkAction', function () {
            if ($(this).val() === 1) {
                product.AddPickedProducts();
            }
        });
    },
    AddPickedProducts: function () {
        var selectedItems = global.getSelectedCheckboxList('.chkProducts', 'productid');
        $.ajax({
            type: "POST",
            url: "/Products/pickSellerProducts",
            dataType: "json",
            async: false,
            data: { products: selectedItems },
            success: function (data) {
                SuccessMessage("Picked successfully");
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
                "data": function (d) { d.category = $('#ddlProductCat').val() }
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
                        return '<label class="mt-checkbox mt-checkbox-outline"><input type="checkbox" productid=' + full.ProductID + ' id="chk_prod_' + full.ProductID + '" class="chkProducts"><span></span></label>';
                    }
                },
                {
                    targets: 1,
                    sortable: true,
                    width: "15%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 2,
                    sortable: true,
                    width: "20%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 3,
                    sortable: true,
                    width: "20%",
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
                    "render": function (data, type, full, meta) {
                        return data === true ? 'Start' : 'Stop';
                    }
                }
            ],
            columns: [
                { "title": '<label class="mt-checkbox mt-checkbox-outline"><input type="checkbox" id="chk_prod_All" class="chkAllProducts"><span></span></label>Check All', "mData": '', sDefaultContent: "", className: "checkAll" },
                { "title": 'Original Product Id', "mData": 'OriginalProductID', sDefaultContent: "", className: "OriginalProductID" },
                { "title": 'Product Title', "mData": 'Title', sDefaultContent: "", className: "Title" },
                { "title": 'Source Product Link', "mData": 'SourceWebsite', sDefaultContent: "", className: "SourceWebsite" },
                { "title": 'Total Sales', "mData": '', sDefaultContent: "", className: "TotalSales" },
                { "title": '# Saller Picked', "mData": '', sDefaultContent: "", className: "SallerPicked" },
                { "title": 'Service Status', "mData": 'IsActive', sDefaultContent: "", className: "IsActive" }
            ]
        });
    }
}
$(document).ready(function () {
    product.init();
});