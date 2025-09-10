var selectedCopies = [];  // Array to hold selected copies (Serial + BookId) on the client side

$(document).ready(function () {
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
            $(this).parents('.js-copy-container').remove();
            prepareInput();

            if (selectedCopies.length == 0)
                $('#CopiesForm').find(':submit').addClass('d-none');
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
    
    prepareInput();
}

function prepareInput() {
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