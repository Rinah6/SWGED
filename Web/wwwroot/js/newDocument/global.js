let currentPdf = {};

function setCurrentPdf(input) {
    currentPdf = input;
}

let currentRecipientId = '';

function setCurrentRecipientId(input) {
    currentRecipientId = input;
}

const globalDynamicFields = [];
let attachements = [];

function setAttachements(array) {
    attachements = array;
}

let usersSteps = [];

function setUsersSteps(input) {
    usersSteps = input;
}

let userRolesList = [];

function setUserRolesList(input) {
    userRolesList = input;
}

function removeAllField() {
    // if (usersDocumentsList.length > 0) {
        $("[page-id]").remove();
        $("[field-id]").remove();
        $("[recipient-id]").remove();

        setUsersSteps([]);
    // }
}

function fullHide() {
    removeAllField();

    $('.box-setting-info').hide();
    $(`[card-id="dynamic-required-field"]`).hide();
    $(`[card-id="recipient"]`).hide();
    $(`[card-id="field"]`).hide();
    $(`[card-id="type"]`).hide();
    $("#ISign").hide();
    $("#YouSign").hide();
    $("#archive").hide();
}

let currentUserFields = [];

function setCurrentUserFields(input) {
    currentUserFields = input;
}

export {
    currentPdf,
    setCurrentPdf,
    currentRecipientId,
    setCurrentRecipientId,
    globalDynamicFields,
    attachements,
    setAttachements,
    usersSteps,
    setUsersSteps,
    userRolesList,
    setUserRolesList,
    fullHide,
    currentUserFields,
    setCurrentUserFields,
}
