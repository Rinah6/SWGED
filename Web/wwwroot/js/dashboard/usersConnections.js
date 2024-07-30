import { apiUrl, webUrl } from '../apiConfig.js';
import userStateManager from '../store.js';
import { formatDate } from '../utils.js';

const loader = $('#loader');

let usersConnections = [];

$(document).ready(async () => {
    loader.removeClass('display-none');

    try {
        await userStateManager.init();

        const { hasAccessToUsersConnectionsInformation } = userStateManager.getUser();

        if (!hasAccessToUsersConnectionsInformation) {
            window.location.href = webUrl + `404`;

            return;
        }

        const { data } = await axios.get(apiUrl + `api/users_connections`, {
            withCredentials: true
        });

        usersConnections = data;
    
        let content = '';
    
        let numberOfConnectedUsers = 0;
    
        for (let i = 0; i < data.length; i += 1) {
            const status = data[i].endDate !== undefined ? `
                <div style="width: 20px; height: 20px; border-radius: 50%; background-color: red; border: none; "></div>
            ` : `
                <div style="width: 20px; height: 20px; border-radius: 50%; background-color: green; border: none; "></div>
            `;
    
            if (data[i].endDate === undefined) {
                numberOfConnectedUsers += 1;
            }
    
            content += `
                <tr data-user-connected-id="${data[i].id}" data-type="users-connections">
                    <td>${status}</td>
                    <td>${data[i].username}</td>
                    <td>${data[i].lastName}</td>
                    <td>${data[i].firstName}</td>
                    <td>${formatDate(data[i].creationDate)}</td>
                    <td>${data[i].endDate === undefined ? '' : formatDate(data[i].endDate)}</td>
                </tr>
            `;
        }
    
        $('#users-connections').html(content);
    
        $('#connected-users-count').text(numberOfConnectedUsers);
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('#wsearch').on('keyup', function () {
    const value = $(this).val().toLowerCase();

    $(`[data-type="users-connections"]`).filter(function () {
        const parent = $(this).closest(`[data-type="users-connections"]`);

        parent.toggle(parent.text().toLowerCase().indexOf(value) > -1);
    });
});

$(document).on('click', '[data-user-connected-id]', async (e) => {
    loader.removeClass('display-none');

    try {
        const id = $(e.currentTarget).attr('data-user-connected-id');
    
        const { data } = await axios.get(apiUrl + `api/connections_history/users/${id}`, {
            withCredentials: true
        });

        let content = '';
    
        for (let i = 0; i < data.length; i += 1) {
            content += `
                <tr>
                    <td>${formatDate(data[i].creationDate)}</td>
                    <td>${data[i].endDate === undefined ? '' : formatDate(data[i].endDate)}</td>
                </tr>
            `;
        }

        const username = usersConnections.find(user => user.id === id).username;
    
        $('#user-connections-history').html(content);

        $('#modal-title').text(username);

        $('#user-connections-history-modal').modal('toggle');
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});
