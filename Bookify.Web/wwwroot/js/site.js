// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
// Write your JavaScript code.
var updatedRow;
var table;
var datatable;
var exportedCols = [];
function showSuccessMessage(message = 'Saved successfully!') {
    Swal.fire({
        icon: 'success',
        title: 'Success',
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}
function showErrorMessage(message = '') {
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: "Something went wrong!",
        customClass: {
            confirmButton: "btn btn-primary"
        }
    })
}

function onModalBegin() {
    $(this).find('button[type="submit"]').prop('disabled', true).attr('data-kt-indicator','on');
    /*goal is to prevent the button that submitted from being pressed 
    again during processing(so the user can't press morthan once).
    */
}
  function onModalSuccess(row) {
        showSuccessMessage();
        $('#Modal').modal('hide');

        if (updatedRow !== undefined) {
            datatable.row(updatedRow).remove().draw();
            updatedRow = undefined;
        }

        var newRow = $(row);
        datatable.row.add(newRow).draw();

        KTMenu.init();
        KTMenu.initHandlers();
}

function onModalComplete() {
    $(this).find('button[type="submit"]').prop('disabled', false).removeAttr('data-kt-indicator');
}
//Data Tables
/*  uses jQuery with the DataTables library to make any HTML table interactive.
           It adds features like:
          -Search box
          -Column sorting
          - Pagination (show few rows per page)
          - Control over how many rows to display
            $('table') selects all tables in the page.
           .DataTable() activates the DataTables plugin on them.
 */


//Building array(exportedCols) of the indexes of all table columns that should be exported, 
//ignoring columns marked with .js - no -export.


var headers = $('th');
$.each(headers, function (i) {
    if (!$(this).hasClass('js-no-export'))
        exportedCols.push(i);
});
// Class definition
var KTDatatables = function () {
    // Private functions
    var initDatatable = function () {
        // Init datatable --- more info on datatables: https://datatables.net/manual/
        datatable = $(table).DataTable({
            "info": false,
            'pageLength': 10,
        });
    }

    // Hook export buttons
    var exportButtons = () => {
        const documentTitle = $('.js-datatables').data('document-title');
        var buttons = new $.fn.dataTable.Buttons(table, {
            buttons: [
                {
                    extend: 'copyHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'excelHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'csvHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                },
                {
                    extend: 'pdfHtml5',
                    title: documentTitle,
                    exportOptions: {
                        columns: exportedCols
                    }
                }
            ]
        }).container().appendTo($('#kt_datatable_example_buttons'));

        // Hook dropdown menu click event to datatable export buttons
        const exportButtons = document.querySelectorAll('#kt_datatable_example_export_menu [data-kt-export]');
        exportButtons.forEach(exportButton => {
            exportButton.addEventListener('click', e => {
                e.preventDefault();

                // Get clicked export value
                const exportValue = e.target.getAttribute('data-kt-export');
                const target = document.querySelector('.dt-buttons .buttons-' + exportValue);

                // Trigger click event on hidden datatable export buttons
                target.click();
            });
        });
    }

    // Search Datatable --- official docs reference: https://datatables.net/reference/api/search()
    var handleSearchDatatable = () => {
        const filterSearch = document.querySelector('[data-kt-filter="search"]');
        filterSearch.addEventListener('keyup', function (e) {
            datatable.search(e.target.value).draw();
        });
    }

    // Public methods
    return {
        init: function () {
            table = document.querySelector('.js-datatables');

            if (!table) {
                return;
            }

            initDatatable();
            exportButtons();
            handleSearchDatatable();
        }
    };
}();

$(function () {
    //Sweet Alert
    var message = $('#Message').text().trim();
    if (message !== '') {
        showSuccessMessage(message);
    }

    //DataTables
    KTUtil.onDOMContentLoaded(function () {
        KTDatatables.init();
    });

    //handel bootstrap Modal
    $('body').on('click', '.js-render-modal', function () {//When any element with the class js-toggle-status is clicked, execute the function.

        var btn = $(this);//The element that was clicked (the button) is stored in the btn variable.
        var modal = $('#Modal');//Holds the element whose ID is Modal
        modal.find('#ModalLabel').text(btn.data('title'));

        if (btn.data('update') !== undefined) {
            updatedRow = btn.parents('tr');

        }

        $.get({
            url: btn.data('url')
            , success: function (form) {
                modal.find('.modal-body').html(form);
                $.validator.unobtrusive.parse(modal);
            },
            error: function () {
                showErrorMessage();
            }
        });
        modal.modal('show');
    });

/*     jQuery Script => Its task is to change the status of Category element between 
Available and Deleted without reloading the page(AJAX). */
    //Handle Toggle Status
    $('body').on('click', '.js-toggle-status', function () {// Wait until the page is ready.
        var btn = $(this);

        bootbox.confirm({
            message: "Are you sure that you need to toggle this item status?",
            buttons: {
                confirm: {
                    label: 'Yes',
                    className: 'btn-danger'
                },
                cancel: {
                    label: 'No',
                    className: 'btn-secondary'
                }
            },
            callback: function (result) {
                if (result) {
                    $.post({//Makes a POST request to the server.
                        url: btn.data('url'),//It takes the id from the data-id in the button.
                        data: {
                            '__RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                        },
                        success: function (lastUpdatedOn) {
                            var row = btn.parents('tr');
                            var status = row.find('.js-status');
                            //It goes to the line (tr) that contains the button, and retrieves the element that has the class .js-status.
                            var newStatus = status.text().trim() === 'Deleted' ? 'Available' : 'Deleted';
                            //If the current status is Deleted, change it to Available, and vice versa.
                            status.text(newStatus).toggleClass('badge-light-success badge-light-danger');
                            row.find('.js-updated-on').html(lastUpdatedOn);
                            row.addClass('animate__animated animate__flash');

                            showSuccessMessage();
                        },
                        error: function () {
                            showErrorMessage();
                        }
                    });
                }
                //No Refresh
                //Dynamic change
            }
        });
    });
});
