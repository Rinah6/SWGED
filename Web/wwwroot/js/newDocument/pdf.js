import userStateManager from '../store.js';
import { currentPdf, setCurrentPdf, fullHide } from './global.js';

let test = false;
let lastFile;

function resetCurrentPdf() {
	setCurrentPdf({
		file: null,
		countOfPages: 0,
		currentPage: 1,
		zoom: 1.5
	});
}

$(document).ready(async () => {
	await userStateManager.init();
});

$('#input-file').on('click', (e) => {
	fullHide();
	document.getElementById("input-file").value = "";
	$('#pdf-viewer').removeAttr('width');
	$('#pdf-viewer').removeAttr('height');
	$("#input-img").removeClass("hidden");
});

$('#input-file').on('change', (e) => {
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
						resetCurrentPdf();

						const pdfFile = pdfjsLib.getDocument(fileReader.result);

						pdfFile.promise.then((doc) => {
							currentPdf.file = doc;
							currentPdf.countOfPages = doc.numPages;

							renderCurrentPage();
						}).then(() => {
							$("#input-img").addClass("hidden");

							const { hasAccessToProcessingCircuitsHandling, hasAccessToSignMySelfFeature, hasAccessToArchiveImmediatelyFeature } = userStateManager.getUser();

							if (!hasAccessToProcessingCircuitsHandling && !hasAccessToSignMySelfFeature && !hasAccessToArchiveImmediatelyFeature) {
								return;
							}

							let count = 3;

							if (!hasAccessToProcessingCircuitsHandling) {
								$('[usign]').remove();

								count -= 1;
							} else {
								$('[usign]').click();
							}

							if (!hasAccessToSignMySelfFeature) {
								$('[isign]').remove();

								count -= 1;
							} else {
								if ($('[usign]').length <= 0) {
									$('[isign]').click();
								}
							}

							if (!hasAccessToArchiveImmediatelyFeature) {
								$('[archiving]').remove();

								count -= 1;
							} else {
								if ($('[usign]').length <= 0 && $('[isign]').length <= 0) {
									$('[archiving]').click();
								}
							}

							$('[usign]').addClass(`col-${12 / count}`);
							$('[isign]').addClass(`col-${12 / count}`);
							$('[archiving]').addClass(`col-${12 / count}`);

							$('.box-setting-info').show();
						}).catch((err) => {
							alert("Une erreur est survenue! Vérifiez que le document uploadé n'est pas corrompu!");
							$("#input-img").removeClass("hidden");
							$('.box-setting-info').hide();
						});
					}
				}
			}
		} else {
			alert("Veuillez sélectionner un fichier \".pdf\" !!!")
		}
	}
});

//appel de la fonction
//getPageSize(9)
//	.then((pageSize) => {
//		console.log("Page size:", pageSize);
//	})
//	.catch((error) => {
//		console.error("Error:", error);
//	});

function renderCurrentPage() {
	currentPdf.file.getPage(currentPdf.currentPage).then((page) => {
		const viewer = $("#pdf-viewer")[0];
		const context = viewer.getContext('2d');
		const viewport = page.getViewport({ scale: currentPdf.zoom, });

		viewer.height = viewport.height;
		viewer.width = viewport.width;

		const renderContext = {
			canvasContext: context,
			viewport: viewport
		};

		page.render(renderContext);
	});

	$("#current_page").html(currentPdf.currentPage + ' sur ' + currentPdf.countOfPages);

	initPage();
}

function initPage() {
	if (!test || lastFile != currentPdf.file) {
		lastFile = currentPdf.file;

		$("[firstPage]").val("1");
		$("[firstPage]").attr("max", currentPdf.countOfPages);
		$("[firstPage]").attr("min", 1);

		$("[LastPage]").val(1);
		$("[LastPage]").attr("max", currentPdf.countOfPages);
		$("[LastPage]").attr("min", 1);
		test = !test;
	}
}

$('#next').on('click', (e) => {
	const isValidPage = currentPdf.currentPage < currentPdf.countOfPages;

	if (!isValidPage) {
		return;
	}

	currentPdf.currentPage += 1;
	renderCurrentPage();
	$(document).trigger('refreshField');
});

$('#previous').on('click', (e) => {
	const isValidPage = currentPdf.currentPage - 1 > 0;

	if (!isValidPage) {
		return;
	}

	currentPdf.currentPage -= 1;
	renderCurrentPage();
	$(document).trigger('refreshField');
});
