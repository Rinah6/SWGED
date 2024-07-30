import { apiUrl, webUrl } from '../../apiConfig.js';
import userStateManager from '../../store.js';
import { formatDate } from '../../utils.js';

const loader = $('#loader');

let projectId = '';

async function getProjectId() {
    const { data } = await axios.get(apiUrl + `api/users/project`, {
        withCredentials: true
    });

    projectId = data;
}

async function getSuppliers() {
    const { data } = await axios.get(apiUrl + `api/suppliers/project`, {
        withCredentials: true
    });

    let code = ``;

    $.each(data, function (_, user) {
        code += `
            <tr data-type="user-cell" id="${user.id}">
                <td>${user.name}</td>
                <td>${user.nif === undefined ? '' : user.nif}</td>
                <td>${user.stat === undefined ? '' : user.stat}</td>
                <td>${user.cin === undefined ? '' : user.cin}</td>
                <td>${user.mail === undefined ? '' : user.mail}</td>
                <td>${user.contact === undefined ? '' : user.contact}</td>
                <td>${formatDate(user.creationDate)}</td>
                <td>
                    <button class="btn btn-danger" data-supplier-deletion="${user.id}">Supprimer</button>
                </td>
            </tr>
        `;
    });

    $(`#suppliers`).html(code);

    $(`#suppliers`).find('[data-supplier-deletion]').on('click', async (e) => {
        try {
            loader.removeClass('display-none');

            const supplierId = $(e.currentTarget).attr('data-supplier-deletion');
    
            await axios.delete(apiUrl + `api/suppliers/${supplierId}/project`, {
                withCredentials: true
            });
    
            window.location.reload();
        } catch (error) {
            alert(error.message);
        } finally {
            loader.addClass('display-none');
        }
    });
}

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');

        await userStateManager.init();

        const { role } = userStateManager.getUser();

        if (role !== 1) {
            window.location.href = webUrl + `404`;

            return;
        }

        await getProjectId();

        await getSuppliers();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#wsearch').on("keyup", function () {
    const value = $(this).val().toLowerCase();

    $(`[data-type="user-cell"]`).filter(function () {
        const parent = $(this).closest(`[data-type="user-cell"]`);

        parent.toggle(parent.text().toLowerCase().indexOf(value) > -1);
    });
});

$('#copy-suppliers-link').on('click', () => {
    ClipboardJS.copy(webUrl + `suppliers/${projectId}`);

    alert("Lien copié avec succés!");
});
