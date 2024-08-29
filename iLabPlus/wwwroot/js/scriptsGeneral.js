
// check if browser support HTML5 local storage
function localStorageSupport() {
    return (('localStorage' in window) && window['localStorage'] !== null)
}


$(document).ready(function () {

    // load a locale
    numeral.register('locale', 'es', {
        delimiters: {
            thousands: '.',
            decimal: ','
        },
        abbreviations: {
            thousand: 'k',
            million: 'm',
            billion: 'b',
            trillion: 't'
        },
        ordinal: function (number) {
            return number === 1 ? 'er' : 'ème';
        },
        currency: {
            symbol: '€'
        }
    });
    numeral.locale('es');


    // Fast fix bor position issue with Propper.js
    // Will be fixed in Bootstrap 4.1 - https://github.com/twbs/bootstrap/pull/24092
    //Popper.Defaults.modifiers.computeStyle.gpuAcceleration = false;


    // Add body-small class if window less than 768px
    if (window.innerWidth < 769) {
        $('body').addClass('body-small')
    } else {
        $('body').removeClass('body-small')
    }


    // MetisMenu
    var sideMenu = $('#side-menu').metisMenu();

    // Collapse ibox function
    $('.collapse-link').on('click', function (e) {
        e.preventDefault();
        var ibox = $(this).closest('div.ibox');
        var button = $(this).find('i');
        var content = ibox.children('.ibox-content');
        content.slideToggle(200);
        button.toggleClass('fa-chevron-up').toggleClass('fa-chevron-down');
        ibox.toggleClass('').toggleClass('border-bottom');
        setTimeout(function () {
            ibox.resize();
            ibox.find('[id^=map-]').resize();
        }, 50);
    });

    // Close ibox function
    $('.close-link').on('click', function (e) {
        e.preventDefault();
        var content = $(this).closest('div.ibox');
        content.remove();
    });

    // Fullscreen ibox function
    $('.fullscreen-link').on('click', function (e) {
        e.preventDefault();
        var ibox = $(this).closest('div.ibox');
        var button = $(this).find('i');
        $('body').toggleClass('fullscreen-ibox-mode');
        button.toggleClass('fa-expand').toggleClass('fa-compress');
        ibox.toggleClass('fullscreen');
        setTimeout(function () {
            $(window).trigger('resize');
        }, 100);
    });

    // Close menu in canvas mode
    $('.close-canvas-menu').on('click', function (e) {
        e.preventDefault();
        ALERT();
        $("body").toggleClass("mini-navbar");
        SmoothlyMenu();
    });

    // Run menu of canvas
    $('body.canvas-menu .sidebar-collapse').slimScroll({
        height: '100%',
        railOpacity: 0.9
    });

    // Open close right sidebar
    $('.right-sidebar-toggle').on('click', function (e) {
        e.preventDefault();
        $('#right-sidebar').toggleClass('sidebar-open');
    });

    // Initialize slimscroll for right sidebar
    $('.sidebar-container').slimScroll({
        height: '100%',
        railOpacity: 0.4,
        wheelStep: 10
    });

    // Open close small chat
    $('.open-small-chat').on('click', function (e) {
        e.preventDefault();
        $(this).children().toggleClass('fa-comments').toggleClass('fa-times');
        $('.small-chat-box').toggleClass('active');
    });

    // Initialize slimscroll for small chat
    $('.small-chat-box .content').slimScroll({
        height: '234px',
        railOpacity: 0.4
    });

    // Small todo handler
    $('.check-link').on('click', function () {
        var button = $(this).find('i');
        var label = $(this).next('span');
        button.toggleClass('fa-check-square').toggleClass('fa-square-o');
        label.toggleClass('todo-completed');
        return false;
    });



    // XBS Minimalize menu
    $('.navbar-minimalize').on('click', function (event) {

        event.preventDefault();
        if ($("#MenuIzq").hasClass('MenuIzqCollapse')) {
            $("#MenuIzq").toggleClass("MenuIzqCollapse");
            $('[data-bs-toggle="tooltip"]').tooltip('disable');

            Status_SlimScrollBar();
        } else {
            $("#MenuIzq").toggleClass("MenuIzqCollapse");
            $('[data-bs-toggle="tooltip"]').tooltip('enable');
        }

    });

    $("body").tooltip({ selector: '[data-bs-toggle=tooltip]' });

    $("[data-bs-toggle=popover]").popover();

    // Move right sidebar top after scroll
    $(window).scroll(function () {
        if ($(window).scrollTop() > 0 && !$('body').hasClass('fixed-nav')) {
            $('#right-sidebar').addClass('sidebar-top');
        } else {
            $('#right-sidebar').removeClass('sidebar-top');
        }
    });

    // Add slimscroll to element
    $('.full-height-scroll').slimscroll({
        height: '100%'
    })


    // XBS
    $('.SubMenuAction').on('click', function (e) {
        e.stopPropagation();
    });

    $('.submenu:has(.active)').addClass('MenuGroupOpen');
    $('.metismenu li:has(.active)').addClass('SubMenuOpen');


    // ABRIR SUBMENU - CARGA INICIAL - PARA QUE AL ARRANCAR ESTE TODO ABIERTO
    $('.metismenu li ul').show();
    $('.SubMenuArrow .fa-angle-left').removeClass('smo');
    $('.metismenu li:has(ul)').addClass('SubMenuOpen');
    $('.metismenu li:has(ul)').find(".fa-angle-left").addClass('smo');
    // ***************************************************************************/


    // ABRIR PRIMER ELEMENTO DEL MENU
    //var Obj1 = document.getElementById('side-menuV').getElementsByTagName('li');
    //$(Obj1[0]).addClass('SubMenuOpen');
    //$(Obj1[0]).find(".fa-angle-left").addClass('smo');
    //var Obj2 = Obj1[0].getElementsByTagName('ul');
    //$(Obj2[0]).css("display", "block");
    // ***************************************************************************/

    //string currentAction = ViewContext.RouteData.GetRequiredString("action");
    //string currentController = (string)html.ViewContext.RouteData.Values["controller"];
    //var path = window.location.pathname;
    //var abc = path.split("/");
    //var controller = abc[0];
    //var action = abc[1] || "index";

    //console.log(controller + "    " + action);

    $('.metismenu .Menu').on('click', function (e) {

        e.preventDefault();

        //console.log($(this).hasClass('SubMenuOpen'));

        if ($(this).hasClass('SubMenuOpen')) {
            // CERRAR SUBMENU

            //$('.active').removeClass('active');


            //$(this).next().removeClass('SubMenuOpen');

            //$(this).removeClass('SubMenuOpen');

            //$(this).children('ul').slideUp(100, function () {
            //    Status_SlimScrollBar();
            //    $('.MenuGroupOpen').removeClass('MenuGroupOpen');
            //});

            $(this).removeClass('SubMenuOpen');

            $(this).next().css("display", "none");

            $(this).find(".fa-angle-left").removeClass('smo');


        } else {
            // ABRIR SUBMENU
            //$('.metismenu li ul').slideUp();
            //$('.metismenu li').removeClass('SubMenuOpen');
            //$('.Menu').removeClass('SubMenuOpen');
            //$('.SubMenuArrow .fa-angle-left').removeClass('smo');

      
            $(this).addClass('SubMenuOpen');

            $(this).next().css("display", "block");
            $(this).find(".fa-angle-left").addClass('smo');

            Status_SlimScrollBar();
            //console.log($(this).children('ul')[0]);
            //console.log($(this).children('ul').parent());

            //var xxx = $(this).children('ul')[0]
            //$(xxx).css("display", "block");
            //$(this).children('ul').parent().slideDown(100, function () {
            //    Status_SlimScrollBar();
            //});

            //$(this).children('ul').slideDown(100, function () {
            //    //Status_SlimScrollBar();
            //});
            //$(this).find(".fa-angle-left").addClass('smo');
        }

    });




    function Status_SlimScrollBar() {

        if ($("#MenuIzq").height() > $(".metismenu").height()) {
            $('.slimScrollBar').hide();
        } else {
            $('.sidebar-collapse').slimScroll();
        }
    }


    var autocollapse = function (menu, maxHeight) {

        var nav = $(menu);
        var navHeight = nav.innerHeight();

        //console.log("INI: " + navHeight + "   " + maxHeight);

        if (navHeight >= maxHeight) {
            /*$(menu + ' .dropdown').removeClass('d-none');*/
            $("#MenuSupCollapse").removeClass('d-none');
            

            while (navHeight > maxHeight) {
                var children = nav.children(menu + ' li:not(:last-child)');
                //console.log(children);
                //console.log(children.length);

                var count = children.length;
                if (count != 0) {
                    //$(children[count - 1]).prependTo(menu + ' .dropdown-menu');
                    $(children[count - 1]).prependTo(menu + ' .MenuSupCollapsedropdownmenu');
                    
                    
                    //console.log($(children[count - 1]));

                    navHeight = nav.innerHeight();
                    //console.log("New navHeight:   " + navHeight);
                } else {
                    // NO se encuentra children
                    //console.log("NO se encuentra children BREAK");
                    break;
                }
            }
        }
        else {
            //var collapsed = $(menu + ' .dropdown-menu').children(menu + ' li');
            var collapsed = $(menu + ' .MenuSupCollapsedropdownmenu').children(menu + ' li');
            //console.log(collapsed);

            if (collapsed.length === 0) {
                //$(menu + ' .dropdown').addClass('d-none');
                $("#MenuSupCollapse").addClass('d-none');
            }

            while (navHeight < maxHeight && (nav.children(menu + ' li').length > 0) && collapsed.length > 0) {
                //collapsed = $(menu + ' .dropdown-menu').children('li');
                collapsed = $(menu + ' .MenuSupCollapsedropdownmenu').children('li');
                $(collapsed[0]).insertBefore(nav.children(menu + ' li:last-child'));
                navHeight = nav.innerHeight();
            }

            if (navHeight > maxHeight) {
                autocollapse(menu, maxHeight);
            }
        }
    };

    /* RESIZE GRIDS */

    var $wn = $(window);
 
    function GridsFitFull() {
        var Objects = $(".flexgrid");
        for (var Obj of Objects) {
            //console.log(Obj);
            //console.log(Obj.id);

            //var ObjOuterHeight = $(Obj).outerHeight();
            //console.log("ObjOuterHeight: " + ObjOuterHeight);

            // get parent 
            var ObjParent = $(Obj).parent();
            //console.log(ObjParent.outerHeight());



            if (Obj.id != "gridArticuloCOMP" && Obj.id != "gridArticuloPIE" && Obj.id != "gridArticuloMO" && Obj.id != "gridArticuloMET") {

                if (Obj.id == "gridArticuloCOMP" || Obj.id == "gridArticuloPIE" || Obj.id == "gridArticuloMO") {
                    $(Obj).css("max-height",    ObjParent.outerHeight() - 50 + "px");
                    $(Obj).css("height",        ObjParent.outerHeight() - 50 + "px");
                } else {
                    $(Obj).css("max-height",    ObjParent.outerHeight() - 10 + "px");
                    $(Obj).css("height",        ObjParent.outerHeight() - 10 + "px");
                }
            }




        }
    }

    GridsFitFull();
    autocollapse('#nav', 50); 
    //$wn.on('resize', GridsFitFull);

    $wn.on('resize', function () {
        GridsFitFull();
        autocollapse('#nav', 50);
    });




    $('.InputDecimalMask').inputmask("decimal", {
        radixPoint: ",",
        autoGroup: true,
    });


    if (localStorageSupport()) {

        var collapse = localStorage.getItem("collapse_menu");
        var fixedsidebar = localStorage.getItem("fixedsidebar");
        var fixednavbar = localStorage.getItem("fixednavbar");
        var boxedlayout = localStorage.getItem("boxedlayout");
        var fixedfooter = localStorage.getItem("fixedfooter");

        var body = $('body');

        if (fixedsidebar == 'on') {
            body.addClass('fixed-sidebar');
            $('.sidebar-collapse').slimScroll({
                height: '100%',
                railOpacity: 0.9
            });
        }

        if (collapse == 'on') {
            if (body.hasClass('fixed-sidebar')) {
                if (!body.hasClass('body-small')) {
                    body.addClass('mini-navbar');
                }
            } else {
                if (!body.hasClass('body-small')) {
                    body.addClass('mini-navbar');
                }

            }
        }

        if (fixednavbar == 'on') {
            $(".navbar-static-top").removeClass('navbar-static-top').addClass('navbar-fixed-top');
            body.addClass('fixed-nav');
        }

        if (boxedlayout == 'on') {
            body.addClass('boxed-layout');
        }

        if (fixedfooter == 'on') {
            $(".footer").addClass('fixed');
        }
    }

});

