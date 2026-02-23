(function ($) {
    "use strict";


/*--
    Menu Sticky
-----------------------------------*/
var windows = $(window);
var sticky = $('.header-sticky');
    var currency = localStorage.getItem("currency");
windows.on('scroll', function() {
    var scroll = windows.scrollTop();
    if (scroll < 300) {
        sticky.removeClass('is-sticky');
    }else{
        sticky.addClass('is-sticky');
    }
});


/*--
   Sidebar Search Active
-----------------------------*/
function sidebarSearch() {
    var searchTrigger = $('.trigger-search'),
        endTriggersearch = $('button.search-close'),
        container = $('.main-search-active');

    searchTrigger.on('click', function() {
        container.addClass('inside');
    });

    endTriggersearch.on('click', function() {
        container.removeClass('inside');
    });

};
    sidebarSearch();

    /*-------- Off Canvas Feature Open close start--------*/
    $(".off-canvas-btn-Feature").on('click touchend', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $("body").addClass('fix');
        $(".off-canvas-wrapper-feature").addClass('open');
        return false;
    });

    $(document).on('click touchend', '.btn-close-off-canvas-feature,.off-canvas-overlay-feature', function (e) {
        e.preventDefault();
        e.stopPropagation();
        $("body").removeClass('fix');
        $(".off-canvas-wrapper-feature").removeClass('open');
        return false;
    });


/*-------- Off Canvas Open close start--------*/
$(".off-canvas-btn").on('click touchend', function (e) {
    e.preventDefault();
    e.stopPropagation();
    $("body").addClass('fix');
    $(".off-canvas-wrapper").addClass('open');
    return false;
});

$(".btn-close-off-canvas").on('click touchend', function (e) {
    e.preventDefault();
    e.stopPropagation();
    $("body").removeClass('fix');
    $(".off-canvas-wrapper").removeClass('open');
    return false;
});

$(".off-canvas-overlay").on('click touchend', function (e) {
    e.preventDefault();
    e.stopPropagation();
    $("body").removeClass('fix');
    $(".off-canvas-wrapper").removeClass('open');
    return false;
});

// Close menu when a regular link is clicked (but not category toggle links)
$(".off-canvas-wrapper .mobile-menu a").on('click', function() {
    var $this = $(this);
    // Only close if it's not a category toggle link (href != '#')
    if ($this.attr('href') !== '#') {
        $("body").removeClass('fix');
        $(".off-canvas-wrapper").removeClass('open');
    }
});

// Handle window resize - close menu and reset state when switching to desktop
$(window).on('resize', function() {
    var windowWidth = $(window).width();
    // Close menu if window is resized to desktop size (larger than 991px)
    if (windowWidth > 991) {
        $("body").removeClass('fix');
        $(".off-canvas-wrapper").removeClass('open');
    }
});

// Close menu when Escape key is pressed
$(document).on('keydown', function(e) {
    if (e.key === 'Escape' && $(".off-canvas-wrapper").hasClass('open')) {
        $("body").removeClass('fix');
        $(".off-canvas-wrapper").removeClass('open');
    }
});


/*------- product view mode change js start -------*/
$('.product-view-mode a').on('click', function (e) {
    e.preventDefault();
    var shopProductWrap = $('.shop-product-wrap');
    var viewMode = $(this).data('target');
    $('.product-view-mode a').removeClass('active');
    $(this).addClass('active');
    shopProductWrap.removeClass('grid-view list-view').addClass(viewMode);
})
/*------- product view mode change js end -------*/


/*------- Countdown Activation start -------*//*
$('[data-countdown]').each(function () {
    var $this = $(this),
        finalDate = $(this).data('countdown');
    $this.countdown(finalDate, function (event) {
        $this.html(event.strftime('<div class="single-countdown"><span class="single-countdown__time">%D</span><span class="single-countdown__text">Days</span></div><div class="single-countdown"><span class="single-countdown__time">%H</span><span class="single-countdown__text">Hours</span></div><div class="single-countdown"><span class="single-countdown__time">%M</span><span class="single-countdown__text">Mins</span></div><div class="single-countdown"><span class="single-countdown__time">%S</span><span class="single-countdown__text">Secs</span></div>'));
    });
});
*//*------- Countdown Activation end -------*/


// quantity change js
$('.pro-qty').prepend('<span class="dec qtybtn">-</span>');
$('.pro-qty').append('<span class="inc qtybtn">+</span>');
$('.qtybtn').on('click', function () {
    var $button = $(this);
    var oldValue = $button.parent().find('input').val();
    if ($button.hasClass('inc')) {
        var newVal = parseFloat(oldValue) + 1;
    } else {
        // Don't allow decrementing below zero
        if (oldValue > 0) {
            var newVal = parseFloat(oldValue) - 1;
        } else {
            newVal = 0;
        }
    }
    $button.parent().find('input').val(newVal);
});


/*-------- scroll to top start --------*/
$(window).on('scroll', function () {
    if ($(this).scrollTop() > 600) {
        $('.scroll-top').removeClass('not-visible');
    } else {
        $('.scroll-top').addClass('not-visible');
    }
});
$('.scroll-top').on('click', function (event) {
    $('html,body').animate({
        scrollTop: 0
    }, 1000);
});
/*-------- scroll to top end --------*/


/*------- Category Menu start -------*/
// Variables
var categoryToggleWrap = $('.category-toggle-wrap');
var categoryToggle = $('.category-toggle');
var categoryMenu = $('.category-menu');

// Category Menu Toggles
function categorySubMenuToggle() {
    var screenSize = $window.width();
    if (screenSize <= 991) {
        $('.category-menu .menu-item-has-children > a').prepend('<span class="expand menu-expand">+</span>');
        $('.category-menu .menu-item-has-children ul').slideUp();
    } else {
        $('.category-menu .menu-item-has-children > a .menu-expand').remove();
        $('.category-menu .menu-item-has-children ul').slideDown();
    }
}
// Category Sub Menu
$('.category-menu').on('click', 'li a, li a .menu-expand', function (e) {
    var $a = $(this).hasClass('menu-expand') ? $(this).parent() : $(this);
    if ($a.parent().hasClass('menu-item-has-children')) {
        if ($a.attr('href') === '#' || $(this).hasClass('menu-expand')) {
            if ($a.siblings('ul:visible').length > 0) $a.siblings('ul').slideUp();
            else {
                $(this).parents('li').siblings('li').find('ul:visible').slideUp();
                $a.siblings('ul').slideDown();
            }
        }
    }
    if ($(this).hasClass('menu-expand') || $a.attr('href') === '#') {
        e.preventDefault();
        return false;
    }
});
/*------- Category Menu end -------*/


/*------- responsive mobile menu start -------*/
//Variables
var $offCanvasNav = $('.mobile-menu'),
    $offCanvasNavSubMenu = $offCanvasNav.find('.dropdown');

//Add Toggle Button With Off Canvas Sub Menu
$offCanvasNavSubMenu.parent().prepend('<span class="menu-expand"><i></i></span>');

//Close Off Canvas Sub Menu
$offCanvasNavSubMenu.slideUp();

//Category Sub Menu Toggle
$offCanvasNav.on('click', 'li a, li .menu-expand', function(e) {
    var $this = $(this);
    if ( ($this.parent().attr('class').match(/\b(menu-item-has-children|has-children|has-sub-menu)\b/)) && ($this.attr('href') === '#' || $this.hasClass('menu-expand')) ) {
        e.preventDefault();
        if ($this.siblings('ul:visible').length){
            $this.parent('li').removeClass('active');
            $this.siblings('ul').slideUp();
        } else {
            $this.parent('li').addClass('active');
            $this.closest('li').siblings('li').removeClass('active').find('li').removeClass('active');
            $this.closest('li').siblings('li').find('ul:visible').slideUp();
            $this.siblings('ul').slideDown();
        }
    }
});

    
    /*--
        Hero Slider
    --------------------------------------------*/
    var heroSlider = $('.hero-slider-one');
    heroSlider.slick({
        arrows: true,
        autoplay: false,
        autoplaySpeed: 900,
        dots: true,
        pauseOnFocus: false,
        pauseOnHover: false,
        fade: true,
        infinite: true,
        slidesToShow: 1,
        slidesToScroll: 1,
        adaptiveHeight: true, // makes height adjust automatically
        prevArrow: '<button type="button" class="slick-prev"><i class="ion-ios-arrow-thin-left"></i></button>',
        nextArrow: '<button type="button" class="slick-next"><i class="ion-ios-arrow-thin-right"></i></button>',
        responsive: [
            {
                breakpoint: 1024, // tablets / iPads
                settings: {
                    adaptiveHeight: true,
                    arrows: false,
                    autoplay: false,
                    autoplaySpeed: 900
                }
            },
            {
                breakpoint: 768, // phones
                settings: {
                    adaptiveHeight: true,
                    arrows: false,
                    autoplay: false,
                    autoplaySpeed: 900,
                    dots: false
                }
            }
        ]
    });

/*--
    Hero Slider Two
--------------------------------------------*/
var heroSlider = $('.hero-slider-two');
heroSlider.slick({
    arrows: true,
    autoplay: false,
    autoplaySpeed: 5000,
    dots: false,
    pauseOnFocus: false,
    pauseOnHover: false,
    fade: true,
    infinite: true,
    slidesToShow: 1,
    prevArrow: '<button type="button" class="slick-prev"> <i class="ion-ios-arrow-thin-left"></i> </button>',
    nextArrow: '<button type="button" class="slick-next"><i class="ion-ios-arrow-thin-right"></i></button>',
    responsive: [
        {
          breakpoint: 767,
          settings: {
            dots: false,
          }
        }
    ]
});
/*--
    Product Slider
--------------------------------------------*/
var product_4 = $('.product-active-lg-4');
    product_4.slick({
    dots: false,
    infinite: true,
    slidesToShow: 4,
    autoplaySpeed:1500,
    slidesToScroll: 1,
    pauseOnHover: true,
    autoplay: true,
    //prevArrow: '<button type="button" class="slick-prev"> <i class="ion-ios-arrow-left"></i> </button>',
    //nextArrow: '<button type="button" class="slick-next"><i class="ion-ios-arrow-right"></i></button>',
    prevArrow: false,
    nextArrow: false,
    responsive: [
        {
            breakpoint: 1490,
            settings: {
                slidesToShow: 4,
            }
        },
        {
            breakpoint: 1199,
            settings: {
                slidesToShow: 3,
            }
        },
        {
            breakpoint: 991,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 767,
            settings: {
                slidesToShow: 1,
            }
        },
        {
            breakpoint: 480,
            settings: {
                slidesToShow: 1,
            }
        },
        {
            breakpoint: 479,
            settings: {
                slidesToShow: 1,
            }
        }
    ]
});

    var occasionSlider = $('.occasion-active');
    occasionSlider.slick({
        dots: false,
        infinite: true,
        slidesToShow: 10,   // how many circles to show
        slidesToScroll: 1,
        autoplay: true,
        autoplaySpeed: 1000,
        arrows: false,
        pauseOnHover: true,
        responsive: [
            {
                breakpoint: 1200,
                settings: { slidesToShow: 5 }
            },
            {
                breakpoint: 992,
                settings: { slidesToShow: 4 }
            },
            {
                breakpoint: 768,
                settings: { slidesToShow: 4 }
            },
            {
                breakpoint: 576,
                settings: { slidesToShow: 4 }
            }
        ]
    });

    var product_4v2 = $('.product-active-lg-4-r3');
    product_4v2.slick({
        dots: false,
        infinite: true,
        slidesToShow: 5,

        autoplaySpeed: 2000,
        slidesToScroll: 1,
        pauseOnHover: true,
        autoplay: true,
        prevArrow: '<button type="button" class="slick-prev"> <i class="ion-ios-arrow-left"></i> </button>',
        nextArrow: '<button type="button" class="slick-next"><i class="ion-ios-arrow-right"></i></button>',
        responsive: [
            {
                breakpoint: 1199,
                settings: {
                    slidesToShow: 3,
                }
            },
            {
                breakpoint: 991,
                settings: {
                    slidesToShow: 2,
                }
            },
            {
                breakpoint: 767,
                settings: {
                    slidesToShow: 2,
                }
            },
            {
                breakpoint: 480,
                settings: {
                    slidesToShow: 2,
                }
            },
            {
                breakpoint: 479,
                settings: {
                    slidesToShow: 3,
                }
            }
        ]
    });
var product_two_row_4 = $('.product-two-row-4');
product_two_row_4.slick({
    dots: false,
    infinite: true,
    rows: 3,
    slidesToShow: 4,
    slidesToScroll: 1,
    autoplaySpeed:900,
    autoplay: true,
    prevArrow: '<button type="button" class="slick-prev"> <i class="ion-ios-arrow-left"></i> </button>',
    nextArrow: '<button type="button" class="slick-next"><i class="ion-ios-arrow-right"></i></button>',
    responsive: [
        {
            breakpoint: 1199,
            settings: {
                slidesToShow: 3,
            }
        },
        {
            breakpoint: 991,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 767,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 480,
            settings: {
                slidesToShow: 2,
            }
        },
        {
            breakpoint: 479,
            settings: {
                slidesToShow: 2,
            }
        }
    ]
});
/*-- 
    Testimonial Slider 
-----------------------------*/
var testimonialSlider = $('.testimonial-slider');
testimonialSlider.slick({
    arrows: false,
    autoplay: true,
    autoplaySpeed: 7000,
    dots: true,
    pauseOnFocus: false,
    pauseOnHover: false,
    infinite: true,
    slidesToShow: 1,
    slidesToScoll: 1,
    prevArrow: '<button type="button" class="slick-prev"> <i class="ion-ios-arrow-thin-left"></i> </button>',
    nextArrow: '<button type="button" class="slick-next"><i class="ion-ios-arrow-thin-right"></i></button>'
});
   
  
/*--
    vertical-product-active
--------------------------------------*/
$('.vartical-product-active').slick({
    slidesToShow: 4,
    autoplay: false,
    vertical:true,
    verticalSwiping:true,
    slidesToScroll: 1,
    prevArrow:'<i class="ion-chevron-up arrow-prv"></i>',
    nextArrow:'<i class="ion-chevron-down arrow-next"></i>',
    button:false,
    responsive: [
        {
          breakpoint: 1024,
          settings: {
          slidesToShow: 4,
          }
        },
        { breakpoint: 991,
          settings: {
            slidesToShow: 3,
            vertical:false,
          }
        },
        {
          breakpoint: 600,
          settings: {
            slidesToShow: 3,
            vertical:false,
          }
        },
        {
          breakpoint: 480,
          settings: {
            slidesToShow: 3,
            vertical:false,
          }
        }
    ]
     
});	     
$('.vartical-product-active a').on('click', function () {
    $('.vartical-product-active a').removeClass('active');
});
   
    
/*--
    vertical-product-active
--------------------------------------*/
$('.horizantal-product-active').slick({
    slidesToShow: 4,
    autoplay: false,
    vertical:false,
    verticalSwiping:true,
    slidesToScroll: 1,
    prevArrow:'<i class="ion-chevron-left arrow-prv"></i>',
    nextArrow:'<i class="ion-chevron-right arrow-next"></i>',
    button:false,
    responsive: [
        {
          breakpoint: 1024,
          settings: {
          slidesToShow: 4,
          }
        },
        { breakpoint: 991,
          settings: {
            slidesToShow: 3,
          }
        },
        {
          breakpoint: 600,
          settings: {
            slidesToShow: 3,
          }
        },
        {
          breakpoint: 480,
          settings: {
            slidesToShow: 3,
          }
        }
    ]
     
});	     
$('.horizantal-product-active a').on('click', function () {
    $('.horizantal-product-active a').removeClass('active');
});
    
/*----------
    price-slider active
-------------------------------*/  
$( "#price-slider" ).slider({
   range: true,
   min: 0,
   max: 50000,
   values: [ 0, 50000 ],
   slide: function( event, ui ) {
       $(".min-price").val(currency + ui.values[ 0 ] );
        $( ".max-price" ).val('RS' + ui.values[ 1 ] );
     }
});
    $(".min-price").val(currency + $( "#price-slider" ).slider( "values", 0 ));   
    $(".max-price").val(currency + $( "#price-slider" ).slider( "values", 1 )); 


    $("#price-slider1").slider({
        range: true,
        min: 0,
        max: 50000,
        values: [0, 50000],
        slide: function (event, ui) {
            $(".min-price").val(currency + ui.values[0]);
            $(".max-price").val(currency + ui.values[1]);
        }
    });
    $(".min-price").val(currency + $("#price-slider1").slider("values", 0));
    $(".max-price").val(currency + $("#price-slider1").slider("values", 1));

    $("#price-slider-home").slider({
        range: true,
        min: 0,
        max: 50000,
        values: [0, 50000],
        slide: function (event, ui) {
            $("#min-price-home").val(currency + ui.values[0]);
            $("#max-price-home").val(currency + ui.values[1]);
        }
    });
    $("#min-price-home").val(currency + $("#price-slider-home").slider("values", 0));
    $("#max-price-home").val(currency + $("#price-slider-home").slider("values", 1));

/*--

    showlogin toggle function
--------------------------*/
$( '#showlogin' ).on('click', function() {
    $('#checkout-login' ).slideToggle(500);
}); 
    
/*--
    showcoupon toggle function
--------------------------*/
$( '#showcoupon' ).on('click', function() {
    $('#checkout-coupon' ).slideToggle(500);
});
    
/*--
    Checkout 
--------------------------*/
$("#chekout-box").on("change",function(){
    $(".account-create").slideToggle("100");
});
    
/*-- 
    Checkout 
---------------------------*/
$("#chekout-box-2").on("change",function(){
    $(".ship-box-info").slideToggle("100");
});    

/*--
    ScrollUp Active
-----------------------------------*/
$.scrollUp({
    scrollText: '<i class="ion-chevron-up"></i>',
    easingType: 'linear',
    scrollSpeed: 900,
    animation: 'fade'
});    
    
    
    
    
    
    
    
    
    
    
    
    
    
})(jQuery);
//var product_two_row_5 = $('.all-products-1');
//product_two_row_5.slick({
//    dots: false,
//    infinite: true,
//    rows: 4,
//    slidesToShow: 4,
//    slidesToScroll: 1,
//    autoplaySpeed: 1500,
//    autoplay: false,
//    prevArrow: false,
//    nextArrow: false,
//    responsive: [
//        {
//            breakpoint: 1199,
//            settings: {
//                slidesToShow: 4,
//            }
//        },
//        {
//            breakpoint: 991,
//            settings: {
//                slidesToShow: 2,
//            }
//        },
//        {
//            breakpoint: 767,
//            settings: {
//                slidesToShow: 2,
//            }
//        },
//        {
//            breakpoint: 480,
//            settings: {
//                slidesToShow: 2,
//            }
//        },
//        {
//            breakpoint: 479,
//            settings: {
//                slidesToShow: 2,
//            }
//        }
//    ]
//});
