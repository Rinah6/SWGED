import { apiUrl } from '../../apiConfig.js';
import { documentId } from './global.js';

const loader = $('#loader');

$(document).on('click', `[download_button]`, async (e) => {
	loader.removeClass('display-none');

	try {
		const { data: pdfBlob } = await axios.get(apiUrl + `api/pdf/${documentId}`, {
			withCredentials: true,
			responseType: 'blob'
		});

		const blobUrl = URL.createObjectURL(pdfBlob);
		const documentName = $("[download_button]").attr("download_button");
		const a = document.createElement("a");
		a.href = blobUrl;
		a.download = documentName;
		document.body.appendChild(a);
		a.click();
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});
