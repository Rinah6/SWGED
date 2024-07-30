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
        <label for="projects">Projets interagissants : </label>
        <select id="projects" name="projects[]" multiple="multiple" class="form-control" style="width: 300px;"></select>
    `);

    $('#projects-container').find('#projects').html(content).select2({
        dropdownParent: $('#update-project-modal')
    });
}

async function getProjects() {
    const { data } = await axios.get(apiUrl + `api/projects`, {
        withCredentials: true
    });

    //console.log(data)

    let code = ``;

    $.each(data, function (_, v) {
        code += `
            <tr data-type="project-cell">
                <td>${v.soaName === undefined ? "" : v.soaName}</td>
                <td>${v.id}</td>
                <td>${v.name === undefined ? "" : v.name}</td>
                <td>${v.storage === undefined ? "" : v.storage}</td>
                <td>${v.serverName === undefined ? "" : v.serverName}</td>
                <td>${v.dataBaseName === undefined ? "" : v.dataBaseName}</td>
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

    const { data } = await axios.get(apiUrl + `api/projects/${projectId}`, {
        withCredentials: true
    });

    $("#current-name").val(data.name);
    $("#current-storage").val(data.storage);
    $("#current-serveur").val(data.servernaame);
    $("#current-login").val(data.login);
    $("#current-password").val(data.password);
    $('#has-access-to-internal-users-handling').prop('checked', data.hasAccessToInternalUsersHandling);
    $('#has-access-to-suppliers-handling').prop('checked', data.hasAccessToSuppliersHandling);
    $('#has-access-to-processing-circuits-handling').prop('checked', data.hasAccessToProcessingCircuitsHandling);
    $('#has-access-to-sign-myself-feature').prop('checked', data.hasAccessToSignMySelfFeature);
    $('#has-access-to-archive-immediately-feature').prop('checked', data.hasAccessToArchiveImmediatelyFeature);
    $('#has-access-to-global-dynamic-fields-handling').prop('checked', data.hasAccessToGlobalDynamicFieldsHandling);
    $('#has-access-to-physical-location-handling').prop('checked', data.hasAccessToPhysicalLocationHandling);
    $('#has-access-to-numeric-library').prop('checked', data.hasAccessToNumericLibrary);
    $('#has-access-to-tomate-db-connection').prop('checked', data.hasAccessToTomProLinking);
    $('#has-access-to-users-connections-information').prop('checked', data.hasAccessToUsersConnectionsInformation);
    $('#has-access-to-document-types-handling').prop('checked', data.hasAccessToDocumentTypesHandling);
    $('#has-access-to-documents-accesses-handling').prop('checked', data.hasAccessToDocumentsAccessesHandling);
    $('#has-access-to-rsf').prop('checked', data.hasAccessToRSF);

    if (data.sites) {
        const sites = JSON.parse(data.sites)
        const selectsites = document.querySelector('#select-current-sites');

        Array.from(selectsites.options).forEach(function (option) {

            if (sites.includes(option.value)) {
                option.selected = true;
            } else {
                option.selected = false;
            }

        });

        $('#select-current-sites').select2({
            dropdownParent: $('#update-project-modal'),
        });

    }

    //const divs = Array.from(document.querySelectorAll('#current-sites .multiselect-dropdown .multiselect-dropdown-list-wrapper .multiselect-dropdown-list div'));
    //const placeholder = document.querySelector('#current-sites .multiselect-dropdown')

    //const span = document.createElement("span");
    //span.className = "placeholder";
    //span.textContent = "Select...";
    //placeholder.append(span);

    //for (const div of divs) {
    //    const input = div.querySelector('input[type="checkbox"]');
    //    if (sites.includes(input.value)){
    //        div.classList.add("checked");
    //        input.checked = true;


    //        const span = document.createElement("span");
    //        span.className = "optext";


    //        const { data: textes } = await axios.get(apiUrl + `api/sites/${input.value}`, {
    //            withCredentials: true
    //        });

    //        span.textContent = textes.name;

    //        const spanx = document.createElement("span");
    //        spanx.className = "optdel";
    //        spanx.textContent = "🗙";
    //        spanx.title = "Remove";
    //        spanx.onclick = function (e) {
    //            span.srcElement.optionElement.dispatchEvent(new Event('click'));
    //            placeholder.refresh();
    //            e.stopPropagation();
    //        };
    //        span.appendChild(spanx);

    //        placeholder.append(span)
    //    }

    //}

    loader.addClass('display-none');
}

async function deleteProject(projectId) {
    await axios.delete(apiUrl + `api/projects/${projectId}`, {
        withCredentials: true
    });

    window.location.reload();
}

