$(document).ready(function () {
    var table = $("#tblData").DataTable({
        "serverSide": true,
        "processing": true,
        "searching": true,
        "ordering": true,
        "lengthChange": false,
        "paging": true,
        "pagingType": "numbers",
        "autoWidth": true,
        ajax: {
            url: '/BookList/Index?handler=LoadBlogPosts',
            type: "GET",
            dataType: "json",
            beforeSend: function (xhr) {
                xhr.setRequestHeader("XSRF-TOKEN",
                    $('input:hidden[name="__RequestVerificationToken"]').val());
            },
            contentType: "application/x-www-form-urlencoded",
            error: function (xhr, error, thrown) {
                console.log("An error occurred: " + error);
            }
        },
        columns: [
            { data: "PostId" },
            { data: "Title" },
            { data: "Content" },
            { data: "ImageURL" },
            { data: "Likes" },
        ]
    });
});
