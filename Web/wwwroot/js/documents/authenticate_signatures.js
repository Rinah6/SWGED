import { apiUrl, webUrl } from '../apiConfig.js';

const loader = $('#loader');

$('form').on('submit', async (e) => {
    e.preventDefault();

    const documentLink = $('#document-link').val();

    const id = documentLink.split(`${webUrl}documents/shared/`)[1];

    try {
        loader.removeClass('display-none');

        await axios.post(apiUrl + `api/verify_signed_document`, {
            documentId: id,
            digitalSignatureId: $('#signature-id').val(),
        }, {
            withCredentials: true,
        });

        $('#result').html(`<span style="color: green; ">La signature appartient à ce document.</span>`);
    } catch (error) {
        $('#result').html(`<span style="color: red; ">La signature n'appartient pas à ce document.</span>`);
    } finally {
        loader.addClass('display-none');
    }
});
