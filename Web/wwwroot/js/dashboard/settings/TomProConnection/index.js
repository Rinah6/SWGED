import { apiUrl } from '../../../apiConfig.js';
import userStateManager from '../../../store.js';

const loader = $('#loader');

let databaseId = '';

$(document).ready(async () => {
    try {
        await userStateManager.init();

        const { role, hasAccessToTomProLinking } = userStateManager.getUser();

        if (role === 1 && hasAccessToTomProLinking) {
            const { data } = await axios.get(apiUrl + `api/tom_pro_db_connections/databases/project`, {
                withCredentials: true
            });
    
            $('#server-name').val(data.serverName ?? '');
    
            databaseId = data.database ?? '';
    
            $('#authentication-modes').select2({
                dropdownParent: $('#tom-pro-db-settings-modal')
            });
        }
    } catch (error) {
        alert(error.message);
    } finally {
        // loader.addClass('display-none');
    }
});

$('#server-name').on('input', () => {
    $('#connection-btn').show();

    $('#save-db-connection-container').find('#save-db-connection-btn').remove();
    
    $('#databases-container').html('');
});

$('#authentication-modes').on('change', (e) => {
    $('#connection-btn').show();

    $('#save-db-connection-container').find('#save-db-connection-btn').remove();

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
            serverName: $('#server-name').val(),
            login: !login ? undefined : login,
            password: !password ? undefined : password
        }, {
            withCredentials: true
        });
        
        $('#save-db-connection-container').html('');

        $('#connection-btn').hide();

        $('#sa-password').find('#login').prop('disabled', true);
        $('#sa-password').find('#password').prop('disabled', true);

        $('#databases-container').html(`
            <div>
                <label for="authentication-modes">Bases de données: </label>

                <select id="databases"></select>
            </div>
        `);

        let tmp = '<option selected value=""></option>';

        for (let i = 0; i < databases.length; i += 1) {
            tmp += `
                <option value="${databases[i].id}">${databases[i].name}</option>
            `;
        }

        $('#databases-container').find('#databases').html(tmp);

        $('#databases-container').find('#databases').select2({
            dropdownParent: $('#tom-pro-db-settings-modal')
        });

        $('#databases-container').find('#databases').val(databaseId).trigger('change');
    } catch (error) {
        alert(`Échec de la connexion à l'instance!`);
    } finally {
        loader.addClass('display-none');
    }
});
