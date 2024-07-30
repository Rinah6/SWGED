import { apiUrl, webUrl } from '../../apiConfig.js';
import userStateManager from '../../store.js';

const loader = $('#loader');

let projects = [];

function renderProjects() {
    let content = '';

    for (let i = 0; i < projects.length; i += 1) {
        content += `
            <option value="${projects[i].id}">${projects[i].name}</option>
        `;
    }

    $('#projects-container').html(`
        <label for="projects">SOAS interagissants : </label>
        <select id="projects" name="projects[]" multiple="multiple" class="form-control" style="width: 300px;"></select>
    `);

    $('#projects-container').find('#projects').html(content).select2({
        dropdownParent: $('#update-project-modal')
    });
}

async function getProjects() {
    const { data } = await axios.get(apiUrl + `api/soas`, {
        withCredentials: true
    });

    let code = ``;

    $.each(data, function (_, v) {
        code += `
            <tr data-type="project-cell">
                <td>${v.id}</td>
                <td>${v.name}</td>
                <td><button class="btn btn-primary" show-update-project-modal="${v.id}">Modifer</button></td>
                <td><button class="btn btn-danger" delete-project="${v.id}">Supprimer</button></td>
            </tr>
        `;
    });

    $(`#liste_user`).html(code);

    $("#societe_nombre").html(Number(data.length))
}

async function getProjectDetails(projectId) {
    loader.removeClass('display-none');

    const { data } = await axios.get(apiUrl + `api/soas/${projectId}`, {
        withCredentials: true
    });

    $("#current-name").val(data.name);
    
    loader.addClass('display-none');
}

async function deleteProject(projectId) {
    await axios.delete(apiUrl + `api/soas/${projectId}`, {
        withCredentials: true
    });

    window.location.reload();
}

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');

        await userStateManager.init();

        const { role } = userStateManager.getUser();

        if (role !== 0) {
            window.location.href = webUrl + `404`;

            return;
        }

        await getProjects();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#wsearch').on('keyup', function () {
    const value = $(this).val().toLowerCase();

    $(`[data-type="project-cell"]`).filter(function () {
        const parent = $(this).closest(`[data-type="project-cell"]`);

        parent.toggle(parent.text().toLowerCase().indexOf(value) > -1);
    });
});

$(document).on('click', '[create-project]', async() => {
    let name = $('#name').val();

    if (name == '') {
        Toast.fire({
            icon: 'error',
            title: "Le nom est obligatoire."
        });

        return;
    }

    try {
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/soas`, {
            name
        }, {
            withCredentials: true
        });

        Toast.fire({
            icon: 'success',
            title: `SOA insérée!`
        });

        window.location.reload();
    } catch (error) {
        alert(error.response.data);
    } finally {
        loader.addClass('display-none');
    }

    $('#create-project-modal').modal('hide');
});

$(document).on('click', '[user-modal]', (e) => {
    $('#name').val('');
    $('#project').val('');

    $('#create-project-modal').modal('toggle');
});

$(document).on('click', '[modal-closed]', (e) => {
    $('#create-project-modal').modal('hide');
});

$(document).on('click', '[show-update-project-modal]', async (e) => {
    loader.removeClass('display-none');

    const currentElement = $(e.target).closest(`[show-update-project-modal]`);
    const id = currentElement.attr('show-update-project-modal');
    
    await getProjectDetails(id);

    renderProjects();

    loader.addClass('display-none');

    $("#update-project-modal").modal('toggle');

    $('#update-project-modal').find('button[type="submit"]').attr('update-project', id);
});

$('#update-project-modal').find('button[type="submit"]').on('click', async (e) => {
    const id = $(e.target).attr('update-project');
    const name = $("#current-name").val();

    loader.removeClass('display-none');

    try {
        await axios.patch(apiUrl + `api/soas/${id}`, {
            name
        }, {
            withCredentials: true
        });

        Toast.fire({
            icon: 'success',
            title: `SOA mise à jour!`
        });

        window.location.reload();
    } catch (error) {
        //Toast.fire({
        //    icon: 'error',
        //    title: error.message
        //});
        alert(error.response.data);
    } finally {
        loader.addClass('display-none');;
    }
});

$(document).on('click', '[delete-project]', async (e) => {
    const header = $(e.target).closest(`[delete-project]`);
    const id = header.attr("delete-project");

    if (confirm('Êtes-vous sûr(e) de supprimer le SOA ?')) {
        await deleteProject(id);
    }
});
