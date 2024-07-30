import { apiUrl, webUrl } from '../../apiConfig.js';
import userStateManager from '../../store.js';

let users = [];

let currentDocumentReceivers = [];

const loader = $('#loader');

let projectId = '';

async function getProjectId() {
    const { data } = await axios.get(apiUrl + `api/users/project`, {
        withCredentials: true
    });

    projectId = data;
}

async function getDocumentsReceivers() {
    const { data } = await axios.get(apiUrl + `api/projects/${projectId}/documents_receivers`, {
        withCredentials: true
    });

    let code = ``;

    $.each(data, function (_, user) {
        code += `
            <tr data-type="user-cell" id="${user.id}">
                <td>${user.username}</td>
                <td>${user.lastName}</td>
                <td>${user.firstName}</td>
                <td>${user.email}</td>
                <td>
                    <button class="btn btn-danger" data-deletion-user="${user.id}">Supprimer</button>
                </td>
            </tr>
        `;
    });

    $(`#receivers`).html(code);

    $(`#receivers`).find('[data-deletion-user]').on('click', async (e) => {
        const id = $(e.currentTarget).attr('data-deletion-user');

        try {
            loader.removeClass('display-none');

            await axios.delete(apiUrl + `api/projects/documents_receivers/${id}`, {
                withCredentials: true
            });

            window.location.reload();
        } catch (error) {
            alert(error.message);
        } finally {
            loader.addClass('display-none');
        }
    });

    currentDocumentReceivers = data;
}

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');

        await userStateManager.init();

        const { role, hasAccessToSuppliersHandling } = userStateManager.getUser();

        if (role !== 1 || !hasAccessToSuppliersHandling) {
            window.location.href = webUrl + `404`;

            return;
        }

        await getProjectId();

        await getDocumentsReceivers();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#copy-suppliers-link').on('click', () => {
    ClipboardJS.copy(webUrl + `suppliers/${projectId}`);

    alert("Lien copié avec succés!");
});

$('#wsearch').on("keyup", function () {
    const value = $(this).val().toLowerCase();

    $(`[data-type="user-cell"]`).filter(function () {
        const parent = $(this).closest(`[data-type="user-cell"]`);

        parent.toggle(parent.text().toLowerCase().indexOf(value) > -1);
    });
});

$(`[data-action="show-new-receivers-modal"]`).on('click', async (e) => {
    let content = '';

    const { data: users } = await axios.get(apiUrl + `api/projects/${projectId}/not-documents_receivers`, {
        withCredentials: true
    });

    for (let i = 0; i < users.length; i += 1) {
        content += `
            <option value="${users[i].id}">${users[i].username}</option>
        `;
    }

    $('#users').html(content).select2({
        dropdownParent: $('#receivers-modal')
    });

    $('#users').val(currentDocumentReceivers.map(currentReceiver => currentReceiver.id)).trigger('change');

    $('#receivers-modal').modal('toggle');
});

$('[data-action="post-receivers"]').on('click', async () => {
    const selectedUsers = $('#users').val();

    try {
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/projects/${projectId}/documents_receivers`, {
            usersId: selectedUsers
        }, {
            withCredentials: true
        });

        window.location.reload();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');

        $('#receivers-modal').modal('toggle');
    }
});
