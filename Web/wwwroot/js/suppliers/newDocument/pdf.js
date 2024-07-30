// import userStateManager from '../store.js';

let test = false;
let lastFile;
let currentPDF = {
    file: null,
    countOfPages: 0,
    currentPage: -1,
    zoom: 0
};

function resetCurrentPDF() {
	currentPDF = {
		file: null,
		countOfPages: 0,
		currentPage: 1,
		zoom: 1.5
	}
}

function renderCurrentPage() {
	currentPDF.file.getPage(currentPDF.currentPage).then((page) => {
		let viewer = $("#pdf-viewer")[0];
		var context = viewer.getContext('2d');
		var viewport = page.getViewport({ scale: currentPDF.zoom, });

		viewer.height = viewport.height;
		viewer.width = viewport.width;

		var renderContext = {
			canvasContext: context,
			viewport: viewport
		};
		page.render(renderContext);
	});

	$("#current-page").html(currentPDF.currentPage + ' sur ' + currentPDF.countOfPages);

	initPage();
}

function initPage() {
	if (!test || lastFile != currentPDF.file) {
		lastFile = currentPDF.file;

		$("[firstPage]").val("1");
		$("[firstPage]").attr("max", currentPDF.countOfPages);
		$("[firstPage]").attr("min", 1);

		$("[LastPage]").val(1);
		$("[LastPage]").attr("max", currentPDF.countOfPages);
		$("[LastPage]").attr("min", 1);

		test = !test;
	}
}

$(document).ready(async () => {
	// await userStateManager.init();
});

$('#input-img').on('click', () => {
	if ($(`#input-pdf`).val() === '') {
		$('#input-pdf').click();
    }
});

$(`[data-action="openPdf"]`).on('click', () => {
	$('#input-pdf').click();
});

$('#input-pdf').on('click', () => {
	$("#input-pdf").val('');

	$('#pdf-viewer').removeAttr('width');
	$('#pdf-viewer').removeAttr('height');
	$("#input-img").removeClass('hidden');
});

$('#input-pdf').on('change', (e) => {
	const inputFile = e.target.files[0];

	if (inputFile) {
		if (inputFile.type === 'application/pdf') {
			const fileChecker = new FileReader();

			fileChecker.readAsArrayBuffer(inputFile);

			fileChecker.onload = async () => {
				const binaryFile = new Blob([fileChecker.result], {
					type: 'application/pdf'
				});

				const fileContent = await binaryFile.text();

				const isEncrypted = fileContent.includes("Encrypt") || fileContent.substring(fileContent.lastIndexOf("<<"), fileContent.lastIndexOf(">>")).includes("/Encrypt");

				if (isEncrypted) {
					alert("Les documents protégés ne peuvent être uploadés!");
				} else {
					const fileReader = new FileReader();
			
					fileReader.readAsDataURL(inputFile);
		
					fileReader.onload = async () => {
						resetCurrentPDF();
						
						const pdfFile = pdfjsLib.getDocument(fileReader.result);

						pdfFile.promise.then((doc) => {
							currentPDF.file = doc;
							currentPDF.countOfPages = doc.numPages;

							renderCurrentPage();
						}).then(() => {
							$("#input-img").addClass('hidden');

							// const { hasAccessToProcessingCircuitsHandling, hasAccessToSignMySelfFeature, hasAccessToArchiveImmediatelyFeature } = userStateManager.getUser();

							// if (!hasAccessToProcessingCircuitsHandling && !hasAccessToSignMySelfFeature && !hasAccessToArchiveImmediatelyFeature) {
							// 	return;
							// }

							// let count = 3;

							// if (!hasAccessToProcessingCircuitsHandling) {
							// 	$('[usign]').remove();

							// 	count -= 1;
							// } else {
							// 	$('[usign]').click();
							// }
							
							// if (!hasAccessToSignMySelfFeature) {
							// 	$('[isign]').hide();
								
							// 	count -= 1;
							// } else {
							// 	if ($('[usign]').length <= 0) {
							// 		$('[isign]').click();
							// 	}
							// }

							// if (!hasAccessToArchiveImmediatelyFeature) {
							// 	$('[archiving]').hide();

							// 	count -= 1;
							// } else {
							// 	if ($('[usign]').length <= 0 && $('[isign]').length <= 0) {
							// 		$('[archiving]').click();
							// 	} 
							// }

							// $('[usign]').addClass(`col-${12 / count}`);
							// $('[isign]').addClass(`col-${12 / count}`);
							// $('[archiving]').addClass(`col-${12 / count}`);

							$('#box-setting-menu').show();
						}).catch((err) => {
							alert("Une erreur est survenue! Vérifiez que le document uploadé n'est pas corrompu!");

							$("#input-img").removeClass('hidden');

							$('#box-setting-menu').hide();
						});
					}
				}
			}
		} else {
			alert("Veuillez sélectionner un fichier \".pdf\" !!!")
		}
	}
});

$('#next').on('click', () => {
	const isValidPage = currentPDF.currentPage < currentPDF.countOfPages;

	if (!isValidPage) {
        return;
    }

	currentPDF.currentPage += 1;

	renderCurrentPage();
	$(document).trigger('refreshField');
});

$('#previous').on('click', () => {
	const isValidPage = currentPDF.currentPage - 1 > 0;

	if (!isValidPage) {
        return;
    }

	currentPDF.currentPage -= 1;

	renderCurrentPage();
	$(document).trigger('refreshField');
});
