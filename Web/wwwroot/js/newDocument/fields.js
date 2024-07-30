import { currentPdf, currentUserFields, setCurrentUserFields } from './global.js';
import { currentRecipientId } from './global.js';

const dragOption = {
    containment: '#pdf-viewer',
    cursor: 'move',
    disabled: false,
    handle: '.ribbon'
}

let count = 0;

const canvasOffset = $('#pdf-viewer').offset();

const posX = canvasOffset.left;
const posY = canvasOffset.top;

let showAll = true;

$('[showAll]').on('click', (e) => {
    let header = $(e.target).closest('[showAll]');
    let icon = header.find("i");

    if (icon.hasClass('fa-square')) {
        showAll = true;
        icon.removeClass('fa-square');
        icon.addClass('fa-square-check');
    } else {
        showAll = false;
        icon.removeClass('fa-square-check');
        icon.addClass('fa-square');
    }
    
    $(document).trigger('refreshField');
})

$(`[data-action="addField"]`).on("click", (e) => {
    let header = $(e.target).closest('[data-type]');

    let firstPage = parseInt(header.find(`[firstPage]`).val());
    let lastPage = parseInt(header.find(`[lastPage]`).val());

    if (Number.isNaN(firstPage)) {
        return alert("Vérifier le numero de page!");
    }

    const listContainer = $(e.target).attr("data-target");

    if (!Number.isNaN(lastPage) && lastPage !== 0) {
        if (firstPage > lastPage) {
            let tempPage = firstPage;
            firstPage = lastPage;
            lastPage = tempPage;
        }
    } else {
        lastPage = firstPage;
    }

    const type = `${header.attr("data-type")}`;
    const variable = `${type}${count++}`;

    const title = `${header.find("[data-title]").text()}`;

    $(`#${listContainer}`).before(`
        <li class="nav-item" page-id="${variable}" by="${currentRecipientId}" data-type="${type}" data-value="${firstPage == lastPage ? `${firstPage}` : `${firstPage} - ${lastPage}`}" field-firstPage="${firstPage}" field-lastPage="${lastPage}">
            <div class="nav-link float-right">
                <span id="${variable}">Page : ${firstPage == lastPage ? `${firstPage}` : `${firstPage} à ${lastPage}`} </span>
                <div class="btn btn-sm" removeField>
                    <i class="fa fa-times text-danger" style="font-size:1.1rem"></i>
                </div>
            </div>
        </li>
    `);

    console.log(title);

    $(`#fields-list-box`).append(`
        <div class="boxSign" data-type="${type}" by="${currentRecipientId}" field-id="${variable}" data-page="${firstPage == lastPage ? `${firstPage}` : `${firstPage} - ${lastPage}`}" field-firstPage="${firstPage}" field-lastPage="${lastPage}" style="border: 5px dashed red; ">
            <div class="ribbon-wrapper">
                <div class="ribbon text-white" style="background-color: red; ">
                    ${title}
                </div>
            </div>
        </div>
    `);

    const newField = {
        variable,
        x: 0,
        y: 0,
        width: 0,
        height: 0,
        fieldType: parseInt($(header).attr('data-id')),
        firstPage: Number(firstPage),
        lastPage: Number(lastPage),
        PDF_Width: 0,
        PDF_Height: 0
    };

    setCurrentUserFields([...currentUserFields, newField]);

    // setUsersDocumentsList(usersDocumentsList.map(userDocument => {
    //     if (userDocument.id === currentRecipientId) {
    //         return { ...userDocument, fields: [...userDocument.fields, newField]}
    //     }

    //     return userDocument;
    // }));

    // const recipient = usersDocumentsList.find(userDocument => userDocument.id === currentRecipientId);

    activeField(variable, newField);

    $(document).trigger('refreshField');
    $(`[field-id]`).mousemove();
});

function getPageSize(p) {
	return new Promise((resolve, reject) => {
		try {
			currentPdf.file.getPage(p).then((page) => {
				var viewport = page.getViewport({ scale: currentPdf.zoom })

				resolve({
					width: viewport.width,
					height: viewport.height
				});
			}, (e) => {
				reject("Page out of range");
			});
		} catch (error) {
			console.error(error);
			reject("An error occurred");
		}
	});
}

