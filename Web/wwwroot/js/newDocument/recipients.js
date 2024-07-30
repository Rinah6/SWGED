import { apiUrl } from '../apiConfig.js';
import { setUserRolesList, setUsersSteps, userRolesList, usersSteps, setCurrentRecipientId } from './global.js';
import userStateManager from '../store.js';
import { generateRandomColor } from '../utils.js';

const loader = $('#loader');

let users = [];

let lastSelectedRecipientId = '';

let selectedRecipientId = '';

let usersStepsBuffer = [];

let selectedDocumentStepId = '';

$(document).ready(async () => {
    await userStateManager.init();

    $('#individual-message').summernote();
});

$("#ccContainer").hide();

$("#ccCheck").on("change", (k, v) => {
    if ($("#ccCheck").is(":checked")) $("#ccContainer").show();
    else {
        $("#cc").val("");
        $("#ccContainer").hide();
    }
});

$(`#receiver`).on("keyup", (e) => {
    $("#receiver").css('color', 'black');
})

$(`#role`).on("keyup", (e) => {
    $("#role").css('color', 'black');
});

function resetRecipient() {
    $("#users").val('');
    // $("#role").val("");
    $("#message").summernote('code', '');
}

function getUsernameById(id) {
    const user = users.find((user) => user.id === id);

    if (!user) {
        return '';
    }

    return user.username;
}

$(`[data-action="resetRecipient"]`).on("click", (e) => {
    resetRecipient();
});

$(`[data-action="saveRecipient"]`).on("click", (e) => {
    if ($('#users-documents-roles').val() === '') {
        alert('Veuillez sélectionner un rôle!');

        return;
    }

    const id = $('#receivers').val();

    const user = {
        id,
        username: getUsernameById(id),
        role: $('#users-documents-roles').val(),
        message: $('#individual-message').summernote('code'),
        color: generateRandomColor(),
        fields: [],
    };

    $(`#recipients-list`).append(`
        <li class="nav-item active" document-step-id="${user.id}" user-role="${user.role}">
            <input type="radio" name="recipient-radio" hidden />

            <div class="nav-link d-flex align-items-center justify-content-between" data-action="checkRadio">
                <div class="d-flex align-items-center">
                    <div class="recipientRoundBox hidden" style="background-color: ${user.color}"></div>

                    <span style="padding-left: 10px">${user.username}</span>
                </div>

                <div remove-recipient="${user.id}" class="btn">
                    <i class="fa fa-times text-danger float-right"></i>
                </div>
            </div>
        </li>
    `);

    // setUsersDocumentsList([...usersDocumentsList, user]);

    setUserRolesList([...userRolesList, { id, role: user.role }]);

    $($(`[document-step-id="${user.id}"]`).find('input[type="radio"]')).click();
    
    resetRecipient();

    $('#recipients').modal('toggle');

    $('#individual-message').summernote('code', '');

    $(`#recipients-list`).find('[remove-recipient]').on('click', (e) => {
        const id = $(e.currentTarget).attr('remove-recipient');
        
        // setUsersDocumentsList(usersDocumentsList.filter(user => user.id !== id));
        
        const tmp = userRolesList.filter(x => x.id !== id);
        
        setUserRolesList(tmp);
        
        if (usersDocumentsList.length === 0) {
            $(`[card-id="field"]`).hide();
        }
        
        $('#recipients-list').find(`[document-step-id="${id}"]`).remove();

    });
});

$(document).on('click', `[data-action="checkRadio"]`, (e) => {
    let header = $(e.target).closest("[document-step-id]");
    let input = header.find("input");
    $(input).click();

    if (lastSelectedRecipientId === '') {
        return;
    }

    $(`[by="${lastSelectedRecipientId}"]`).hide();
    $(`[by="${selectedRecipientId}"]`).show();
});

$(document).on('change', `[name=recipient-radio]`, (e) => {
    const header = $(e.target).closest("[document-step-id]");

    if (lastSelectedRecipientId != selectedRecipientId) {
        lastSelectedRecipientId = selectedRecipientId;
    }

    selectedRecipientId = header.attr("document-step-id");

    $(".recipientRoundBox").each(function (i, obj) {
        if (!$(obj).hasClass('hidden')) {
            $(obj).addClass('hidden')
        }
    });

    const box = header.find('.recipientRoundBox');
    box.removeClass('hidden');

    const userRole = userRolesList.find(x => x.id === selectedRecipientId);

    // if (userRole.role !== '2') {
    //     $(`[card-id="field"]`).hide();
    // } else {
    //     $(`[card-id="field"]`).show();
    // }

    setCurrentRecipientId(selectedRecipientId);
});

