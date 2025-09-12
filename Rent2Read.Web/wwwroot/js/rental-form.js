var currentCopies = [];
var selectedCopies = [];  // Array to hold selected copies (Serial + BookId) on the client side
var isEditMode = false;

$(document).ready(function () {

    if ($('.js-copy').length > 0) {
        prepareInputs();
        currentCopies = selectedCopies;
        isEditMode = true;
    }
    // Handle click on search button
    $('.js-search').on('click', function (e) {
        e.preventDefault(); // Prevent default form submission or link behavior

        var serial = $('#Value').val().trim(); // Get and trim the serial value

        // Prevent adding the same copy (by Serial)
        if (selectedCopies.find(c => c.serial === serial)) {
            showErrorMessage('You cannot add the same copy');
            return;
        }

        // Prevent exceeding the allowed number of copies
        if (selectedCopies.length >= maxAllowedCopies) {
            showErrorMessage(`You cannot add more than ${ maxAllowedCopies } ${ maxAllowedCopies === 1 ? "book" : "books"}`);
            return;
        }

        // Submit the form to search and get copy details
        $('#SearchForm').submit();
    });

    $('body').delegate('.js-remove', 'click', function () {

            var btn = $(this);
            var container = btn.parents('.js-copy-container');

        if (isEditMode) {
            btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Re-Add');// Toggle the button style and text: Remove → Re-Add
            container.find('img').css('opacity', '0.5');// Make the book image look faded
            container.find('h4').css('text-decoration', 'line-through');// Add a strike-through on the book title
            container.find('.js-copy').toggleClass('js-copy js-removed').removeAttr('name').removeAttr('id'); // Change the copy from "active" to "removed"
            // and remove 'name' and 'id' so it's not sent with the form
        }
        else {
                container.remove();
        }
       prepareInputs();

        if ($.isEmptyObject(selectedCopies) || JSON.stringify(currentCopies) == JSON.stringify(selectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    });

    $('body').delegate('.js-readd', 'click', function () {
        var btn = $(this);
        var container = btn.parents('.js-copy-container');

        btn.toggleClass('btn-light-danger btn-light-success js-remove js-readd').text('Remove');
        container.find('img').css('opacity', '1');
        container.find('h4').css('text-decoration', 'none');
        container.find('.js-removed').toggleClass('js-copy js-removed');

        prepareInputs();

        if (JSON.stringify(currentCopies) == JSON.stringify(selectedCopies))
            $('#CopiesForm').find(':submit').addClass('d-none');
        else
            $('#CopiesForm').find(':submit').removeClass('d-none');
    });
});

function onAddCopySuccess(copy) {
    $('#Value').val(''); // Clear the input field after adding a copy

    // Get the bookId from the returned copy element
    var bookId = $(copy).find('.js-copy').data('book-id');

    // Prevent adding more than one copy for the same book
    if (selectedCopies.find(c => c.bookId === bookId)) {
        showErrorMessage('You cannot add more than one copy for the same book');
        return;

    }

    // Insert the new copy at the beginning of the CopiesForm container
    $('#CopiesForm').prepend(copy);
    $('#CopiesForm').find(':submit').removeClass('d-none');
    
    prepareInputs();
}

function prepareInputs() {
    // Get all current copies from the DOM
    var copies = $('.js-copy');

    // Reset the selectedCopies array and rebuild it
    selectedCopies = [];

    $.each(copies, function (i, input) {
        var $input = $(input);

        // Add serial and bookId to the array
        selectedCopies.push({ serial: $input.val(), bookId: $input.data('book-id') });

        // Update input attributes to match array indexing for model binding
        $input.attr('name', `SelectedCopies[${i}]`).attr('id', `SelectedCopies_${i}_`);
    });
}