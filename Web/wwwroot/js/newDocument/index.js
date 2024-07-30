import { apiUrl } from '../apiConfig.js';
import userStateManager from '../store.js';

$(document).ready(async () => {
    await userStateManager.init();

    const { hasAccessToRSF } = userStateManager.getUser();

    console.log(hasAccessToRSF);

    if (hasAccessToRSF) {
        $('#rsf-container').prepend(`
            <div class="form-group">
                <input type="checkbox" id="rsf" />
    
                <label for="rsf">RSF</label>
            </div>
        `);
    }

    await getSites();


});

$(`#togglebox-setting`).on('click', (e) => {
	$("#box-setting-menu").removeClass('closed');
});

$(`#closeSideMenu`).on('click', (e) => {
	$("#box-setting-menu").addClass('closed');
});

$(`#input-img`).on('click', (e) => {
	if ($(`#input-file`).val() == "")
		$('#input-file').click();
});

$(`[data-action="open-pdf"]`).on('click', (e) => {
	$('#input-file').click();
});

$(document).ready(function () {
    //$('[data-toggle="tooltip"]').tooltip();
    $('#message').summernote({
        lang: 'fr-FR',
        height: 200,
        toolbar: [
        ]
    });
    $('#mailMessage').summernote({
        lang: 'fr-FR',
        height: 300,
        toolbar: [
            //['style', ['bold', 'italic', 'underline', 'clear']],
            //['font', ['strikethrough', 'superscript', 'subscript']],
            ////['fontsize', ['fontsize']],
            //['color', ['color']],
            //['para', ['ul', 'ol']],//, 'paragraph']],
            ////['height', ['height']]
        ]
    });



});

async function getSites() {

    const select = document.createElement("select");
    //select.multiple = true;
    //select.setAttribute("search", true);
    select.className = "form-control";
    //select.style.display = "none";
    select.id = "select-current-site";

    const { data: Sites } = await axios.get(apiUrl + `api/sites`, {
        withCredentials: true
    });

    for (let i = 0; i < Sites.length; i += 1) {
        const opt = document.createElement("option");
        opt.value = Sites[i].id;
        opt.textContent = Sites[i].siteId + ' - ' + Sites[i].name;
        select.append(opt);
    }

    const s = document.getElementById("div-current-site");
    s.append(select);


}



function resetMail() {
    $("#objectId").val("");
    $("#mailMessage").summernote('code', "");
}

$(`[data-action="resetMail"]`).on("click", (e) => {
    resetMail();
});