$('#show-document-types-modal').on('click', async () => {
    const { data: documentTypes } = await axios.get(apiUrl + `api/document_types`, {
        withCredentials: true
    });

    let content = `<option value=""></option>`;

    for (let i = 0; i < documentTypes.length; i += 1) {
        content += `
            <option value="${documentTypes[i].id}">${documentTypes[i].title}</option>
        `;
    }

    $('#document-type-recipients').html(content);

    $('#steps').html('');
    $('#document-type-recipients-modal').modal('toggle');

    $('#document-type-recipients').on('change', async (e) => {
        const id = $(e.currentTarget).val();

        const { data: documentTypeDetails } = await axios.get(apiUrl + `api/document_types/${id}`, {
            withCredentials: true
        });

        let content = ``;

        usersStepsBuffer = documentTypeDetails.steps.map((usersStep) => {
            return {
                id: usersStep.id,
                stepNumber: usersStep.stepNumber,
                processingDuration: usersStep.processingDuration,
                role: 1,
                usersId: usersStep.validators,
                color: generateRandomColor(),
                message: '',
                fields: [],
            };
        });

        for (let i = 0; i < documentTypeDetails.steps.length; i += 1) {
            let validators = ``;

            for (let j = 0; j < documentTypeDetails.steps[i].validators.length; j += 1) {
                validators += `
                    <li data-validator-id="${documentTypeDetails.steps[i].validators[j].id}">
                        ${documentTypeDetails.steps[i].validators[j].username}
                    </li>
                `;
            }

            content += `
                <li id="${documentTypeDetails.steps[i].id}">
                    <span>Étape ${documentTypeDetails.steps[i].stepNumber}</span> (${documentTypeDetails.steps[i].processingDuration} heure(s))
        
                    <ul>
                        ${validators}
                    </ul>
                </li>
            `;
        }

        $('#steps').html(content);
    });
});

$('#close-document-type-recipients-modal').on('click', () => {
    $('#document-type-recipients-modal').modal('toggle');
});

$('#select-document-type-recipients').on('click', () => {
    for (let i = 0; i < usersStepsBuffer.length; i += 1) {
        usersStepsBuffer[i].stepNumber = usersSteps.length + 1;

        let usersListContent = ``;

        for (let j = 0; j < usersStepsBuffer[i].usersId.length; j += 1) {
            usersListContent += `
                <li data-id="${usersStepsBuffer[i].usersId[j].id}">
                    ${usersStepsBuffer[i].usersId[j].username}
                </li>
            `;
        }

        $(`#recipients-list`).append(`
            <li class="nav-item active" document-step-id="${usersStepsBuffer[i].id}" user-role="${usersStepsBuffer[i].role}">
                <input type="radio" name="recipient-radio" hidden />

                <div class="nav-link d-flex align-items-center justify-content-between" data-action="checkRadio">
                    <div class="d-flex align-items-center">
                        <div class="recipientRoundBox hidden" style="background-color: ${usersStepsBuffer[i].color}"></div>

                        <span style="padding-left: 10px">
                            Étape ${usersStepsBuffer[i].stepNumber} 
                            <button class="btn btn-outline-danger" data-delete-document-step="${usersStepsBuffer[i].id}">Supprimer</button>
                            <button class="btn btn-outline-primary" data-modify-document-step="${usersStepsBuffer[i].id}">Modifier</button>
                        </span>
                    </div>
                </div>

                <div style="margin-left: 40px; ">
                    <div data-element="processing-duration">${usersStepsBuffer[i].role === 1 ? `Validateurs` : `Signataires`} (${usersStepsBuffer[i].processingDuration} heure(s)): </div>

                    <ul style="margin-left: 30px; ">
                        ${usersListContent}
                    </ul>
                </div>
            </li>
        `);

        usersStepsBuffer[i].usersId = usersStepsBuffer[i].usersId.map((user) => user.id);

        setUsersSteps([...usersSteps, usersStepsBuffer[i]]);
    }

    $('#document-type-recipients-modal').modal('toggle');
});

$(window).on('load', async () => {
    const { data: usersByProjectId } = await axios.get(apiUrl + `api/projects/users`, {
        withCredentials: true
    });

    users = usersByProjectId;
});

