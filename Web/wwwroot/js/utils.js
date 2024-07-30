const colorsList = [
    "#bc5090", "#58508d", "#003f5c", "#c7522a", "#057dcd",
    "#007f4e", "#008585", "#bf5b04", "#007bff", "#6610f2",
    "#6f42c1", "#e83e8c", "#dc3545", "#fd7e14", "#ffc107",
    "#28a745", "#20c997", "#6c757d", "#17a2b8"
];

function formatDate(date) {
    if (!dayjs(date).isValid()) {
        return '';
    }

    return dayjs(date).format('DD/MM/YYYY HH:mm:ss');
}

function convertToPlain(html) {
    const tempDivElement = document.createElement('div');
    tempDivElement.innerHTML = html;

    return tempDivElement.textContent || tempDivElement.innerText || '';
}

function dateDiff(date1, date2) {
	const diff = {};

	let tmp = date1 - date2;

	tmp = Math.floor(tmp / 1000);
	diff.sec = tmp % 60;

	tmp = Math.floor((tmp - diff.sec) / 60);
	diff.min = tmp % 60;

	tmp = Math.floor((tmp - diff.min) / 60);
	diff.hour = tmp % 24;

	tmp = Math.floor((tmp - diff.hour) / 24);
	diff.day = tmp % 30;

	tmp = Math.floor((tmp - diff.day) / 30);
	diff.month = tmp % 12;


	tmp = Math.floor((tmp - diff.month) / 12);
	diff.year = tmp;

	return diff;
}

function pastPeriod(date) {
    date = dateDiff(new Date(), new Date(Date.parse(date)));

    let s = 'Il y a ';

    if (date.year == 0) {
        if (date.month == 0) {
            if (date.day == 0) {
                if (date.hour == 0) {
                    if (date.min == 0) return s + date.sec + ' s';
                    else {
                        if (date.min < 6) return s + 'environ ' + date.min + ' mn';
                        return s + date.min + ' mn';
                    }
                }
                return s + date.hour + ' h';
            } else {
                if (date.day == 1) return s + date.day + ' jour';
                return s + date.day + ' jours';
            }
        }

        return s + date.month + ' mois';
    } else {
        if (date.year == 1) return s + date.year + ' an';
        return s + date.year + ' ans';
    }
}

function verifyMail(email) {
	const regex = /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;

	return regex.test(String(email).toLowerCase());
}

function generateRandomColor() {
	// return colorsList[Math.floor(Math.random() * colorsList.length)];

    const letters = '0123456789abcdef';
    let color = '#';

    for (let i = 0; i < 6; i += 1) {
        color += letters[Math.floor(Math.random() * 16)];
    }

    return color;
}

function getNotFoundHTMLContent() {
    return `
        <h1 style="font-size: 128px; position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%);">
            404
        </h1>
    `;
}

function escapeRegex(string) {
    return string.replace(/[/\-\\^$*+?.()|[\]{}]/g, '\\$&');
}

function formatCurrency(amount) {
    let number = amount.toLocaleString('fr-FR', {
        style: 'decimal',
        minimumFractionDigits: 2,
        currencySign: "accounting",
    });

    number += '';

    const sep = ' ';
    const reg = /(\d+)(\d{3})/;

    while (reg.test(number)) {
        number = number.replace(reg, '$1' + sep + '$2');
    }

    return number.toString().replace('.', ',');
}

function getChanges(originalArray, changes) {
    const additions = [];
    const deletions = [];

    changes.forEach((newItem) => {
        if (!originalArray.some((oldItem) => oldItem.id === newItem.id)) {
            additions.push(newItem);
        }
    });

    originalArray.forEach((oldItem) => {
        if (!changes.some((newItem) => newItem.id === oldItem.id)) {
            deletions.push(oldItem);
        }
    });

    return {
        additions,
        deletions,
    };
}

export {
   formatDate,
   convertToPlain,
   pastPeriod,
   verifyMail,
   generateRandomColor,
   getNotFoundHTMLContent,
   escapeRegex,
   formatCurrency,
   getChanges,
};
