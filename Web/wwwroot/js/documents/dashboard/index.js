import { apiUrl, webUrl } from '../../apiConfig.js';
import { pastPeriod, formatDate } from '../../utils.js';

const loader = $('#loader');

$(document).ready(async () => {
    loader.removeClass('display-none');

    const { data: documents } = await axios.get(apiUrl + `api/late_documents`, {
        withCredentials: true,
    });

    for (let i = 0; i < documents.length; i += 1) {
        $(`#documents-list`).append(documentsListUI(documents[i]));
    }

    loader.addClass('display-none');
});

function documentsListUI(document) {
	const creationDate = formatDate(document.creationDate);

	return `
		<li 
			class="dropdown document" 
            data-id="${document.id}"
		>
			<div class="mailbox-name">${document.title}</div>

            <div class="mailbox-date">${creationDate}</div>

            <div class="mailbox-date">${pastPeriod(document.creationDate)}</div>
		</li>
	`;
}

$('#documents-status').on('change', async (e) => {
    const url = $(e.currentTarget).val() === '1' ? `late_documents` : `non_late_documents`;

    const { data: documents } = await axios.get(apiUrl + `api/${url}`, {
        withCredentials: true,
    });

    for (let i = 0; i < documents.length; i += 1) {
        $(`#documents-list`).append(documentsListUI(documents[i]));
    }
});


$(document).on('click', '[data-id]', (e) => {
    const documentId = $(e.currentTarget).attr('data-id');

    window.location.href = webUrl + `documents/${documentId}/validation_history`;
})
