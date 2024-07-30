import { getNotFoundHTMLContent } from './utils.js';

$(document).ready(() => {
    $('body').append(getNotFoundHTMLContent());
});
