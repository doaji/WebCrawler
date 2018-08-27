$(function () {


    $('#url').keyup(function () {
        $("#crawlerForm").validate(
            {
                errorPlacement: function (error, element) {
                }
            }
        );
        if ($('#url').valid()) {
            $('#submitbtn').removeAttr('disabled');
        } else {
            $('#submitbtn').attr('disabled', 'disabled');
        }
    });
});