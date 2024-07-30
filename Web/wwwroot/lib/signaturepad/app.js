$(document).ready(() => {
    $("#signature_tab").hide();
    $("#signature_tab").css("opacity", 1.0);
});

var wrapper = document.getElementById("signature-pad");
var clearButton = document.getElementById("signClear");
var clearButton1 = document.getElementById("parapheClear");

//var clearButton = wrapper.querySelector("[data-action=clear]");
//var changeColorButton = wrapper.querySelector("[data-action=change-color]");
//var undoButton = wrapper.querySelector("[data-action=undo]");
//var savePNGButton = wrapper.querySelector("[data-action=save-png]");
//var saveJPGButton = wrapper.querySelector("[data-action=save-jpg]");
//var saveSVGButton = wrapper.querySelector("[data-action=save-svg]");

var signcanvas = document.getElementById("signaturePad");
var signaturePad = new SignaturePad(signcanvas, {
    backgroundColor: 'rgba(255, 255, 255, 0.0)'
});


var paraphecanvas = document.getElementById("paraphePad");
var paraphePad = new SignaturePad(paraphecanvas, {
    backgroundColor: 'rgba(255, 255, 255, 0.0)'
});

function signValidation1() {
    var files = $("#input-file").get(0).files;
    if (files.length == 0) {
        _alert("Upload document", "Auncun Document s�lectionn�.");
        return;
    }
    if (signaturePad.isEmpty()) {
        alert("Vous avez oublier de mettre la signature");
        return;
    }

    var datasign = signaturePad.toDataURL();

    var formData = new FormData();

    formData.append("datasign", datasign);
    formData.append("upload", files[0]);

    formData.append("stockage.Source", $("#selectSource").val()[0]);
    formData.append("stockage.Destinataire", $("#selectDest").val()[0]);
    formData.append("stockage.Type", $("#selectType").val()[0]);

    var pdfContainer = $('#pdf-viewer').offset();
    var posSign = { top: null, left: null };

    formData.append("pdfDetail.pdfInfo", parseFloat($("#pdf-viewer").width()).toString().replace('.', ',') +
        "|" + parseFloat($("#pdf-viewer").height()).toString().replace('.', ','));

    //Signature
    if (!$("#signatureListBox").hasClass("hidden")) {
        var listsignaturebox = $("#signatureListBox").find(`[data-type="signature"]`);
        var list = [];
        listsignaturebox.each(function () {
            var offsetSign = $(this).offset();
            posSign = {
                top: offsetSign.top - pdf-container.top,
                left: offsetSign.left - pdf-container.left
            };

            id = $(this).attr("data-id");

            list.push($(`#signature${id}`).data("value") + "!" + parseFloat(posSign.top).toString().replace('.', ',') +
                "!" + parseFloat(posSign.left).toString().replace('.', ',') +
                "!" + parseFloat($(this).width()).toString().replace('.', ',') +
                "!" + parseFloat($(this).height()).toString().replace('.', ','));
        });

        formData.append("pdfDetail._SignInfo", JSON.stringify(list));
    }

    formData.append("url[]", $("#selectSource").val());
    formData.append("url[]", $("#selectType").val());
    formData.append("url[]", $("#selectDest").val());

    formData.append("Username", User.Username);
    formData.append("Password", User.Password);

    $.ajax({
        type: 'POST',
        url: "/Main/ISign",
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        xhrFields: { withCredentials: true },
        success: function (result) {
            var Datas = JSON.parse(result);
            if (Datas.type == "success") {
                setTimeout(function () {
                    window.location = "Main/MyDocument";
                    window.location = window.location.origin + "/Main/MyDocument";
                }, 2000);
                alert("Document envoye");
            } else if (Datas.type == "format") {
                alert("Veuillez verifier le document svp!");
                if ($("#sendMail").hasClass("disabled")) $("#sendMail").removeClass("disabled");
                input.click();
            } else if (Datas.type == "required") {
                alert("Veuillez uploader un document");
                if ($("#sendMail").hasClass("disabled")) $("#sendMail").removeClass("disabled");
                input.click();
            } else if (Datas.type == "login") {
                alert("Veuillez vous reconnecter");
                window.location = window.location.origin;
            } else {
                alert("Mail non envoye / Verifier votre internet");
                //if ($("#sendMail").hasClass("disabled")) $("#sendMail").removeClass("disabled");
            }

        },
        error: function (e) {
            if ($(this).hasClass("disabled")) $(this).removeClass("disabled");
            alert("Document non Envoye, svp verifier votre internet ou votre mail");
            if ($(this).hasClass("disabled")) $(this).removeClass("disabled");
        }
    });
}