function activeField(variable, recipient) {
    $(`[field-id="${variable}"],[page-id="${variable}"],[recipient-id="${recipient}"]`).hover((e, x) => {
        if ($(`[recipient-id="${recipient.id}"]`).css("background-color") !== recipient.color) {
            $(`[recipient-id="${recipient.id}"]`).css("background-color", recipient.color);
        }

        if ($(`[page-id="${variable}"]`).css("background-color") !== recipient.color) {
            $(`[page-id="${variable}"]`).css("background-color", recipient.color);
        }

        if ($(`[field-id="${variable}"]`).css("background-color") !== recipient.color + "60") {
            $(`[field-id="${variable}"]`).css("background-color", recipient.color + "60");
        }

        $($(`[recipient-id="${recipient}"]`).find('span')).css("color", "white");
        $(`[page-id="${variable}"]`).find('span').css("color", "white");
    }, (e) => {
        if ($(`[recipient-id="${recipient.id}"]`).css("background-color")) {
            $(`[recipient-id="${recipient.id}"]`).css("background-color", "");
        }

        if ($(`[page-id="${variable}"]`).css("background-color")) {
            $(`[page-id="${variable}"]`).css("background-color", "");
        }
            
        $(`[field-id="${variable}"]`).css("background-color", "rgba(68, 65, 65, 0.59)");

        $($(`[recipient-id="${recipient}"]`).find('span')).css("color", "");
        $(`[page-id="${variable}"]`).find('span').css("color", "");
    });

    $(`[field-id="${variable}"]`).mousemove((e) => {
        const header = $(e.target).closest("[field-id]");

        const divPos = {
            left: e.pageX - header.offset().left,
            top: e.pageY - header.offset().top,
        };

        if (divPos.left > $(this).width() - 11 && divPos.top > $(this).height() - 11) {
            dragOption.disabled = true;
        } else {
            dragOption.disabled = false;
        }

        const pdfContainer = $('#pdf-viewer').offset();
        const offsetSign = header.offset();
        const posSign = {
            top: offsetSign.top - pdfContainer.top,
            left: offsetSign.left - pdfContainer.left
        };

        setCurrentUserFields(currentUserFields.map((field) => {
            if (field.variable === variable) {
                return { 
                    ...field,
                    x: parseInt(posSign.left),
                    y: parseInt(posSign.top),
                    width: parseInt(header.width()),
                    height: parseInt(header.height()),
                }
            }

            return field;
        }));

        const page = currentUserFields.find(field => field.variable === variable).firstPage;

        getPageSize(page)
            .then((pageSize) => {
                setCurrentUserFields(currentUserFields.map((field) => {
                    if (field.variable === variable) {
                        return { 
                            ...field,
                            PDF_Width: pageSize.width,
                            PDF_Height: pageSize.height,
                        }
                    }
        
                    return field;
                }));
            })
            .catch((error) => {
                console.error("Error:", error);
            });
        $(`[field-id="${variable}"]`).draggable(dragOption);
    });

    $(`[field-id="${variable}"]`).hover();
    //$(`[field-id="${id}"]`).mousemove();

    $(`[field-id="${variable}"]`).css({ "top": (posY + 25) + "px", "left": (posX + 25) + "px" });
}

$(document).on('click', '[removeField]', (e) => {
    const id = $(e.target).closest("[page-id]").attr('page-id');
    
    removeField(id);
});

function removeField(variable) {
    const field = $(`[field-id="${variable}"]`);
    const recipient = field.attr("by");
    const pageLine = $(`[page-id="${variable}"]`);

    // setUsersDocumentsList(usersDocumentsList.map(userDocument => {
    //     if (userDocument.id === recipient.id) {
    //         return {
    //             ...userDocument,
    //             fields: userDocument.fields.filter(userDocumentField => userDocumentField.variable !== variable)
    //         }
    //     }

    //     return userDocument;
    // }));

    field.remove();
    pageLine.remove();
}

$(document).on('refreshField', (e) => {
    $('[by][field-id]').show();
    
    if (showAll) return;
    
    $(`[by][field-id]`).each((k, v) => {
        const page = currentPdf.currentPage;
        const fpage = Number.parseInt($(v).attr('field-firstPage'));
        const lpage = Number.parseInt($(v).attr('field-lastPage'));
        
        if (fpage > page || lpage < page) {
            $(v).hide();
        }
    });
});

$('[firstPage], [LastPage]').on('change', (e) => {
    const max = parseInt($(e.target).attr('max'));
    const val = parseInt($(e.target).val());
    const min = parseInt($(e.target).attr('min'));

    const firstPage = $(e.target).closest('[data-type]').find('[firstPage]');
    const lastPage = $(e.target).closest('[data-type]').find('[LastPage]');

    if (Number.parseInt(firstPage.val()) >= Number.parseInt(lastPage.val())) {
        const attr = $(e.target).attr('firstPage');

        if (typeof attr !== 'undefined' && attr !== false) {
            lastPage.val(firstPage.val());
        } else {
            firstPage.val(lastPage.val());
        }
    }
    
    if (val > max) {
        $(e.target).val(max);
    } else if (val <= 0) {
        $(e.target).val(min);
    }
});
