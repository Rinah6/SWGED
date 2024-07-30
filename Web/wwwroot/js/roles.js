import { apiUrl, webUrl } from './apiConfig.js';
import userStateManager from './store.js';
import { ws, setWs } from './global.js';

const loader = $('#loader');

const SERVER_HOST = apiUrl.split('http://')[1];

$(document).ready(async () => {
    setWs(new WebSocket(`ws://${SERVER_HOST}ws`));

    await userStateManager.init();

    const { username, role, hasAccessToNumericLibrary, hasAccessToProcessingCircuitsHandling } = userStateManager.getUser();

    $('#user').text(username);

    if (role === 0 || role === 1) {
        const { 
            hasAccessToInternalUsersHandling, 
            hasAccessToSuppliersHandling, 
            hasAccessToGlobalDynamicFieldsHandling, 
            hasAccessToTomProLinking,
            hasAccessToUsersConnectionsInformation,
            hasAccessToDocumentTypesHandling,
            hasAccessToSignMySelfFeature
        } = userStateManager.getUser();

        const projectsManagementLink = role === 0 ? `
            <a href="${webUrl}soas" class="nav-link" id="soas_management">
                <i class="nav-icon fas fa-id-card-alt"></i>
                <p>
                    Gestion des SOA
                </p>
            </a>

            <a href="${webUrl}projects" class="nav-link" id="projects_management">
                <i class="nav-icon fas fa-building"></i>
                <p>
                    Gestion des projets
                </p>
            </a>

            <a href="${webUrl}gestion_sites" class="nav-link" id="sites_management">
                <i class="nav-icon fas fa-keyboard"></i>
                <p>
                    Gestion des sites
                </p>
            </a>

        ` : '';

        // <i class="nav-icon fas fa-male"></i>

        const suppliersManagementLink = role === 1 && hasAccessToSuppliersHandling ? `
            <a href="${webUrl}dashboard/suppliers" class="nav-link">
                <img src="/icons/supplier.png" alt="Suppliers" width="20" height="20" />

                <p>
                    Liste des fournisseurs
                </p>
            </a>
        ` : '';

        $('#espace_client').html(`
            <li class="nav-header" data-id="w-menu">
                Espace client
            </li>
            <li class="nav-item" id="dashboard"></li>
        `);

        if (hasAccessToInternalUsersHandling) {
            $('#espace_client').find('#dashboard').append(`
                ${projectsManagementLink}
                <a href="${webUrl}users" class="nav-link">
                    <i class="nav-icon fas fa-male"></i>
                    <p>
                        Gestion des utilisateurs
                    </p>
                </a>
                ${suppliersManagementLink}
            `);
        }

        if (hasAccessToSignMySelfFeature) {
            $('#espace_client').find('#dashboard').append(`
                <a href="${webUrl}authenticate_document" class="nav-link">
                    <i class="nav-icon fas fa-signature"></i>
                    <p>
                        Vérification de signatures
                    </p>
                </a>
            `);
        }

        if (hasAccessToNumericLibrary) {
            $('#espace_client').find('#dashboard').append(`
                <a href="${webUrl}numeric_library" class="nav-link">
                    <i class="nav-icon fas fa-folder-open"></i>
                    <p>
                        Bibliothèque numérique
                    </p>
                </a>
            `);
        }

        if (role !== 0) {
            $('#espace_client').append(`
            <li class="nav-flat"><hr /></li>
            <li class="nav-header" data-id="w-menu">
                Paramétrages
            </li>
        `);
        }

        let settings = ``;

        if (role === 1 && hasAccessToSuppliersHandling) {
            settings += `
                <a href="${webUrl}dashboard/suppliers_documents_receivers" class="nav-link">
                    <img src="/icons/receiver.png" alt="Suppliers" width="20" height="20" />

                    <p>
                        Receveurs de documents des fournisseurs
                    </p>
                </a>
            `;
        }

        if (hasAccessToGlobalDynamicFieldsHandling) {
            settings += `
                <a href="${webUrl}dynamic_fields" class="nav-link">
                    <i class="nav-icon fas fa-cogs"></i>
                    <p>
                        Champs dynamiques globaux
                    </p>
                </a>
            `;
        }

        if (role === 1 && hasAccessToDocumentTypesHandling) {
            settings += `
                <a href="${webUrl}document_types" class="nav-link">
                    <img src="/icons/document-type.svg" alt="Document types" width="20" height="20" />

                    <p>
                        Types de document
                    </p>
                </a>
            `
        }

        

        if (role === 1 && hasAccessToTomProLinking) {
            settings += `
                <a href="${webUrl}tom_pro_connections" id="tom-pro-db-settings" class="nav-link">
                    <img src="/icons/db.svg" alt="DB Connection" width="20" height="20" />

                    <p>
                        Mappage à une base TOMATE
                    </p>
                </a>
            `;
        }

        $('#espace_client').append(`
            <li class="nav-item">
                ${settings}
            </li>
        `);

        if (hasAccessToUsersConnectionsInformation) {
            $('#dashboard').append(`
                <li class="nav-item">
                    <a href="${webUrl}users_connections" class="nav-link">
                        <img src="/icons/login.svg" alt="DB Connection" width="20" height="20" />

                        <p>
                            Connexions des utilisateurs
                        </p>
                    </a>
                </li>
            `);
        }

        $('#espace_client').append(`
            <li class="nav-flat"><hr /></li>
        `);
    }

    if (hasAccessToProcessingCircuitsHandling) {
        $('#espace_client').find('#dashboard').append(`
            <a href="${webUrl}documents_dashboard" class="nav-link">
                <i class="nav-icon fas fa-book"></i>
                <p>
                    Tableau de bord
                </p>
            </a>  
        `);
    }

    if (role !== 0) {
        const { isADocumentsReceiver } = userStateManager.getUser();

        if (isADocumentsReceiver) {
            $('#links-list').prepend(`
                <li class="nav-item active">
                    <div class="nav-link" document-status="-1">
                        <i class="fa fa-boxes-packing"></i> <span document-name>Reçus des fournisseurs</span>
                        <span class="badge bg-secondary float-right" data-id="Foo">0</span>
                    </div>
                </li>
            `);
        }
    }
});

$(document).on('click', '#tom-pro-db-settings', () => {
    $('#tom-pro-db-settings-modal').modal('show');
});

$('[data-id="disconnect"]').on('click', async () => {
    loader.removeClass('display-none');

    try {
        await axios.delete(apiUrl + `api/logout`, {
            withCredentials: true
        }); 
    } catch (error) {
        alert(error.message);
    } finally {
        if (ws !== undefined) {
            ws.send(1);

            ws.close();
        }

        loader.addClass('display-none');

        window.location = webUrl;
    }
});
