var table;
var jsonProducts;
$(document).ready(function () {
    GetData();
    ProductFormValidate();
    function GetData() {
        ShowLoader();
        $.ajax('getPickedAliProducts', {
            type: 'GET',  // http method
            data: { UserID: 1 },  // data to submit
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
                "OriginalProductID": json[i].ParentProduct.OriginalProductID,
                "ParentProductID": json[i].ParentProduct.ParentProductID,
                "check": "<label class='mt-checkbox'><input type='checkbox' class='parentChk' data-SKU=" + json[i].ParentProduct.OriginalProductID + " value='1'><span></span></label>",
                "ChildProductList": json[i].ChildProductList,
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
                '<tr><td>' + value.Title +
                '</td> <td>' + brandsHTML + value.Brand +
                '</td><td>' + unitHTML + value.NetWeight +
                '</td><td>' + colorHTML + value.Color +
                '</td><td>' + sizeHTML + value.Size +
                '</td><td>' + value.Inventory +
                '</td><td>' + "$" + value.Cost +
                '</td><td>' + "<input name='pickedInvetory' onkeypress='return IsNumeric(event);' disabled dataInvetory=" + value.Inventory + " dataSKU='" + value.OriginalProductID + "' type='text' value=" + value.UpdatedInvetory + " class='pickedInvetory txtEdit_" + value.ParentProductID + " '>" +
                '</td><td>' + "<input name='updatedPrice' onkeypress='return IsNumeric(event);' disabled dataSKU='" + value.OriginalProductID + "' type='text' value=" + value.UpdatedPrice + " class='updatedPrice txtEdit_" + value.ParentProductID + "'>" +
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
            '<th>Picked Inventory</th>' +
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
            { "data": "check" }

            ]
        });

    }
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
        }
    });

    $('#chkAllProduct').change(function () {
        var parentCheckboxes = $('.parentChk');
        ProductFormValidate();
        if (this.checked) {
            parentCheckboxes.prop('checked', true);
            $('#btnSave').prop('disabled', false);
        }
        else {
            parentCheckboxes.prop('checked', false);
            $('#btnSave').prop('disabled', true);
        }
    });

    $("#TblPickedProduct tbody").on("change", ".parentChk", function () {
        ProductFormValidate();
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
            $('#btnSave').prop('disabled', false);

        }
        else {
            $('#chkAllProduct').prop('checked', false);
            $('.txtEdit_' + parentProductID + '').prop("disabled", true);
        }
    });

    $("#TblPickedProduct tbody").on("change", ".pickedInvetory", function () {
        $("#frmPickedProduct").valid();
    });
    $("#TblPickedProduct tbody").on("change", ".updatedPrice", function () {
        $("#frmPickedProduct").valid();
    });
});

function SavePickedProducts() {
    if ($('#frmPickedProduct').valid() === false) {
        return;
    }

    var parentCheckboxes = $('.parentChk');
    var activeCheckBoxes = [];
    var alltxtInventory = $('.pickedInvetory');
    var alltxtPrice = $('.updatedPrice');

    var allDdlBrands = $('.ddlBrand');
    var allDdlUnits = $('.ddlUnit');
    var allDdlSizes = $('.ddlSize');
    var allDdlColors = $('.ddlColor');

    var activeInvetoryList = [];
    var activePriceList = [];

    var activeBrandList = [];
    var activeUnitList = [];
    var activeSizeList = [];
    var activeColorList = [];


    if (alltxtInventory && alltxtInventory.length > 0) {
        for (var i = 0; i < alltxtInventory.length; i++) {
            if (alltxtInventory[i].disabled === false) {
                activeInvetoryList.push(alltxtInventory[i]);
                activePriceList.push(alltxtPrice[i]);

                activeBrandList.push(allDdlBrands[i]);
                activeColorList.push(allDdlColors[i]);
                activeSizeList.push(allDdlSizes[i]);
                activeUnitList.push(allDdlUnits[i]);
            }
        }

        var UpdatedModels = [];
        if (activeInvetoryList.length > 0) {
            for (var j = 0; j < activeInvetoryList.length; j++) {
                var SKU = $(activeInvetoryList[j]).attr('datasku');
                //var parentSKU = $(activeInvetoryList[j]).attr('datapsku');
                var pickedInventory = activeInvetoryList[j].value;
                var updatedPrice = activePriceList[j].value;

                var updatedBrand = activeBrandList[j].value;
                var updatedColor = activeColorList[j].value;
                var updatedSize = activeSizeList[j].value;
                var updatedUnit = activeUnitList[j].value;
                var UpdateProductModel = {
                    "SKU": SKU,
                    //"ParentSKU": parentSKU,
                    "PickedInventory": pickedInventory,
                    "UpdatedPrice": updatedPrice,
                    "UpdatedBrand": updatedBrand,
                    "UpdatedColor": updatedColor,
                    "UpdatedUnit": updatedUnit,
                    "UpdatedSize": updatedSize
                };
                UpdatedModels.push(UpdateProductModel);
                console.log('UpdatedModels', UpdatedModels);
            }

            if (UpdatedModels.length > 0) {
                $("#btnSave").attr("disabled", true);
                ShowLoader();
                $.ajax({
                    type: "POST",
                    url: "UpdatePickedProduct",
                    dataType: "json",
                    async: true,
                    data: { UpdatedModels: UpdatedModels },
                    success: function (data, status, xhr) {
                        HideLoader();
                        if (data) {
                            SuccessMessage("Product updated successfully");
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
        }
    }
}

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