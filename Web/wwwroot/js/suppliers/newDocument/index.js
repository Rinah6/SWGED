import { attachements } from './global.js';
import { apiUrl, webUrl } from '../../apiConfig.js';
import { supplierStateManager } from '../../store.js';
import { globalDynamicFields } from './global.js';
import { verifyMail } from '../../utils.js';

const loader = $('#loader');

let projectId = '';

let globalDynamicFieldsToSend = [];

const dynamicAttachements = [];

const title = $('[detail-id="title"]');
const email = $(`[data-id="email"]`);
const object = $('#object');
const message = $('#message');

function verifyAllRequiredDynamicFields() {
	globalDynamicFieldsToSend = [];

	if (title.val() === '') {
		alert('Champs obligatoires non vides!');
	
		return false;
	}

	for (let i = 0; i < globalDynamicFields.length; i += 1) {
		const id = globalDynamicFields[i].id;

		if (globalDynamicFields[i].isRadioButton) {
			const radios = document.getElementsByName(id);

			for (let j = 0; j < radios.length; j += 1) {
                if (radios[j].checked) {
                    globalDynamicFieldsToSend.push({ id, value: radios[j].value });

                    break;
                }
            }
		} else if (globalDynamicFields[i].isOfTypeFile) {
			const dynamicAttachement = $(`#${id}`).get(0).files;

			if (globalDynamicFields[i].isRequired && dynamicAttachement.length === 0) {
				alert('Champs obligatoires non vides!');
	
				return false;
			}

			dynamicAttachements.push({ id, file: dynamicAttachement[0] });
		} else {
			const value = document.getElementById(id).value;
	
			if (globalDynamicFields[i].isRequired) {
				if (!value) {
					alert('Champs obligatoires non vides!');
	
					return false;
				}
			}
	
			globalDynamicFieldsToSend.push({ id, value });
		}
    }

	return true;
}

$(document).ready(async () => {
	try {
		await supplierStateManager.init();
	
		const { projectId: projectId_, project } = supplierStateManager.getSupplier();

		projectId = projectId_;

        $('#project-name').text(project);

        await getSites();

    } catch (error) {
        $('body').html(`
			<h1 style="font-size: 128px; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">404</h1>
		`);
    } finally {
        loader.addClass('display-none');
    }
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



$('#send-document').on('click', async () => {
	try {
		if (!verifyAllRequiredDynamicFields()) {
			return;
		}
	
		if (!verifyMail(email.val())) {
			alert('Email invalide!');
	
			return;
		}
	
		const files = $("#input-pdf").get(0).files;


        const select = document.querySelector('#select-current-site');
        const currentSelectedSitesId = [];
        for (const option of select.options)
            option.selected && currentSelectedSitesId.push(option.value);


		let formData = new FormData();
		
		formData.append('pdfDocument', files[0]);
		formData.append('title', title.val());
		formData.append('email', email.val());
		formData.append('object', object.val());
		formData.append('message', message.summernote('code'));
		formData.append('globalDynamicFields', JSON.stringify(globalDynamicFieldsToSend));
        formData.append('Site', currentSelectedSitesId);

		for (let i = 0; i < attachements.length; i += 1) {
			formData.append('attachements', attachements[i].file);
		}
	
		loader.removeClass('display-none');
	
		const { data: documentId } = await axios.post(apiUrl + 'api/suppliers/documents', formData, {
			withCredentials: true,
			headers: {
				'Content-Type': 'multipart/form-data',
			},
		});

		for (let i = 0; i < dynamicAttachements.length; i += 1) {
			if (dynamicAttachements[i].file !== undefined) {
				formData = new FormData();
	
				formData.append('File', dynamicAttachements[i].file);
	
				await axios.post(apiUrl + `api/dynamic_attachements/documents/${documentId}?globalDynamicFieldId=${dynamicAttachements[i].id}`, formData, {
					withCredentials: true,
					headers: {
						'Content-Type': 'multipart/form-data',
					},
				});
			}
		}

		await axios.delete(apiUrl + `api/suppliers/logout`, {
			withCredentials: true,
		});
	
		alert("Document envoyé!");
        window.location.href = webUrl + `suppliers/${projectId}`;

	} catch (error) {
		console.log(error.message);

        alert("Document envoyé!");
        window.location.href = webUrl + `suppliers/${projectId}`;

		//alert('Votre session a expiré! Veuillez vous reconnecter!');
	} finally {
		loader.addClass('display-none');
	}
});
