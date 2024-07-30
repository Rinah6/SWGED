import { apiUrl } from '../../apiConfig.js';
import { documentId } from './global.js';

const loader = $('#loader');

let originalDocumentAccessors = [];
let documentAccessorsListContent = ``;

$(document).on('click', '#manage-document-accesses', async (e) => {
	try {
		loader.removeClass('display-none');

		const { data: canBeAccessedByAnyone } = await axios.get(apiUrl + `api/document_accesses/documents/${documentId}/can_be_accessed_by_anyone`, {
			withCredentials: true,
		});

		const { data: documentAccessors } = await axios.get(apiUrl + `api/document_accesses/documents/${documentId}/accessors`, {
			withCredentials: true,
		});

		let content = ``;

		originalDocumentAccessors = [];

		for (let i = 0; i < documentAccessors.length; i += 1) {
			originalDocumentAccessors.push({
				id: documentAccessors[i].id,
				canAccess: documentAccessors[i].canAccess,
			});

			content += `
				<div class="form-group">
					<input 
						type="checkbox" 
						id="${documentAccessors[i].id}" 
						${documentAccessors[i].canAccess && "checked"}
						data-action="change-access"
					/>

					<label for="${documentAccessors[i].id}">${documentAccessors[i].username}</label>
				</div>
			`;
		}

		documentAccessorsListContent = content;

		if (canBeAccessedByAnyone) {
			$('#visible-by-everyone').click();
			
			$('#document-accessors-list').html(``);
		} else {
			$('#custom-accesses').click();

			$('#document-accessors-list').html(documentAccessorsListContent);
		}

		$('#manage-document-accesses-modal').modal('toggle');
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$('#close-document-accesses-modal').on('click', () => {
	$('#manage-document-accesses-modal').modal('toggle');
});

$('[name="accesses-group"]').on('change', (e) => {
	if ($(e.currentTarget).val() === 'visible-by-everyone') {
		$('#document-accessors-list').html(``);
	} else {
		$('#document-accessors-list').html(documentAccessorsListContent);
	}
});

async function setCanBeAccessedByAnyone(canBeAccessedByAnyone) {
	await axios.patch(apiUrl + `api/document_accesses/documents/${documentId}/can_be_accessed_by_anyone?status=${canBeAccessedByAnyone}`, {

	}, {
		withCredentials: true,
	});
}

$('#save-document-accesses').on('click', async () => {
	if ($('[name="accesses-group"]:checked').val() === 'visible-by-everyone') {
		try {
			loader.removeClass('display-none');

			await setCanBeAccessedByAnyone(true);

			window.location.reload();
		} catch (error) {
			console.log(error.message);
		} finally {
			loader.addClass('display-none');
		}
		
		return;
	}

	const documentAccessors = [];

	$('[data-action="change-access"]').each(function () {
		documentAccessors.push({
			id: $(this).attr('id'),
			canAccess: $(this).prop('checked'),
		});
	});

	const additions = [];
	const deletions = [];

	for (let i = 0; i < documentAccessors.length; i += 1) {
		const res = originalDocumentAccessors.find((documentAccessor) => documentAccessor.id === documentAccessors[i].id);

		if (!res) {
			continue;
		}

		if (res.canAccess !== documentAccessors[i].canAccess) {
			if (documentAccessors[i].canAccess === true) {
				additions.push(documentAccessors[i].id);
			} else {
				deletions.push(documentAccessors[i].id);
			}
		}
	}

	try {
		loader.removeClass('display-none');

		if (additions.length > 0) {
			await axios.post(apiUrl + `api/document_accesses/documents/${documentId}/accessors`, {
				usersId: additions
			}, {
				withCredentials: true
			});
		}

		if (deletions.length > 0) {
			await axios.patch(apiUrl + `api/document_accesses/documents/${documentId}/accessors`, {
				usersId: deletions
			}, {
				withCredentials: true
			});
		}

		await setCanBeAccessedByAnyone(false);

		window.location.reload();
	} catch (error) {
		console.log(error.message);
	} finally {
		loader.addClass('display-none');
	}
});
