$(document).ready(function () {


    $('.page-scroll').click(function () {
        $('.page-scroll').tooltip('hide');
        $('body,html').animate({
            scrollTop: 0
        }, 900);
        return false;
    });

    $('.page-scroll').tooltip('show');


    function toggleChevron(e) {
        $(e.target)
            .prev('.panel-heading')
            .find("i.indicator")
            .toggleClass('glyphicon-chevron-down glyphicon-chevron-up');
    }
    $('#accordion').on('hidden.bs.collapse', toggleChevron);
    $('#accordion').on('shown.bs.collapse', toggleChevron);

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
});

$(window).scroll(function () {
    var sticky = $('.menu-header'),
        scroll = $(window).scrollTop();

    if (scroll >= 1) {

        sticky.addClass('nav-fixed');
    }
    else {
        sticky.removeClass('nav-fixed');
    }
});


