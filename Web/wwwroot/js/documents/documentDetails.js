$(document).on('click', '[ViewDocument]', async (e) => {
	signExist = false;
	parapheExist = false;
	$("[attachement-member]").remove();

	loaderDoc();

	$(`#p_MyDocument`).hide();
	$("#sign").text("");

	if (!documentInfo.isTheCurrentUserTheSender || documentInfo.status === 3) {
		$('#Panel').find('[principal_file]').remove();
		$('#Panel').find('[change_document]').remove();
		$('#list_pj').remove();
		$('#Addattachement').remove();
	}

	if (!documentInfo.isTheCurrentUserTheSender) {
		$('#attachment_menu').find('[remove-attachement]').remove();
	}
});

// $(document).on('click', '[ViewHiStory]', (e) => {
// 	const header = $(e.target).closest('[ViewHiStory]');
// 	const id = header.attr('ViewHiStory');

// 	location.href = webUrl + `document/historique/` + id;
// });

// $(document).on('click', '[document-action="share"]', () => {
// 	const documentId = $('[document-id]').attr('document-id');
// 	const list = $("#email_receiver").val().replace(" ", "");

// 	$.ajax({
// 		type: "POST",
// 		url: apiUrl + "api/document/share/" + documentId,
// 		contentType: 'application/json',
// 		datatype: 'json',
// 		xhrFields: { withCredentials: true },
// 		data: JSON.stringify(list),
// 		beforeSend: function () {
// 			// loader.removeClass('display-none')
// 		},
// 		complete: function () {
// 			// loader.addClass('display-none')
// 		},
// 		success: function (result) {
// 			if (result) {
// 				Toast.fire({
// 					icon: 'success',
// 					title: "Opération réussie!"
// 				});
// 			} else {
// 				Toast.fire({
// 					icon: 'error',
// 					title: "Erreur s'est produit"
// 				});
// 			}
// 		},
// 		Error: function () {
// 			alert("Some error");
// 		}
// 	});
// });
