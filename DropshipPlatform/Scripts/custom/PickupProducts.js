var ProductsDt = null;
var table = '#ProductsDt';

var product = {
    init: function () {
        product.initCheckSelection();
        product.change();
        //product.initProductDT();
    },
    change: function () {
        $(document).on('change', '#ddlProductCat, #ddlPickupFilter', function () {
            GetData();
        });
        $(document).on('change', '#ddlProductBulkAction', function () {
            //$('.spinner').show();
            if (Number($(this).val()) === 1) {
                var selectedItems = global.getSelectedCheckboxList('.chkProducts', 'productid');
                var pickedProducts = [];
                $.each(selectedItems, function (index, item) {
                    pickedProducts.push({ key: item, value: $('.chkProducts[productid=' + item + ']').parents('tr').find('.SellerPriceInp').val() });
                });
                if (pickedProducts.length > 0) {
                    product.AddPickedProducts();
                }
                else {
                    $("#ddlProductBulkAction").val("");
                    ErrorMessage("Please select atleast one product");
                }
                $('#ddlProductBulkAction').val('');
                $('#ddlProductBulkAction').selectpicker('refresh');
            }
        });
    },
    AddPickedProducts: function () {

        var selectedItems = global.getSelectedCheckboxList('.chkProducts', 'productid');
        var pickedProducts = [];
        var update_price = true;
        $.each(selectedItems, function (index, item) {
            pickedProducts.push({ productId: item, price: $('.parentChk[productid=' + item + ']').parents('tr').find('.updatedParentPrice').val(), SKUModels: [] });
            if ($(".innertable").find("tr.skuRow[data-for='" + item + "']")) {
                $.each($(".innertable").find("tr.skuRow[data-for='" + item + "']"), function (i, data) {
                    pickedProducts[pickedProducts.length - 1].SKUModels.push({ skuCode: $(data).data("sku"), inventory: $(data).data("inventory"), price: $(data).find(".updatedPrice").val(), discount_price: 1 })
                });
            }

            //pickedProducts.push({ key: item, value: $('.chkProducts[productid=' + item + ']').parents('tr').find('.SellerPriceInp').val() });
        });

        $.each(pickedProducts, function (j, result) {
            $(result.SKUModels).filter(function (index, updatedData) {
                if (!(updatedData.price > 0)) {
                    return update_price = false;
                }

            })
        });
        if (update_price) {
            $('.spinner').show();
            $.ajax({
                type: "POST",
                url: "/Products/pickSellerProducts",
                dataType: "json",
                async: false,
                data: { products: pickedProducts },
                success: function (data) {
                    SuccessMessage("Submitted to aliExpress for pickup process");
                    GetData();
                    $('.spinner').hide();
                }
            });
        }
        else {
            $("#ddlProductBulkAction").val("");
            ErrorMessage("Please enter the price you want to sell");
            $('.spinner').hide();
        }

    },
    initCheckSelection: function () {
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
};

$(document).ready(function () {
    product.init();
    GetData();

    $('#ProductsDt tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = ProductsDt.row(tr);

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

    $('#chkAllProduct').change(function () {
        var parentCheckboxes = $('.parentChk');
        //ProductFormValidate();
        if (event.target.checked) {
            parentCheckboxes.prop('checked', true);
            $('#btnSave').prop('disabled', false);
        }
        else {
            parentCheckboxes.prop('checked', false);
            $('#btnSave').prop('disabled', true);
        }
    });

    $("#ProductsDt tbody").on("change", ".parentChk", function () {
        //ProductFormValidate();
        console.log($(this).text());
        var parentProductID = $(this).attr('productid');
        if (event.target.checked) {
            var parentCheckboxes = $('.parentChk');
            var allchecked = true;
            if (parentCheckboxes && parentCheckboxes.length > 0) {
                for (var i = 0; i < parentCheckboxes.length; i++) {
                    if (!($(parentCheckboxes[i]).prop('checked'))) {
                        allchecked = false;
                        break;
                    }
                }
            }
            if (allchecked) {
                $('#chkAllProduct').prop('checked', true);
            }

            $('.txtEdit_' + parentProductID + '').prop("disabled", false);
            $('.txtParent_' + parentProductID + '').prop("disabled", false);
            $('#btnSave').prop('disabled', false);

        }
        else {
            $('#chkAllProduct').prop('checked', false);
            $('.txtEdit_' + parentProductID + '').prop("disabled", true);
            $('.txtParent_' + parentProductID + '').prop("disabled", true);
        }
    });

    $("#ProductsDt tbody").on("change", ".pickedInvetory", function () {
        $("#frmPickedProduct").valid();
    });
    $("#ProductsDt tbody").on("change", ".updatedParentPrice", function () {
        var parentPrice = this.value;
        var parentID = $(this).attr("datasku");
        $(".txtEdit_" + parentID).val(parentPrice);
        //$("#frmPickedProduct").valid();
    });
});

function GetData() {
    ShowLoader();
    $.ajax("/Products/getProductManagementDT", {
        "type": "POST",
        "datatype": "json",
        "data": { category : $('#ddlProductCat').val(), filterOptions : $('#ddlPickupFilter').val() },
        success: function (data, status, xhr) {
            HideLoader();
            jsonProducts = FormatData(data.data);
            console.log(jsonProducts);
            BindData(jsonProducts);
        },
        error: function (jqXhr, textStatus, errorMessage) {
            console.log(errorMessage);
            HideLoader();
        }
    });
}

function FormatData(json) {
    console.log("json", json);
    jsonData = [];
    for (var i = 0; i < json.length; i++) {
        var productGroup = {
            "Title": json[i].ParentProduct.Title,
            "category": json[i].ParentProduct.CategoryName,
            "cost": json[i].ParentProduct.Cost,
            "inventory": json[i].ParentProduct.Inventory,
            "shippingweight": json[i].ParentProduct.ShippingWeight,
            "color": json[i].ParentProduct.Color,
            "manufacturerName": json[i].ParentProduct.ManufacturerName,
            "origin": json[i].ParentProduct.CountryOfOrigin,
            "website": json[i].ParentProduct.SourceWebsite,
            "OriginalProductID": json[i].ParentProduct.OriginalProductID,
            "ParentProductID": json[i].ParentProduct.ParentProductID,
            "SellerPrice": json[i].ParentProduct.SellerPrice,
            "hasProductSkuSync": json[i].ParentProduct.hasProductSkuSync,
            "isProductPicked": json[i].ParentProduct.isProductPicked,
            "check": "<label class='mt-checkbox'><input type='checkbox' class='parentChk' data-SKU=" + json[i].ParentProduct.OriginalProductID + " value='1'><span></span></label>",
            "ChildProductList": json[i].ChildProductList,
            "SellerPickedCount": json[i].ParentProduct.SellerPickedCount,
            "IsActive": json[i].ParentProduct.IsActive
        };
        jsonData.push(productGroup);
    }
    console.log("jsonData", jsonData);

    return jsonData;
}

function format(d) {
    console.log(d.ChildProductList);
    var trs = '';
    $.each($(d.ChildProductList), function (key, value) {

        trs +=
            '<tr class="skuRow" data-for="' + value.ParentProductID + '" data-inventory="' + value.Inventory + '" data-sku="' + value.OriginalProductID + '"><td>' + value.Title +
            '</td> <td>' + value.Brand +
            '</td><td>' + value.NetWeight +
            '</td><td>' + value.Color +
            '</td><td>' + value.Size +
            '</td><td>' + value.Inventory +
            '</td><td>' + "$" + value.Cost +
            '</td><td>' + "<input name='updatedPrice'  disabled dataSKU='" + value.OriginalProductID + "' type='number' value=" + value.UpdatedPrice + " class='updatedPrice txtEdit_" + value.ParentProductID + "'>" +
            //'</td><td>' + value.Description +
            '</td></tr>';
    })
    // `d` is the original data object for the row
    return '<table class="table table-border table-hover innertable">' +
        '<thead>' +
        '<th style="width:15%">Title</th>' +
        '<th>Brand</th>' +
        '<th>Weight</th>' +
        '<th>Color</th>' +
        '<th>Size</th>' +
        '<th>Inventory</th>' +
        '<th>Price</th>' +
        '<th>Updated Price</th>' +
        //'<th style="width:30%">Description</th>' +
        //'<th>Rating</th>' +
        '</thead><tbody>' +

        trs +
        '</tbody></table>';
}

function GeneratePropertyList(propertyList, elementName, Id) {
    var OptionHTML = "";
    var schemaValues = propertyList;
    if (schemaValues && schemaValues.length > 0) {
        for (var i = 0; i < schemaValues.length; i++) {
            if (schemaValues[i]) {
                OptionHTML = OptionHTML + "<option value=" + schemaValues[i].PropertyID + ">" + schemaValues[i].PropertyName + "</option>";
            }
        }
    }
    var selectHTML = "<select id=" + elementName + Id + " class='" + elementName + "'>" + OptionHTML + "</select>";
    return selectHTML;
}

function BindData(jsonProducts) {
    if (ProductsDt) {
        ProductsDt.destroy();
    }
    ProductsDt = $(table).DataTable({
        "data": jsonProducts,
        "columns": [{
            "class": 'details-control',
            "orderable": false,
            "data": null,
            "defaultContent": ''
        },
        { "data": "Title" },
        { "data": "category" },
        { "data": "cost" },
        { "data": "inventory" },
        { "data": "shippingweight" },
        {
            "data": "UpdatedPrice", "render": function (data, type, full) {
                if (!(full.ChildProductList.length > 0)) {
                    full.cost > 0 ? full.cost : 0;
                    return "<input name='updatedPrice' disabled dataSKU='" + full.OriginalProductID + "' type='number' value=" + full.cost + " class='updatedParentPrice txtParent_" + full.OriginalProductID + "'>";
                }
                else {
                    return "";
                }
            }
        },
        {
            "data": "pick", "render": function (data, type, full) {
                var rawHTML = "";
                if (full.isProductPicked) {
                    rawHTML = "Picked";
                }
                else if (full.hasProductSkuSync) {
                    rawHTML = '<label class="mt-checkbox mt-checkbox-outline disabled"><input disabled type="checkbox" productid=' + full.OriginalProductID + ' id="chk_prod_' + full.OriginalProductID + '" class="chkProducts parentChk"><span></span></label>';
                }
                else {
                    rawHTML = '<label class="mt-checkbox mt-checkbox-outline"><input type="checkbox" productid=' + full.OriginalProductID + ' id="chk_prod_' + full.OriginalProductID + '" class="chkProducts parentChk"><span></span></label>';
                }
                return rawHTML;
            }
        },

        {
            "data": "check", "render": function (data, type, full) {
                var rawHTML = "";
                if (full.hasProductSkuSync && !full.isProductPicked) {
                    rawHTML = "In Progress"
                }
                else {
                    rawHTML = full.SellerPickedCount > 0 ? ("Picked by " + full.SellerPickedCount + " sellers") : (full.IsActive ? "Online" : "Offline");
                }

                return rawHTML;
            }
        }
        ]
    });
}
