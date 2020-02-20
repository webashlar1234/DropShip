
$(document).ready(function () {
    $('#toggle-two').bootstrapToggle({
        on: 'Enabled',
        off: 'Disabled'
    });

   

    var myTable = $('#example').DataTable({
        responsive: true
    });

    $('#example').on('click', 'tbody td', function () {
        //myTable.cell(this).edit('bubble');
    });
})