async function getSOAS() {
    const { data: projects } = await axios.get(apiUrl + `api/soas`, {
        withCredentials: true
    });

    let content = `
        <option value="" selected></option>
    `;

    for (let i = 0; i < projects.length; i += 1) {
        content += `
            <option value="${projects[i].id}">${projects[i].name}</option>
        `;
    }

    $('#soas').html(content).select2({
        dropdownParent: $('#create-project-modal')
    });
}

async function getSites() {
    
    const select = document.createElement("select");
    select.id = "select-current-sites";
    select.name = "select-current-sites";
    select.setAttribute("multiple", "multiple");
    select.setAttribute("style", "width: 400px");

    const { data: Sites } = await axios.get(apiUrl + `api/sites`, {
        withCredentials: true
    });

    for (let i = 0; i < Sites.length; i += 1) {
        const opt = document.createElement("option");
        opt.value = Sites[i].id;
        opt.textContent = Sites[i].siteId + ' - ' + Sites[i].name;
        select.append(opt);
    }

    const s = document.getElementById("current-sites");
    s.append(select);


}

async function getSitesNew() {
    const select = document.createElement("select");
    select.id = "select-new-current-sites";
    select.name = "select-new-current-sites";
    select.setAttribute("multiple", "multiple");
    select.setAttribute("style", "width: 400px");

    const { data: Sites } = await axios.get(apiUrl + `api/sites`, {
        withCredentials: true
    });

    for (let i = 0; i < Sites.length; i += 1) {
        const opt = document.createElement("option");
        opt.value = Sites[i].id;
        opt.textContent = Sites[i].siteId + ' - ' + Sites[i].name;
        select.append(opt);
    }

    const s = document.getElementById("new-current-sites");
    s.append(select);

    $('#select-new-current-sites').select2({
        dropdownParent: $('#create-project-modal')
    });

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

        await getSOAS();

        await getSites();

        await getSitesNew();

        $('#authentication-modes').select2({
            dropdownParent: $('#create-project-modal'),
        })

    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#server-name').on('input', () => {
    $('#connection-btn').show();

    $('#databases-container').html('');
});

$('#authentication-modes').on('change', (e) => {
    $('#connection-btn').show();

    $('#databases-container').html('');

    const value = Number($(e.currentTarget).val());

    if (value === 0) {
        $('#sa-password').html('');

        return;
    }

    $('#sa-password').html(`
        <div class="form-group">
            <label for="login">Connexion: </label>

            <input type="text" id="login" />
        </div>

        <div class="form-group">
            <label for="password">Mot de passe: </label>

            <input type="password" id="password" />
            <i class="fa fa-eye fa-lg" id="toggle-password" style="margin: 10px; cursor: pointer;"></i>
        </div>
    `);

    $('#sa-password').find('#toggle-password').on('click', () => {
        const password = $('#password');

        const type = password.attr('type') === 'password' ? 'text' : 'password';

        password.attr('type', type);

        $(e.currentTarget).toggleClass('bi-eye');
    });
});

