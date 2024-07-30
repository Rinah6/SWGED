'use strict';

import { apiUrl, webUrl } from '../apiConfig.js';

const loader = $('#loader');

//new Typed(".typing", {
//    strings: ['Zéro Papier', 'Facile à utiliser', 'Rapide et Sécurisé'],
//    typeSpeed: 100,
//    backSpeed: 70,
//    loop: true
//});

$('[data-id="username"], [data-id="password"]').on('keydown', (e) => {
	if (e.keyCode == 13) {
		$('[data-id="login"]').click();
	}
});

$('[data-id="login"]').on('click', async () => {
	const username = $('[data-id="username"]').val();
	const password = $('[data-id="password"]').val();

	if (username === '' || password === '') {
		alert('Veuillez vérifier vos identifiants!');

		return;
	}

	try {
		loader.removeClass('display-none');

		await axios.post(apiUrl + `api/login`, {
			username,
			password
		}, {
			withCredentials: true
		});

		window.location = webUrl + `documents`;
	} catch (error) {
		alert(`Email ou mot de passe erroné!`);
	} finally {
		loader.addClass('display-none');
	}
});
