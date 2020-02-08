var stripe = Stripe('pk_test_E9HeQv0TvOfPCUvf3yFBR2Kz00joNNU5qD');
var CardsDT = null;
var stripeCustom = {
    init: function () {
        stripeCustom.click();
        stripeCustom.initCardsDT();
    },
    click: function () {
        $(document).on('click', '.add-card-details', function () {
            $('#stripe_card_setup_body').show();
            stripeCustom.SetUpCard();
        });
        $(document).on('click', '#cancelCardSetup', function () {
            $('#stripe_card_setup_body').hide();
        }); 
        $(document).on('click', '.deletePaymentMethod', function () {
            stripeCustom.DeleteCard($(this).attr('payment_method_id'));
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
                stripeCustom.initCardsDT();
            }
        });
    },
    DeleteCard: function (data) {
        $.ajax({
            type: "POST",
            url: "/Stripe/DeletePaymentMethod",
            dataType: "json",
            data:{paymentMethodID:data},
            async: false,
            success: function (data) {
                SuccessMessage("Card deleted successfully");
                stripeCustom.initCardsDT();
            }
        });
    },
    initCardsDT: function () {
        if (CardsDT) {
            CardsDT.destroy();
        }
        CardsDT = $('#CardsDT').DataTable({
            "ajax": {
                "url": "/Stripe/getStripePaymentMethodsList",
                "type": "GET",
                "datatype": "json"
            },
            sort: false,
            filter: false,
            paging: false,
            info: false,
            columnDefs: [
                {
                    targets: 0,
                    sortable: true,
                    width: "80%",
                    "render": function (data, type, full) {
                        return '<span class="brand">' + data.Brand + '</span>' + '<span class="card_lastdigit">...' + data.Last4 + '</span>';
                    }
                },
                {
                    targets: 1,
                    sortable: false,
                    width: "10%",
                    "render": function (data, type, full, meta) {
                        return '<a payment_method_id = '+full.Id+' class="deletePaymentMethod"><i class="fa fa-trash" aria-hidden="true"></i></a>';
                    }
                }
            ],
            columns: [
                { "title": 'Cards', "mData": 'Card', sDefaultContent: "", className: "card_row" },
                { "title": 'Action', "mData": '', sDefaultContent: "", className: "" }
            ]
        });
    }
}

$(document).ready(function () {
    stripeCustom.init();
})