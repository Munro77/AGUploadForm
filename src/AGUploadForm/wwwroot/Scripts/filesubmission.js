$(document).ready(function ($) {
    var $vars = $('#formsubmission\\.js').data();
    var web_app = '/';

    // We use the upload handler integrated into Backload:
    // In this example we set an objectContect (id) in the url query (or as form parameter). You can use a user id as 
    // objectContext give users only access to their own uploads. ObjectContext can also be set server side (See Custom Data Provider Demo, 2.2+).
    var url = web_app + 'Backload/FileHandler?objectContext=' + $vars.objectContext;


    // Initialize the jQuery File Upload widget:
    var up = $('#fileupload');
    $('#fileupload').fileupload({
        url: url,
        acceptFileTypes: /(jpg)|(jpeg)|(png)|(gif)|(tif)|(pdf)$/i              // Allowed file types
    });

    // Load existing files:
    $('#fileupload').addClass('fileupload-processing');
    $.ajax({
        // Uncomment the following to send cross-domain cookies:
        // xhrFields: {withCredentials: true},
        url: url,
        dataType: 'json',
        context: $('#fileupload')[0]
    }).always(function () {
        $(this).removeClass('fileupload-processing');
    }).done(function (result) {
        $(this).fileupload('option', 'done')
            .call(this, $.Event('done'), { result: result });
    });

    $("#SelectedOfficeName").change(function () {
        $("#SelectedDepartmentName").empty();
        $("#SelectedDepartmentName").append('<option value="">Select One</option>').prop('disabled', false);
        if ($("#SelectedOfficeName").val() != "") {
            $.ajax({
                type: 'POST',
                url: $vars.getDepartmentListUrl,
                dataType: 'json',
                data: { officeName: $("#SelectedOfficeName").val() },
                success: function (departments) {
                    $.each(departments, function (i, department) {
                        $("#SelectedDepartmentName").append('<option value="' + department.value + '">' + department.text + '</option>');
                    });
                },
                error: function (ex) {
                    alert('Failed to retrieve departments.' + ex);
                }
            });
        }
        else {
            $("#SelectedDepartmentName").empty();
            $("#SelectedDepartmentName").append('<option value="">Choose a Location</option>').prop('disabled', true);
        }
        return false;
    });
    $('input[type="reset"]').click(function () {
        $("#SelectedOfficeName").val("");
        $("#SelectedOfficeName").trigger('change');
    });
    $("#SubmitButton").click(function () {
        var index = 0;
        $("#fileupload table > tbody.files > tr.template-download > td > span > a").each(function () {
            $("#SubmitForm").prepend("<input type='hidden' name='UploadedFilenames[" + index + "]' value='" + $(this).prop('title') + "' />");
            index++;
        });
        $("#SubmitForm").submit();
    });

    $("#SelectedDepartmentName").prop('disabled', ($("#SelectedOfficeName").val() == ""));
});
