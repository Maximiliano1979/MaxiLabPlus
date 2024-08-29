// Namespace
var Rats = {};

Rats.UI = {};
Rats.UI.LoadAnimation = {
    "start": function (target) {
        var loader = document.getElementById("loader");
        var tmpColor = '#157FD1';
        if (loader !== null) {
            loader.style.display = "block";
            tmpColor = '#f2f2f2';
        }
        var opts = {
            lines: 13, // The number of lines to draw
            length: 32, // The length of each line
            width: 10, // The line thickness
            radius: 37, // The radius of the inner circle
            scale: 0.4, // Scales overall size of the spinner
            corners: 1, // Corner roundness (0..1)
            color: tmpColor, // CSS color or array of colors
            fadeColor: 'transparent', // CSS color or array of colors
            speed: 1.4, // Rounds per second
            rotate: 0, // The rotation offset
            animation: 'spinner-line-fade-quick', // The CSS animation name for the lines
            direction: 1, // 1: clockwise, -1: counterclockwise
            zIndex: 2e9, // The z-index (defaults to 2000000000)
            className: 'spinner', // The CSS class to assign to the spinner
            top: '45%', // Top position relative to parent
            left: '50%', // Left position relative to parent
            shadow: '0 0 1px transparent', // Box-shadow for the lines
            position: 'relative' // Element positioning / //position: 'absolute' // Element positioning
        };
        if (target === null || target === undefined) target = $("body")[0];
        return new Spinner(opts).spin(target);
    },
    "stop": function (spinner) {
        if (spinner !== null && spinner !== undefined) {
            spinner.stop();
        }
    }
};
