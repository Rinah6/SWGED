import { apiUrl } from '../../../apiConfig.js';
import userStateManager from '../../../store.js';

const loader = $('#loader');

let selectSiteId = '';

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');

        await userStateManager.init();

        const { role } = userStateManager.getUser();

        if (role !== 0) {
            window.location.href = webUrl + `404`;

            return;
        }

        const { data: Sites } = await axios.get(apiUrl + `api/sites`, {
            withCredentials: true
        });

        let content = '';

        for (let i = 0; i < Sites.length; i += 1) {
            content += `
                <li data-site-id="${Sites[i].id}" class="document-type">
                    <span>${Sites[i].siteId} - ${Sites[i].name}</span>
                </li>
            `;
        }

        $('#sites-list').html(content);


    } catch (error) {
        console.log(error);
    } finally {
        loader.addClass('display-none');
    }
});

$('#add-site').on('click', async () => {
    $('h4.card-title').text('Nouveau site');

    $('#add-site-modal').modal('toggle');
});

$('#close-add-site-modal').on('click', () => {

    $('#add-site-modal').modal('toggle');
});


$('#post-site').on('click', async () => {
    if ($('#new-code-label').val() === '') {
        alert(`Le code est obligatoire!`);

        return;
    }
    if ($('#new-site-label').val() === '') {
        alert(`Le nom est obligatoire!`);

        return;
    }
    try {
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/sites`, {
            siteid: $('#new-code-label').val(),
            name: $('#new-site-label').val(),


        }, {
            withCredentials: true
        });

        window.location.reload();
    } catch (error) {
        alert(error.response.data);
        //console.log(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$(document).on('click', '[data-site-id]', async (e) => {
    selectSiteId = $(e.currentTarget).attr('data-site-id');

    const { data: siteDetails } = await axios.get(apiUrl + `api/sites/${selectSiteId}`, {
        withCredentials: true,
    });
    $('#current-site-siteid').val(siteDetails.siteId);
    $('#current-site-name').val(siteDetails.name);

    $('h4.card-title').text(siteDetails.name);

    $('#site-details').modal('show');
});

$('#delete-site').on('click', async () => {
    await axios.delete(apiUrl + `api/sites/${selectSiteId}`, {
        withCredentials: true,
    });

    window.location.reload();
});

$('#save-site-details').on('click', async () => {
    if ($('#current-site-siteid').val() === '') {
        alert(`Le code est obligatoire!`);

        return;
    }
    if ($('#current-site-name').val() === '') {
        alert(`Le nom est obligatoire!`);

        return;
    }

    const siteid = $("#current-site-siteid").val();
    const name = $("#current-site-name").val();

    loader.removeClass('display-none');

    try {

        await axios.patch(apiUrl + `api/sites/${selectSiteId}`, {
            siteid,
            name,
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