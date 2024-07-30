import { documentId } from './global.js';

const loader = $('#loader');

$(document).on('input', '#physical-location', () => {
	$(`#Panel`).find('#set-physical-location').css({ 'visibility': 'visible' });
});

$(document).on('keydown', '#physical-location', (e) => {
	if (e.code === 'Enter') {
		e.preventDefault();
	}
});

$(document).on('click', '#set-physical-location', async () => {
	const physicalLocation = $(`#Panel`).find('#physical-location').text();

	if (!physicalLocation) {
		alert("L'emplacement physique ne doit pas être vide!");

		return;
	}

    loader.removeClass('display-none');

    await axios.patch(apiUrl + `api/documents/physical_location`, {
        id: documentId,
		physicalLocation,
    }, {
        withCredentials: true,
    });

    loader.addClass('display-none');
});
