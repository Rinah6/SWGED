import { apiUrl } from '../../../apiConfig.js';
import userStateManager from '../../../store.js';
import { setUsers, users } from './global.js';

const loader = $('#loader');

const steps = $('#steps');

let stepsList = [];

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');

        await userStateManager.init();

        const { role, hasAccessToDocumentTypesHandling } = userStateManager.getUser();

        if (role !== 1 || !hasAccessToDocumentTypesHandling) {
            // window.location.href = webUrl + `404`;

            return;
        }

        const { data: documentTypes } = await axios.get(apiUrl + `api/document_types`, {
            withCredentials: true
        });

        let content = '';

        for (let i = 0; i < documentTypes.length; i += 1) {
            content += `
                <li data-document-type-id="${documentTypes[i].id}" class="document-type">
                    <span>${documentTypes[i].title}</span>
                </li>
            `;
        }

        $('#document-types-list').html(content);

        const { data: usersByProjectId } = await axios.get(apiUrl + `api/projects/users`, {
            withCredentials: true
        });
    
        setUsers(usersByProjectId);

        await getSites();

        await getSitesNew();

    } catch (error) {
        console.log(error);
    } finally {
        loader.addClass('display-none');
    }
});


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

    //$('#select-current-sites').select2({
    //    dropdownParent: $('#document-type-details'),
    //});
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
        dropdownParent: $('#add-document-type-modal')
    });

}


$('#add-document-type').on('click', async () => {
    steps.html('');

    $('#add-document-type-modal').modal('toggle');
});

$('#close-add-document-type-modal').on('click', () => {
    steps.html('');

    $('#add-document-type-modal').modal('toggle');
});

$('#add-step-btn').on('click', () => {
    stepsList.push({
        id: Date.now(),
        stepNumber: stepsList.length + 1,
        processingDescription: '',
        processingDuration: 0,
        usersId: []
    });

    const newStep = stepsList[stepsList.length - 1];

    steps.append(`
        <li id="${newStep.id}" style="margin-bottom: 20px; ">
            <span data-type="stepNumber">Étape ${stepsList.length}</span>

            <div>
                <div>
                    <label for="processing-description-${newStep.id}">Description: </label>
                    <input type="text" id="processing-description-${newStep.id}" value="${newStep.processingDescription}" min="0" />
                </div>
                <div>
                    <label for="processing-duration-${newStep.id}">Durée de traitement (en heures): </label>
                    <input type="number" id="processing-duration-${newStep.id}" value="${newStep.processingDuration}" min="0" />
                </div>
                <div>
                    <label for="users-${newStep.id}">Validateurs: </label>
                    <select id="users-${newStep.id}" name="users-${newStep.id}[]" multiple="multiple" style="width: 200px;"></select>
                </div>
            </div>
        </li>
    `);

    let content = `<option value=""></option>`;

    for (let i = 0; i < users.length; i += 1) {
        content += `
            <option value="${users[i].id}">${users[i].username}</option>
        `;
    }

    steps.find(`#users-${newStep.id}`).html(content).select2({
        dropdownParent: $('#add-document-type-modal')
    });
});

$('#post-document-type').on('click', async () => {

    const select = document.querySelector('#select-new-current-sites');
    const currentSelectedSitesId = [];
    for (const option of select.options)
        option.selected && currentSelectedSitesId.push(option.value)
    const sites = JSON.stringify(currentSelectedSitesId)

    if ($('#new-document-type-label').val() === '') {
        alert(`L'étiquette est obligatoire!`);

        return;
    }

    try {
        loader.removeClass('display-none');

        for (let i = 0; i < stepsList.length; i += 1) {
            if (steps.find(`#users-${stepsList[i].id}`).val().length === 0) {
                alert(`L'étape ${stepsList[i].stepNumber} doit contenir au moins un validateur!`);

                loader.addClass('display-none');

                return;
            }

            stepsList[i] = {
                ...stepsList[i],
                processingDescription: Number(steps.find(`#processing-description-${stepsList[i].id}`).val()),
                processingDuration: Number(steps.find(`#processing-duration-${stepsList[i].id}`).val()), 
                usersId: steps.find(`#users-${stepsList[i].id}`).val()
            }
        }

        await axios.post(apiUrl + `api/document_types`, {
            title: $('#new-document-type-label').val(),
            sites,
            steps: stepsList.map((step) => {
                return {
                    stepNumber: step.stepNumber,
                    processingDescription: step.processingDescription,
                    processingDuration: step.processingDuration,
                    usersId: step.usersId,
                };
            })
        }, {
            withCredentials: true
        });

        window.location.reload();
    } catch (error) {
        console.log(error.message);
    } finally {
        loader.addClass('display-none');
    }
});
