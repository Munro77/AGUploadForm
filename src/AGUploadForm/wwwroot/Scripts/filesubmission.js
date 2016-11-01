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
        autoUpload: true,
        url: url,
        acceptFileTypes: /(jpg)|(jpeg)|(png)|(gif)|(tif)|(pdf)$/i              // Allowed file types
    });

    $('#fileupload')
        .bind('fileuploaddone', function (e, data) {
            $('#SubmitButton').prop('disabled', ($(this).fileupload('option').getNumberOfFiles() <= 0));
        })
        .bind('fileuploaddestroyed', function (e, data) {
            $('#SubmitButton').prop('disabled', ($(this).fileupload('option').getNumberOfFiles() <= 0));
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
        $('#SubmitButton').prop('disabled', ($('#fileupload').fileupload('option').getNumberOfFiles() <= 0));
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
    $("#SubmitButton").click(function () {
        var index = 0;
        $("#fileupload table > tbody.files > tr.template-download > td > p > span").each(function () {
            $("#SubmitForm").prepend("<input type='hidden' name='UploadedFilenames[" + index + "]' value='" + $(this).prop('title') + "' />");
            index++;
        });
        $("#SubmitForm").submit();
    });

    $("#SelectedDepartmentName").prop('disabled', ($("#SelectedOfficeName").val() == ""));

    if ($vars.vip != null) {
        $.each($vars.vip.fields, function (i, field) {
            updateAGField(field.agFieldId, field.value, field.disabled, field.visible);
        });
    }
});

var isAgDisabled = function (e) {
    var attr = e.prop("data-ag-disabled");
    return (typeof attr !== typeof undefined && attr === true);
};

var resetSubmitForm = function () {
    if (!(isAgDisabled($("#SelectedOfficeName")))) {
        $("#SelectedOfficeName").val("");
        $("#SelectedOfficeName").trigger('change');
    }

    $('#SubmitForm').find('input, textarea').filter(function () { return !isAgDisabled($(this)); }).not('[readonly], [disabled], :button, :hidden').val('');
};

var updateAGField = function (agFieldId, value, disabled, visible) {
    var field = '*[data-ag-field="' + agFieldId + '"';
    $(field).val(value);
    $(field).prop('readonly', disabled);
    $(field).prop('data-ag-disabled', disabled);
    if (!visible) $(field).parents(".form-group").hide(); else $(field).parents(".form-group").show();

    //Need to trigger the change for the select boxes to work (changing to implement server side using tag helpers).
    //$(field).trigger('change');
}
