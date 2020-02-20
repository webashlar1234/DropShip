var categoryDT = null;
var category = {
    init: function () {
        category.initCategoryTable();
    },
    change: function () {
        
    },

    initCategoryTable: function () {
        categoryDT = $('#categoryMappingDT').DataTable({
            ajax: {
                "url": "/Category/getCategoryDatatable",
                "type": "POST",
                "datatype": "json",
                "data": { "ddlMappingValue": $("#category-filter").val() }
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: true,
            search: {
                //search: "Search"
            },
            language: {
                sSearch: "",
                searchPlaceholder: "Search category",
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
                    $searchButton = $('<button>')
                        .text('search')
                        .click(function () {
                            self.search(input.val()).draw();
                        }),
                    $clearButton = $('<button>')
                        .text('clear')
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
                    width: "25%",
                    "render": function (data, type, full) {
                        var htmlrow = "<input type='hidden' class='categoryInfo' value='" + JSON.stringify(full) + "'>" + data;
                        htmlrow += '<div class="fullCategoryPath">(' + full.categoryFullPath + ')</div>'
                        return htmlrow;
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
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 4,
                    sortable: true,
                    width: "15%",
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
                    width: "15%",
                    "render": function (data, type, full, meta) {
                        var options = '<select title="Map Category" data-live-search="true" class="selectpicker dllAliExpressCat form-control">';
                        var selected;
                        $.each(meta.settings.json.aliCategory, function () {
                            selected = this.AliExpressCategoryID === full.AliExpressCategoryId ? "selected" : "";
                            options += '<option ' + selected + ' data-subtext="(' + this.AliCategoryFullPath + ')" value="' + this.AliExpressCategoryID + '">' + this.AliExpressCategoryName + '</option>';
                        });
                        options += '</select>';

                        return options;
                    }
                }
            ],
            columns: [
                { "title": 'Category Name', "mData": 'Name', sDefaultContent: "", className: "Name" },
                { "sTitle": 'Category Level', "mData": 'CategoryLevel', sDefaultContent: "", className: "CategoryLevel" },
                { "sTitle": 'Parent CategoryID', "mData": 'ParentCategoryID', sDefaultContent: "", className: "ParentCategoryID" },
                { "sTitle": 'IsLeaf Category', "mData": 'IsLeafCategory', sDefaultContent: "", className: "IsLeafCategory" },
                { "sTitle": 'AliExpress Category Name', "mData": 'AliExpressCategoryName', sDefaultContent: "", className: "AliExpressCategoryName" },
                { "sTitle": 'AliExpress CategoryId', "mData": 'AliExpressCategoryId', sDefaultContent: "", className: "AliExpressCategoryId" },
                { "sTitle": 'Map Category', "mData": '', sDefaultContent: "", className: "MapCat" }
            ]
        });
    }
}

$(document).ready(function () {
    category.init();
    $('#category-filter').change(function () {
        category.change();
    });
    $(document).on('change', '.dllAliExpressCat', function () {
        if ($(this).val() > 0) {
            var alicategory = $(this).find('option:selected');
            var categoryObj = $(this).parents('tr').find('.categoryInfo').val();
            $.ajax({
                type: "POST",
                url: "/Category/MapCategory",
                dataType: "json",
                async: false,
                data: { CategoryID: JSON.parse(categoryObj).CategoryID, AliExpressCategoryName: alicategory.text(), AliExpressCategoryId: $(this).val() },
                success: function (data) {
                    SuccessMessage("Category mapped successfully");
                    categoryDT.clear().draw();
                }
            });
        }
    });
});


