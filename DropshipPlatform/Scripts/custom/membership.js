
$(document).ready(function () {
    var myTable = $('#example').DataTable({
        responsive: true
    });

    $('#example').on('click', 'tbody td', function () {
        //myTable.cell(this).edit('bubble');
    });
})
