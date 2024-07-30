import { webUrl, apiUrl } from '../apiConfig.js';
import userStateManager  from '../store.js';
import { pastPeriod, formatDate } from '../utils.js';

const loader = $('#loader');

function documentsListUI(document, documentsGroup) {
	const creationDate = formatDate(document.creationDate);

	let checkbox = '';

	if (documentsGroup === 'received' && document.role === 1) {
		checkbox = `
			<div class="mailbox-star">
				<input 
					type="checkbox" 
					data-document-id="${document.id}" 
				/> 
			</div>
		`;
	}

	if ((documentsGroup === '1' || documentsGroup === 'received') && document.isTheCurrentStepTurn && document.role === 1) {
		checkbox = `
			<div class="mailbox-star">
				<input 
					type="checkbox" 
					data-document-id="${document.id}" 
				/> 
			</div>
		`;
	}

	return `
		<li 
			data-document-status="${document.status}" 
			data-document-origin-state="${document.wasSendedByASupplier === undefined ? false : document.wasSendedByASupplier}" 
			class="dropdown document" ViewDocument="${document.id}"
		>
			${checkbox}

			<div class="mailbox-name">${document.title}</div>

            <div class="mailbox-date">${creationDate}</div>

            <div class="mailbox-date">${pastPeriod(document.creationDate)}</div>
		</li>
	`;
}

