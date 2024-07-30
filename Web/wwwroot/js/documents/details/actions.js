import { apiUrl } from '../../apiConfig.js';
import { documentId, formerDocumentSteps } from './global.js';

let signExist = false;
let parapheExist = false;

const loader = $('#loader');

async function validationSign(show) {
	try {
		loader.removeClass('display-none');

		await axios.post(apiUrl + `api/documents/${documentId}/validate`, {
			message: show
		}, {
			withCredentials: true
		});

		alert("Document validé!");

		window.location.reload();
    } catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
}

async function cancelDocument(commentaire) {
	try {
		loader.removeClass('display-none');

		await axios.post(apiUrl + `api/documents/${documentId}/cancel`, {
			message: commentaire
		}, {
			withCredentials: true
		});

		alert(`Document refusé!`);

		window.location.reload();
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
}

$(document).on('click', '[data-action="acknowledge"]', async () => {
	try {
		loader.removeClass('display-none');

		await axios.post(apiUrl + `api/suppliers/documents/${documentId}/acknowledge`, {}, {
			withCredentials: true
		});

		window.location.reload();
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$(document).on('click', `[document-action="validate"]`, (e) => {
	$('#validation-modal').modal('show');
});

$(document).on('click', `[data-action="validation"]`, async () => {
	await validationSign($("#commentaryv").val());
});

$(document).on('click', `[document-action="sign"]`, (e) => {
	$("#signature_tab").modal('show');
});

$(document).on('click', `[document-action="cancel"]`, (e) => {
	$(`#redirection-container`).hide();
	$(`[data-action="send_cancel"]`).show();
	$('#canceling-modal').modal('show');
});

$('#cancel-document').on('click', async () => {
	const commentaire = $('#comment-of-canceling').val();

	await cancelDocument(commentaire);
});

$(document).on('click', `[sign-confirm]`, async (e) => {
	try {
		loader.removeClass('display-none');

		const documentId = $('[document-id]').attr('document-id');

		if (documentId) {
			const signImage = signaturePad.toDataURL();
			const parapheImage = paraphePad.toDataURL();

			if (signExist && signaturePad._isEmpty) {
				alert("Vous avez oublié le signature!");

				return;
			}

			if (parapheExist && paraphePad._isEmpty) {
				alert("Vous avez oublié le paraphe!");

				return;
			}

			const formData = new FormData();

			formData.append("SignImage", signImage);
			formData.append("ParapheImage", parapheImage);

			await axios.patch(apiUrl + `api/documents/sign/${documentId}`, formData, {
				withCredentials: true,
				headers: {
					'Content-Type': 'multipart/form-data',
				}
			});

			window.location.reload();
		}
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$(document).on('click', `#show-redirection-modal`, () => {
	let content = ``;

	for (let i = 0; i < formerDocumentSteps.length; i += 1) {
		content += `
			<option value="${formerDocumentSteps[i].id}">Étape ${formerDocumentSteps[i].stepNumber}</option>
		`;
	}

	$('#former-document-steps').append(content);

	$('#former-document-steps').select2({
        dropdownParent: $('#redirection-modal')
    });
	
	$('#redirection-modal').modal('toggle');
});

$(document).on('change', `#former-document-steps`, async (e) => {
	$('#former-document-users-step').html('');

	const { data: users } = await axios.get(apiUrl + `api/document_steps/${$(e.currentTarget).val()}/users`, {
		withCredentials: true,
	});

	let content = ``;

	for (let i = 0; i < users.length; i += 1) {
		content += `
			<li>${users[i].username}</li>
		`;
	}

	$('#former-document-users-step').html(content);
});

$(document).on('click', `#redirect`, async () => {
	const comment = $('#comment-of-redirection').val();

	try {
		loader.removeClass('display-none');

		await axios.post(apiUrl + `api/documents/${documentId}/redirect`, {
			targetDocumentStepId: $('#former-document-steps').val(),
			message: comment
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
