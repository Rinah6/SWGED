import { apiUrl } from '../apiConfig.js';
import { globalDynamicFields } from './global.js';
import userStateManager from '../store.js';

const loader = $('#loader');

const dynamic_fields = $('#dynamic_fields');

async function getGlobalDynamicFields(callback) {
    $.ajax({
        type: 'GET',
        async: true,
        url: apiUrl + 'api/dynamic_fields/global',
        contentType: 'application/json',
        datatype: 'json',
        xhrFields: { withCredentials: true },
        beforeSend: function () {
            loader.removeClass('display-none');
        },
        complete: function () {
            loader.addClass('display-none');
        },
        success: function (result) {
            callback(result);
        },
        Error: function (_, e) {
            alert(e);
        }
    });
}

function displayGlobalDynamicFields(_globalDynamicFields) {
    for (let i = 0; i < _globalDynamicFields.length; i += 1) {
        const container = document.createElement('div');
        container.classList.add('input-group', 'mb-2');

        const parent = document.createElement('div');
        parent.classList.add('input-group-prepend');

        const span = document.createElement('label');
        span.classList.add('input-group-text');

        if (_globalDynamicFields[i].isRequired) {
            span.innerText += '*** ';
        }

        span.innerText += _globalDynamicFields[i].label;

        let input = document.createElement('input');
        
        switch (_globalDynamicFields[i].type) { 
            case 1:
                input.classList.add("form-control");
                input.type = 'date';

                break;
            case 2:
                input.type = 'checkbox';
                input.classList.add("form-check");                
                input.addEventListener('change', (e) => {
                    e.target.value = e.target.checked;
                });

                input.style.marginRight = `10px`;

                span.classList.remove('input-group-text');

                break;
            case 3:
                input = document.createElement('select');
                input.classList.add("form-select");
                input.innerHTML = `<option value="" selected> -- Sélectionner une option -- </option>`;

                for (let j = 0; j < _globalDynamicFields[i].values.length; j += 1) {
                    input.innerHTML += `
                        <option value="${_globalDynamicFields[i].values[j].value}" name="${_globalDynamicFields[i].id}">${_globalDynamicFields[i].values[j].value}</option>
                    `;
                }

                break;
            case 4:
                input = document.createElement('div');

                for (let j = 0; j < _globalDynamicFields[i].values.length; j += 1) {
                    input.innerHTML += `
                        <div style="margin-left: 10px;">
                            <input type="radio" id="${_globalDynamicFields[i].values[j].id}" value="${_globalDynamicFields[i].values[j].value}" name="${_globalDynamicFields[i].id}" />
                            <label for="${_globalDynamicFields[i].values[j].id}">${_globalDynamicFields[i].values[j].value}</label>
                        </div>
                    `;
                }

                break;
            case 5:
                input.classList.add("form-control");

                input.type = 'file';
                input.id = _globalDynamicFields[i].id;

                break;
            default:
                input.type = 'text';
                input.classList.add("form-control");

                break;
        }

        switch (_globalDynamicFields[i].type) {
            case 2:
                span.htmlFor = _globalDynamicFields[i].id;
                input.id = _globalDynamicFields[i].id;

                container.appendChild(input);
                container.appendChild(span);

                globalDynamicFields.push({ id: _globalDynamicFields[i].id, isRequired: _globalDynamicFields[i].isRequired, isRadioButton: false, isOfTypeFile: false });

                break;
            case 4:
                container.appendChild(span);
                container.appendChild(input);

                globalDynamicFields.push({ id: _globalDynamicFields[i].id, isRequired: _globalDynamicFields[i].isRequired, isRadioButton: true, isOfTypeFile: false });

                break;
            case 5:
                container.appendChild(span);
                container.appendChild(input);

                globalDynamicFields.push({ id: _globalDynamicFields[i].id, isRequired: _globalDynamicFields[i].isRequired, isRadioButton: false, isOfTypeFile: true });

                break;
            default:
                span.htmlFor = _globalDynamicFields[i].id;
                input.id = _globalDynamicFields[i].id;

                parent.appendChild(span);
                container.appendChild(parent);
                container.appendChild(input);

                globalDynamicFields.push({ id: _globalDynamicFields[i].id, isRequired: _globalDynamicFields[i].isRequired, isRadioButton: false, isOfTypeFile: false });

                break;
        }

        dynamic_fields.append(container);
    }
}

window.addEventListener('load', async () => {
    await userStateManager.init();

    const { hasAccessToGlobalDynamicFieldsHandling } = userStateManager.getUser();

    if (hasAccessToGlobalDynamicFieldsHandling) {
        await getGlobalDynamicFields(displayGlobalDynamicFields);
    }
});
