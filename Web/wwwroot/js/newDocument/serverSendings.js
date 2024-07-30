import { apiUrl, webUrl } from '../apiConfig.js';
import userStateManager from '../store.js';
import { globalDynamicFields, attachements, setAttachements, usersSteps, currentUserFields } from './global.js';

const loader = $('#loader');

let globalDynamicFieldsToSend = [];
const dynamicAttachements = [];

// let signExist = false;
// let parapheExist = false;

let signatureId = '';

const title = $('[detail-id="title"]');

function verifyAllRequiredDynamicFields() {
	globalDynamicFieldsToSend = [];

	if (title.val() === '') {
		alert('Champs obligatoires non vides!');
	
		return false;
	}

	for (let i = 0; i < globalDynamicFields.length; i += 1) {
		const id = globalDynamicFields[i].id;

		if (globalDynamicFields[i].isRadioButton) {
			const radios = document.getElementsByName(id);

			for (let j = 0; j < radios.length; j += 1) {
                if (radios[j].checked) {
                    globalDynamicFieldsToSend.push({ id, value: radios[j].value });

                    break;
                }
            }
		} else if (globalDynamicFields[i].isOfTypeFile) {
			const dynamicAttachement = $(`#${id}`).get(0).files;

			if (globalDynamicFields[i].isRequired && dynamicAttachement.length === 0) {
				alert('Champs obligatoires non vides!');
	
				return false;
			}

			dynamicAttachements.push({ id, file: dynamicAttachement[0] });
		} else {
			const value = document.getElementById(id).value;
	
			if (globalDynamicFields[i].isRequired) {
				if (!value) {
					alert('Champs obligatoires non vides!');
	
					return false;
				}
			}
	
			globalDynamicFieldsToSend.push({ id, value });
		}
    }

	return true;
}

// $("#signature_tab").on('show.bs.modal', function (e) {
//     signExist = false;
//     parapheExist = false;
// 	//$("[sign-modal-dialog]").removeClass("modal-sm").addClass("modal-lg");
// 	//$("#paraphe-pad").removeClass("col-12").addClass("col-4");

// 	if ($(`[page-id][data-type="signature"]`).length > 0) {
// 		$("#signature-pad").show();
// 		signExist = true;
// 	}
// 	if ($(`[page-id][data-type="paraphe"]`).length > 0) {
// 		$("#paraphe-pad").show();
// 		parapheExist = true;
// 		//if (!signExist) {
// 		//	$("#paraphe-pad").removeClass("col-4").addClass("col-12");
// 		//	$("[sign-modal-dialog]").removeClass("modal-lg").addClass("modal-sm");
// 		//}
// 	}

// 	if (!signExist && !parapheExist) {
// 		alert("Veuillez renseigner les signatures ou/et paraphes.");
		
// 		e.preventDefault();
//     }
// });

// $("#MailService").on('show.bs.modal', function (e) {
	// for (let i = 0; i < usersDocumentsList.length; i += 1) {
	// 	if (usersDocumentsList[i].role === '2' && usersDocumentsList[i].fields.length <= 0) {
	// 		alert(`Veuillez renseigner une position de signature ou de paraphe pour ${usersDocumentsList[i].username}.`);
			
	// 		e.preventDefault();
			
	// 		break;
	// 	}
	// }
// });

