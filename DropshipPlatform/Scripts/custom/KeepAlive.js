var keepSessionAlive = false;
var keepSessionAliveUrl = null;
var response = true;

var KeepAlive = {

    KeepSessionAliveTimer: function () {
        setTimeout("KeepAlive.KeepSessionAlive()", 60000);
    },

    KeepSessionAlive: function () {
        if (keepSessionAliveUrl != null && response == true) {
            response = false;
            var req = $.ajax({
                type: "POST",
                url: keepSessionAliveUrl,
                success: function () { },
                error: function () { }
            });
            req.always(function (data, textStatus, jqXHROrErrorThrown) {
                response = true;
                console.log(data);
            })
        }
        this.KeepSessionAliveTimer();
    },

    init: function (actionUrl) {
        keepSessionAliveUrl = actionUrl;
        this.KeepSessionAliveTimer();
    }

};

$(document).ready(function () {
    KeepAlive.init('/BackendService');
});
