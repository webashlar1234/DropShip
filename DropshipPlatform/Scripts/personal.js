$(document).ready(function () {
    //rotation speed and timer
    var speed = 3000;

    $('#example').on('click', 'tbody td:not(:first-child)', function (e) {
        editor.inline(this);
    });

    var run = setInterval(rotate, speed);
    var slides = $('.slide');
    var container = $('#slides ul');
    var elm = container.find(':first-child').prop("tagName");
    var item_width = container.width();
    var previous = 'prev'; //id of previous button
    var next = 'next'; //id of next button
    slides.width(item_width); //set the slides to the correct pixel width
    container.parent().width(item_width);
    container.width(slides.length * item_width); //set the slides container to the correct total width
    container.find(elm + ':first').before(container.find(elm + ':last'));
    resetSlides();


    //if user clicked on prev button

    $('#buttons a').click(function (e) {
        //slide the item

        if (container.is(':animated')) {
            return false;
        }
        if (e.target.id == previous) {
            container.stop().animate({
                'left': 0
            }, 1500, function () {
                container.find(elm + ':first').before(container.find(elm + ':last'));
                resetSlides();
            });
        }

        if (e.target.id == next) {
            container.stop().animate({
                'left': item_width * -2
            }, 1500, function () {
                container.find(elm + ':last').after(container.find(elm + ':first'));
                resetSlides();
            });
        }

        //cancel the link behavior            
        return false;

    });

    //if mouse hover, pause the auto rotation, otherwise rotate it    
    container.parent().mouseenter(function () {
        clearInterval(run);
    }).mouseleave(function () {
        run = setInterval(rotate, speed);
    });


    function resetSlides() {
        //and adjust the container so current is in the frame
        container.css({
            'left': -1 * item_width
        });
    }


    var myTable = $('#example').DataTable({
        responsive: true
    });
    $('#example').on('click', 'tbody td', function () {
        myTable.cell(this).edit('bubble');
    });

    var JsonData = {

        "data": [{
            "title": "Grace Hopper",
            "category": "Shirt",
            "cost": "20",
            "inventory": "12",
            "shippingweight": "20 KG",
            "status": "Online",
            "updateprice": "Update",
            "yourprice": "<input type=\"text\" name=\"price\">",
            "check": "<input type=\"checkbox\" name=\"price\"> ",
            "product": [
                "product1", "product2", "product1", "product2"
            ],
            "price": [
                "$5", "$5", "$5", "$5"
            ],
            "size": [
                "120 cm", "120 cm", "120 cm", "120 cm"
            ],
            "weight": [
                "55 kg", "55 kg", "55 kg", "55 kg"
            ],
            "color": [
                "red", "red", "red", "red"
            ],

            "checkin": [
                "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">"
            ],
        },
        {
            "title": "Grace Hopper",
            "category": "Shirt",
            "cost": "20",
            "inventory": "12",
            "shippingweight": "20 KG",
            "status": "Online",
            "updateprice": "Update",
            "yourprice": "<input type=\"text\" name=\"price\">",
            "check": "<input type=\"checkbox\" name=\"price\"> ",
            "product": [
                "product1", "product2", "product1", "product2"
            ],
            "price": [
                "$5", "$5", "$5", "$5"
            ],
            "size": [
                "120 cm", "120 cm", "120 cm", "120 cm"
            ],
            "weight": [
                "55 kg", "55 kg", "55 kg", "55 kg"
            ],
            "color": [
                "red", "red", "red", "red"
            ],

            "checkin": [
                "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">"
            ],
        },
        {
            "title": "Grace Hopper",
            "category": "Shirt",
            "cost": "20",
            "inventory": "12",
            "shippingweight": "20 KG",
            "status": "Online",
            "updateprice": "Update",
            "yourprice": "<input type=\"text\" name=\"price\">",
            "check": "<input type=\"checkbox\" name=\"price\"> ",
            "product": [
                "product1", "product2", "product1", "product2"
            ],
            "price": [
                "$5", "$5", "$5", "$5"
            ],
            "size": [
                "120 cm", "120 cm", "120 cm", "120 cm"
            ],
            "weight": [
                "55 kg", "55 kg", "55 kg", "55 kg"
            ],
            "color": [
                "red", "red", "red", "red"
            ],

            "checkin": [
                "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">", "<input type=\"checkbox\" name=\"price\">"
            ],
        }
        ]
    }

    function format(d) {
        console.log(d.product);
        var trs = '';
        $.each($(d.product), function (key, value) {
            trs += '<tr><td>' + value + '</td><td>' + d.price[key] + '</td> <td>' + d.size[key] + '</td><td>' + d.weight[key] + '</td><td>' + d.color[key] + '</td><td>' + d.checkin[key] + '</td></tr>';
        })
        // `d` is the original data object for the row
        return '<table class="table table-border table-hover">' +
            '<thead>' +
            '<th>Product</th>' +
            '<th>Price</th>' +
            '<th>Size</th>' +
            '<th>Weight</th>' +
            '<th>Color</th>' +
            '<th>Check</th>' +
            '</thead><tbody>' +

            trs +
            '</tbody></table>';
    }



    var table = $('#examplechild').DataTable({
        //  "ajax": 'https://raw.githubusercontent.com/kshkrao3/JsonFileSample/master/Employees.json',
        "data": JsonData.data,
        "columns": [{
            "class": 'details-control',
            "orderable": false,
            "data": null,
            "defaultContent": ''
        },
        { "data": "title" },
        { "data": "category" },
        { "data": "cost" },
        { "data": "inventory" },
        { "data": "shippingweight" },
        { "data": "yourprice" },
        { "data": "status" },
        { "data": "updateprice" },
        { "data": "check" },

        ]
    });

    // Add event listener for opening and closing details
    $('#examplechild tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        } else {
            // Open this row
            row.child(format(row.data())).show();
            tr.addClass('shown');
        }
    });

});
//a simple function to click next link
//a timer will call this function, and the rotation will begin

