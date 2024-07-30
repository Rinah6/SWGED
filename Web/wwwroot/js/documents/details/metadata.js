import { apiUrl } from '../../apiConfig.js';
import { documentId } from './global.js';

$(document).on('click', '[file-document-name]', (e) => {
    let sinput = $('[edit-document-name]');
    let sname = $('[file-document-name]');
    sinput.show();
    sinput.trigger("focus");
    sname.hide();
});

let attachementIds = $(`[document-id]`).attr(`document-id`);

$(document).on('focusout', '[edit-document-name]', (e) => {
    let sinput = $('[edit-document-name]');
    let sname = $('[file-document-name]');

    if (sinput.val().localeCompare(sname.text()) != 0) {
        sname.html(sinput.val());
        sname.attr("file-document-name", sinput.val());
        const formData = new FormData();
        formData.append("Filename", sinput.val());
        $.ajax({
            type: 'PUT',
            data: formData,
            contentType: false,
            processData: false,
            async: true,
            url: apiUrl + "api/document/changefilename/" + attachementIds,
            xhrFields: { withCredentials: true },
            beforeSend: function () {
                loader.removeClass('display-none');
            },
            complete: function () {
                loader.addClass('display-none');
            },
            success: function () {
                alert("Document Renommer avec succès!");

            },
            error: function (x, e) {
                alert("Please contact the administrator!");
            }
        });
    }

    sinput.hide();
    sname.show();
});

$("[change_document]").on('click', (e) => {
    $("[principal_file]").click();
});

$(document).on('change', `[principal_file]`, (e) => {
    const inputFile = e.target.files[0];
    const formData = new FormData();
    formData.append("PJ", inputFile);
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
                $.ajax({
                    type: 'PUT',
                    data: formData,
                    contentType: false,
                    processData: false,
                    async: true,
                    url: apiUrl + "api/document/changefile/" + attachementIds,
                    xhrFields: { withCredentials: true },
                    beforeSend: function () {
                        loader.removeClass('display-none');
                    },
                    complete: function () {
                        loader.addClass('display-none');
                    },
                    success: function () {
                        alert("Document remplacé avec succès!");
                    },
                    error: function (x, e) {
                        alert("Please contact the administrator!");
                    }
                });

                const fileReader = new FileReader();

                fileReader.readAsDataURL(inputFile);

                fileReader.onload = async () => {
                    resetCurrentPDF();

                    const pdfFile = pdfjsLib.getDocument(fileReader.result);

                    pdfFile.promise.then((doc) => {
                        currentPDF.file = doc;
                        currentPDF.countOfPages = doc.numPages;

                        renderCurrentPage();
                    }).then(() => { });
                }
            }
        }
    }
});