function signValidation() {
    if (signaturePad.isEmpty()) {
        alert("Vous avez oublier de mettre la signature");
        return;
    }
    var datasign = signaturePad.toDataURL();
    var dataparaphe = paraphePad.toDataURL();

    let formData = new FormData();

    id = $("#btnSign").data("target");

    formData.append("Username", User.Username);
    formData.append("Password", User.Password);
    formData.append("datasign", datasign);
    formData.append("dataparaphe", dataparaphe);
    formData.append("signataire", $("#signataire").val());
    formData.append("idDoc", id);

    $.ajax({
        type: 'POST',
        url: "../Main/ClientSign",
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        async: true,
        xhrFields: { withCredentials: true },
        success: function (result) {
            if (result == "error") {
                window.location = "../";
            } else {
                window.location.reload();
            }
            /*var Datas = JSON.parse(result);

            loading(false);*/
        },

        Error: function (x, e) {
            alert("Some error");
            //loading(false);
        }
    });
}

function resizeCanvas() {
    var ratio = Math.max(window.devicePixelRatio || 1, 1);

    signcanvas.width = signcanvas.offsetWidth * ratio;
    signcanvas.height = signcanvas.offsetHeight * ratio;
    signcanvas.getContext("2d").scale(ratio, ratio);

    signaturePad.clear();

    paraphecanvas.width = paraphecanvas.offsetWidth * ratio;
    paraphecanvas.height = paraphecanvas.offsetHeight * ratio;
    paraphecanvas.getContext("2d").scale(ratio, ratio);

    paraphePad.clear();
}

window.onresize = resizeCanvas;
resizeCanvas();

function download(dataURL, filename) {
    if (navigator.userAgent.indexOf("Safari") > -1 && navigator.userAgent.indexOf("Chrome") === -1) {
        window.open(dataURL);
    } else {
        var blob = dataURLToBlob(dataURL);
        var url = window.URL.createObjectURL(blob);

        var a = document.createElement("a");
        a.style = "display: none";
        a.href = url;
        a.download = filename;

        document.body.appendChild(a);
        a.click();

        window.URL.revokeObjectURL(url);
    }
}

function dataURLToBlob(dataURL) {
    var parts = dataURL.split(';base64,');
    var contentType = parts[0].split(":")[1];
    var raw = window.atob(parts[1]);
    var rawLength = raw.length;
    var uInt8Array = new Uint8Array(rawLength);

    for (var i = 0; i < rawLength; ++i) {
        uInt8Array[i] = raw.charCodeAt(i);
    }

    return new Blob([uInt8Array], { type: contentType });
}

clearButton.addEventListener("click", function (event) {
    signaturePad.clear();
});

clearButton1.addEventListener("click", function (event) {
    paraphePad.clear();
});

/*undoButton.addEventListener("click", function (event) {
  var data = signaturePad.toData();

  if (data) {
    data.pop(); // remove the last dot or line
    signaturePad.fromData(data);
  }
});*/

/*changeColorButton.addEventListener("click", function (event) {
  var r = Math.round(Math.random() * 255);
  var g = Math.round(Math.random() * 255);
  var b = Math.round(Math.random() * 255);
  var color = "rgb(" + r + "," + g + "," + b +")";

  signaturePad.penColor = color;
});*/

/*savePNGButton.addEventListener("click", function (event) {
  if (signaturePad.isEmpty()) {
    alert("Please provide a signature first.");
  } else {
    var dataURL = signaturePad.toDataURL();
    download(dataURL, "signature.png");
  }
});*/

/*saveJPGButton.addEventListener("click", function (event) {
  if (signaturePad.isEmpty()) {
    alert("Please provide a signature first.");
  } else {
    var dataURL = signaturePad.toDataURL("image/jpeg");
    download(dataURL, "signature.jpg");
  }
});*/

/*saveSVGButton.addEventListener("click", function (event) {
  if (signaturePad.isEmpty()) {
    alert("Please provide a signature first.");
  } else {
    var dataURL = signaturePad.toDataURL('image/svg+xml');
    download(dataURL, "signature.svg");
  }
});*/