// Minimalize menu when screen is less than 768px
$(window).bind("resize", function () {
    if (window.innerWidth < 769) {
        $('body').addClass('body-small')
    } else {
        $('body').removeClass('body-small')
    }
});

// Fixed Sidebar
$(window).bind("load", function () {
    if ($("body").hasClass('fixed-sidebar')) {
        $('.sidebar-collapse').slimScroll({
            height: '100%',
            railOpacity: 0.9
        });
    }
});


function animationHover(element, animation) {
    element = $(element);
    element.hover(
        function () {
            element.addClass('animated ' + animation);
        },
        function () {
            //wait for animation to finish before removing classes
            window.setTimeout(function () {
                element.removeClass('animated ' + animation);
            }, 2000);
        });
}

function SmoothlyMenu() {
    if (!$('body').hasClass('mini-navbar') || $('body').hasClass('body-small')) {
        // Hide menu in order to smoothly turn on when maximize menu
        $('#side-menu').hide();
        // For smoothly turn on menu
        setTimeout(
            function () {
                $('#side-menu').fadeIn(400);
            }, 200);
    } else if ($('body').hasClass('fixed-sidebar')) {
        $('#side-menu').hide();
        setTimeout(
            function () {
                $('#side-menu').fadeIn(400);
            }, 100);
    } else {
        // Remove all inline style from jquery fadeIn function to reset menu state
        $('#side-menu').removeAttr('style');
    }
}

