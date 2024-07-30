import { apiUrl, webUrl } from '../../../apiConfig.js';
import userStateManager from '../../../store.js';

const loader = $('#loader');
const dynamic_field_types = $('#dynamic-field-types');
const addValuesContainer = $('#add-values-container');
const addGlobalDynamicFieldModal = $('#add-global-dynamic-field-modal');

const globalDynamicFieldsList = $('#dynamic-fields-list');

let values = [];

let id = '';

async function getDynamicFieldsTypes(callback) {
    const { data } = await axios.get(apiUrl + 'api/dynamic_fields/global/types', {
        withCredentials: true
    });

    callback(data);
}

async function getGlobalDynamicFields(callback) {
    loader.removeClass('display-none');

    const { data } = await axios.get(apiUrl + 'api/dynamic_fields/global/list', {
        withCredentials: true
    });

    callback(data);
}

function displayDynamicFieldTypes(types) {
    for (let i = 0; i < types.length; i += 1) {
        const option = document.createElement('option');

        option.text = types[i].title;
        option.value = types[i].id;

        dynamic_field_types.append(option);
    }
}

function displayGlobalDynamicFields(globalDynamicFields) {
    for (let i = 0; i < globalDynamicFields.length; i += 1) {
        const li = document.createElement('li');

        li.id = globalDynamicFields[i].id;
        li.innerText = globalDynamicFields[i].label;
        li.classList.add('list');

        li.addEventListener('click', async () => {
            try {
                loader.removeClass('display-none');
        
                await userStateManager.init();
        
                const { hasAccessToSuppliersHandling } = userStateManager.getUser();
        
                if (!hasAccessToSuppliersHandling) {
                    $('#access-control-container').remove();
                }

                id = globalDynamicFields[i].id;
        
                await getGlobalDynamicFieldDetails(globalDynamicFields[i].id, displayGlobalDynamicFieldDetails);

                $('#dynamic-field-details').modal('toggle');
            } catch (error) {
                alert(error.message);
            } finally {
                loader.addClass('display-none');
            }
        });

        globalDynamicFieldsList.append(li);
    }
}

window.addEventListener('load', async () => {
    try {
        loader.removeClass('display-none');

        await userStateManager.init();

        const { role, hasAccessToGlobalDynamicFieldsHandling } = userStateManager.getUser();

        if (role !== 1 || !hasAccessToGlobalDynamicFieldsHandling) {
            window.location.href = webUrl + `404`;

            return;
        }
        
        const { hasAccessToSuppliersHandling } = userStateManager.getUser();

        if (!hasAccessToSuppliersHandling) {
            $('#access-control-container').remove();
        }

        $('#values-detail').hide();

        await getDynamicFieldsTypes(displayDynamicFieldTypes);

        await getGlobalDynamicFields(displayGlobalDynamicFields);
    } catch (error) {
        console.log(error.message);
    } finally {
        loader.addClass('display-none');

        addValuesContainer.hide();
    }
});

$('#add-global-dynamic-field').on('click', () => {
    addGlobalDynamicFieldModal.modal('toggle');
});

$('[data-action="cancel"]').on('click', () => {
    addGlobalDynamicFieldModal.modal('toggle');
});

dynamic_field_types.on('change', (e) => {
    values = [];

    const value = Number(e.target.value);

    if (value === 0 || value === 1 || value === 2 || String(value) === '' || value === 5) {
        addValuesContainer.hide();

        return;
    }

    addValuesContainer.show();

    const valuesList = $('#values');
    valuesList.empty();

    const textValue = $('#text-value');
    textValue.val('');

    const addValue = $('#add-value');

    textValue.on('keydown', (e) => {
        if (e.keyCode === 13) {
            addValue.click();
        }
    });

    addValue.on('click', () => {
        if (textValue.val() === '') {
            return;
        }

        const valueList = document.createElement('li');

        valueList.innerText = textValue.val();

        valueList.classList.add('list-group-item');

        values.push(textValue.val());

        valuesList.append(valueList);

        textValue.val('');
    });
});

function getValues() {
    const value = Number(dynamic_field_types.val());

    if (value === 0 || value === 1 || value === 2 || value === 5) {
        return null;
    }

    return values;
}

