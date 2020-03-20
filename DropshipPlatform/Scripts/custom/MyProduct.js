var table;
var jsonProducts;
$(document).ready(function () {
    GetData();
    ProductFormValidate();

    // Add event listener for opening and closing details
    $('#TblPickedProduct tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        } else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');

            if ($('.parentChk').is(":checked")) {
                $('input[name=updatedPrice]', row.child().eq(0)).prop("disabled", false);
            }
            else {
                $('input[name=updatedPrice]', row.child().eq(0)).prop("disabled", true);
            }
        }
    });

    $('#chkAllProduct').change(function () {

        var parentCheckboxes = $('.parentChk');
        // ProductFormValidate();

        if (event.target.checked) {
            parentCheckboxes.prop('checked', true);
            $('#btnSave').prop('disabled', false);
        }
        else {
            parentCheckboxes.prop('checked', false);
            $('#btnSave').prop('disabled', true);
        }
    });

    $("#TblPickedProduct tbody").on("change", ".parentChk", function () {
        // ProductFormValidate();
        console.log($(this).text());
        var parentProductID = $(this).attr('data-SKU');
        if (this.checked) {
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

    $("#TblPickedProduct tbody").on("change", ".pickedInvetory", function () {
        $("#frmPickedProduct").valid();
    });
    $("#TblPickedProduct tbody").on("change", ".updatedPrice", function () {
        $(this).data("isupdated", true);
        $("#frmPickedProduct").valid();
    });
    $(document).on('change', '#ddlProductCat', function () {
        GetData();
    });
});

function GetData() {
    ShowLoader();
    $.ajax('getPickedAliProducts', {
        type: 'GET',  // http method
        data: { category: $('#ddlProductCat').val() },  // data to submit
        success: function (data, status, xhr) {
            jsonProducts = FormatData(data.data);
            console.log(jsonProducts);
            BindData(jsonProducts);
            HideLoader();
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
            "check": "<label class='mt-checkbox'><input type='checkbox' class='parentChk' aliexpressproductid=" + json[i].ParentProduct.AliExpressProductID + " data-SKU=" + json[i].ParentProduct.ProductID + " value='1'><span></span></label>",
            "IsOnline": json[i].ParentProduct.IsOnline ? '<a class="btn btn-info btn-sm" href="#" onclick=updateStatus("' + json[i].ParentProduct.AliExpressProductID + '","' + json[i].ParentProduct.IsOnline + '")>' + 'Online' + '</a>' : '<a class="btn btn-info btn-sm" href="#" onclick=updateStatus("' + json[i].ParentProduct.AliExpressProductID + '","' + json[i].ParentProduct.IsOnline + '")>' + 'Offline' + '</a>',
            "ChildProductList": json[i].ChildProductList,
            "AliExpressProductID": json[i].ParentProduct.AliExpressProductID
        };
        jsonData.push(productGroup);
    }
    console.log("jsonData", jsonData);

    return jsonData;
}

//BindData();

function format(d) {
    console.log(d.ChildProductList);
    var trs = '';
    $.each($(d.ChildProductList), function (key, value) {

        var brandsHTML = "";
        var colorHTML = "";
        var unitHTML = "";
        var sizeHTML = "";

        if (value.schemaProprtiesModel) {
            brandsHTML = GeneratePropertyList(value.schemaProprtiesModel.ProductBrands, "ddlBrand", value.OriginalProductID);
            colorHTML = GeneratePropertyList(value.schemaProprtiesModel.ProductColors, "ddlColor", value.OriginalProductID);
            unitHTML = GeneratePropertyList(value.schemaProprtiesModel.ProductUnits, "ddlUnit", value.OriginalProductID);
            sizeHTML = GeneratePropertyList(value.schemaProprtiesModel.ProductSizes, "ddlSize", value.OriginalProductID);
        }

        trs +=
            '<tr class="skuRow" data-for="' + d.AliExpressProductID + '"><td>' + value.Title +
            '</td> <td>' + brandsHTML + value.Brand +
            '</td><td>' + unitHTML + value.NetWeight +
            '</td><td>' + colorHTML + value.Color +
            '</td><td>' + sizeHTML + value.Size +
            '</td><td>' + value.Inventory +
            '</td><td>' + "$" + value.Cost +
            '</td>' +
        '<td>' + "<input name='updatedPrice' data-isUpdated='false' onkeypress='return IsNumeric(event);' disabled data-sku='" + value.SkuID + "' data-productid='" + value.ProductID + "' type='text' value=" + value.UpdatedPrice + " class='updatedPrice txtEdit_" + value.ParentProductID + "'>" +
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
        //'<th>Picked Inventory</th>' +
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
    if (table) {
        table.destroy();
    }
    table = $('#TblPickedProduct').DataTable({
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
        { "data": "manufacturerName" },
        { "data": "origin" },
        {
            "data": "SellerPrice", "render": function (data, type, full) {
                if (!(full.ChildProductList.length > 0)) {
                    full.cost > 0 ? full.cost : 0;
                    return "<input name='updatedPrice' disabled dataSKU='" + full.ProductID + "' type='number' value=" + full.SellerPrice + " class='updatedParentPrice txtParent_" + full.ProductID + "'>";
                }
                else {
                    return "";
                }
            }
        },
        { "data": "check" },
        { "data": "IsOnline" }

        ]
    });

}

function SavePickedProducts() {
    //var heads = [];
    //$("thead").find("th").each(function () {
    //    heads.push($(this).text().trim());

    //});


    var selectedItems = global.getSelectedCheckboxList('.parentChk', 'aliexpressproductid');
    var updatedProducts = [];
    var update_price = true;
    $.each(selectedItems, function (index, item) {
        updatedProducts.push({ aliExpressProductId: item, price: $('.parentChk[aliexpressproductid=' + item + ']').parents('tr').find('.updatedParentPrice').val(), SKUModels: [] });
        var innerTableLength = $(".innertable").find("tr.skuRow[data-for='" + item + "']").length;
        if (innerTableLength > 0) {

            if ($(".innertable").find("tr.skuRow[data-for='" + item + "']")) {
                $.each($(".innertable").find("tr.skuRow[data-for='" + item + "']"), function (i, data) {
                    if ($(data).find(".updatedPrice").data("isupdated")) {
                        updatedProducts[updatedProducts.length - 1].SKUModels.push({ childproductId: $(data).find(".updatedPrice").data("productid"), skuCode: $(data).find(".updatedPrice").data("sku"), price: $(data).find(".updatedPrice").val(), discount_price: 1 });
                    }
                    else {
                        updatedProducts[updatedProducts.length - 1].SKUModels.push({ childproductId: $(data).find(".updatedPrice").data("productid"), skuCode: $(data).find(".updatedPrice").data("sku"), inventory: $(data).data("inventory"), price: $(data).find(".updatedPrice").val(), discount_price: 1 })
                    }
                });
            }
        }
        else {
            var parentItem = jsonData.filter(m => m.AliExpressProductID == item);
            if (parentItem.length > 0) {
                parentItem = parentItem[0];
                var childrens = parentItem.ChildProductList;
                for (var i = 0; i < childrens.length; i++) {
                    if (childrens[i].UpdatedPrice > 0) {
                        updatedProducts[updatedProducts.length - 1].SKUModels.push({ skuCode: childrens[i].SkuID, inventory: childrens[i].Inventory, price: childrens[i].UpdatedPrice, childproductId: childrens[i].ProductID });
                    }
                    else {
                        updatedProducts[updatedProducts.length - 1].SKUModels.push({ skuCode: childrens[i].SkuID, inventory: childrens[i].Inventory, price: childrens[i].Cost, childproductId: childrens[i].ProductID });
                    }
                }
            }
        }
    });
    $.each(updatedProducts, function (j, result) {
        $(result.SKUModels).filter(function (index, updatedData) {
            if (!(updatedData.price > 0)) {
                return update_price = false;
            }

        })
    });


    var jsondata = JSON.stringify(jsondata);
    console.log(jsondata);

    ShowLoader();

    $.ajax({
        type: "POST",
        url: "/Products/UpdatePickedProduct",
        dataType: "json",
        async: true,
        data: { products: updatedProducts },
        success: function (data, status, xhr) {
            HideLoader();
            if (data) {
                SuccessMessage("Product submitted for Update price on alieexpress");
                GetData();
            }
            else {
                ErrorMessage("Product not updated, try again");
            }
            $("#btnSave").removeAttr("disabled");
        },
        error: function (jqXhr, textStatus, errorMessage) {
            HideLoader();
            ErrorMessage("Product not updated, try again");
            console.log(errorMessage);
            $("#btnSave").removeAttr("disabled");
        }
    });
}

//function SavePickedProducts() {
//    if ($('#frmPickedProduct').valid() === false) {
//        return;
//    }

//    var parentCheckboxes = $('.parentChk');
//    var activeCheckBoxes = [];
//    var alltxtInventory = $('.pickedInvetory');
//    var alltxtPrice = $('.updatedPrice');

//    var allDdlBrands = $('.ddlBrand');
//    var allDdlUnits = $('.ddlUnit');
//    var allDdlSizes = $('.ddlSize');
//    var allDdlColors = $('.ddlColor');

//    var activeInvetoryList = [];
//    var activePriceList = [];

//    var activeBrandList = [];
//    var activeUnitList = [];
//    var activeSizeList = [];
//    var activeColorList = [];


//    if (alltxtInventory && alltxtInventory.length > 0) {
//        for (var i = 0; i < alltxtInventory.length; i++) {
//            if (alltxtInventory[i].disabled === false) {
//                activeInvetoryList.push(alltxtInventory[i]);
//                activePriceList.push(alltxtPrice[i]);

//                activeBrandList.push(allDdlBrands[i]);
//                activeColorList.push(allDdlColors[i]);
//                activeSizeList.push(allDdlSizes[i]);
//                activeUnitList.push(allDdlUnits[i]);
//            }
//        }

//        var UpdatedModels = [];
//        if (activeInvetoryList.length > 0) {
//            for (var j = 0; j < activeInvetoryList.length; j++) {
//                var SKU = $(activeInvetoryList[j]).attr('datasku');
//                //var parentSKU = $(activeInvetoryList[j]).attr('datapsku');
//                var pickedInventory = activeInvetoryList[j].value;
//                var updatedPrice = activePriceList[j].value;

//                var updatedBrand = activeBrandList[j].value;
//                var updatedColor = activeColorList[j].value;
//                var updatedSize = activeSizeList[j].value;
//                var updatedUnit = activeUnitList[j].value;
//                var UpdateProductModel = {
//                    "SKU": SKU,
//                    //"ParentSKU": parentSKU,
//                    "PickedInventory": pickedInventory,
//                    "UpdatedPrice": updatedPrice,
//                    "UpdatedBrand": updatedBrand,
//                    "UpdatedColor": updatedColor,
//                    "UpdatedUnit": updatedUnit,
//                    "UpdatedSize": updatedSize
//                };
//                UpdatedModels.push(UpdateProductModel);
//                console.log('UpdatedModels', UpdatedModels);
//            }

//            if (UpdatedModels.length > 0) {
//                $("#btnSave").attr("disabled", true);
//                ShowLoader();
//                $.ajax({
//                    type: "POST",
//                    url: "UpdatePickedProduct",
//                    dataType: "json",
//                    async: true,
//                    data: { UpdatedModels: UpdatedModels },
//                    success: function (data, status, xhr) {
//                        HideLoader();
//                        if (data) {
//                            SuccessMessage("Product updated successfully");
//                            GetData();
//                        }
//                        else {
//                            ErrorMessage("Product not updated, try again");
//                        }
//                        $("#btnSave").removeAttr("disabled");
//                    },
//                    error: function (jqXhr, textStatus, errorMessage) {
//                        HideLoader();
//                        ErrorMessage("Product not updated, try again");
//                        console.log(errorMessage);
//                        $("#btnSave").removeAttr("disabled");
//                    }
//                });
//            }
//        }
//    }
//}

function ProductFormValidate() {
    $("#frmPickedProduct").validate({
        rules: {
            pickedInvetory: {
                required: true,
            },
            updatedPrice: {
                required: true
            }
        },
        messages: {
            pickedInvetory: {
                required: ""
            },
            updatedPrice: {
                required: ""
            }
        }
    });

    $.validator.addClassRules({
        pickedInvetory: {
            lessThanEqual: this
        }
    });
}


function IsNumeric(e) {
    var keyCode = e.which ? e.which : e.keyCode
    var ret = ((keyCode >= 48 && keyCode <= 57));
    return ret;
}

$.validator.addMethod('lessThanEqual', function (value, element, param) {
    var preInvetory = element.attributes.datainvetory.value;
    return this.optional(element) || parseInt(value) <= parseInt(preInvetory);
}, "value must be less than stock inventory");


function updateStatus(AliExpressProductID, Status) {
    ShowLoader();
    $.ajax({
        type: "POST",
        url: "/Products/updateProductStatuts",
        dataType: "json",
        async: false,
        data: { id: AliExpressProductID, status: Status },
        success: function (data) {
            if (data) {
                SuccessMessage("Product Status Updated successfully");
            }
            GetData();
            HideLoader();
        },
        error: function (err) {
        }
    });
}