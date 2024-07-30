import { apiUrl } from "../../apiConfig.js";
import { formatDate } from "../../utils.js";

const loader = $('#loader');

let currentPDF = {};
let test = false;
let lastFile;

let documentId = '';

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
        const viewer = ($('#Panel').find("#pdf-viewer"))[0];
        const context = viewer.getContext('2d');
        const viewport = page.getViewport({ scale: currentPDF.zoom, });

        viewer.height = viewport.height;
        viewer.width = viewport.width;

        const renderContext = {
            canvasContext: context,
            viewport: viewport
        };
        page.render(renderContext);
    });
    $("#current_page").html(currentPDF.currentPage + ' sur ' + currentPDF.countOfPages);
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

function displayGlobalDynamicFields(arr) {
    const container = $('#document-info');

    for (let i = 0; i < arr.length; i += 1) {
        container.append(`
			<div class="label-flex mailbox-controls with-border p-3" id="${arr[i].id}">
				<h6>
					<u>${arr[i].label}</u> : 
				</h6>
				<div>
					${arr[i].value}
				</div>
			</div>
		`);
    }
}

function displayDynamicAttachements(arr) {
    const container = $('#document-info');

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

function documentUI(document) {
    return `
		<div id="p_doc" data-type="panel" document-id="${document.id}">
			<div class="card card-primary card-outline mb-1">
				<div class="card-header" _status="${document.status}">
					<br />
					<br />

					<h6 class="col">
						<u>Objet</u> : ${document.object}
					</h6>

					<div class="col">
						<span class="mailbox-read-time"></span>
						<span class="mailbox-read-time float-right">${formatDate(document.dateSend)}</span>
					</div>
				</div>

				<div class="card-body">
					<div id="document-info">
						<div class="label-flex mailbox-controls with-border p-3">
							<h6>
								<u>Message </u> : 
							</h6>
							<div>
								${document.message === null ? 'Aucun message' : document.message}
							</div>
						</div>
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
                    <button class="btn btn-primary btn-sm" download_button="${document.filename}">
						Télécharger document
					</button>
				</div>
			</div>
		</div>
	`;
}

function attachementUI(id, name) {
    return `
        <li class="nav-item" style="cursor: pointer;">
            <div class="nav-link d-flex align-items-center justify-content-between" >
	            <div class="nav-link">
		            <i class="fas fa-file-archive"></i> <span attachement-name="${name}">${name}</span>
	            </div>
                <div class="float-right">
                    <i class="fa fa-download text-success" style="cursor:pointer;" attachement-id="${id}"></i>
                    <br />
                    <br />
                    <i class="fa fa-eye" style="cursor:pointer;" display-attachement="${id}"></i>
                </div>
            </div>
        </li>
    `;
}

async function showAttachements(id) {
    $("[attachement-id]").remove();
    $('#attachment_menu').show();

    const { data } = await axios.get(apiUrl + `api/documents/${id}/attachements`, {
        withCredentials: true
    });

    let code = "";

    $.each(data, (_, v) => {
        code += attachementUI(v.id, v.filename);
    });

    $("#attachement").after(code);
}
$(document).on("click", "[display-attachement]", async (e) => {
    try {
        loader.removeClass('display-none');

        const d_attachement = $(e.target).attr("display-attachement");
        const filename = $(e.target).parent().parent().find("[attachement-name]").attr("attachement-name");
        let blobObj = "";
        let ext = filename.split(".");
        let filetype = ext[ext.length - 1];
        const { data: attBlob } = await axios.get(apiUrl + `api/attachement/render/${d_attachement}`, {
            withcredentials: true,
            responsetype: 'blob'
        });

        let inputFile = "";

        if (filetype.toLowerCase() == "pdf") {
            blobObj = new Blob([attBlob], { type: "application/pdf" });
            inputFile = URL.createObjectURL(blobObj);
            $("#represent-image").hide();
            $("#represent-attachement").show();
            $("#represent-attachement").attr(`src`, inputFile);
        } else if (filetype.toLowerCase().indexOf("jp") > -1 || filetype.toLowerCase().indexOf("webp") > -1 || filetype.toLowerCase().indexOf("tif") > -1 || filetype.toLowerCase().indexOf("png") > -1) {
            inputFile = attBlob;
            $("#represent-image").show();
            $("#represent-attachement").hide();
            $("#represent-image").attr(`src`, inputFile);
        } else {
            blobObj = new Blob([attBlob], { type: "text/plain" });
            inputFile = URL.createObjectURL(blobObj);
            $("#represent-image").hide();
            $("#represent-attachement").show();
            $("#represent-attachement").attr(`src`, inputFile);
        }

        $("#modal-attachement").show();
        $("#modal-attachement").modal("show");
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$(document).ready(async () => {
    const url = window.location.href;
    const tab = url.split("/");
    const id = tab[tab.length - 1];

    documentId = id;

    try {
        loader.removeClass('display-none');

        const { data: documentInfo } = await axios.get(apiUrl + `api/documents/${id}/details`, {
            withCredentials: true
        });

        $(`#Panel`).html(documentUI(documentInfo));

        const { data: dynamicFields } = await axios.get(apiUrl + `api/dynamic_fields?documentId=${id}`, {
            withCredentials: true
        });

        displayGlobalDynamicFields(dynamicFields);

        const { data: dynamicAttachements } = await axios.get(apiUrl + `api/dynamic_attachements/documents/${id}`, {
            withCredentials: true
        });

        displayDynamicAttachements(dynamicAttachements);

        const { data: pdfBlob } = await axios.get(apiUrl + `api/documents/archived/${id}`, {
            withCredentials: true,
            responseType: 'blob'
        });

        const inputFile = URL.createObjectURL(pdfBlob);

        resetCurrentPDF();

        const pdfFile = pdfjsLib.getDocument(inputFile);

        pdfFile.promise.then((doc) => {
            currentPDF.file = doc;
            currentPDF.countOfPages = doc.numPages;

            renderCurrentPage();
        }).then(() => {
            $('#default-pdf-viewer').remove();

            $('#Panel').find('#next').on('click', () => {
                const isValidPage = currentPDF.currentPage < currentPDF.countOfPages;
                if (!isValidPage) return;

                currentPDF.currentPage += 1;
                renderCurrentPage();
                $(document).trigger('refreshField');
            });

            $('#Panel').find('#previous').on('click', () => {
                const isValidPage = currentPDF.currentPage - 1 > 0;
                if (!isValidPage) {
                    return;
                }

                currentPDF.currentPage -= 1;
                renderCurrentPage();
                $(document).trigger('refreshField');
            });
        }).catch((_) => {
            throw new Error("Une erreur est survenue! Vérifiez que le document uploadé n'est pas corrompu !");
        });

        await showAttachements(id);
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$(document).on('click', `[download_button]`, async (e) => {
    const url = window.location.href;
    const tab = url.split('/');
    const id = tab[tab.length - 1];
    let documentName = '';

    try {
        loader.removeClass('display-none');

        const { data } = await axios.get(apiUrl + `api/documents/name/${id}`);
        documentName = data;

        const { data: pdfBlob } = await axios.get(apiUrl + `api/documents/archived/${id}`, {
            withCredentials: true,
            responseType: 'blob'
        });

        const blobUrl = URL.createObjectURL(pdfBlob);
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

$(document).on('click', '[attachement-id]', async (e) => {
    const id = $(e.currentTarget).closest("[attachement-id]").attr("attachement-id");
    const filename = $(e.currentTarget).parent().parent().find("[attachement-name]").attr("attachement-name");

    try {
        loader.removeClass('display-none');

        const { data } = await axios.get(apiUrl + `api/download/attachements/${id}`, {
            withCredentials: true,
            responseType: 'blob'
        });

        const url = window.URL.createObjectURL(data);
        const a = document.createElement('a');
        a.href = url;
        a.download = filename;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
    } catch (error) {
        alert(error.message);
    } finally {
        loader.addClass('display-none');
    }
});

$(document).on('click', '[dynamic-field-id]', async (e) => {
    try {
        loader.removeClass('display-none');

        const id = $(e.currentTarget).attr('dynamic-field-id');

        const { data } = await axios.get(apiUrl + `api/download/dynamic_attachements/${id}?documentId=${documentId}`, {
            withCredentials: true,
            responseType: 'blob'
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
