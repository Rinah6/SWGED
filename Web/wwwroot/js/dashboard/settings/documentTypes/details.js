import { users } from './global.js';
import { apiUrl } from '../../../apiConfig.js';
import { getChanges } from '../../../utils.js';

const loader = $('#loader');

let selectDocumentStepId = '';

let originalTitle = '';

let originalDocumentSteps = [];

let documentSteps = [];

$(document).on('click', '[data-document-type-id]', async (e) => {
    selectDocumentStepId = $(e.currentTarget).attr('data-document-type-id');

    const { data: documentTypeDetails } = await axios.get(apiUrl + `api/document_types/${selectDocumentStepId}`, {
        withCredentials: true,
    });

    if (documentTypeDetails.sites) {
        const sites = JSON.parse(documentTypeDetails.sites)
        const selectsites = document.querySelector('#select-current-sites');

        Array.from(selectsites.options).forEach(function (option) {

            if (sites.includes(option.value)) {
                option.selected = true;
            } else {
                option.selected = false;
            }

        });

        $('#select-current-sites').select2({
            dropdownParent: $('#document-type-details'),
        });
    }


    originalDocumentSteps = documentTypeDetails.steps.map((documentType) => {
        return {
            ...documentType,
            validators: documentType.validators.map((user) => user.id),
        };
    });

    originalTitle = documentTypeDetails.title;

    documentSteps = [...originalDocumentSteps];

    $('#current-document-type-title').val(documentTypeDetails.title);

    let content = ``;

    for (let i = 0; i < documentTypeDetails.steps.length; i += 1) {
        let foo = `<option value=""></option>`;

        for (let j = 0; j < users.length; j += 1) {
            foo += `
                <option value="${users[j].id}">${users[j].username}</option>
            `; 
        }

        content += `
            <li id="${documentTypeDetails.steps[i].id}" style="margin-bottom: 20px; ">
                <div>
                    Étape <span data-type="stepNumber">${documentTypeDetails.steps[i].stepNumber}</span>

                    <button class="btn btn-outline-danger" data-delete-document-step="${documentTypeDetails.steps[i].id}">Supprimer</button>
                </div>

                <div>
                    <div>
                        <label for="processing-description-${documentTypeDetails.steps[i].id}">Description: </label>
                        <input type="text" id="processing-description-${documentTypeDetails.steps[i].id}" data-type="processing-description" value="${documentTypeDetails.steps[i].processingDescription}" min="0" />
                    </div>

                    <div>
                        <label for="processing-duration-${documentTypeDetails.steps[i].id}">Durée de traitement (en heures): </label>
                        <input type="number" id="processing-duration-${documentTypeDetails.steps[i].id}" data-type="processing-duration" value="${documentTypeDetails.steps[i].processingDuration}" min="0" />
                    </div>

                    <div>
                        <label for="users-${documentTypeDetails.steps[i].id}">Validateurs : </label>
                        <select id="users-${documentTypeDetails.steps[i].id}" name="users-${documentTypeDetails.steps[i].id}[]" multiple="multiple" style="width: 200px;">
                            ${foo}
                        </select>
                    </div>
                </div>

                <hr />
            </li>
        `;
    }

    $('#current-steps').html(content);

    $('#current-steps').find(`select`).select2({
        dropdownParent: $('#document-type-details'),
    });

    $('#current-steps').find(`select`).each(function(i, element) {
        const selectedUsers = documentTypeDetails.steps[i].validators.map((user) => user.id);

        $(element).val(selectedUsers).trigger('change');
    });

    $('#document-type-details').modal('show');
});

$('#append-step-btn').on('click', () => {
    documentSteps.push({
        id: String(Date.now()),
        stepNumber: documentSteps.length + 1,
        processingDescription: '',
        processingDuration: 0,
        usersId: [],
    });

    const newStep = documentSteps[documentSteps.length - 1];

    $('#current-steps').append(`
        <li id="${newStep.id}" style="margin-bottom: 20px; ">
            <div>
                Étape <span data-type="stepNumber">${documentSteps.length}</span>

                <button class="btn btn-outline-danger" data-delete-document-step="${newStep.id}">Supprimer</button>
            </div>

            <div>

                <div>
                    <label for="processing-description-${newStep.id}">Description: </label>
                    <input type="text" id="processing-description-${newStep.id}" data-type="processing-description" value="${newStep.processingDescription}" min="0" />
                </div>                
                
                <div>
                    <label for="processing-duration-${newStep.id}">Durée de traitement (en heures): </label>
                    <input type="number" id="processing-duration-${newStep.id}" data-type="processing-duration" value="${newStep.processingDuration}" min="0" />
                </div>

                <div>
                    <label for="users-${newStep.id}">Validateurs : </label>
                    <select id="users-${newStep.id}" name="users-${newStep.id}[]" multiple="multiple" style="width: 200px;"></select>
                </div>
            </div>

            <hr />
        </li>
    `);

    let content = `<option value=""></option>`;

    for (let i = 0; i < users.length; i += 1) {
        content += `
            <option value="${users[i].id}">${users[i].username}</option>
        `;
    }

    $('#current-steps').find(`#users-${newStep.id}`).html(content).select2({
        dropdownParent: $('#document-type-details'),
    });
});

