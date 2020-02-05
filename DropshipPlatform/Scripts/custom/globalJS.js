var global = {
    init: function () {
        global.ajaxSetup();
    },
    ajaxSetup: function () {
        //setup ajax error handling
        $.ajaxSetup({
            error: function (x, status, error) {
                $('#LoadingModel').hide();

                //hide wait loader
                $('.btn-sending').prevAll('button').show();
                $('.btn-sending').nextAll('button').show();
                $('.btn-sending').hide();

                if (x.status == 400) {
                    ErrorMessage("Something went wrong, please try agin later");
                }
                else if (x.status == 401) {
                    window.location.href = "/Login?Msg=SessionExpired";
                }
                else if (x.status == 404) {
                    ErrorMessage("The requested page could not be found");
                }
                else if (x.status == 408) {
                    ErrorMessage("The server timed out waiting for the request");
                }
                else if (x.status == 500) {
                    ErrorMessage("Something went wrong, please try agin later");
                }
                else {
                    ErrorMessage("Something went wrong, please try agin later");
                }
            }
        });
    },
    getSelectedCheckboxList: function (item, attrid) {
        var selectedItems = [];
        $.each($(item+':checkbox:checked'), function (index, item) {
            selectedItems.push($(item).attr(attrid));
        });
        return selectedItems;
    }
}


$(document).ready(function () {
    global.init();
});