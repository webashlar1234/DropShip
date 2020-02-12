CreatePlan: function (data) {
    $.ajax({
        type: "POST",
        url: "/Stripe/AddCardToCustomer",
        dataType: "json",
        async: false,
        data: { intent: data },
        success: function (data) {
            SuccessMessage("Card Added successfully");
            stripeCustom.initCardsDT();
        }
    });
}