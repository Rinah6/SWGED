import { apiUrl, webUrl } from '../apiConfig.js';

const loader = $('#loader');

let nifInput = $('#nif');
let stat = $('#stat');
let name = $('#name');
let mail = $('#mail');
let contact = $('#contact');
let cin = $('#cin');

let isASignupOperation = false;

let projectId = '';

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');

        projectId = window.location.href.split('suppliers/')[1];

        await axios.get(apiUrl + `api/suppliers/check_projects?Id=${projectId}`);

        $('#without-nif-and-stat').prop('checked', false);

        $('#cin-container').html(``);
        $('#mail-container').html(``);
        $('#contact-container').html(``);

    } catch (error) {
        $('body').html(`
            <h1 style="font-size: 128px; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">404</h1>
        `);
    } finally {
        loader.addClass('display-none');
    }
});

$('#without-nif-and-stat').on('change', (e) => {
    if ($(e.currentTarget).prop('checked') === true) {
        $('#nif-and-stat-container').html(``);

        $('#cin-container').html(`
            <div class="input-group mb-3">
                <input type="text" placeholder="N° CIN" id="cin" class="form-control" />
            </div>
        `);
    } else {
        $('#nif-and-stat-container').html(`
            <div class="input-group mb-3">
                <input type="text" placeholder="NIF" id="nif" class="form-control" />
            </div>

            <div class="input-group mb-3">
                <input type="text" placeholder="STAT" id="stat" class="form-control" />
            </div>
        `);

        $('#cin-container').html(``);
    }
});

$('#signin-signup-toggler').on('click', () => {
    isASignupOperation = !isASignupOperation;

    if (isASignupOperation) {
        $('#auth-title').text("Création d'un nouveau compte");
        $('#signin-signup-toggler').text('Déja inscrit');
        $('#suppliers-form-button').text("Créer un nouveau compte");

        $('#mail-container').html(`
            <div class="input-group mb-3">
                <input type="email" placeholder="MAIL" id="mail" class="form-control" />
            </div>
        `);

        $('#contact-container').html(`
            <div class="input-group mb-3">
                <input type="text" placeholder="CONTACT" id="contact" class="form-control" />
            </div>
        `);

        return;
    }
    
    $('#auth-title').text('Connexion');
    $('#signin-signup-toggler').text("Créer un nouveau compte");
    $('#suppliers-form-button').text('Se connecter');

    $('#mail-container').html(``);
    $('#contact-container').html(``);
});

$('#suppliers-form').on('submit', async (e) => {

    nifInput = $('#nif');
    stat = $('#stat');
    name = $('#name');
    mail = $('#mail');
    contact = $('#contact');
    cin = $('#cin');

    e.preventDefault();

    let payload = {};

    if (name.val() === '') {
        alert('Le nom du fournisseur est obligatoire!');

        return;
    }

    if ($('#without-nif-and-stat').prop('checked') === true) {

        if (cin.val() === '') {
            alert('Le N° CIN est obligatoire si sans NIF et STAT!');

            return;
        }

        payload = {
            name: name.val(),
            cin: cin.val(),
            projectId
        };

    } else {
        if (nifInput.val() === '') {
            alert('Le NIF est obligatoire!');
    
            return;
        }

        if (nifInput.val().length !== 10) {
            alert('Le NIF doit être exactement de 10 caractères de longueur!');

            return;
        } 

        if (stat.val() === '') {
            alert('Le STAT est obligatoire!');
    
            return;
        }

        if (stat.val().length !== 17) {
            alert('Le STAT doit être exactement de 17 caractères de longueur!');

            return;
        }

        //if (contact.val() === '') {
        //    alert('Le CONTACT est obligatoire!');

        //    return;
        //}

        payload = {
            nif: nifInput.val(),
            stat: stat.val(),
            contact: contact.val(),
            mail: mail.val(),
            name: name.val(),
            cin: cin.val(),
            projectId
        };
    }

    if (isASignupOperation) {
        try
        {
            loader.removeClass('display-none');

            //if (mail.val() === '') {
            //    alert('Le MAIL est obligatoire!');

            //    return;
            //}

            await axios.post(apiUrl + `api/suppliers/register`, {
                ...payload
            });
    
            window.location.href = webUrl + `suppliers/${projectId}`;

        } catch (error) {
            alert('Compte déjà existant!');
        } finally {
            loader.addClass('display-none');
        }
    }
    else
    {
        try
        {
            loader.removeClass('display-none');
    
            await axios.post(apiUrl + `api/suppliers/auth`, {
                ...payload
            }, {
                withCredentials: true
            });

            window.location.href = webUrl + `suppliers/new_document`;
        } catch (error) {
            alert('Identifiants erronés!');
        } finally {
            loader.addClass('display-none');
        }
    }
});
