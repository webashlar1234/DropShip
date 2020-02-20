


$(document).ready(function () {

    $('.navbar-nav .nav-link').click(function () {
        $('.navbar-nav .nav-link').removeClass('active');
        $(this).addClass('active');
    })
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
        ignoreTouch = 30;
    $('#buttons a').click(function (e) {

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
        return false;
    });


    //container.parent().mouseenter(function () {
    //    clearInterval(run);
    //}).mouseleave(function () {
    //    run = setInterval(rotate, speed);
    //});

    AOS.init();

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
});
function resetSlides() {
    container.css({
        'left': -1 * item_width
    });
}

function rotate() {
    $('#next').click();
}








// function toggleChevron(e) {
//     $(e.target)
//         .prev('.panel-heading')
//         .find("i.indicator")
//         .toggleClass('glyphicon-chevron-down glyphicon-chevron-up');
// }
// $('#accordion').on('hidden.bs.collapse', toggleChevron);
// $('#accordion').on('shown.bs.collapse', toggleChevron);


// ------------------------------------------------------- //
// Multi Level dropdowns
// ------------------------------------------------------ //
$(function () {

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