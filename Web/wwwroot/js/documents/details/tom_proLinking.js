import { apiUrl, webUrl } from '../../apiConfig.js';
import { escapeRegex, formatCurrency } from '../../utils.js';
import { documentId } from './global.js';

const loader = $('#loader');
const skeletonLoader = $('#skeleton-loader');

let tmpLiquidations = [];
let tmpAvances = [];
let tmpJustificatifs = [];
let tmpReversements = [];

$(document).ready(async () => {
	const { data: tomProConnections } = await axios.get(apiUrl + `api/tom_pro_connections`, {
		withCredentials: true,
	});

	let content = `<option value="" selected></option>`;

	for (let i = 0; i < tomProConnections.length; i += 1) {
		content += `
			<option value="${tomProConnections[i].id}">${tomProConnections[i].serverName}</option>
		`;
	}

	$('#tom-pro-connections').html(content).select2({
		dropdownParent: $('#tom-pro-linking-modal'),
	});
});

$(document).on('click', `[data-action="tom-pro-link-transfer"]`, () => {
	$('#tom-pro-linking-modal').modal('show');
});

$('#tom-pro-connections').on('change', async (e) => {
    const currentValue = $(e.currentTarget).val();

    if (currentValue === '') {
        $('#tom-pro-databases-container').html(``);

        return;
    }

    const { data: tomProDatabases } = await axios.get(apiUrl + `api/tom_pro_connections/${currentValue}/databases`, {
        withCredentials: true,
    });

    let content = `<option value="" selected></option>`;

    for (let i = 0; i < tomProDatabases.length; i += 1) {
        content += `
            <option value="${tomProDatabases[i].id}">${tomProDatabases[i].databaseName}</option>
        `;
    }

    $('#tom-pro-databases-container').html(`
        <label for="tom-pro-databases">Base de données: </label>

		<select id="tom-pro-databases" style="width: 200px; "></select>   
    `);

    $('#tom-pro-databases-container').find('#tom-pro-databases').html(content).select2({
        dropdownParent: $('#tom-pro-linking-modal'),
    });
});

$(document).on('change', '#tom-pro-databases', (e) => {
    if ($(e.currentTarget).val() === '') {
        $('#operation-types-container').html('');

        return;
    }

    $('#operation-types-container').html(`
        <div class="form-group">
            <label for="operation-type">Type d'opération: </label>

            <select id="operation-type" style="width: 200px; ">
                <option value="" selected></option>
                <option value="0">Liquidation</option>
                <option value="1">Avance</option>
                <option value="2">Justificatif</option>
                <option value="3">Reversement</option>
            </select>
        </div>
    `);

    $('#operation-types-container').find(`#operation-type`).select2({
        dropdownParent: $('#tom-pro-linking-modal'),
    });
});

$(document).on('change', '#operation-type', (e) => {
    $('#attachements-operation-result').html('');

	switch ($(e.currentTarget).val()) {
		case '0':
			$('#operation-number-type-container').html(`
				<div class="form-group">
					<label for="operation-code-input">Numéro de liquidation: </label>
					<input 
						type="text" 
						id="operation-code-input" 
					/>
				</div>
			`);

			break;
		case '1':
			$('#operation-number-type-container').html(`
				<div class="form-group">
					<label for="operation-code-input">Numéro d'avance: </label>
					<input 
						type="text" 
						id="operation-code-input" 
					/>
				</div>
			`);

			break;
		case '2':
			$('#operation-number-type-container').html(`
				<div class="form-group">
					<label for="operation-code-input">Numéro de justificatif: </label>
					<input 
						type="text" 
						id="operation-code-input" 
					/>
				</div>
			`);

			break;
		case '3':
			$('#operation-number-type-container').html(`
				<div class="form-group">
					<label for="operation-code-input">Numéro de reversement: </label>
					<input 
						type="text" 
						id="operation-code-input" 
					/>
				</div>
			`);

			break;
		default:
			$('#operation-number-type-container').html('');
	}
});

