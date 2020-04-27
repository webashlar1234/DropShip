var keepSessionAlive = false;
var keepSessionAliveUrl = null;

var KeepAlive = {

    KeepSessionAliveTimer: function () {
        setTimeout("KeepAlive.KeepSessionAlive()", 120000);
    },

    KeepSessionAlive: function () {
        if (keepSessionAliveUrl != null) {
            $.ajax({
                type: "POST",
                url: keepSessionAliveUrl,
                success: function () { },
                error: function () { }
            });
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
