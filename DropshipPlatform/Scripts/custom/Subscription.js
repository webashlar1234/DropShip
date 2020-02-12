var stripe;

var stripeElements = function (publicKey) {
    stripe = Stripe(publicKey);
    var elements = stripe.elements();

    // Element styles
    var style = {
        base: {
            fontSize: '16px',
            color: '#32325d',
            fontFamily:
                '-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif',
            fontSmoothing: 'antialiased',
            '::placeholder': {
                color: 'rgba(0,0,0,0.4)'
            }
        }
    };

    var card = elements.create('card', { style: style });

    card.mount('#card-element');

    card.on('focus', function () {
        var el = document.getElementById('card-element');
        el.classList.add('focused');
    });

    card.on('blur', function () {
        var el = document.getElementById('card-element');
        el.classList.remove('focused');
    });

    document.querySelector('#submit').addEventListener('click', function (evt) {
        evt.preventDefault();
        changeLoadingState(true);
        // Initiate payment
        createPaymentMethodAndCustomer(stripe, card);
    });
};

function showCardError(error) {
    changeLoadingState(false);
    // The card was declined (i.e. insufficient funds, card has expired, etc)
    var errorMsg = document.querySelector('.sr-field-error');
    errorMsg.textContent = error.message;
    setTimeout(function () {
        errorMsg.textContent = '';
    }, 8000);
}

var createPaymentMethodAndCustomer = function (stripe, card) {
    var cardholderEmail = document.querySelector('#email').value;
    stripe
        .createPaymentMethod('card', card, {
            billing_details: {
                email: cardholderEmail
            }
        })
        .then(function (result) {
            if (result.error) {
                showCardError(result.error);
            } else {
                createCustomer(result.paymentMethod.id, cardholderEmail);
            }
        });
};

async function createCustomer(paymentMethod, cardholderEmail) {

    var request =
    {
        email: cardholderEmail,
        PaymentMethod: paymentMethod
    };
    $.ajax({
        type: "POST",
        url: "/Subscriptions/CreateCustomerAsync",
        data: { "request": request },
        dataType: "json",
        async: false,
        success: function (subscription) {
            handleSubscription(subscription);
        },
        error: function (err) {
            console.log(err);
        }
    });
}

function handleSubscription(subscription) {
    if (subscription) {
        var status = subscription.Status;
        if (status === 'requires_action' || status === 'requires_payment_method') {
            stripe.confirmCardPayment(client_secret).then(function (result) {
                if (result.error) {
                    // Display error message in your UI.
                    // The card was declined (i.e. insufficient funds, card has expired, etc)
                    changeLoadingState(false);
                    showCardError(result.error);
                } else {
                    // Show a success message to your customer
                    confirmSubscription(subscription.id);
                }
            });
        } else {
            // No additional information was needed
            // Show a success message to your customer
            orderComplete(subscription);
        }
    } else {
        showCardError("Subscription error");
    }
}

function confirmSubscription(subscriptionId) {
    return fetch('/subscription', {
        method: 'post',
        headers: {
            'Content-type': 'application/json'
        },
        body: JSON.stringify({
            subscriptionId: subscriptionId
        })
    })
        .then(function (response) {
            return response.json();
        })
        .then(function (subscription) {
            orderComplete(subscription);
        });
}

function getPublicKey() {
    stripeElements("pk_test_E9HeQv0TvOfPCUvf3yFBR2Kz00joNNU5qD");
    //$.ajax({
    //    type: "GET",
    //    url: "/Subscriptions/GetPublishableKey",
    //    dataType: "json",
    //    async: false,
    //    success: function (data) {
    //        stripeElements(data);
    //    },
    //    error(err) {
    //        console.log('error', err);
    //    }
    //});
}

getPublicKey();

/* ------- Post-payment helpers ------- */

/* Shows a success / error message when the payment is complete */
var orderComplete = function (subscription) {
    changeLoadingState(false);
    var subscriptionJson = JSON.stringify(subscription, null, 2);
    document.querySelectorAll('.payment-view').forEach(function (view) {
        view.classList.add('hidden');
    });
    document.querySelectorAll('.completed-view').forEach(function (view) {
        view.classList.remove('hidden');
    });
    document.querySelector('.order-status').textContent = subscription.Status;
    //document.querySelector('#code').textContent = subscriptionJson;
    console.log('subscriptionJson', subscriptionJson);
};

// Show a spinner on subscription submission
var changeLoadingState = function (isLoading) {
    if (isLoading) {
        document.querySelector('#spinner').classList.add('loading');
        document.querySelector('button').disabled = true;

        document.querySelector('#button-text').classList.add('hidden');
    } else {
        document.querySelector('button').disabled = false;
        document.querySelector('#spinner').classList.remove('loading');
        document.querySelector('#button-text').classList.remove('hidden');
    }
};