async function getSuppliers() {
	try {
		loader.removeClass('display-none');

		const { data } = await axios.get(apiUrl + `api/suppliers/project`, {
			withCredentials: true
		});

		$('#suppliers-filter').html(`
			<label for="suppliers">Fournisseurs: </label>
			<select id="suppliers" style="width: 100px; "></select>
		`);

		let code = `
			<option selected value="-1"></option>
		`;

		$.each(data, function (_, supplier) {
			code += `
				<option value="${supplier.id}">${supplier.name}</option>
			`;
		});

		$('#suppliers-filter').find(`#suppliers`).html(code).select2();

		$('#suppliers-filter').find(`#suppliers`).on('change', async (e) => {
			const supplierId = $(e.currentTarget).val();

			if (supplierId === '-1') {
				$(`#documents-list`).html('');
			} else {
				try {
					loader.removeClass('display-none');

					const { data: documents } = await axios.get(apiUrl + `api/documents/suppliers/${supplierId}`, {
						withCredentials: true
					});

					$(`#documents-list`).html('');

					for (let i = 0; i < documents.length; i += 1) {
						$(`#documents-list`).append(documentsListUI(documents[i], '', 'suppliers'));
					}
				} catch (error) {
					alert(error.message);
				} finally {
					loader.addClass('display-none');
				}
			}
		});
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
}

function showList() {
	$('#p_doc').remove();
	$("#sign").text("");
}

async function getDocumentsByGroupLabel(documentsGroup) {
	$(`#documents-list`).text('');
	$("#links-list").show();
	$(".card-header").show();
	showList();

	let res;

	loader.removeClass('display-none');

	if (documentsGroup === 'received_from_suppliers') {
		const { hasAccessToSuppliersHandling } = userStateManager.getUser();

		if (!hasAccessToSuppliersHandling) {
			$('[data-documents-label="received_from_suppliers"]').remove();

			$('#suppliers-filter').html('');
		} else {
			await getSuppliers();
		}
	} else {
		$('#suppliers-filter').html('');
	}

	try {
		const { data } = await axios.get(apiUrl + `api/documents/${documentsGroup}`, {
			withCredentials: true
		});

		res = data;

		$(`#documents-list`).text('');

		$.each(data, (_, v) => {
			// var icon = "fa-star";

			// switch (v.status) {
			// 	case 0:
			// 		icon = "fa-spinner fa-spin";

			// 		break;
			// 	case 1:
			// 		icon = "fa-file-signature";

			// 		break;
			// 	case 2:
			// 		icon = "fa-file-archive";

			// 		break;
			// 	default:
			// 		icon = "fa-star";

			// 		break;
			// }

			$(`#documents-list`).append(documentsListUI(v, documentsGroup));
		});
	} catch (error) {
		alert(error.message);

		return [];
	} finally {
		loader.addClass('display-none');
	}

	return res;
}

async function getDocumentsByStatus(documentStatus) {
	$(`#documents-list`).text('');
	$("#links-list").show();
	$(".card-header").show();
	showList();

	loader.removeClass('display-none');

	try {
		const { data } = await axios.get(apiUrl + `api/documents?status=${documentStatus}`, {
			withCredentials: true
		});

		$(`#documents-list`).text('');

		$.each(data, (_, v) => {
			// var icon = "fa-star";

			// switch (v.status) {
			// 	case 0:
			// 		icon = "fa-spinner fa-spin";

			// 		break;
			// 	case 1:
			// 		icon = "fa-file-signature";

			// 		break;
			// 	case 2:
			// 		icon = "fa-file-archive";

			// 		break;
			// 	default:
			// 		icon = "fa-star";

			// 		break;
			// }

			$(`#documents-list`).append(documentsListUI(v, documentStatus));
		});
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
}

$(document).ready(async () => {
	await userStateManager.init();

	const { role } = userStateManager.getUser();

	if (role === 0) {
		$(`[data-documents-label="common_documents"]`).remove();
	}

	const { data: numberOfDocumentsByStatus } = await axios.get(apiUrl + `api/documents/total_number_by_status`, {
		withCredentials: true
	});

	const documents = await getDocumentsByGroupLabel('received');

	let count = 0;

	Object.keys(numberOfDocumentsByStatus).forEach((key) => {
		$(`[data-documents-label="${key}"]`).find('.number-of-documents').text(numberOfDocumentsByStatus[key]);

		count += numberOfDocumentsByStatus[key];
	});

	$(`#ongoing-documents-link`).find('h3').text(numberOfDocumentsByStatus.ongoing);
	$(`#canceled-documents-link`).find('h3').text(numberOfDocumentsByStatus.canceled);
	$(`#archived-documents-link`).find('h3').text(numberOfDocumentsByStatus.archived);

	$(`#total-number-of-documents`).text(count);

	$('#attachment_menu').hide();
	// $("#sign").text("");

	if (documents.length > 0) {
		$('#validate-all-documents-container').html(`
			<input type="checkbox" data-action="check-all-documents" style="margin-left: 16px;" />
			<button class="btn btn-success" style="margin-left:40px;" id="validate-all-documents">Valider les documents sélectionnés</button>
		`);
	}
});

$('#ongoing-documents-link').on('click', async () => {
	if (window.location.pathname.includes('newDocument')) {
		return;
	}

	$(`#p_MyDocument`).show();
	$(`#redirection-container`).html('');
	$('#label_document').hide();
	$('#attachment_menu').hide();
	$('[document-title]').text('Documents en cours');

	await getDocumentsByStatus(1);
});

$('#canceled-documents-link').on('click', async () => {
	if (window.location.pathname.includes('newDocument')) {
		return;
	}

	$(`#p_MyDocument`).show();
	$(`#redirection-container`).html("");
	$('#label_document').hide();
	$('#attachment_menu').hide();
	$('[document-title]').text('Documents signés/validés');

	await getDocumentsByStatus(2);
});

$('#archived-documents-link').on('click', async () => {
	if (window.location.pathname.includes('newDocument')) {
		return;
	}

	$(`#p_MyDocument`).show();
	$(`#redirection-container`).html("");
	$('#label_document').hide();
	$('#attachment_menu').hide();
	$('[document-title]').text('Documents archivés');

	await getDocumentsByStatus(3);
});

$(`[data-documents-label]`).on(`click`, async (e) => {
	$(`#p_MyDocument`).show();
	$(`#redirection-container`).html('');
	$('#label_document').hide();
	$('#attachment_menu').hide();
	// $('[document-title]').text(textBox.text());

	if ($(e.currentTarget).attr('data-documents-status') !== undefined) {
		$('#validate-all-documents-container').html('');

		await getDocumentsByStatus($(e.currentTarget).attr('data-documents-status'));
	} else {
		const documents = await getDocumentsByGroupLabel($(e.currentTarget).attr('data-documents-label'));

		if ($(e.currentTarget).attr('data-documents-label') === 'received' && documents.length > 0) {
			$('#validate-all-documents-container').html(`
				<input type="checkbox" data-action="check-all-documents" style="margin-left: 16px;" />
				<button class="btn btn-success" style="margin-left:40px;" id="validate-all-documents">Valider les documents sélectionnés</button>
			`);
		} else {
			$('#validate-all-documents-container').html('');
		}
	}
});

$(document).on('click', "[action-reply]", (e) => {
	showList();
});

$(document).on('change', '[data-action="check-all-documents"]', () => {
	if ($('[data-action="check-all-documents"]').prop('checked') === true) {
		$('[data-document-id]').prop('checked', true);
	}
	else {
		$('[data-document-id]').prop('checked', false);
	}
});

$(document).on('click', '[data-document-id]', (e) => {
	e.stopPropagation();
});

$(document).on('change', '[data-document-id]', (e) => {
	if ($(e.currentTarget).prop('checked') === false) {
		$('[data-action="check-all-documents"]').prop('checked', false);
	}
});

$(document).on('click', '#validate-all-documents', async () => {
	const documents = $('[data-document-id]:checked');

	if (documents.length === 0) {
		return;
	}

	for (let i = 0; i < documents.length; i += 1) {
		if ($(documents[i]).prop('checked')) {
			try {
				loader.removeClass('display-none');

				const id = $(documents[i]).attr('data-document-id');

				await axios.post(apiUrl + `api/documents/${id}/validate`, {
					message: '',
				}, {
					withCredentials: true,
				});
			} catch (error) {
				alert(error.message);
			} finally {
				loader.addClass('display-none');
			}
		}
	}

	alert("Documents validés!");

	window.location.reload();
});

$("#date_envoi").on("change", function () {
	const value = $(this).val().toLowerCase();

	$(`[date-sending]`).filter(function () {
		var parent = $(this).closest(`[data-document-status]`);
		parent.toggle($(this).text() == pastPeriod(value));
	});
});

$("#destinataires").on("keyup", function () {
	const value = $(this).val().toLowerCase();

	$(`[receiver]`).filter(function () {
		var parent = $(this).closest(`[data-document-status]`);
		parent.toggle($(this).text().toLowerCase().indexOf(value) > -1);
	});
});

$("#objets").on("keyup", function () {
	const value = $(this).val().toLowerCase();

	$(`[text_libre]`).filter(function () {
		const parent = $(this).closest(`[data-document-status]`);

		parent.toggle($(this).text().toLowerCase().indexOf(value) > -1);
	});
});

$(document).on('click', '[ViewDocument]', async (e) => {
	$("#links-list").hide();
	$(".card-header").hide();

	const id = $(e.currentTarget).attr('ViewDocument');

	window.location.href = webUrl + `documents/${id}`;
});
