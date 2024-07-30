import { apiUrl } from '../apiConfig.js';

let storage = '';

$(document).ready(async () => {
    const { data } = await axios.get(apiUrl + `api/storage`, {
        withCredentials: true
    });

    storage = data;

    $('#elfinder').elfinder(
        // 1st Arg - options
        {
            // Disable CSS auto loading
            cssAutoLoad: false,
            // Base URL to css/*, js/*
            baseUrl: './',
            // Connector URL
            url: apiUrl + `el-finder/file-system/connector/${storage}`,
            // Callback when a file is double-clicked
            getFileCallback: function (file) {
                // ...
            },
            height: $(window).height() - 20
        },
        // 2nd Arg - before boot up function
        function (fm, extraObj) {
            // `init` event callback function
            fm.bind('init', function () {
                // Optional for Japanese decoder "extras/encoding-japanese.min"
                delete fm.options.rawStringDecoder;
                if (fm.lang === 'ja') {
                    fm.loadScript(
                        [fm.baseUrl + 'js/extras/encoding-japanese.min.js'],
                        function () {
                            if (window.Encoding && Encoding.convert) {
                                fm.options.rawStringDecoder = function (s) {
                                    return Encoding.convert(s, { to: 'UNICODE', type: 'string' });
                                };
                            }
                        },
                        { loadType: 'tag' }
                    );
                }
            });

            // Optional for set document.title dynamically.
            const title = document.title;

            fm.bind('open', function () {
                var path = '',
                    cwd = fm.cwd();
                if (cwd) {
                    path = fm.path(cwd.hash) || null;
                }
                document.title = path ? path + ':' + title : title;
            }).bind('destroy', function () {
                document.title = title;
            });
        }
    );
});
