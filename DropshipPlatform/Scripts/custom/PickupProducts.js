﻿var ProductsDt = null;
var table = '#ProductsDt';
var jsonProducts = null;
var allChecked = false;

var product = {
    init: function () {
        product.initCheckSelection();
        product.change();
    },
    change: function () {
        $(document).on('change', '#ddlProductCat, #ddlPickupFilter', function () {
            BindData();
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
            var innerTableLength = $(".innertable").find("tr.skuRow[data-for='" + item + "']").length;
            if (innerTableLength > 0) {

                if ($(".innertable").find("tr.skuRow[data-for='" + item + "']")) {
                    $.each($(".innertable").find("tr.skuRow[data-for='" + item + "']"), function (i, data) {
                        if ($(data).find(".updatedPrice").val() > 0) {
                            pickedProducts[pickedProducts.length - 1].SKUModels.push({ skuCode: $(data).data("sku"), inventory: $(data).data("inventory"), price: $(data).find(".updatedPrice").val(), discount_price: 1, childproductId: $(data).data("childproductid") })
                        }
                        else {
                            pickedProducts[pickedProducts.length - 1].SKUModels.push({ skuCode: $(data).data("sku"), inventory: $(data).data("inventory"), price: $('.parentChk[productid=' + item + ']').parents('tr').find('td:eq(3)').text(), discount_price: 1, childproductId: $(data).data("childproductid") })
                        }
                    });
                }
            }
            else {
                var parentItem = jsonData.filter(m => m.ProductID == item);
                if (parentItem.length > 0) {
                    parentItem = parentItem[0];
                    var childrens = parentItem.ChildProductList;
                    for (var i = 0; i < childrens.length; i++) {
                        if (childrens[i].UpdatedPrice > 0) {
                            pickedProducts[pickedProducts.length - 1].SKUModels.push({ skuCode: childrens[i].SkuID, inventory: childrens[i].Inventory, price: childrens[i].UpdatedPrice, discount_price: 1, childproductId: childrens[i].ProductID });
                        }
                        else {
                            pickedProducts[pickedProducts.length - 1].SKUModels.push({ skuCode: childrens[i].SkuID, inventory: childrens[i].Inventory, price: childrens[i].Cost, discount_price: 1, childproductId: childrens[i].ProductID });
                        }
                    }
                }
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
                    BindData();
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
    }
};

$(document).ready(function () {
    product.init();
    BindData();

    function SetPickedDisable() {
        var chkUpdatedPriceList = $('input[name=updatedPrice]');
        if (chkUpdatedPriceList && chkUpdatedPriceList.length > 0) {
            for (var i = 0; i < chkUpdatedPriceList.length; i++) {
                var ParentID = $(chkUpdatedPriceList[i]).attr("dataparentid");
                if (ParentID && parseInt(ParentID) > 0) {
                    ParentID = parseInt(ParentID);
                    var ParentProduct = jsonProducts.filter(m => m.ProductID === ParentID);
                    if (ParentProduct != null && ParentProduct.length > 0) {
                        ParentProduct = ParentProduct[0];
                    }
                    if (ParentProduct.hasProductSkuSync || ParentProduct.isProductPicked) {
                        $(chkUpdatedPriceList[i]).prop("disabled", true);
                    }
                }
                else {
                    var elementID = $(chkUpdatedPriceList[i]).attr("datasku");
                    if (elementID && parseInt(elementID) > 0) {
                        elementID = parseInt(elementID);
                        var ParentProduct = jsonProducts.filter(m => m.ProductID === elementID);
                        if (ParentProduct != null && ParentProduct.length > 0) {
                            ParentProduct = ParentProduct[0];
                        }
                        if (ParentProduct.hasProductSkuSync || ParentProduct.isProductPicked) {
                            $(chkUpdatedPriceList[i]).prop("disabled", true);
                        }
                    }
                }
            }
        }
    }



    $('#ProductsDt tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = ProductsDt.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
            //$('input[name=updatedPrice]').prop("disabled", true);
            $('input[name=updatedPrice]', row.child().eq(0)).prop("disabled", true);
        } else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');

            if ($('.parentChk').is(":checked")) {
                $('input[name=updatedPrice]', row.child().eq(0)).prop("disabled", false);
                SetPickedDisable();
            }
            else {
                $('input[name=updatedPrice]', row.child().eq(0)).prop("disabled", true);
            }
        }
    });

    $('#chkAllProduct').change(function (e) {
        var parentCheckboxes = $('.parentChk');
        //ProductFormValidate();
        if (event.target.checked) {
            parentCheckboxes.prop('checked', true);
            $('#btnSave').prop('disabled', false);
            $('input[name=updatedPrice]').prop("disabled", false);
            SetPickedDisable();
        }
        else {
            parentCheckboxes.prop('checked', false);
            $('#btnSave').prop('disabled', true);
            $('input[name=updatedPrice]').prop("disabled", true);
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
        "data": { category: $('#ddlProductCat').val(), filterOptions: $('#ddlPickupFilter').val() },
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
            "ProductID": json[i].ParentProduct.ProductID,
            "ParentProductID": json[i].ParentProduct.ParentProductID,
            "SellerPrice": json[i].ParentProduct.SellerPrice,
            "hasProductSkuSync": json[i].ParentProduct.hasProductSkuSync,
            "isProductPicked": json[i].ParentProduct.isProductPicked,
            "check": "<label class='mt-checkbox'><input type='checkbox' class='parentChk' data-SKU=" + json[i].ParentProduct.ProductID + " value='1'><span></span></label>",
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

        var isPicked = false;
        if (value.isProductPicked) {
            isPicked = true;
        }
        console.log(value);
        trs +=
            '<tr class="skuRow" data-for="' + value.ParentProductID + '" data-inventory="' + value.Inventory + '" data-sku="' + value.SkuID + '" data-childProductId="' + value.ProductID + '"><td>' + value.Title +
            '</td> <td>' + value.Brand +
            '</td><td>' + value.NetWeight +
            '</td><td>' + value.Color +
            '</td><td>' + value.Size +
            '</td><td>' + value.Inventory +
            '</td><td>' + "$" + value.Cost +
            '</td><td>' + "<input name='updatedPrice'  disabled dataParentID=" + value.ParentProductID + "  dataSKU='" + value.SkuID + "' type='number' value=" + value.UpdatedPrice + " class='updatedPrice txtEdit_" + value.ParentProductID + "'>" +
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

function BindData() {
    if (ProductsDt) {
        ProductsDt.destroy();
    }

    ProductsDt = $(table).DataTable({
        "ajax": {
            "type": "POST",
            "url": "/Products/getProductManagementDT",
            "data": { category: $('#ddlProductCat').val(), filterOptions: $('#ddlPickupFilter').val() },
            "dataSrc": function (json) {
                jsonProducts = FormatData(json.data);
                return jsonProducts;
            }
        },
        "processing": true,
        "serverSide": true,
        "drawCallback": function () {
            [...$('#ProductsDt [type="checkbox"]')].forEach(checkbox => $(checkbox).prop('checked', allChecked));
            [...$('#ProductsDt [name="updatedPrice"]')].forEach(function (updatedPrice) {
                var ParentID = $(updatedPrice).attr("dataparentid");
                if (ParentID && parseInt(ParentID) > 0) {
                    ParentID = parseInt(ParentID);
                    var ParentProduct = jsonProducts.filter(m => m.ProductID === ParentID);
                    if (ParentProduct != null && ParentProduct.length > 0) {
                        ParentProduct = ParentProduct[0];
                    }
                    if (ParentProduct.hasProductSkuSync || ParentProduct.isProductPicked) {
                        $(updatedPrice).prop("disabled", true);
                    }
                    else {
                        $(updatedPrice).prop("disabled", !allChecked);
                    }
                }
                else {
                    var elementID = $(updatedPrice).attr("datasku");
                    if (elementID && parseInt(elementID) > 0) {
                        elementID = parseInt(elementID);
                        var ParentProduct = jsonProducts.filter(m => m.ProductID === elementID);
                        if (ParentProduct != null && ParentProduct.length > 0) {
                            ParentProduct = ParentProduct[0];
                        }
                        if (ParentProduct.hasProductSkuSync || ParentProduct.isProductPicked) {
                            $(updatedPrice).prop("disabled", true);
                        }
                        else {
                            $(updatedPrice).prop("disabled", !allChecked);
                        }
                    }
                }
            })
        },
        "columns": [{
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
                    return "<input name='updatedPrice' disabled dataSKU='" + full.ProductID + "' type='number' value=" + full.cost + " class='updatedParentPrice txtParent_" + full.ProductID + "'>";
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
                    rawHTML = '<label class="mt-checkbox mt-checkbox-outline disabled"><input disabled type="checkbox" productid=' + full.ProductID + ' id="chk_prod_' + full.ProductID + '" class="chkProducts parentChk"><span></span></label>';
                }
                else {
                    rawHTML = '<label class="mt-checkbox mt-checkbox-outline"><input type="checkbox" productid=' + full.ProductID + ' id="chk_prod_' + full.ProductID + '" class="chkProducts parentChk"><span></span></label>';
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
        ],
        "createdRow": function (row, data, dataIndex) {
            if (data.ChildProductList.length > 0) {
                $(row).find("td:eq(0)").addClass('details-control');
            }
        }
    });




}