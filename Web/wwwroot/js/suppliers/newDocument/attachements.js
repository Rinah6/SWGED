import { attachements } from './global.js';

let compteur = 0;

$('#attachement').on('click', (e) => {
    $(`#input-attachements`).click();
});

$(document).on('click', '[remove-attachement]', (e) => {
    let attachement = $(e.currentTarget).parent().closest("[attachement-ids]");
    let index = parseInt(attachement.attr("attachement-ids"));

    const pos = attachements.map(e => e.id).indexOf(index);

    attachements.splice(pos, 1);
    attachement.remove();
});

$(`#input-attachements`).on('change', (e) => {
    const inputFile = e.target.files;

    const extension = ["JPG", "JPEG", "PNG", "WEBP", "TIFF", "PDF", "TXT"];

    if (inputFile) {
        let can = 0;
        for (let i = 0; i < inputFile.length; i++) {
            let filetype = inputFile[i].name.split(".");

            if (extension.indexOf(filetype[filetype.length - 1].toUpperCase()) > -1) {
                can++;
            }
        }
        if (inputFile.length == can)
        {
            for (let i = 0; i < inputFile.length; i++)
            {
                $('#attachements-list-tab').before(`
                    <li class="nav-item" attachement-ids="${compteur}" attachement hidden>
                        <div class="nav-link d-flex align-items-center justify-content-between">
                            <div class="d-flex align-items-baseline">
                                <i class="fa fa-file"> </i><span style="padding-left: 10px" attachement-name></span>
                            </div>
                            <i class="fa fa-times text-danger float-right" style="cursor:pointer;" remove-attachement="${compteur}"></i>
                        </div>
                    </li>
                `);

                $(`[attachement-ids]`).removeAttr("hidden");
                $(`[attachement-ids=${compteur}]`).find('[attachement-name]').text(inputFile[i].name);

                attachements.push({
                    id: compteur,
                    file: inputFile[i]
                });

                compteur += 1;
            }
        }
        else
        {
            alert("Seuls les formats de fichiers suivant sont acceptés : .jpg, .jpeg, .png, .webp , .tiff , .pdf, .txt !");
        }
    }
});
