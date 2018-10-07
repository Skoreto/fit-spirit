$(document).ready(function () {
    /* ======= Flexslider hlavní strana ======= */
    /* nastavení https://github.com/woothemes/FlexSlider/wiki/FlexSlider-Properties */
    $('.flexslider').flexslider({ animation: "fade", prevText: "", nextText: "" });

    /* ======= Carousely ohlasů a videí ======= */
    $('#videos-carousel').carousel({ interval: false });
    $('#testimonials-carousel').carousel({ interval: 6000, pause: "hover" });
});