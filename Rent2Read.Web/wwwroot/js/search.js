$(document).ready(function () {
    var books = new Bloodhound({
        datumTokenizer: Bloodhound.tokenizers.obj.whitespace('value'),
        queryTokenizer: Bloodhound.tokenizers.whitespace,
        remote: {
            url: '/Search/Find?query=%QUERY',
            wildcard: '%QUERY'
        }
    });

    $('#Search').typeahead({
        minLength: 4,
        highlight: true
    }, {
        name: 'book',
        limit: 100,
        display: 'title',
       source: books,
         templates: {
             header: '<h3 class="fw-bold text-primary border-start border-4 ps-3 bg-light rounded shadow-sm d-inline-block mb-3">Books</h3> ',
            empty: [
                '<div class="alert alert-warning text-dark text-center fw-bold fs-5 m-3 rounded shadow-sm">',
                'Oops! No books found😔💔',
                 '</div>'

            ].join('\n'),
            suggestion: Handlebars.compile('<div class="py-2"><span>{{title}}</span><br/><span class="f-xs text-gray-400">by {{author}}</span></div>')
        }
    }).on('typeahead:select', function (e, book) {
        window.location.replace(`/Search/Details?bKey=${book.key}`);
    });
});