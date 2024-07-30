const dragOption = {
    containment: '#pdf-viewer',
    cursor: 'move',
    disabled: false,
    stop: function () {
        var containerPos = $('#pdf-viewer').offset();
        var inputPos = $(this).offset();
        relativeOffset = {
            top: inputPos.top - containerPos.top,
            left: inputPos.left - containerPos.left
        };
        var w = $('#pdf-viewer').width();
        var h = $('#pdf-viewer').height();
        $("#pagination").text('[x: ' + relativeOffset.left + '| y: ' + relativeOffset.top + ']' +
            ' [w:' + w + ' | h:' + h + ']' +
            ' [ sx: ' + $(this).width() + ' | sy: ' + $(this).height() + ']');
    },
    handle: '.ribbon'
}

$(`[data-action="addSignBox"]`).on("click", (e) => {
    if ($("#input-file").val() != "" && $("#input-file").val() != null) {
        if ($("#addSignature").hasClass("fa-plus")) {
            $("#addSignature").removeClass("fa-plus").addClass("fa-minus");

            if ($("#signatureListBox").hasClass("hidden"))
                $("#signatureListBox").removeClass("hidden");

            //var canvasOffset = $('#pdf-viewer').offset();
            //$("#signature").css({ "top": (canvasOffset.top + 125) + "px", "left": (canvasOffset.left + 25) + "px" });

            $("#plussignature").removeClass("hidden");

            var listSignature = $("#actionmenu").find(`[data-type="signature"]`);
            listSignature.each(function () {
                if ($(this).hasClass("parapheList"))
                    $(this).removeClass("hidden");
            });
        } else {
            $("#addSignature").removeClass("fa-minus").addClass("fa-plus");

            $("#signatureListBox").addClass("hidden");

            $("#plussignature").addClass("hidden");

            var listSignature = $("#actionmenu").find(`[data-type="signature"]`);
            listSignature.each(function () {
                if ($(this).hasClass("parapheList"))
                    $(this).addClass("hidden");
            });
        }
    } else {
        alert("Veuillez uploader d'abord un document");
    }
});

let countSignature = 0;

$(`[data-action="addListSignature"]`).on("click", (e) => {
    var Page = $("#signpage").val();

    if (Page == "" || Page == null)
        return;

    var code =
        `<li class="nav-item parapheList" id="signature${countSignature}" data-type="signature" data-value="${Page}">
            <div class="nav-link float-right">
                <span id="signature' + countSignature + '">Page : ${Page}</span>
                <i class="fa fa-times text-danger" onclick="return removeListSignature('${countSignature}')"></i>
            </div>
        </li>`;
    $(code).insertBefore($("#showListSignature"));

    code = `
        <div class="boxSign" id="signatureBox${countSignature}" data-type="signature" data-id="${countSignature}">
	        <div class="ribbon-wrapper">
		        <div class="ribbon bg-primary">
			        sign
		        </div>
	        </div>
        </div>
    `;
    $(`#signatureListBox`).append(code);
    let i = countSignature;

    $(`#signatureBox${i}`).hover((e, x) => {
        $(`#signature${i}`).addClass("bg-primary");
        if (!$(`#signature${i}`).hasClass("bg-primary")) {
            $(`#signature${i}`).addClass("bg-primary");
            //$(`#signature${countSignature} .span`).css("color", "white");
        }
    }, (e) => {
        if ($(`#signature${i}`).hasClass("bg-primary"))
            $(`#signature${i}`).removeClass("bg-primary");
    });

    $(`#signatureBox${i}`).mousemove((e) => {
        divPos = {
            left: e.pageX - $(e.target).offset().left,
            top: e.pageY - $(e.target).offset().top,
        };

        console.log(`W = `, $(this).width());
        console.log(`H = `, $(this).height());

        if (divPos.left > $(this).width() - 11 && divPos.top > $(this).height() - 11) dragOption.disabled = true;
        else dragOption.disabled = false;

        $(`#signatureBox${i}`).draggable(dragOption);
        //$("#signatureBox${countSignature}").draggableTouch(dragOption);
    });
    

    $(`#signatureBox${i}`).hover();
    $(`#signatureBox${i}`).mousemove();

    var canvasOffset = $('#pdf-viewer').offset();
    $(`#signatureBox${i}`).css({ "top": (canvasOffset.top + 125) + "px", "left": (canvasOffset.left + 25) + "px" });

    $("#signpage").val($("#signpage").attr("min"));

    countSignature++;
});

function removeListSignature(id) {
    $(`#signature${id}`).remove();
    $(`#signatureBox${id}`).remove();
}

$(`[data-action="addParapheBox"]`).on("click", (e) => {
    if ($("#input-file").val() == "" || $("#input-file").val() == null) {
        alert("Veuillez uploader d'abord un document");
        return;
    }

    if ($("#addParaphe").hasClass("fa-plus")) {
        $("#addParaphe").removeClass("fa-plus").addClass("fa-minus");

        $("#paraphe").removeClass("hidden");

        var canvas = $('#pdf-viewer').offset();
        $("#paraphe").css({ "top": (canvas.top + 125) + "px", "left": (canvas.left + 25) + "px" });

        $("#plusparaphe").removeClass("hidden");

        var listParaphe = $("#actionmenu").find(`[data-type="paraphe"]`);
        var countParaphe = listParaphe.length;
        listParaphe.each(function () {
            if ($(this).hasClass("parapheList")) $(this).removeClass("hidden");
        });

    } else {
        $("#addParaphe").removeClass("fa-minus");
        $("#addParaphe").addClass("fa-plus");

        $("#paraphe").addClass("hidden");

        $("#plusparaphe").addClass("hidden");

        var listParaphe = $("#actionmenu").find(`[data-type="paraphe"]`);
        var countParaphe = listParaphe.length;
        listParaphe.each(function () {
            if ($(this).hasClass("parapheList")) $(this).addClass("hidden");
        });
    }
});

let countParaphe = 0;

$(`[data-action="addListParaphe"]`).on("click", (e) => {
    var firstPage = $("#firstPage").val();
    var LastPage = $("#LastPage").val();
    if (firstPage == "" || firstPage == null || LastPage == "" || LastPage == null)
        return;

    var code = `
        <li class="nav-item parapheList" id="parapheCount${countParaphe}" data-type="paraphe" data-value="${firstPage}|${LastPage}">
            <div class="nav-link float-right">
                <span id="paraphe${countParaphe}">Page ${firstPage} - ${LastPage}</span>
                <i class="fa fa-times text-danger" onclick="return removeListParaphe('parapheCount${countParaphe}')"></i>
            </div>
        </li>
    `;
    $(code).insertBefore($("#showListParaphe"));
    countParaphe++;

    $("#firstPage").val($("#firstPage").attr("min"));
    
});

function removeListParaphe(id) {
    $("#" + id).remove();
}

$("#paraphe").mousemove(function (e) {
    divPos = {
        left: e.pageX - $(this).offset().left,
        top: e.pageY - $(this).offset().top,
    };
    if (divPos.left > $(this).width() - 11 && divPos.top > $(this).height() - 11) dragOption.disabled = true;
    else dragOption.disabled = false;

    $("#paraphe").draggable(dragOption);
});