// Dragable panels
function WinMove() {
    var element = "[class*=col]";
    var handle = ".ibox-title";
    var connect = "[class*=col]";
    $(element).sortable(
        {
            handle: handle,
            connectWith: connect,
            tolerance: 'pointer',
            forcePlaceholderSize: true,
            opacity: 0.8
        })
        .disableSelection();
}


// CONFIGURACOIN DE MENSAJES toastr
//toastr.options.positionClass = 'toast-top-full-width';
toastr.options.positionClass = 'toast-top-right';
toastr.options.positionClass = 'toast-bottom-right';
toastr.options.positionClass = 'toast-bottom-right-ILABPLUS';
toastr.options.extendedTimeOut = 0; //1000;
toastr.options.timeOut = 2000;
//toastr.options.fadeOut = 250;
//toastr.options.fadeIn = 250;
//toastr.options.progressBar = true;

function ComboSelectedIndexChanged(s, e) {
    // Provocar lostfocus
    $('.wj-form-control').blur();

};
function ComboSelectedIndexChangedMulti(s, e) {
    // Provocar lostfocus
    $('.wj-form-control').blur();

    setTimeout(function () {
        s.isDroppedDown = true;
        $('#ArtMedidasAnillo').find('.wj-form-control').focus();
    }, 100);


};
function ComboSelectedIndexChangedFechas(s, e) {
    // Provocar lostfocus
    //$('.wj-form-control').blur();
    //$('.wj-input-group').blur();

    setTimeout(function () {
        $('.wj-form-control').blur();
    }, 5);


};
function ComboGotFocus(s, e) {
    // Abrir popup
    /*    console.log(s);*/
    s.isDroppedDown = true;
};
function IsDroppedDownChanged(s, e) {
    // Provocar lostfocus

    //console.log(s);
    //console.log(s._oldText);

    if (s.iLabData != s._oldText && s._oldText != "") {
        s.iLabData = s._oldText;
        $('.wj-form-control').blur();
        //console.log("xxxxxx");
    }

};