$(document).on('click', '[data-delete-document-step]', (e) => {
    const id = $(e.currentTarget).attr('data-delete-document-step');

    documentSteps = documentSteps.filter((documentStep) => {
        return documentStep.id !== id;
    });

    $(`#${id}`).remove();

    for (let i = 0; i < documentSteps.length; i += 1) {
        documentSteps[i].stepNumber = i + 1;

        $(`#${documentSteps[i].id}`).find('[data-type="stepNumber"]').text(i + 1);
    }
});

$('#delete-document-type').on('click', async () => {
    await axios.delete(apiUrl + `api/document_types/${selectDocumentStepId}`, {
        withCredentials: true,
    });

    window.location.reload();
});

$('#save-document-type-details').on('click', async () => {

    const select = document.querySelector('#select-current-sites');
    const currentSelectedSitesId = [];
    for (const option of select.options)
        option.selected && currentSelectedSitesId.push(option.value)


    if ($('#current-document-type-title').val() === '') {
        alert(`L'étiquette est obligatoire!`);

        return;
    }

    loader.removeClass('display-none');

    try {
        for (let i = 0; i < documentSteps.length; i += 1) {
            if ($('#current-steps').find(`#users-${documentSteps[i].id}`).val().length === 0) {
                alert(`L'étape ${documentSteps[i].stepNumber} doit contenir au moins un validateur!`);

                loader.addClass('display-none');

                return;
            }
        }

        documentSteps = documentSteps.map((documentStep) => {
            return {
                ...documentStep,
                processingDescription: $(`#processing-description-${documentStep.id}`).val(),
                processingDuration: Number($(`#processing-duration-${documentStep.id}`).val()),             
                validators: $(`#users-${documentStep.id}`).val(),
            };
        });

        const { additions: documentStepsAdditions, deletions: documentStepsDeletions } = getChanges(originalDocumentSteps, documentSteps);

        const updates = [];

        for (let i = 0; i < documentSteps.length; i += 1) {
            const documentStep = originalDocumentSteps.find((documentStep) => documentStep.id === documentSteps[i].id);

            if (documentStep === undefined) {
                continue;
            }

            if (documentStep.stepNumber !== documentSteps[i].stepNumber || documentStep.processingDescription !== documentSteps[i].processingDescription || documentStep.processingDuration !== documentSteps[i].processingDuration) {
                updates.push({
                    id: documentSteps[i].id,
                    stepNumber: documentSteps[i].stepNumber,
                    processingDescription: documentSteps[i].processingDescription,
                    processingDuration: Number(documentSteps[i].processingDuration),
                });
            }

            const { additions, deletions } = getChanges(documentStep.validators.map((userId) => ({ id: userId })), documentSteps[i].validators.map((userId) => ({ id: userId })));

            if (additions.length > 0) {
                await axios.post(apiUrl + `api/document_types/steps/${documentStep.id}/validators`, {
                    validatorsId: additions.map((user) => user.id),
                }, {
                    withCredentials: true,
                });
            }

            if (deletions.length > 0) {
                await axios.patch(apiUrl + `api/document_types/steps/${documentStep.id}/validators`, {
                    validatorsId: deletions.map((user) => user.id),
                }, {
                    withCredentials: true,
                });
            }
        }

        if (documentStepsAdditions.length > 0) {
            await axios.post(apiUrl + `api/document_types/${selectDocumentStepId}/steps`, {
                steps: documentStepsAdditions.map((documentStep) => {
                    return {
                        stepNumber: documentStep.stepNumber,
                        processingDescription: documentStep.processingDescription,
                        processingDuration: documentStep.processingDuration,
                        usersId: documentStep.validators,
                    };
                }),
            }, {
                withCredentials: true,
            });
        }

        if (documentStepsDeletions.length > 0) {
            await axios.patch(apiUrl + `api/document_types/steps`, {
                stepsId: documentStepsDeletions.map((documentStep) => documentStep.id),
            }, {
                withCredentials: true,
            });
        }

        if (updates.length > 0) {
            for (let i = 0; i < updates.length; i += 1) {
                await axios.patch(apiUrl + `api/document_types/steps/${updates[i].id}/details`, {
                    stepNumber: updates[i].stepNumber,
                    processingDescription: updates[i].processingDescription,
                    processingDuration: Number(updates[i].processingDuration),
                }, {
                    withCredentials: true,
                });
            }
        }

        const sites = JSON.stringify(currentSelectedSitesId);
        await axios.patch(apiUrl + `api/document_types/${selectDocumentStepId}/title`, {
            title: $('#current-document-type-title').val(),
            sites
        }, {
            withCredentials: true,
        });

        //if (originalTitle !== $('#current-document-type-title').val()) {
        //    await axios.patch(apiUrl + `api/document_types/${selectDocumentStepId}/title`, {
        //        title: $('#current-document-type-title').val(),
        //        sites
        //    }, {
        //        withCredentials: true,
        //    });
        //}

        window.location.reload();
    } catch (error) {
        alert(error.message); 
    } finally {
        loader.addClass('display-none');
    }
});
