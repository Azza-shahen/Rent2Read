$(document).ready(function () {
    $('[data-kt-filter="search"]').on('keyup', function () {
        var input = $(this);
        datatable.search(this.value).draw();
    });

    datatable = $('#Books').DataTable({
        serverSide: true,//It means that all paging, searching and sorting will be done on the server, not on the client.
        processing: true,
        stateSave: true,
        language: {
            processing: '<div class="d-flex justify-content-center text-primary align-items-center dt-spinner"><div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div><span class="text-muted ps-2">Loading...</span></div>'
        },
        ajax: {
            url: '/Books/GetBooks',
            type: 'POST'
        }
        ,
        'drawCallback': function () {
            KTMenu.createInstances();
        },

        order: [[1, 'asc']],// Default ordering: sort by the second column (Title) in ascending order.
        columnDefs: [{//Controls showing/hiding/searching columns.
            targets: [0],
            visible: false, // Hide the first column (Id) from the table view
            searchable: false // Exclude Id column from search results (since it's internal only)
        }],


        columns: [
            { "data": "id", "name": "Id", "className": "d-none" }, //data =>must be camel Case
            // Id column: included in data but hidden in UI. Useful for actions like edit/delete.
            { // DataTables requires property names in camelCase (e.g., "id", "title") to match JSON keys correctly.
                "name": "Title",
                "className": "d-flex align-items-center",
                "render": function (data, type, row) {
                    // Custom render: show book thumbnail, title (linked to details), and author.
                    return `<div class="symbol symbol-50px overflow-hidden me-3">
                                                <a href="/Books/Details/${row.id}">
                                                    <div class="symbol-label h-70px">
                                                        <img src="${(row.imageThumbnailUrl === null ? '/images/books/No_Image_Available.jpg' : row.imageThumbnailUrl)}" alt="Cover" class="w-100">
                                                    </div>
                                                </a>
                                            </div>
                                            <div class="d-flex flex-column">
                                                <a href="/Books/Details/${row.id}" class="text-primary fw-bolder mb-1">${row.title}</a>
                                                <span>${row.author}</span>
                                            </div>`;
                }
            },
            { "data": "publisher", "name": "Publisher" },  // Display publisher directly from dataset.
            {
                "name": "PublishingDate",
                "render": function (data, type, row) {
                    return moment(row.publishingDate).format('ll') // Format publishing date using moment.js for better readability.
                }
            },
            { "data": "hall", "name": "Hall" },// Show which hall the book is located in.

            { "data": "categories", "name": "Categories", "orderable": false }, // Display categories, but disable sorting on this column.
            {
                "name": "IsAvailableForRental",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isAvailableForRental ? 'success' : 'warning')}">
                                                ${(row.isAvailableForRental ? 'Available' : 'Not Available')}
                                            </span>`;
                }
            },
            {
                "name": "IsDeleted",
                "render": function (data, type, row) {
                    return `<span class="badge badge-light-${(row.isDeleted ? 'danger' : 'success')} js-status">
                                                ${(row.isDeleted ? 'Deleted' : 'Available')}
                                            </span>`;
                }
            },
            {
                "className": 'text-end',
                "orderable": false,
                "render": function (data, type, row) {
                    return `<a href="#" 
                             class="btn btn-light btn-active-light-primary btn-sm d-inline-flex align-items-center gap-1" 
                             data-kt-menu-trigger="click" 
                             data-kt-menu-placement="bottom-end">
                              Actions
                              <span class="svg-icon svg-icon-5 m-0">
                                  <svg width="24" height="24" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                      <path d="M11.4343 12.7344L7.25 8.55005C6.83579 8.13583 6.16421 8.13584 5.75 8.55005C5.33579 8.96426 5.33579 9.63583 5.75 10.05L11.2929 15.5929C11.6834 15.9835 12.3166 15.9835 12.7071 15.5929L18.25 10.05C18.6642 9.63584 18.6642 8.96426 18.25 8.55005C17.8358 8.13584 17.1642 8.13584 16.75 8.55005L12.5657 12.7344C12.2533 13.0468 11.7467 13.0468 11.4343 12.7344Z" fill="currentColor"></path>
                                  </svg>
                              </span>
                          </a>
                                <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-gray-800 menu-state-bg-light-primary fw-semibold w-200px py-3" data-kt-menu="true" style="">
                            <!--begin::Menu item-->
                            <div class="menu-item px-3">
                                <a href="/Books/Edit/${row.id}" class="menu-link px-3">
                                    Edit
                                </a>
                            </div>
                            <!--end::Menu item-->
                            <!--begin::Menu item-->
                            <div class="menu-item px-3">
                                        <a href="javascript:;" class="menu-link flex-stack px-3 js-toggle-status" data-url="/Books/ToggleStatus/${row.id}">
                                    Toggle Status
                                </a>
                            </div>
                            <!--end::Menu item-->
                        </div>`;
                }
            }
        ]
    });
});