function rotate() {
    $('#next').click();
}

//$(window).scroll(function () {
//    var sticky = $('.menu-header'),
//        scroll = $(window).scrollTop();

//    if (scroll >= 50) sticky.addClass('nav-fixed');
//    else sticky.removeClass('nav-fixed');
//});

function toggleChevron(e) {
    $(e.target)
        .prev('.panel-heading')
        .find("i.indicator")
        .toggleClass('glyphicon-chevron-down glyphicon-chevron-up');
}
$('#accordion').on('hidden.bs.collapse', toggleChevron);
$('#accordion').on('shown.bs.collapse', toggleChevron);


// vars
'use strict'
var testim = document.getElementById("testim"),
    testimDots = Array.prototype.slice.call(document.getElementById("testim-dots").children),
    testimContent = Array.prototype.slice.call(document.getElementById("testim-content").children),
    testimLeftArrow = document.getElementById("left-arrow"),
    testimRightArrow = document.getElementById("right-arrow"),
    testimSpeed = 5000,
    currentSlide = 0,
    currentActive = 0,
    testimTimer,
    touchStartPos,
    touchEndPos,
    touchPosDiff,
    ignoreTouch = 30;;

window.onload = function () {

    // Testim Script
    function playSlide(slide) {
        for (var k = 0; k < testimDots.length; k++) {
            testimContent[k].classList.remove("active");
            testimContent[k].classList.remove("inactive");
            testimDots[k].classList.remove("active");
        }

        if (slide < 0) {
            slide = currentSlide = testimContent.length - 1;
        }

        if (slide > testimContent.length - 1) {
            slide = currentSlide = 0;
        }

        if (currentActive != currentSlide) {
            testimContent[currentActive].classList.add("inactive");
        }
        testimContent[slide].classList.add("active");
        testimDots[slide].classList.add("active");

        currentActive = currentSlide;

        clearTimeout(testimTimer);
        testimTimer = setTimeout(function () {
            playSlide(currentSlide += 1);
        }, testimSpeed)
    }

    testimLeftArrow.addEventListener("click", function () {
        playSlide(currentSlide -= 1);
    })

    testimRightArrow.addEventListener("click", function () {
        playSlide(currentSlide += 1);
    })

    for (var l = 0; l < testimDots.length; l++) {
        testimDots[l].addEventListener("click", function () {
            playSlide(currentSlide = testimDots.indexOf(this));
        })
    }

    playSlide(currentSlide);

    // keyboard shortcuts
    document.addEventListener("keyup", function (e) {
        switch (e.keyCode) {
            case 37:
                testimLeftArrow.click();
                break;

            case 39:
                testimRightArrow.click();
                break;

            case 39:
                testimRightArrow.click();
                break;

            default:
                break;
        }
    })

    testim.addEventListener("touchstart", function (e) {
        touchStartPos = e.changedTouches[0].clientX;
    })

    testim.addEventListener("touchend", function (e) {
        touchEndPos = e.changedTouches[0].clientX;

        touchPosDiff = touchStartPos - touchEndPos;

        console.log(touchPosDiff);
        console.log(touchStartPos);
        console.log(touchEndPos);


        if (touchPosDiff > 0 + ignoreTouch) {
            testimLeftArrow.click();
        } else if (touchPosDiff < 0 - ignoreTouch) {
            testimRightArrow.click();
        } else {
            return;
        }

    })
}


$(function () {
    // ------------------------------------------------------- //
    // Multi Level dropdowns
    // ------------------------------------------------------ //
    $("ul.dropdown-menu [data-toggle='dropdown']").on("click", function (event) {
        event.preventDefault();
        event.stopPropagation();

        $(this).siblings().toggleClass("show");


        if (!$(this).next().hasClass('show')) {
            $(this).parents('.dropdown-menu').first().find('.show').removeClass("show");
        }
        $(this).parents('li.nav-item.dropdown.show').on('hidden.bs.dropdown', function (e) {
            $('.dropdown-submenu .show').removeClass("show");
        });

    });
});