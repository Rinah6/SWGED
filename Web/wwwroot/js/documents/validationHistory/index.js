import { apiUrl, webUrl } from '../../apiConfig.js';
import { formatDate } from '../../utils.js';

$(document).ready(async () => {
    const tmp = window.location.href.split(webUrl + 'documents/');

    const documentId = tmp[1].split('/validation_history')[0]; 

    const { data } = await axios.get(apiUrl + `api/documents/${documentId}/validation_history`, {
        withCredentials: true,
    });

    let content = ``;

    for (let i = 0; i < data.length; i += 1) {
        content += `
            <tr>
                <td>${data[i].username}</td>
                <td>${formatDate(data[i].creationDate)}</td>
                <td>${data[i].comment}</td>
            </tr>
        `;
    }
    
    $(`#column_table`).after(content);
});