$('[data-action="post"]').on('click', async () => {
    if (String(dynamic_field_types.val()) === '') {
        alert("L'étiquette ne doit pas être vide!");

        return;
    }

    const values_ = getValues();

    if (values_ !== null && values_.length <= 1) {
        alert("Les valeurs ne doivent pas être vides!");

        return;
    }

    try {
        loader.removeClass('display-none');

        const hasAccessToSuppliersHandling = $('#access-control-container').length > 0;

        const isForUsersProject = hasAccessToSuppliersHandling ? $('#is-for-users-project').is(':checked') : true;
        const isForSuppliers = hasAccessToSuppliersHandling ? $('#is-for-suppliers').is(':checked') : false;

        await axios.post(apiUrl + `api/dynamic_fields/global`, {
            label: $('#label').val(),
            isForUsersProject: isForUsersProject,
            isForSuppliers: isForSuppliers,
            isRequired: $('#isRequired').is(':checked'),
            type: Number(dynamic_field_types.val()),
            values: values_
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

const addValueModal = $('#add-value-modal');

function displayGlobalDynamicFieldDetails(globalDynamicField) {
    $('#label-details').val(globalDynamicField.label);
    $('#dynamic-field-title').text(globalDynamicField.label);

    $('#type').text(globalDynamicField.type);

    $('#is-for-users-project-details').prop('checked', globalDynamicField.isForUsersProject);
    $('#is-for-suppliers-details').prop('checked', globalDynamicField.isForSuppliers);
    $('#is-required-details').prop('checked', globalDynamicField.isRequired);
    
    if (globalDynamicField.values !== undefined && globalDynamicField.values.length > 0) {
        $('#values-detail').show();

        const values = $('#values-list');

        values.html('');

        for (let i = 0; i < globalDynamicField.values.length; i += 1) {
            const dynamicfieldItemId = globalDynamicField.values[i].id;

            values.append($(`
                <li id="${dynamicfieldItemId}" class="list-group-item" style="display: flex; align-items: center; gap: 20px; margin-bottom: 10px;">
                    <span>${globalDynamicField.values[i].value}</span>
                    
                    <span aria-hidden="true" data-id="${dynamicfieldItemId}" class="text-red" style="cursor: pointer; font-size: 24px;">
                        &times;
                    </span>
                </li>
            `));

            $(`span[data-id="${dynamicfieldItemId}"]`).on('click', async () => {
                if (globalDynamicField.values.length === 2) {
                    alert(`Le champ dynamique global doit avoir au minimum 2 valeurs!`);

                    return;
                }

                const res = confirm('Voulez-vous supprimer cette valeur du champ dynamique global ?');

                if (!res) {
                    return;
                }

                try {
                    loader.removeClass('display-none');

                    await axios.delete(apiUrl + `api/dynamic_fields/global/${dynamicfieldItemId}/types`, {
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
    } else {
        $('#values-detail').hide();
    }
}

async function getGlobalDynamicFieldDetails(id, callback) {
    const { data } = await axios.get(apiUrl + `api/dynamic_fields/global/${id}`, {
        withCredentials: true
    });

    callback(data);
}

$('#add-value-btn').on('click', () => {
    addValueModal.modal('toggle');
});

$('[data-action="cancel"]').on('click', () => {
    addValueModal.modal('toggle');
});

$('[data-action="close-details"]').on('click', () => {
    $('#dynamic-field-details').modal('toggle');
});

$('[data-action="add"]').on('click', () => {
    $('#add-value-form').submit();
});

$('#add-value-form').on('submit', async (e) => {
    e.preventDefault();

    const value = $('#text-value-details').val();

    if (value === '') {
        alert('Une valeur est requise!');

        return;
    }

    try {
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/dynamic_fields/global/items`, {
            value,
            dynamicFieldId: id
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

$('#delete-global-dynamic-field-btn').on('click', async () => {
    const res = confirm("Voulez-vous réellement supprimer ce champ dynamique global ?");

    if (!res) {
        return;
    }

    try {
        loader.removeClass('display-none');

        await axios.delete(apiUrl + `api/dynamic_fields/global/${id}`, {
            withCredentials: true
        });

        window.location.reload();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#label-details').on('keydown', async (e) => {
    if (e.which === 13) {
        const newLabel = $(e.currentTarget).val();

        if (newLabel === '') {
            return;
        }

        try {
            loader.removeClass('display-none');

            await axios.patch(apiUrl + `api/dynamic_fields/global/${id}/title`, {
                label: newLabel
            }, {
                withCredentials: true
            });

            window.location.reload();
        } catch (error) {
            alert(error.message);
        } finally {
            loader.addClass('display-none');
        }
    }
});

$('#is-required-details').on('change', async (e) => {
    try {
        loader.removeClass('display-none');

        await axios.patch(apiUrl + `api/dynamic_fields/global/${id}/requirement`, {
            isRequired: e.target.checked
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

$('#is-for-users-project-details').on('change', async (e) => {
    try {
        loader.removeClass('display-none');

        await axios.patch(apiUrl + `api/dynamic_fields/global/${id}/visibility/users-project`, {
            isForUsersProject: e.target.checked
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

$('#is-for-suppliers-details').on('change', async (e) => {
    try {
        loader.removeClass('display-none');

        await axios.patch(apiUrl + `api/dynamic_fields/suppliers/${id}/global/visibility`, {
            isForSuppliers: e.target.checked
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
