import { apiUrl } from '../../apiConfig.js';
import { generateRandomColor } from '../../utils.js';
import userStateManager from '../../store.js';
import { documentId } from './global.js';

const loader = $('#loader');

let lastSelectedRecipientId = '';
let selectedRecipientId = '';

let users = [];

let usersDocumentsList = [];
let userRolesList = [];

let suppliersDocumentValidatorsSteps = [];

let usersSteps = [];

async function getUsersByProjectId() {
    const { data } = await axios.get(apiUrl + `api/projects/users`, {
        withCredentials: true
    });

    return data;
}

function resetRecipient() {
    $("#receiver").val("");
    // $("#role").val("");
    $("#message").summernote('code', "");
}

function getUsernameById(id) {
    const user = users.find((user) => user.id === id);

    if (!user) {
        return '';
    }

    return user.username;
}

$(document).ready(async () => {
    $('#individual-message').summernote();

    await userStateManager.init();

    const { hasAccessToDocumentTypesHandling, hasAccessToSuppliersHandling } = userStateManager.getUser();

    if (!hasAccessToSuppliersHandling) {
        return;
    }

    if (hasAccessToDocumentTypesHandling && $('#recipients-list-container').length !== 0) {
        $('#send-to-container').html(`
            <div class="btn btn-success bg-gradient mb-3 col-12" data-action="set-recipients">
                <i class="fa fa-paper-plane p-2"></i>Sélectionner un type de document
            </div>
        `);
    
        $('#send-to-container').find('[data-action="set-recipients"]').on('click', async () => {
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
    
            $('#document-type-recipients-modal').attr('data-id', documentId);
            $('#document-type-recipients-modal').modal('toggle');
    
            $('#document-type-recipients').on('change', async (e) => {
                const id = $(e.currentTarget).val();
    
                const { data: documentTypeDetails } = await axios.get(apiUrl + `api/document_types/${id}`, {
                    withCredentials: true
                });
    
                let content = ``;
    
                suppliersDocumentValidatorsSteps = documentTypeDetails.steps;
    
                for (let i = 0; i < documentTypeDetails.steps.length; i += 1) {
                    let validators = ``;
    
                    for (let j = 0; j < documentTypeDetails.steps[i].validators.length; j += 1) {
                        validators += `
                            <li data-validator-id="${documentTypeDetails.steps[i].validators[j].id}">
                                ${documentTypeDetails.steps[i].validators[j].firstName} ${documentTypeDetails.steps[i].validators[j].lastName} (${documentTypeDetails.steps[i].validators[j].username})
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

        return;
    }
});

$(`[data-action="resetRecipient"]`).on('click', (e) => {
    resetRecipient();
});

$(`[data-action="saveRecipient"]`).on('click', (e) => {
    const id = $('#users-container').find('#users').val();

    const user = {
        id,
        username: getUsernameById(id),
        role: '2',
        message: $('#individual-message').summernote('code'),
        color: generateRandomColor(),
        fields: []
    };

    $(`#recipients-list`).append(`
        <li class="nav-item active" recipient-id="${user.id}" user-role="${user.role}">
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

            <hr />
        </li>
    `);

    usersDocumentsList.push(user);

    userRolesList.push({ id, role: user.role });

    $($(`[recipient-id="${user.id}"]`).find('input[type="radio"]')).click();

    $('#recipients').modal('toggle');
    
    resetRecipient();

    $(`#recipients-list`).find('[remove-recipient]').on('click', (e) => {
        const id = $(e.currentTarget).attr('remove-recipient');
        
        usersDocumentsList = usersDocumentsList.filter(user => user.id !== id);
        
        const tmp = userRolesList.filter(x => x.id !== id);
        
        userRolesList = tmp;
        
        // if (Object.keys(usersDocumentsList).length === 0) {
            $(`[card-id="field"]`).hide();
        // }
        
        $('#recipients-list').find(`[recipient-id="${id}"]`).remove();

    });

    $('#individual-message').summernote('code', '');
});

$(document).on('click', `[data-action="checkRadio"]`, (e) => {
    let header = $(e.target).closest('[recipient-id]');
    let input = header.find("input");
    $(input).click();

    if (lastSelectedRecipientId === '') {
        return;
    }

    //$(`[by="${lastSelectedRecipientId}"]`).hide();
    //$(`[by="${selectedRecipientId}"]`).show();
});

$(document).on('change', `[name=recipient-radio]`, (e) => {
    const header = $(e.target).closest('[recipient-id]');

    if (lastSelectedRecipientId !== selectedRecipientId) {
        lastSelectedRecipientId = selectedRecipientId;
    }

    selectedRecipientId = header.attr('recipient-id');

    $('.recipientRoundBox').each(function (i, obj) {
        if (!$(obj).hasClass('hidden')) {
            $(obj).addClass('hidden')
        }
    });

    header.find('.recipientRoundBox').removeClass('hidden');

    const userRole = userRolesList.find(x => x.id === selectedRecipientId);

    if (userRole.role !== '2') {
        $(`[card-id="field"]`).show();
    } else {
        $(`[card-id="field"]`).hide();
    }
});

$('#close-document-type-recipients-modal').on('click', () => {
	$('#document-type-recipients-modal').modal('toggle');	
});

$('#select-document-type-recipients').on('click', () => {
    let content = ``;

    for (let i = 0; i < suppliersDocumentValidatorsSteps.length; i += 1) {
        let validators = ``;

        for (let j = 0; j < suppliersDocumentValidatorsSteps[i].validators.length; j += 1) {
            validators += `
                <li data-validator-id="${suppliersDocumentValidatorsSteps[i].validators[j].id}">
                    ${suppliersDocumentValidatorsSteps[i].validators[j].firstName} ${suppliersDocumentValidatorsSteps[i].validators[j].lastName} (${suppliersDocumentValidatorsSteps[i].validators[j].username})
                </li>
            `;
        }

        content += `
            <li id="${suppliersDocumentValidatorsSteps[i].id}">
                <span>Étape ${suppliersDocumentValidatorsSteps[i].stepNumber}</span> (${suppliersDocumentValidatorsSteps[i].processingDuration} heure(s))
    
                <ul>
                    ${validators}
                </ul>
            </li>
        `;
    }

    $('#recipients-list').html(content);
    
	$('#document-type-recipients-modal').modal('toggle');	
});

$(document).on('click', '[data-action="set-recipients-steps"]', async (e) => {
    try {
        loader.removeClass('display-none');

        const usersByProjectId = await getUsersByProjectId();

        let content = ``;

        // const lastIndex = usersDocumentsList.length === 0 ? 0 : usersDocumentsList.length - 1;

        for (let i = 0; i < usersByProjectId.length; i += 1) {
            // if (!usersDocumentsList[lastIndex] || usersDocumentsList[lastIndex].id !== usersByProjectId[i].id) {
                content += `
                    <option value="${usersByProjectId[i].id}">${usersByProjectId[i].username}</option>
                `;
            // }
        }

        $('#recipients').html(content).select2({
            dropdownParent: $('#recipients-modal')
        });

        users = usersByProjectId;

        $('#recipients-modal').attr('data-id', documentId);
        $('#recipients-modal').modal('toggle');
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#close-users-steps-modal').on('click', () => {
    $('#recipients-modal').modal('toggle');
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
        role: $('#users-documents-roles').val(),
        usersId: $('#recipients').val(),
        color: generateRandomColor(),
        message: $('#message').val(),
    }

    usersSteps = [...usersSteps, step];

    let usersListContent = ``;

    for (let i = 0; i < usersStep.length; i += 1) {
        usersListContent += `
            <li data-id="${usersStep[i].id}">${usersStep[i].text}</li>
        `;
    }

    $(`#recipients-list`).append(`
        <li class="nav-item active" recipient-id="${step.id}" user-role="${step.role}">
            <input type="radio" name="recipient-radio" hidden />

            <div class="nav-link d-flex align-items-center justify-content-between" data-action="checkRadio">
                <div class="d-flex align-items-center">
                    <div class="recipientRoundBox hidden" style="background-color: ${step.color}"></div>

                    <span style="padding-left: 10px">
                        Étape ${step.stepNumber} 
                    </span>
                </div>
            </div>

            <div style="margin-left: 40px; ">
                <div>${step.role === 1 ? `Validateurs` : `Signataires`} (${step.processingDuration} heure(s)) : </div>

                <ul style="margin-left: 30px; ">
                    ${usersListContent}
                </ul>
            </div>
        </li>
    `);

    $('#recipients-modal').modal('toggle');
});

$('#recipients').on('change', (e) => {
    console.log($(e.currentTarget).val());
});

$('#send-validation-circuit').on('click', async () => {
    const { hasAccessToDocumentTypesHandling } = userStateManager.getUser();

    let payload = [];

    if (hasAccessToDocumentTypesHandling) {
        if (suppliersDocumentValidatorsSteps.length === 0) {
            alert("Veuillez sélectionner un type de document!");
    
            return;
        }

        payload = suppliersDocumentValidatorsSteps.map((usersStep) => {
            return {
                stepNumber: usersStep.stepNumber,
                processingDuration: usersStep.processingDuration,
                color: generateRandomColor(),
                usersId: usersStep.validators.map((user) => {
                    return user.id;
                }),
            };
        });
    } else {
        if (usersSteps.length === 0) {
            alert(`Veuillez ajouter au minimum une étape de validation!`);
    
            return;
        }

        payload = usersSteps.map((usersStep) => {
            return {
                stepNumber: usersStep.stepNumber,
                processingDuration: usersStep.processingDuration,
                color: usersStep.color,
                usersId: usersStep.usersId,
            };
        });
    }

    try {        
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/supplier_documents/${documentId}/validation_circuit`, {
            usersSteps: payload
        }, {
            withCredentials: true
        });

        window.location.reload();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});
