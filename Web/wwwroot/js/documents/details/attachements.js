import { apiUrl } from '../../apiConfig.js';

const loader = $('#loader');

async function showAttachements(id) {
	$("[attachement-id]").remove();
	$("#attachement").after("");

	$('#attachment_menu').show();

	const { data } = await axios.get(apiUrl + `api/documents/${id}/attachements`, {
		withCredentials: true
	});

	let code = "";

	$.each(data, (_, v) => {
		code += attachementUI(v.id, v.filename);
	});

	$("#attachement").after(code);

	$('#Addattachement').on('click', (e) => {
		$(`#list_pj`).click();
	});

	$(document).on('click', '[remove-attachement]', (e) => {
		if (!window.confirm('Voulez-vous vraiment supprimer cette élément?'))
			return;
		let attachement = $(e.target).closest("[attachement-member]");
		let PJID = attachement.attr("attachement-member");

		$.ajax({
			type: 'DELETE',
			contentType: false,
			processData: false,
			async: true,
			url: apiUrl + "api/attachement/delete/" + PJID,
			xhrFields: { withCredentials: true },
			beforeSend: function () {
				loader.removeClass('display-none');
			},
			complete: function () {
				loader.addClass('display-none');
			},
			success: function () {
				alert("Pièce jointe suprrimée!");
			},
			Error: function (x, e) {
				alert("Please contact the administrator!");
			}
		});
		attachement.remove();
	});

	$(document).on('change', `#list_pj`, (e) => {
		const header = $(document).find('[document-id]');
		const Documentids = header.attr('document-id');

		const inputFile = e.target.files;
		const formData = new FormData();
		if (inputFile) {
			for (let i = 0; i < inputFile.length; i++) {
				formData.append('PJ', inputFile[i]);
			}
			$.ajax({
				type: 'POST',
				data: formData,
				contentType: false,
				processData: false,
				async: true,
				url: apiUrl + "api/attachement/add/" + Documentids,
				xhrFields: { withCredentials: true },
				beforeSend: function () {
					loader.removeClass('display-none');
				},
				complete: function () {
					loader.addClass('display-none');
				},
				success: async function () {
					$("[attachement-member]").remove();

					const { data } = await axios.get(apiUrl + `api/documents/attachements/${id}`, {
						withCredentials: true
					});

					let code = "";

					$.each(data, (_, v) => {
						code += attachementUI(v.id, v.filename);
					});

					$("#attachement").after(code);

					alert("Pièce jointe ajoutée!");
				},
				error: function (x, e) {
					alert("Please contact the administrator!");
				}
			});
		}
	});

	$(document).on('click', '[attachement-name]', (e) => {
		let headerss = $(e.target).closest(`[attachement-member]`);
		let sinput = headerss.find('[edit-attachement]');
		let sname = headerss.find('[attachement-name]');
		sinput.show();
		sinput.trigger("focus");

		sname.hide();
	});

	$(document).on('focusout', '[edit-attachement]', (e) => {
		let headerss = $(e.target).closest(`[attachement-member]`);
		let attachementIds = headerss.attr(`attachement-member`);
		let sinput = headerss.find('[edit-attachement]');
		let sname = headerss.find('[attachement-name]');

		if (sinput.val().localeCompare(sname.text()) != 0) {
			sname.html(sinput.val());
			sname.attr("attachment-name", sinput.val());
			const formData = new FormData();
			formData.append("Filename", sinput.val());
			$.ajax({
				type: 'PUT',
				data: formData,
				contentType: false,
				processData: false,
				async: true,
				url: apiUrl + "api/attachement/rename/" + attachementIds,
				xhrFields: { withCredentials: true },
				beforeSend: function () {
					loader.removeClass('display-none');
				},
				complete: function () {
					loader.addClass('display-none');
				},
				success: function () {
					alert("Pièce jointe renommée avec succès!");

				},
				error: function (x, e) {
					alert("Please contact the administrator!");
				}
			});
		}

		sinput.hide();
		sname.show();
	});
}

function attachementUI(id, name) {
	return `
        <li class="nav-item" attachement-member="${id}">
            <div class="nav-link d-flex align-items-center justify-content-between" >
	            <div class="nav-link" >		            
                    <i class="fas fa-file-archive"></i>
                    <span attachement-name="${name}">${name}</span>
                    <input class="" type="text" value=${name} edit-attachement="${id}" style="display:none;border:none;" size="50" />
	            </div>

                <div class="float-right">
                    <i class="fa fa-download text-success" style="cursor:pointer;" attachement-id="${id}"></i>

                    <br />

                    <i class="fa fa-times text-danger" style="cursor:pointer;" remove-attachement="${id}"></i>

                    <br />

                    <i class="fa fa-eye" style="cursor:pointer;" display-attachement="${id}"></i>
                </div>
            </div>
        </li>
	`;
}

$(document).on('click', '[attachement-id]', async (e) => {
	const id = $(e.currentTarget).closest("[attachement-id]").attr("attachement-id");
	let headerss = $(e.target).closest(`[attachement-member]`);
	let attachementIds = headerss.attr(`attachement-member`);
	let filename = headerss.find('[attachement-name]').attr('attachement-name');

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

$(document).on('click', "[display-attachement]", async (e) => {
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
		$("#modal-attachement").modal('show');
	} catch (error) {
		alert(error.message);
	} finally {
		loader.addClass('display-none');
	}
});