$('#show-add-users-steps-modal').on('click', async () => {
    try {
        
        loader.removeClass('display-none');

        let content = ``;

        for (let i = 0; i < users.length; i += 1) {
            content += `
                <option value="${users[i].id}">${users[i].username}</option>
            `;
        }

        $('#recipients').html(content).select2({
            dropdownParent: $('#users-steps-modal')
        });

        const { data: usersDocumentsRoles } = await axios.get(apiUrl + `api/users_documents_roles`, {
            withCredentials: true
        });

        content = `<option value="" selected></option>`;

        for (let i = 0; i < usersDocumentsRoles.length; i += 1) {
            content += `
                <option value="${usersDocumentsRoles[i].id}">${usersDocumentsRoles[i].title}</option>
            `;
        }

        $('#users-documents-roles').html(content).select2({
            dropdownParent: $('#users-steps-modal')
        });

        $('#users-steps-modal').modal('toggle');

        loader.addClass('display-none');
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#close-users-steps-modal').on('click', () => {
    $('#users-steps-modal').modal('toggle');
});

$('#users-documents-roles').on('change', (e) => {
    switch ($(e.currentTarget).val()) {
        case '1':
            $('[for="recipients"]').text(`Validateurs : `);

            break;
        case '2':
            $('[for="recipients"]').text(`Signataires : `);

            break;
        default:
            $('[for="recipients"]').text(`Utilisateurs : `);

            break;
    }
});

$('#save-users-steps').on('click', () => {
    if ($('#processing-duration').val() === '') {
        alert('Veuillez définir une durée de traitement!');

        return;
    }

    if ($('#users-documents-roles').val() === '') {
        alert('Veuillez sélectionner un rôle!');

        return;
    }

    const usersStep = $('#recipients').select2('data');

    if (usersStep.length === 0) {
        alert('Veuillez sélectionner au moins un utilisateur!');

        return;
    }

    if ($('#message').val() === '') {
        alert('Veuillez sélectionner un message!');

        return;
    }

    const step = {
        id: Date.now(),
        stepNumber: usersSteps.length + 1,
        processingDuration: Number($('#processing-duration').val()),
        // role: $('#users-documents-roles').val(),
        role: 1,
        usersId: $('#recipients').val(),
        color: generateRandomColor(),
        message: $('#message').val(),
        fields: [],
    }

    setUsersSteps([...usersSteps, step]);

    let usersListContent = ``;

    for (let i = 0; i < usersStep.length; i += 1) {
        usersListContent += `
            <li data-id="${usersStep[i].id}">${usersStep[i].text}</li>
        `;
    }

    $(`#recipients-list`).append(`
        <li class="nav-item active" document-step-id="${step.id}" user-role="${step.role}">
            <input type="radio" name="recipient-radio" hidden />

            <div class="nav-link d-flex align-items-center justify-content-between" data-action="checkRadio">
                <div class="d-flex align-items-center">
                    <div class="recipientRoundBox hidden" style="background-color: ${step.color}"></div>

                    <span style="padding-left: 10px">
                        Étape ${step.stepNumber} 
                        <button class="btn btn-outline-danger" data-delete-document-step="${step.id}">Supprimer</button>
                        <button class="btn btn-outline-primary" data-modify-document-step="${step.id}">Modifier</button>
                    </span>
                </div>
            </div>

            <div style="margin-left: 40px; ">
                <div data-element="processing-duration">${step.role === 1 ? `Validateurs` : `Signataires`} (${step.processingDuration} heure(s)) : </div>

                <ul style="margin-left: 30px; ">
                    ${usersListContent}
                </ul>
            </div>
        </li>
    `);

    $('#users-steps-modal').modal('toggle');
});

$(document).on('click', '[data-delete-document-step]', (e) => {
    e.stopPropagation();

    selectedDocumentStepId = $(e.currentTarget).attr('data-delete-document-step');

    const tmp = usersSteps.filter((usersStep) => {
        return usersStep.id !== selectedDocumentStepId;
    });

    setUsersSteps(tmp);

    $(`[document-step-id="${selectedDocumentStepId}"]`).remove();
});

$(document).on('click', '[data-modify-document-step]', (e) => {
    selectedDocumentStepId = $(e.currentTarget).attr('data-modify-document-step');

    const documentStep = usersSteps.find((usersStep) => {
        return usersStep.id === selectedDocumentStepId;
    });

    if (documentStep === undefined) {
        return;
    }

    $('#current-processing-duration').val(documentStep.processingDuration);
    $('#current-message').val(documentStep.message);

    let content = `<option value="" selected></option>`;

    for (let i = 0; i < users.length; i += 1) {
        content += `
            <option value="${users[i].id}">${users[i].username}</option>
        `;
    }

    $('#current-recipients').html(content).select2({
        dropdownParent: '#users-step-modal-details',
    });

    $('#current-recipients').val(documentStep.usersId).trigger('change');

    $('#users-step-modal-details').modal('show');
});

$('#save-users-step-details').on('click', () => {
    console.log(usersSteps);

    const tmp = usersSteps.map((usersStep) => {
        if (usersStep.id === selectedDocumentStepId) {
            return {
                ...usersStep,
                processingDuration: Number($('#current-processing-duration').val()),
                usersId: $('#current-recipients').val(),
                message: $('#current-message').val(),
            };
        }

        return usersStep;
    });

    const usersStep = $('#current-recipients').select2('data');

    let content = ``;

    for (let i = 0; i < usersStep.length; i += 1) {
        content += `
            <li data-id="${usersStep[i].id}">${usersStep[i].text}</li>
        `;
    }

    $(`[document-step-id="${selectedDocumentStepId}"]`).find('ul').html(content);
    $(`[document-step-id="${selectedDocumentStepId}"]`).find('[data-element="processing-duration"]').html(`Validateurs (${Number($('#current-processing-duration').val())} heure(s))`);

    setUsersSteps(tmp);

    $('#users-step-modal-details').modal('hide');
});
