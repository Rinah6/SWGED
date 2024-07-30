import { generateRandomColor } from '../utils.js';
import { currentRecipientId, setCurrentRecipientId, fullHide } from './global.js';

const loader = $('#loader');

// async function renderDocumentRole() {
//     const { data } = await axios.get(apiUrl + `api/users_documents_roles`, {
//         withCredentials: true
//     });

//     let code = ``;

//     $.each(data, function (_, v) {
//         code += optionUI(v);
//     });
    
//     $(`#role`).html(code);
// }

$(document).ready(async () => {
    try {
        loader.removeClass('display-none');
        
        fullHide();
        
        $("#ISign").hide();
        $(`[card-id="field"]`).hide();
        $('#signature_tab').hide();

        // await renderDocumentRole();
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$('[usign]').on('click', () => {
    $("[usign]").addClass('active');
    $("[isign]").removeClass('active');
    $("[archiving]").removeClass('active');
    
    $("#ISign").hide();
    $("#YouSign").show();
    $('#archive').hide();
    
    $(`[card-id="object"]`).show();
    $(`[card-id="field"]`).hide();
    $(`[card-id="recipient"]`).show();
});

$('[isign]').on('click', () => {
    $("[usign]").removeClass('active');
    $("[isign]").addClass('active');
    $("[archiving]").removeClass('active');

    $(`[card-id="detail"]`).show();
    $(`[card-id="field"]`).show();
    $(`[card-id="recipient"]`).hide();

    $("#ISign").show();
    $(`[card-id="object"]`).show();
    $("#YouSign").hide();
    $('#archive').hide();

    setCurrentRecipientId('-1');

    // setUsersDocumentsList([{
    //     id: currentRecipientId,
    //     role: '',
    //     cc: '',
    //     message: '',
    //     color: generateRandomColor(),
    //     fields: []
    // }]);
});

$('[archiving]').on('click', () => {
    $("[isign]").removeClass('active');
    $("[usign]").removeClass('active');
    $("[archiving]").addClass('active');

    $("#ISign").hide();
    $("#YouSign").hide();
    $('#archive').show();

    $(`[card-id="object"]`).show();
    $(`[card-id="field"]`).hide();
    $(`[card-id="recipient"]`).hide();
});
