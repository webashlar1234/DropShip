var stripe = Stripe('pk_test_E9HeQv0TvOfPCUvf3yFBR2Kz00joNNU5qD');
var stripeCustom = {
    init: function () {
        stripeCustom.click();
    },
    click: function () {
        $(document).on('click', '.add-card-details', function () {
            $(this).hide();
            $('#stripe_card_setup_body').show();
            stripeCustom.SetUpCard();
        });
        $(document).on('click', '#cancelCardSetup', function () {
            $('#stripe_card_setup_body').hide();
            $('.add-card-details').show();
        });
        
    },
    SetUpCard: function (data) {
        $.ajax({
            type: "POST",
            url: "/Stripe/SetupCard",
            dataType: "json",
            async: false,
            success: function (data) {
                stripeCustom.setUpCardElements(data);
            }
        });
    },
    setUpCardElements: function (clientSecret) {

        var style = {
            base: {
                color: "#32325d",
                fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
                fontSmoothing: "antialiased",
                fontSize: "16px",
                "::placeholder": {
                    color: "#aab7c4"
                }
            },
            invalid: {
                color: "#fa755a",
                iconColor: "#fa755a"
            }
        };

        var elements = stripe.elements();
        var cardElement = elements.create('card', { style: style });
        cardElement.mount('#card-element');

        var cardholderName = document.getElementById('cardholder-name');
        var form = document.getElementById('payment-form');
       
        form.addEventListener('submit', function (ev) {
            ev.preventDefault();
            stripe.confirmCardSetup(
              clientSecret,
              {
                  payment_method: {
                      card: cardElement,
                      billing_details: {
                          name: cardholderName.value,
                      },
                  },
              }
            ).then(function (result) {
                if (result.error) {
                    // Display error.message in your UI.
                } else {
                    stripeCustom.AddCardToCustomer(result.setupIntent);
                }
            });
        });
    },
    AddCardToCustomer: function (data) {
        data.PaymentMethodId = data.payment_method;
        data.PaymentMethodTypes = data.payment_method_types;
        $.ajax({
            type: "POST",
            url: "/Stripe/AddCardToCustomer",
            dataType: "json",
            async: false,
            data: { intent: data },
            success: function (data) {
                SuccessMessage("Card Added successfully");
            }
        });
    }
}

$(document).ready(function () {
    stripeCustom.init();
})