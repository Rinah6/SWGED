import { webUrl, apiUrl } from '../../apiConfig.js';
import userStateManager from '../../store.js';
import { formatDate } from '../../utils.js';
import { documentId, setDocumentId, setFormerDocumentSteps } from './global.js';

const loader = $('#loader');

let currentPDF = {};
let test = false;
let lastFile = undefined;

async function listFormerUsersDocuments(documentId) {
	const { data: documentSteps } = await axios.get(apiUrl + `api/documents/${documentId}/former_document_steps`, {
		withCredentials: true
	});

	if (documentSteps.length === 0) {
		$('#redirection-container').remove();

		return;
	}

	setFormerDocumentSteps(documentSteps);

	$('#redirection-container').html(`
		<button class="btn btn-warning bg-gradient mb-3 offset-1" style="padding: 10px; " id="show-redirection-modal">
			<i class="fa fa-arrow-alt-circle-up p-2"></i> Redirection
		</button>
	`);
}

async function showDocumentActionsButtons(documentDetails) {
	const { hasAccessToTomProLinking } = userStateManager.getUser();

	let content = ``;

	if (documentDetails.status !== 0 && documentDetails.status !== 2) {
		content += `
			<button class="btn btn-info bg-gradient mb-3 col-12" document-action="link">
				<i class="fa fa-link p-2"></i>Copier le lien
			</button>

			<a id="link_copied" href="${webUrl}documents/shared/${documentId}" target="_blank" class="col-6" style="display:none;">
				${webUrl}documents/shared/${documentId}
			</a>

			<br />
		`;

		if (hasAccessToTomProLinking) {
			content += `
				<button class="btn btn-info bg-success mb-3 col-12" data-action="tom-pro-link-transfer">
					<i class="fa fa-link p-2"></i>Transférer le lien vers Tom²Pro
				</button>
	
				<br />
			`;
		}
	}

	if (!documentDetails.wasAcknowledged && documentDetails.wasSendedByASupplier && documentDetails.status === 0) {
		content += `
			<div class="btn btn-success bg-gradient mb-3 col-12" data-action="acknowledge">
				<i class="fa fa-envelope p-2"></i>Accuser réception
			</div>

			<br />
		`;
	}

	const { hasAccessToDocumentTypesHandling, hasAccessToSuppliersHandling } = userStateManager.getUser();

	if (documentDetails.wasAcknowledged && documentDetails.wasSendedByASupplier && documentDetails.status === 0 && hasAccessToSuppliersHandling) {
		if (!hasAccessToDocumentTypesHandling) {
			$('#send-to-container').html(`
				<div class="btn btn-success bg-gradient mb-3 col-12" data-action="set-recipients-steps">
					<i class="fa fa-paper-plane p-2"></i> Ajouter un étape de validation
				</div>
			`);
		}

		$('#recipients-list-container').show();
	} else {
		$('#recipients-list-container').remove();
	}

	if (documentDetails.isTheCurrentStepTurn && documentDetails.status !== 0) {
		$('#label_document').show();

		await listFormerUsersDocuments(documentId);

		if (!documentDetails.hasSign) {
			$("#signature-pad").remove();
		}

		if (!documentDetails.hasParaphe) {
			$("#paraphe-pad").remove();
		}

		let buttonContent = '';

		if (documentDetails.hasSign || documentDetails.hasParaphe) {
			signExist = documentDetails.hasSign;
			parapheExist = documentDetails.hasParaphe;

			buttonContent = `
				<div class="btn btn-success bg-gradient mb-3 col-3 offset-1" document-action="sign">
					<i class="fa fa-signature p-2"> </i>Signer
				</div>
			`;
		} else {
			if (documentDetails.status === 1) {
				buttonContent = `
					<div class="btn btn-success bg-gradient mb-3 offset-1" document-action="validate">
						<i class="fa fa-check p-2"> </i>Valider
					</div>`;
			} else {
				$('#label_document').remove();
			}
		}

		content += buttonContent + `
			<div class="btn btn-danger bg-gradient mb-3 offset-1" document-action="cancel">
				<i class="fa fa-close p-2"> </i> Refuser
			</div>
		`;
	}

	$('#document-actions-container').html(content);
}

function resetCurrentPDF() {
	currentPDF = {
		file: null,
		countOfPages: 0,
		currentPage: 1,
		zoom: 1.5,
	}
}

function initPage() {
	if (!test || lastFile != currentPDF.file) {
		lastFile = currentPDF.file;

		$('[firstPage]').val('1');
		$('[firstPage]').attr('max', currentPDF.countOfPages);
		$('[firstPage]').attr('min', 1);

		$("[LastPage]").val(1);
		$("[LastPage]").attr('max', currentPDF.countOfPages);
		$("[LastPage]").attr('min', 1);

		test = !test;
	}
}