$(document).on('input', '#operation-code-input', async (e) => {
    if ($('#operation-type').val() === '') {
        return;
    }

    const code = $(e.currentTarget).val();

    if (code === '') {
        return;
    }

    switch ($('#operation-type').val()) {
        case '0':
            skeletonLoader.removeClass('display-none');

            $('#attachements-operation-result').html('');

            tmpLiquidations = [];

            try {
                const { data: liquidations } = await axios.post(apiUrl + `api/tom_pro/liquidations`, {
                    code,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                }, {
                    withCredentials: true,
                });

                let content = ``;

                for (let i = 0; i < liquidations.length; i += 1) {
                    const text = liquidations[i].code;
                    const highlightedText = text.replace(new RegExp(escapeRegex(code), 'gi'), '<span style="background-color: yellow; ">$&</span>');

                    content += `
                        <tr>
                            <td>
                                <input 
                                    type="radio" 
                                    name="liquidation" 
                                    value="${liquidations[i].id}" 
                                />
                            </td>
                            <td>${highlightedText}</td>
                            <td>${liquidations[i].designation}</td>
                            <td>${formatCurrency(liquidations[i].montant)}</td>
                            <td>${liquidations[i].typePiece}</td>
                        </tr>
                    `;

                    tmpLiquidations.push({
                        id: liquidations[i].id,
                        link: liquidations[i].lien
                    });
                }

                $('#attachements-operation-result').html(`
                    <thead>
                        <th></th>
                        <th>Code</th>
                        <th style="overflow-wrap: break-word; ">Désignation</th>
                        <th>Montant</th>
                        <th>TypePiece</th>
                    </thead>

                    <tbody>
                        ${content}
                    </tbody>
                `);
            } catch (error) {
                console.log(error.message);
            } finally {
                skeletonLoader.addClass('display-none');
            }

            break;
        case '1':
            skeletonLoader.removeClass('display-none');

            $('#attachements-operation-result').html('');

            tmpAvances = [];

            try {
                const { data: avances } = await axios.post(apiUrl + `api/tom_pro/avances`, {
                    code,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                }, {
                    withCredentials: true,
                });

                let content = ``;

                for (let i = 0; i < avances.length; i += 1) {
                    const text = avances[i].code;
                    const highlightedText = text.replace(new RegExp(escapeRegex(code), 'gi'), '<span style="background-color: yellow; ">$&</span>');

                    content += `
                        <tr>
                            <td>
                                <input 
                                    type="radio" 
                                    name="liquidation" 
                                    value="${avances[i].id}" 
                                />
                            </td>
                            <td>${highlightedText}</td>
                            <td>${avances[i].designation}</td>
                            <td>${formatCurrency(avances[i].montant)}</td>
                            <td>${avances[i].typePiece}</td>
                        </tr>
                    `;

                    tmpAvances.push({
                        id: avances[i].id,
                        link: avances[i].lien
                    });
                }

                $('#attachements-operation-result').html(`
                    <thead>
                        <th></th>
                        <th>Code</th>
                        <th style="overflow-wrap: break-word; ">Désignation</th>
                        <th>Montant</th>
                        <th>TypePiece</th>
                    </thead>

                    <tbody>
                        ${content}
                    </tbody>
                `);
            } catch (error) {
                console.log(error.message);
            } finally {
                skeletonLoader.addClass('display-none');
            }

            break;
        case '2':
            skeletonLoader.removeClass('display-none');

            $('#attachements-operation-result').html('');

            tmpJustificatifs = [];

            try {
                const { data: justificatifs } = await axios.post(apiUrl + `api/tom_pro/justificatifs`, {
                    code,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                }, {
                    withCredentials: true,
                });

                let content = ``;

                for (let i = 0; i < justificatifs.length; i += 1) {
                    const text = justificatifs[i].code;
                    const highlightedText = text.replace(new RegExp(escapeRegex(code), 'gi'), '<span style="background-color: yellow; ">$&</span>');

                    content += `
                        <tr>
                            <td>
                                <input 
                                    type="radio" 
                                    name="justificatif" 
                                    value="${justificatifs[i].id}" 
                                />
                            </td>
                            <td>${highlightedText}</td>
                            <td>${formatCurrency(justificatifs[i].montant)}</td>
                        </tr>
                    `;

                    tmpJustificatifs.push({
                        id: justificatifs[i].id,
                        link: justificatifs[i].commentaire,
                    });
                }

                $('#attachements-operation-result').html(`
                    <thead>
                        <th></th>
                        <th>Code</th>
                        <th>Montant</th>
                    </thead>

                    <tbody>
                        ${content}
                    </tbody>
                `);
            } catch (error) {
                console.log(error.message);
            } finally {
                skeletonLoader.addClass('display-none');
            }

            break;
        case '3':
            skeletonLoader.removeClass('display-none');

            $('#attachements-operation-result').html('');

            tmpReversements = [];

            try {	
                const { data: reversements } = await axios.post(apiUrl + `api/tom_pro/reversements`, {
                    code,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                }, {
                    withCredentials: true,
                });

                let content = ``;

                for (let i = 0; i < reversements.length; i += 1) {
                    const text = reversements[i].code;
                    const highlightedText = text.replace(new RegExp(escapeRegex(code), 'gi'), '<span style="background-color: yellow; ">$&</span>');

                    content += `
                        <tr>
                            <td>
                                <input 
                                    type="radio" 
                                    name="reversement" 
                                    value="${reversements[i].id}" 
                                />
                            </td>
                            <td>${highlightedText}</td>
                            <td>${formatCurrency(reversements[i].montant)}</td>
                        </tr>
                    `;

                    tmpReversements.push({
                        id: reversements[i].id,
                        link: reversements[i].commentaire,
                    });
                }

                $('#attachements-operation-result').html(`
                    <thead>
                        <th></th>
                        <th>Code</th>
                        <th>Montant</th>
                    </thead>

                    <tbody>
                        ${content}
                    </tbody>
                `);
            } catch (error) {
                console.log(error.message);
            } finally {
                skeletonLoader.addClass('display-none');
            }

            break;
        default:
            break;
    }
});