/***************************************************************************
    GESTION DE COOKIES
***************************************************************************/
function getCookie(name) {
    let matches = document.cookie.match(new RegExp(
        "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
}

function setCookie(name, value, options = {}) {

    let updatedCookie = encodeURIComponent(name) + "=" + encodeURIComponent(value);

    //options = {
    //    path: '/',
    //    // add other defaults here if necessary
    //    ...options
    //};

    //if (options.expires instanceof Date) {
    //    options.expires = options.expires.toUTCString();
    //}

    //let updatedCookie = encodeURIComponent(name) + "=" + encodeURIComponent(value);

    //for (let optionKey in options) {
    //    updatedCookie += "; " + optionKey;
    //    let optionValue = options[optionKey];
    //    if (optionValue !== true) {
    //        updatedCookie += "=" + optionValue;
    //    }
    //}

    document.cookie = updatedCookie;
}

function deleteCookie(name) {
    setCookie(name, "", {
        'max-age': -1
    })
}

/***************************************************************************/
/***************************************************************************/

var LimitNumbers = function (e) {
    var Valor = e.value;
    var DelimiterDecimal = ",";
    var ValorArr = Valor.split(DelimiterDecimal);

    if (ValorArr[0].length > 0) {
        if (ValorArr[0].length > 11) {
            if (ValorArr.length > 1) {
                e.value = ValorArr[0].substring(0, ValorArr[0].length - 1) + DelimiterDecimal + ValorArr[1];
            } else {
                e.value = ValorArr[0].substring(0, ValorArr[0].length - 1);
            }
        }
    } else {
        e.value = Valor;
    }
}

function validateEmail(Email, UserSession) {

    var EmailName = Email.substring(0, Email.lastIndexOf("@"));
    var EmailDomain = Email.substring(Email.lastIndexOf("@") + 1);

    var UserSessionEmailDomain = UserSession.substring(UserSession.lastIndexOf("@") + 1);


    if (EmailDomain == UserSessionEmailDomain) {
        return true;
    } else {
        return false;
    }


}


function formatearFecha(fecha) {
    // Aca me aseguro de que solo estoy trabajando con la parte de la fecha y sin decimales al final
    fecha = fecha.substring(0, 10);

    // Divido la fecha en sus componentes (año, mes, día)
    var partes = fecha.split('-');
    if (partes.length === 3) {
        // Reordena los componentes en el formato 'dd/mm/yyyy'
        return partes[2] + '/' + partes[1] + '/' + partes[0];
    }
    return fecha;
}



function validarHorarios() {
    var horaEntrada = document.getElementById('HoraEntrada').value;
    var horaSalida = document.getElementById('HoraSalida').value;
    var errorHoraEntrada = document.getElementById('errorHoraEntrada');
    var errorHoraSalida = document.getElementById('errorHoraSalida');

    var entrada = horaEntrada ? new Date('1970-01-01T' + horaEntrada) : null;
    var salida = horaSalida ? new Date('1970-01-01T' + horaSalida) : null;

    if (horaEntrada && horaSalida) {
        // Convierte las horas a objetos Date para facilitar la comparación
        var entrada = new Date('1970-01-01T' + horaEntrada);
        var salida = new Date('1970-01-01T' + horaSalida);

        if (entrada > salida) {
            // Muestro el mensaje de error y cambia el estilo si es necesario
            errorHoraEntrada.style.display = 'block';
            return false;
        } else {
            // Oculta el mensaje de error cuando la validación es correcta
            errorHoraEntrada.style.display = 'none';
        }
    }
    return true; // Permito guardar si todo está correcto
}