function renderCurrentPage() {
	currentPDF.file.getPage(currentPDF.currentPage).then((page) => {
		const viewer = $('#document-details-container').find("#pdf-viewer")[0];
		const context = viewer.getContext('2d');
		const viewport = page.getViewport({ scale: currentPDF.zoom, });

		viewer.height = viewport.height;
		viewer.width = viewport.width;

		const renderContext = {
			canvasContext: context,
			viewport: viewport,
		};

		page.render(renderContext);
	});

	$("#current_page").html(currentPDF.currentPage + ' sur ' + currentPDF.countOfPages);

	initPage();
}

function displayGlobalDynamicFields(arr) {
	const container = $('#document-details-container').find('#document-fields');

	for (let i = 0; i < arr.length; i += 1) {
		let value = arr[i].value;

		if (arr[i].value === 'true') {
			value = 'Oui';
		} else if (value === 'false') {
			arr[i].value = 'Non'
		}

		container.append(`
			<div class="label-flex mailbox-controls with-border p-3" id="${arr[i].id}" style="display: flex; align-items: center; ">
				<h6>
					<u>${arr[i].label}</u> : 
				</h6>

				<div>
					${value}
				</div>
			</div>
		`);
	}
}

function displayDynamicAttachements(arr) {
	const container = $('#document-details-container').find('#document-fields');

	for (let i = 0; i < arr.length; i += 1) {
		container.append(`
			<div class="label-flex mailbox-controls with-border p-3" style="display: flex; align-items: center; ">
				<h6>
					<u>${arr[i].label}</u> : 
				</h6>

				<figure dynamic-field-id="${arr[i].id}" style="cursor: pointer; ">
					<img 
						src="/icons/file-download.svg" 
						alt="${arr[i].filename}"
						width="50" 
						height="50" 
					/>

					<figcaption>${arr[i].filename}</figcaption>
				</figure>
			</div>
		`);
	}
}

function renderDocumentDetails(documentDetails) {
	const { hasAccessToPhysicalLocationHandling, hasAccessToDocumentsAccessesHandling } = userStateManager.getUser();

	const additionalDetails = documentDetails.nif !== undefined && documentDetails.stat !== undefined ? `
		<div class="label-flex mailbox-controls with-border p-3">
			<h6>
				<u>NIF </u> : 
			</h6>
			<div>
				${documentDetails.nif}
			</div>
		</div>

		<div class="label-flex mailbox-controls with-border p-3">
			<h6>
				<u>STAT </u> :
			</h6>
			<div>
				${documentDetails.stat}
			</div>
		</div>

		<div class="label-flex mailbox-controls with-border p-3">
			<h6>
				<u>Nom </u> :
			</h6>
			<div>
				${documentDetails.name}
			</div>
		</div>
	` : documentDetails.name !== undefined ? `
		<div class="label-flex mailbox-controls with-border p-3">
			<h6>
				<u>Nom </u> :
			</h6>
			<div>
				${documentDetails.name}
			</div>
		</div>
	` : ``;

	const documentAccessesHandling = documentDetails.status === 3 && documentDetails.isTheCurrentUserTheSender && hasAccessToDocumentsAccessesHandling ? (
		`
			<button class="btn btn-secondary btn-sm" id="manage-document-accesses">
				Gérer les accès de ce document
			</button>
		`
	) : ``;

	return `
		<div id="p_doc" data-type="panel">
			<div class="card card-primary card-outline mb-1">
				<div class="card-header" _status="${documentDetails.status}">
					<button class="btn btn-default btn-sm" action-reply>
						<i class="fas fa-reply"></i>
					</button>

					<br />
					<br />

					<h6 class="col">
						<u>Objet</u> : ${documentDetails.object}
					</h6>
					<br />

                    <h6 class="col">
						<u> Nom du Fichier Télécharger </u>: <span file-document-name="${documentDetails.filename}">${documentDetails.filename}</span>
                        <input class="" type="text" value=${documentDetails.filename} edit-document-name="${documentId}" style="display:none;border:none;" size="50" />
					</h6>

					<div class="col">
						<span class="mailbox-read-time"></span>
						<span class="mailbox-read-time float-right">${formatDate(documentDetails.creationDate)}</span>
					</div>
				</div>

				<div class="card-body">
					<div id="document-fields">
						<div class="label-flex mailbox-controls with-border p-3">
							<h6>
								<u>Message </u> : 
							</h6>
							<div>
								${documentDetails.message === undefined ? 'Aucun message' : documentDetails.message}
							</div>
						</div>
						${additionalDetails}
					</div>

					<div class="label-flex mailbox-controls with-border p-3" style="display: ${documentDetails.physicalLocation === null || !hasAccessToPhysicalLocationHandling ? 'none' : 'flex'};">
						<h6>
							<u>Emplacement physique </u> : 
						</h6>
						<p id="physical-location" contenteditable="true" style="outline: none;">
							${documentDetails.physicalLocation === '' ? 'Éditez ici' : documentDetails.physicalLocation}
						</p>

						<button class="btn btn-primary" id="set-physical-location" style="visibility: hidden;">OK</button>
					</div>

					<div class="label-flex mailbox-controls with-border p-3" style="display: ${documentDetails.status === 0 && documentDetails.isTheCurrentUserTheSender && false ? 'flex' : 'none'};">
						<h6>
							<u>Circuit de traitement </u> : 

							<ul id="users-documents" class="nav nav-pills flex-column"></ul>

							<button class="btn btn-primary" style="margin-top: 10px;" id="show-users-document-modal">
								Ajouter un utilisateur au circuit de traitement
							</button>

							<button class="btn btn-primary" style="margin-top: 10px; visibility: hidden;" id="save-changes">
								Enregistrer les modifications
							</button>
						</h6>
					</div>
					
					<div class="mailbox-read-message text-center flex-column">
                    <div id="pagination"></div>
                        <div id="pdf-container" style="font-size: 40px;">
                            <div class="col-lg-12">
                                <div id="previous" class="nav-link d-inline-block">
                                    <i class="fas fa-arrow-alt-circle-left"
                                        style="cursor: pointer"></i>
                                </div>

                                <span id="current_page" class="nav-link d-inline-block">0 sur 0</span>

                                <div id="next" class="nav-link d-inline-block">
                                    <i class="fas fa-arrow-alt-circle-right"
                                        style="cursor: pointer"></i>
                                </div>
                            </div>

                            <canvas id="pdf-viewer" class="pdf-viewer"></canvas>
                        </div>
                    </div>

                    <button class="btn btn-primary btn-sm" download_button="${documentDetails.filename}">
						Télécharger document
					</button>

                    <input type="file" hidden accept="application/pdf" principal_file />

					${documentAccessesHandling}
				</div>
			</div>
		</div>
	`;

	//<button class="btn btn-secondary btn-sm" change_document="${documentDetails.filename}">
	// Remplacer le document
	// </button>
}