$('#update-tom-pro-attachement-link').on('click', async () => {
    const operationType = $('#operation-type').val();

    if (operationType === '' || $('#operation-code-input').val() === '') {
        return;
    }

    switch (operationType) {
        case '0':
            const liquidationId = $('#attachements-operation-result').find('[name="liquidation"]:checked').val();

            if (liquidationId === undefined) {
                return;
            }

            loader.removeClass('display-none');
            
            const newLiquidationLink = `${webUrl}documents/shared/${documentId}`;

            const liquidation = tmpLiquidations.find((liquidation) => liquidation.id === liquidationId);

            if (liquidation && liquidation.link !== undefined) {
                const res = confirm(`Voulez-écraser ce lien: "${liquidation.link}" par: "${newLiquidationLink}"?`);

                if (!res) {
                    loader.addClass('display-none');

                    return;
                }
            }

            try {
                await axios.patch(apiUrl + `api/tom_pro/liquidations`, {
                    liquidationId,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                    newLink: newLiquidationLink,
                }, {
                    withCredentials: true,
                });

                window.location.reload();
            } catch (error) {
                console.log(error.message);
            } finally {
                loader.addClass('display-none');
            }

            break;

        case '1':
            const avanceId = $('#attachements-operation-result').find('[name="avance"]:checked').val();

            if (avanceId === undefined) {
                return;
            }

            loader.removeClass('display-none');
            
            const newAvanceLink = `${webUrl}documents/shared/${documentId}`;

            const avance = tmpAvances.find((avance) => avance.id === avanceId);

            if (avance && avance.link !== undefined) {
                const res = confirm(`Voulez-écraser ce lien: "${avance.link}" par: "${newAvanceLink}"?`);

                if (!res) {
                    loader.addClass('display-none');

                    return;
                }
            }

            try {
                await axios.patch(apiUrl + `api/tom_pro/avances`, {
                    avanceId,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                    newLink: newAvanceLink,
                }, {
                    withCredentials: true,
                });

                window.location.reload();
            } catch (error) {
                console.log(error.message);
            } finally {
                loader.addClass('display-none');
            }

            break;

        case '2':
            const justificatifId = $('#attachements-operation-result').find('[name="justificatif"]:checked').val();

            if (justificatif === undefined) {
                return;
            }

            loader.removeClass('display-none');

            const newJustificatifLink = `${webUrl}documents/shared/${documentId}`;

            const justificatif = tmpJustificatifs.find((justificatif) => justificatif.id === justificatifId);

            if (justificatif && justificatif.link !== undefined) {
                const res = confirm(`Voulez-écraser ce lien: "${justificatif.link}" par: "${newJustificatifLink}"?`);

                if (!res) {
                    loader.addClass('display-none');

                    return;
                }
            }

            try {
                await axios.patch(apiUrl + `api/tom_pro/justificatifs`, {
                    justificatifId,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                    newLink: newJustificatifLink,
                }, {
                    withCredentials: true,
                });

                window.location.reload();
            } catch (error) {
                console.log(error.message);
            } finally {
                loader.addClass('display-none');
            }

            break;
        
        case '3':
            const reversementId = $('#attachements-operation-result').find('[name="reversement"]:checked').val();

            if (reversementId === undefined) {
                return;
            }

            loader.removeClass('display-none');

            const newReversementLink = `${webUrl}documents/shared/${documentId}`;

            const reversement = tmpJustificatifs.find((reversement) => reversement.id === reversementId);

            if (reversement && reversement.link !== undefined) {
                const res = confirm(`Voulez-écraser ce lien: "${reversement.link}" par: "${newReversementLink}"?`);

                if (!res) {
                    loader.addClass('display-none');

                    return;
                }
            }

            try {
                await axios.patch(apiUrl + `api/tom_pro/reversements`, {
                    reversementId,
                    tomProConnectionId: $('#tom-pro-connections').val(),
                    tomProDatabaseId: $('#tom-pro-databases').val(),
                    newLink: newReversementLink,
                }, {
                    withCredentials: true,
                });

                window.location.reload();
            } catch (error) {
                console.log(error.message);
            } finally {
                loader.addClass('display-none');
            }

            break;
        
        default:
            break;
    }
});
