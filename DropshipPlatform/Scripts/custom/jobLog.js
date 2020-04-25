var jobLogDT = null;
var jobLog = {
    init: function () {
        jobLog.initCategoryTable();
    },
    change: function () {

    },

    initCategoryTable: function () {
        jobLogDT = $('#jobLogDT').DataTable({
            ajax: {
                "url": "/AliExpress/getJobLogData",
                "type": "POST",
                "datatype": "json",
            },
            destroy: true,
            processing: true,
            serverSide: true,
            sort: true,
            filter: false,
            "order": [[3, "desc"]],
            language: {
                loadingRecords: '&nbsp;',
                processing: '<div class="spinner"></div>'
            },
            columnDefs: [
                {
                    targets: 0,
                    sortable: false,
                    width: "5%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 1,
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, full) {
                        return data;
                    }
                },
                {
                    targets: 2,
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, row) {
                        if (row.ErrorType == "system") {
                            return "ErrorID: " + row.Id;
                        }
                        else {
                            if (data != "null") {
                                if (type === 'display') {
                                    data = strtrunc(data, 50);
                                }
                                return data;
                            }
                            else {
                                return data;
                            }
                        }
                    }
                },
                {
                    targets: 3,
                    sortable: true,
                    width: "5%",
                    "render": function (data, type, full) {
                        if (data == null) {
                            return "";
                        }
                        else {
                            var pattern = /Date\(([^)]+)\)/;
                            var results = pattern.exec(data);
                            var dt = new Date(parseFloat(results[1]));
                            var day = dt.getDate() > 9 ? dt.getDate() : "0" + dt.getDate();
                            return dt.getFullYear() + "/" + (dt.getMonth() + 1) + "/" + day + " " + dt.getHours() + ":" + dt.getMinutes() + ":" + dt.getSeconds();
                        }
                    }
                },
                {
                    targets: 4,
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, row) {
                        if (row.ErrorType == "system") {
                            return '<a class="btn btn-info btn-sm btnAction" href="#" onclick=openResultModal("' + row.JobId + '","' + row.Id + '")>' + 'Show Result' + '</a>'
                        }
                        else {
                            if (row.Result != "null") {
                                return '<a class="btn btn-info btn-sm btnAction" href="#" onclick=openResultModal("' + row.JobId + '","' + row.Id +  '")>' + 'Show Result' + '</a>' +
                                    ' <a class="btn btn-info btn-sm btnAction" href="#" onclick=updateResult("' + row.JobId + '","' + row.Id + '")>' + 'Update Result' + '</a>';
                            }
                            else {
                                return '<a class="btn btn-info btn-sm" href="#" onclick=updateResult("' + row.JobId + '","' + row.Id + '")>' + 'Update Result' + '</a>';
                            }
                        }
                    }
                },
            ],
            columns: [
                { "sTitle": 'Job ID', "mData": 'JobId', sDefaultContent: "", className: "JobId" },
                { "sTitle": 'Content ID', "mData": 'ContentId', sDefaultContent: "", className: "ContentId" },
                { "sTitle": 'Result', "mData": 'Result', sDefaultContent: "", className: "Result" },
                { "sTitle": 'Created Date', "mData": 'CreatedOn', sDefaultContent: "", className: "CreatedOn" }
            ]
        });
    }
}

$(document).ready(function () {
    jobLog.init();
});

function openResultModal(jobId,id) {
    $.ajax({
        type: 'GET',
        url: '/AliExpress/getResultByJobId',
        dataType: 'json',
        data: { jobid: jobId,id:id },
        success: function (res) {
            if (res != null) {
                $('#jobLogModal').modal("show");
                $('#jobLogModal').on("shown.bs.modal", function () {
                    if (isJson()) {
                        var jsonobj = JSON.parse(res);
                        $('.modal-body #jsonResult').html(JSON.stringify(jsonobj, undefined, 2));
                    }
                    else {
                        $('.modal-body #jsonResult').html("ErrorID: " + res);
                    }
                });
            }
        },
        error: function (err) {
        }
    });
}

function updateResult(jobId,id) {
    $('.spinner').show();
    $.ajax({
        type: 'GET',
        url: '/AliExpress/checkResultByJobId',
        dataType: 'json',
        data: { jobid: jobId ,id:id },
        success: function (res) {
            $('.spinner').hide();
            if (res != 'null') {
                SuccessMessage("Job Result Updated successfully");
                jobLogDT.clear().draw(false);
            }
            else {
                ErrorMessage("Result still not fetched, please try after some time");
            }
        },
        error: function (err) {
        }
    });
}


// Truncate a string
function strtrunc(str, max, add) {
    add = add || '...';
    return (typeof str === 'string' && str.length > max ? str.substring(0, max) + add : str);
};

//check string is json
function isJson(str) {
    try {
        JSON.parse(str);
    } catch (e) {
        return false;
    }
    return true;
}
