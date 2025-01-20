var dataTable;




$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/user/getall' },
        "columns": [
            { data: 'name', "width": "25%" },
            { data: 'email', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'company.name', "width": "10%" },
            { data: 'role', "width": "10%" },
            
            {
                data: {id: "id",lockoutEnd:"lockoutEnd"},
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();

                    if (lockout > today) {
                        return `
                        <div class="text-center">
                        
                        <a onclick="LockUnlock('${data.id}')" class="btn btn-danger text-white" style="cursor:pointer;width:100px;">Lock <i class="bi bi-unlock-fill"></i></a>
                        <a class="btn btn-danger text-white" style="cursor:pointer;width:150px;">Permission <i class="bi bi-pencil-square"></i></a>
                            </div>`
                    }
                    else {
                        return `
                        <div class="text-center">
                        <a onclick="LockUnlock('${data.id}')" class="btn btn-success text-white" style="cursor:pointer;width:100px;">Unlock <i class="bi bi-unlock-fill"></i></a>
                        <a class="btn btn-danger text-white" style="cursor:pointer;width:150px;">Permission <i class="bi bi-pencil-square"></i></a>
                            </div>`
                    }
                    
                },
                "width": "25%"
            }
        ]
    });
}

function LockUnlock(id) {
    $.ajax({
        type: "POST",
        url: '/Admin/User/LockUnlock',
        data: JSON.stringify(id),
        contentType: "application/json",
        success: function (data) {
            if (data.success == true) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            
            }
        }
    });
}