$('#connection-btn').on('click', async (e) => {
    try {
        loader.removeClass('display-none');

        const login = $('#sa-password').find('#login').val();
        const password = $('#sa-password').find('#password').val();

        const { data: databases } = await axios.post(apiUrl + `api/tom_pro_db_connections/databases`, {
            serverName: $('#serveur').val(),
            login: !login ? undefined : login,
            password: !password ? undefined : password
        }, {
            withCredentials: true
        });

        $('#databases-container').html(`
            <div>
                <label for="authentication-modes">Bases de données: </label>

                <select id="databases"></select>
            </div>
        `);

        let tmp = '';

        for (let i = 0; i < databases.length; i += 1) {
            tmp += `
                <option value="${databases[i].id}">${databases[i].name}</option>
            `;
        }

        $('#databases-container').find('#databases').html(tmp);

        $('#databases-container').find('#databases').select2({
            dropdownParent: $('#create-project-modal'),
        });
    } catch (error) {
        console.log(error.message);

        alert(`Échec de la connexion à l'instance!`);
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
    loader.removeClass('display-none');

    let name = $('#name').val();

    const select = document.querySelector('#select-new-current-sites');
    const currentSelectedSitesId = [];
    for (const option of select.options)
        option.selected && currentSelectedSitesId.push(option.value)
    const sites = JSON.stringify(currentSelectedSitesId)

    let storage = $("#storage").val();
    let servername = $("#serveur").val();

    const login = $('#sa-password').find('#login').val();
    const password = $('#sa-password').find('#password').val();

    //const databaseName = $('#databases-container').find('#databases').select2('data').map((database) => {
    //    return {
    //        databaseName: database.text,
    //    };
    //});
    //let dbasename = databaseName[0].databaseName;

    let dbasename = "xxxx";

    //const soaName = $('#soa-container').find('#soas').select2('data').map((soas) => {
    //    return {
    //        soaName: soas.text,
    //    };
    //});
    //let soa = soaName[0].soaName;

    let soa = "xxxxxxx";

    if (name == '') {
        Toast.fire({
            icon: 'error',
            title: "Le nom est obligatoire."
        });

        return;
    }
    if (soa == '') {
        Toast.fire({
            icon: 'error',
            title: "Le SOA est obligatoire."
        });

        return;
    }
    if (servername == '') {
        Toast.fire({
            icon: 'error',
            title: "Le serveur est obligatoire."
        });

        return;
    }
    if (dbasename == '') {
        Toast.fire({
            icon: 'error',
            title: "La base de données est obligatoire."
        });

        return;
    }

    try {
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/projects`, {
            name,
            sites,
            storage,
            soa,
            login: !login ? undefined : login,
            password: !password ? undefined : password,
            serverName: servername,
            dataBaseName: dbasename
        }, {
            withCredentials: true
        });

        Toast.fire({
            icon: 'success',
            title: `Projet insérée!`
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
    $('#storage').val('');

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

    const select = document.querySelector('#select-current-sites');
    const currentSelectedSitesId = [];
    for (const option of select.options) 
        option.selected && currentSelectedSitesId.push(option.value)

    //const inputs = document.querySelectorAll('#current-sites .multiselect-dropdown .multiselect-dropdown-list-wrapper .multiselect-dropdown-list div input');
    //const currentSelectedSitesId = [];
    //for (const input of inputs) 
    //    input.checked && currentSelectedSitesId.push(input.value)

    const id = $(e.target).attr('update-project');
    const name = $("#current-name").val();
    const hasAccessToInternalUsersHandling = $('#has-access-to-internal-users-handling').prop('checked');
    const hasAccessToSuppliersHandling = $('#has-access-to-suppliers-handling').prop('checked');
    const HasAccessToProcessingCircuitsHandling = $('#has-access-to-processing-circuits-handling').prop('checked');
    const hasAccessToSignMySelfFeature = $('#has-access-to-sign-myself-feature').prop('checked');
    const hasAccessToArchiveImmediatelyFeature = $('#has-access-to-archive-immediately-feature').prop('checked');
    const hasAccessToGlobalDynamicFieldsHandling = $('#has-access-to-global-dynamic-fields-handling').prop('checked');
    const hasAccessToPhysicalLocationHandling = $('#has-access-to-physical-location-handling').prop('checked');
    const hasAccessToNumericLibrary = $('#has-access-to-numeric-library').prop('checked');
    const hasAccessToTomProLinking = $('#has-access-to-tomate-db-connection').prop('checked');
    const hasAccessToUsersConnectionsInformation = $('#has-access-to-users-connections-information').prop('checked');
    const hasAccessToDocumentTypesHandling = $('#has-access-to-document-types-handling').prop('checked');
    const hasAccessToDocumentsAccessesHandling = $('#has-access-to-documents-accesses-handling').prop('checked');
    const hasAccessToRSF = $('#has-access-to-rsf').prop('checked');
    const sites = JSON.stringify(currentSelectedSitesId)
    loader.removeClass('display-none');

    try {
        await axios.patch(apiUrl + `api/projects/${id}`, {
            name,
            hasAccessToInternalUsersHandling,
            hasAccessToSuppliersHandling,
            HasAccessToProcessingCircuitsHandling,
            hasAccessToSignMySelfFeature,
            hasAccessToArchiveImmediatelyFeature,
            hasAccessToGlobalDynamicFieldsHandling,
            hasAccessToPhysicalLocationHandling,
            hasAccessToNumericLibrary,
            hasAccessToTomProLinking,
            hasAccessToUsersConnectionsInformation,
            hasAccessToDocumentTypesHandling,
            hasAccessToDocumentsAccessesHandling,
            hasAccessToRSF,
            sites,
        }, {
            withCredentials: true
        });

        Toast.fire({
            icon: 'success',
            title: `Projet mise à jour!`
        });

        window.location.reload();
    } catch (error) {
        Toast.fire({
            icon: 'error',
            title: error.message
        });
    } finally {
        loader.addClass('display-none');;
    }
});

$(document).on('click', '[delete-project]', async (e) => {
    const header = $(e.target).closest(`[delete-project]`);
    const id = header.attr("delete-project");

    if (confirm('Êtes-vous sûr(e) de supprimer le projet ?')) {
        await deleteProject(id);
    }
});
