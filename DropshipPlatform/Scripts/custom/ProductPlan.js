function createPlan() {
    var productPlanName = $("#TxtPlanName").val();
    var amount = $("#TxtAmount").val();
    var currency = $("#LstCurrency").val();
    var interval = $("#LstInterval").val();

    var planModel = {
        "name": productPlanName,
        "amount": amount,
        "currency": currency,
        "interval": interval
    };

    $.ajax({
        type: "POST",
        url: "/Subscriptions/createPlan",
        dataType: "json",
        async: false,
        data: { planModel: planModel },
        success: function (data) {
            if (data) {
                SuccessMessage("Plan created successfully");
                window.location.href = "/MyAccount";
            }
            else {
                ErrorMessage("Something went wrong, please try again later");
            }
        },
        error: function (e) {
            console.log(e.responseText);
        }
    });
}