$(document).ready(function () {
    /* ======= BS 3 DateTimePicker od Eonasdan ======= */
    $('#datetimepicker1').datetimepicker({ locale: 'cs' });
    $('#datetimepicker2').datetimepicker({ locale: 'cs' });

    /* ======= TinyMCE ======= */
    tinymce.init({
        selector: '.tinymce',
        language: 'cs',
        plugins: ["advlist autolink lists link image charmap print preview anchor", "searchreplace visualblocks code fullscreen", "insertdatetime media table contextmenu paste"],
        toolbar: "insertfile undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image"
    });
});