$(`#YouSign`).on('click', async (e) => {
	$(`[field-id]`).mousemove();
	
	// if (usersDocumentsList.length === 0) {
	// 	alert("Veuillez ajouter au minimum un destinataire!");

	// 	return;
	// }

	if (!verifyAllRequiredDynamicFields()) {
		return;
	}

	const object = $('#object').val();
	const message = $('#global-message').summernote('code');

	if (message === '<p><br></p>') {
		alert("Veuillez renseigner le message du document!");

		return;
	}

	if (object === '') {
		alert("Veuillez renseigner l'objet du document!");

		return;
	}

	try {
		loader.removeClass('display-none');

		const { hasAccessToRSF } = userStateManager.getUser();

		const files = $("#input-file").get(0).files;

        const select = document.querySelector('#select-current-site');
        const currentSelectedSitesId = [];
        for (const option of select.options)
            option.selected && currentSelectedSitesId.push(option.value);


		let formData = new FormData();
		
		formData.append('documentFile', files[0]);
		formData.append('title', title.val());
		formData.append('object', object);
		formData.append('message', message);
		formData.append('recipients', JSON.stringify(usersSteps));
		formData.append('globalDynamicFields', JSON.stringify(globalDynamicFieldsToSend));
		formData.append('RSF', hasAccessToRSF ? String($('#rsf-container').find('#rsf').prop('checked')) : String(false));
        formData.append('Site', currentSelectedSitesId);
	
		for (let i = 0; i < attachements.length; i += 1) {
			formData.append('attachements', attachements[i].file);
		}
	
		const { data: documentId } = await axios.post(apiUrl + 'api/documents/new_validation_circuit', formData, {
			withCredentials: true,
			headers: {
				'Content-Type': 'multipart/form-data',
			}
		});

		for (let i = 0; i < dynamicAttachements.length; i += 1) {
			if (dynamicAttachements[i].file !== undefined) {
				formData = new FormData();
	
				formData.append('File', dynamicAttachements[i].file);
	
				await axios.post(apiUrl + `api/dynamic_attachements/documents/${documentId}?globalDynamicFieldId=${dynamicAttachements[i].id}`, formData, {
					withCredentials: true,
					headers: {
						'Content-Type': 'multipart/form-data',
					}
				});
			}
		}

		setAttachements([]);
		
		alert("Document envoyé!");

		window.location = webUrl + 'documents';
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$('#ISign').on('click', async () => {
	$('#signature-modal').modal('show');
});

$('[data-action="trigger-signature-image-file-input"]').on('click', () => {
	$('#signature-image-file-input').click();
});

$('#signature-image-file-input').on('change', (e) => {
	const file = e.target.files[0];

	const allowedFileTypesList = ['image/jpeg', 'image/x-png'];

	if (allowedFileTypesList.find((fileType) => fileType === file.type) === undefined) {
		alert(`Le fichier doit être de format JPG/JPEG ou PNG!`);

		return;
	}

	const imgSrc = URL.createObjectURL(file);

	$('#uploaded-signature-image-preview-container').html(`
		<img
			src="${imgSrc}"
			alt="Uploaded signature image"
			style="width: 300px; height: 200px; "
		/>
	`);

	$('#extract-signature-from-image-container').html(`
		<button class="btn btn-primary" id="extract-signature-from-image">Extraire la signature de l'image</button>
	`);
});

$(document).on('click', '#extract-signature-from-image', async () => {
	const formData = new FormData();

	formData.append('file', $('#signature-image-file-input').get(0).files[0]);

	const { data: id } = await axios.post(apiUrl + `api/signatures/extract`, formData, {
		withCredentials: true,
		headers: {
			'Content-Type': 'multipart/form-data',
		},
	});

	signatureId = id;

	const { data: signature } = await axios.get(apiUrl + `api/signatures/${signatureId}`, {
		withCredentials: true,
		responseType: 'blob',
	});

	const blobUrl = URL.createObjectURL(signature);

	$('#signature-image-preview-container').html(`
		<img 
			src="${blobUrl}"
			alt="Extracted signature preview"
			style="width: 300px; height: 200px; "
		/>
	`);
});

$(`[data-action="sign"]`).on('click', async () => {
	$(`[field-id]`).mousemove();

	if (!verifyAllRequiredDynamicFields()) return;

	const files = $('#input-file').get(0).files;

	if (files.length <= 0) {
		alert('Veuillez sélectionner un document!');

		return;
	}

    const object = $(`#object`).val();
    const message = $(`#global-message`).val();

    if (!message && !object) {
        alert('Vous devez ajouter un message!');

        return;
    }

    // if (signExist && signaturePad._isEmpty) {
	// 	alert('Vous avez oublié la signature!');

	// 	return;
	// }
    // if (parapheExist && paraphePad._isEmpty) {
	// 	alert('Vous avez oublié le paraphe!');

	// 	return;
	// }

	try {
		loader.removeClass('display-none');

		await axios.post(apiUrl + `api/signatures/send_token?signatureId=${signatureId}`, {}, {
			withCredentials: true,
		});
	
		alert(`Un token a été envoyé vers votre adresse email. Ne jamais supprimer le mail envoyé!`);
		
		$('#signature-token-container').html(`
			<div class="form-group">
				<label for="signature-token">Le token envoyé dans le mail: </label>

				<input type="text" id="signature-token" />

				<button class="btn btn-primary" id="sign-document">Signer le document</button>
			</div>
		`);
	} catch (error) {
		console.log(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$(document).on('click', '#sign-document', async () => {
	loader.removeClass('display-none');

	const token = $('#signature-token-container').find('#signature-token').val();

	try {
		await axios.post(apiUrl + `api/signatures/check_token_authenticity`, {
			token,
		}, {
			withCredentials: true,
		});
	} catch (error) {
		alert(`Token incorrect!`);

		loader.addClass('display-none');

		return;
	}

	try {
		loader.removeClass('display-none');

		const { hasAccessToRSF } = userStateManager.getUser();

		const files = $('#input-file').get(0).files;
		
		let formData = new FormData();

		for (let i = 0; i < attachements.length; i += 1) {
			formData.append('attachements', attachements[i].file);
		}

		formData.append('documentFile', files[0]);
		formData.append('object', object);
		formData.append('message', message);
		formData.append('title', title.val());
		formData.append('globalDynamicFields', JSON.stringify(globalDynamicFieldsToSend));
		formData.append('fieldDetails', JSON.stringify(currentUserFields[0]));
		formData.append('signatureId', signatureId);
		formData.append('token', token);
		formData.append('RSF', hasAccessToRSF ? String($('#rsf-container').find('#rsf').prop('checked')) : String(false));
	
		const { data: documentId } = await axios.post(apiUrl + 'api/sign_document', formData, {
			withCredentials: true,
			headers: {
				'Content-Type': 'multipart/form-data',
			},
		});

		for (let i = 0; i < dynamicAttachements.length; i += 1) {
			if (dynamicAttachements[i].file !== undefined) {
				formData = new FormData();
	
				formData.append('File', dynamicAttachements[i].file);
	
				await axios.post(apiUrl + `api/dynamic_attachements/documents/${documentId}?globalDynamicFieldId=${dynamicAttachements[i].id}`, formData, {
					withCredentials: true,
					headers: {
						'Content-Type': 'multipart/form-data',
					},
				});
			}
		}

		alert("Document Signé!");

		window.location = webUrl + 'documents';
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$('#archive').on('click', async () => {
	$(`[field-id]`).mousemove();

	if (!verifyAllRequiredDynamicFields()) return;

	const files = $('#input-file').get(0).files;

	if (files.length <= 0 ) {
		alert('Veuillez sélectionner un document!');

		return;
    }

	console.log($('#rsf-container').find('#rsf').prop('checked'));

	try {
		loader.removeClass('display-none');

		const { hasAccessToRSF } = userStateManager.getUser();

		const object = $(`#object`).val();
		const message = $(`#global-message`).val();
		
		let formData = new FormData();
		formData.append("Object", object);
		formData.append("Message", message);
		formData.append("Title", title.val());
		formData.append("File", files[0]);
		formData.append('GlobalDynamicFields', JSON.stringify(globalDynamicFieldsToSend));
		formData.append('RSF', hasAccessToRSF ? String($('#rsf-container').find('#rsf').prop('checked')) : String(false));

		for (let i = 0; i < attachements.length; i += 1) {
			formData.append('Attachements', attachements[i].file);
		}

		const { data: documentId } = await axios.post(apiUrl + 'api/documents/archive', formData, {
			withCredentials: true,
			headers: {
				'Content-Type': 'multipart/form-data',
			}
		});

		for (let i = 0; i < dynamicAttachements.length; i += 1) {
			if (dynamicAttachements[i].file !== undefined) {
				formData = new FormData();
	
				formData.append('File', dynamicAttachements[i].file);
	
				await axios.post(apiUrl + `api/dynamic_attachements/documents/${documentId}?globalDynamicFieldId=${dynamicAttachements[i].id}`, formData, {
					withCredentials: true,
					headers: {
						'Content-Type': 'multipart/form-data',
					}
				});
			}
		}		
		
		setAttachements([]);

		alert("Document archivé!");

		window.location = webUrl + 'documents';
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});
