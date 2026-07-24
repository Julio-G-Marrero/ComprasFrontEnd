(function () {
    if (window.__columnResizeInit) {
        return;
    }
    window.__columnResizeInit = true;

    var activeTh = null;
    var startX = 0;
    var startWidth = 0;

    function lockColumnWidths(table) {
        if (table.classList.contains('col-resize-fixed')) {
            return;
        }
        var headerCells = table.querySelectorAll('thead th');
        headerCells.forEach(function (th) {
            var width = th.offsetWidth;
            th.style.width = width + 'px';
            th.style.minWidth = width + 'px';
            th.style.maxWidth = width + 'px';
        });
        table.classList.add('col-resize-fixed');
    }

    document.addEventListener('mousedown', function (e) {
        var handle = e.target.closest('.col-resize-handle');
        if (!handle) {
            return;
        }
        var th = handle.closest('th');
        var table = handle.closest('table');
        if (!th || !table) {
            return;
        }

        lockColumnWidths(table);

        activeTh = th;
        startX = e.clientX;
        startWidth = th.offsetWidth;
        handle.classList.add('resizing');
        document.body.style.cursor = 'col-resize';
        document.body.style.userSelect = 'none';
        e.preventDefault();
    });

    document.addEventListener('mousemove', function (e) {
        if (!activeTh) {
            return;
        }
        var newWidth = Math.max(40, startWidth + (e.clientX - startX));
        activeTh.style.width = newWidth + 'px';
        activeTh.style.minWidth = newWidth + 'px';
        activeTh.style.maxWidth = newWidth + 'px';
    });

    document.addEventListener('mouseup', function () {
        if (!activeTh) {
            return;
        }
        var handle = activeTh.querySelector('.col-resize-handle');
        if (handle) {
            handle.classList.remove('resizing');
        }
        activeTh = null;
        document.body.style.cursor = '';
        document.body.style.userSelect = '';
    });
})();