$(document).ready(async () => {
	const pathname = window.location.pathname;

    setDocumentId(pathname.split('/documents/')[1]);

	loader.removeClass('display-none');

	try {
		await userStateManager.init();

		const { data: pdfBlob } = await axios.get(apiUrl + `api/pdf/${documentId}`, {
			responseType: 'blob',
			withCredentials: true,
		});

		const { data: documentDetails } = await axios.get(apiUrl + `api/documents/${documentId}`, {
			withCredentials: true,
		});

		const { data: dynamicFields } = await axios.get(apiUrl + `api/dynamic_fields?documentId=${documentId}`, {
			withCredentials: true,
		});

		const { data: dynamicAttachements } = await axios.get(apiUrl + `api/dynamic_attachements/documents/${documentId}`, {
			withCredentials: true,
		});
		
		$('#document-details-container').html(renderDocumentDetails(documentDetails));

		const inputFile = URL.createObjectURL(pdfBlob);

		resetCurrentPDF();

		const pdfFile = pdfjsLib.getDocument(inputFile);

		pdfFile.promise.then((doc) => {
			currentPDF.file = doc;
			currentPDF.countOfPages = doc.numPages;

			renderCurrentPage();
		}).then(() => {
			// loader.addClass('display-none');
		}).catch((error) => {
			console.log(error);

			alert("Une erreur est survenue! Vérifiez que le document uploadé n'est pas corrompu!");
		});

		displayGlobalDynamicFields(dynamicFields);
		displayDynamicAttachements(dynamicAttachements);
		await showDocumentActionsButtons(documentDetails);
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});

$(document).on('click', '#next', (e) => {
    const isValidPage = currentPDF.currentPage < currentPDF.countOfPages;

    if (!isValidPage) {
        return;
    }

    currentPDF.currentPage += 1;
    renderCurrentPage();

    $(document).trigger('refreshField');
});

$(document).on('click', '#previous', (e) => {
    const isValidPage = currentPDF.currentPage - 1 > 0;

    if (!isValidPage) {
        return;
    }

    currentPDF.currentPage -= 1;

    renderCurrentPage();

    $(document).trigger('refreshField');
});

$(document).on('click', `[document-action="link"]`, (e) => {
	ClipboardJS.copy(document.querySelector('#link_copied').href);

	alert("Lien copié avec succés!");
});
