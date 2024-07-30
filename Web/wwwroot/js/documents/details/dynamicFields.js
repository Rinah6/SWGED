import { apiUrl } from '../../apiConfig.js';
import { documentId } from './global.js';

const loader = $('#loader');

$(document).on('click', '[dynamic-field-id]', async (e) => {
	try {
		loader.removeClass('display-none');

		const id = $(e.currentTarget).attr('dynamic-field-id');

		const { data } = await axios.get(apiUrl + `api/download/dynamic_attachements/${id}?documentId=${documentId}`, {
			withCredentials: true,
			responseType: 'blob',
		});

		const blobUrl = URL.createObjectURL(data);
		const filename = $(e.currentTarget).find('figcaption').text();
		const a = document.createElement('a');

		a.href = blobUrl;
		a.download = filename;
		document.body.appendChild(a);
		a.click();
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});
