// wwwroot/js/receipt-print.js
// Prints ONLY the receipt element inside a hidden iframe, sized for a thermal roll.
// This avoids the blank / A4 pages caused by printing the whole app layout.
window.agriPrintReceipt = function(elementId, widthMm) {
    var el = document.getElementById(elementId);
    if (!el) { return; }

    var w = widthMm || 100;

    var old = document.getElementById('agri-print-frame');
    if (old) { old.remove(); }

    var frame = document.createElement('iframe');
    frame.id = 'agri-print-frame';
    frame.setAttribute('aria-hidden', 'true');
    frame.style.cssText = 'position:fixed;left:-9999px;top:0;border:0;width:' + w + 'mm;height:200mm;';
    document.body.appendChild(frame);

    // copy the page styles so the receipt looks the same (including Blazor scoped css)
    var head = '';
    document.querySelectorAll('link[rel="stylesheet"]').forEach(function(l) {
        head += '<link rel="stylesheet" href="' + l.href + '">';
    });
    document.querySelectorAll('style').forEach(function(s) {
        head += s.outerHTML;
    });

    // print-only overrides (thermal roll, no margins, black text)
    head += '<style>' +
        '@page { size: ' + w + 'mm auto; margin: 0; }' +
        'html, body { margin:0 !important; padding:0 !important; background:#fff !important;' +
        '  width:' + w + 'mm !important; height:auto !important; overflow:visible !important; }' +
        '.thermal-receipt-card { width:' + w + 'mm !important; max-width:none !important; margin:0 !important;' +
        '  padding:2mm 4mm 4mm !important; border:0 !important; border-radius:0 !important;' +
        '  box-shadow:none !important; background:#fff !important; box-sizing:border-box !important; }' +
        '.thermal-receipt-card, .thermal-receipt-card * { color:#000 !important; background:transparent !important; }' +
        '.receipt-doc-tag { border:1px solid #000 !important; }' +
        '.receipt-dash { border-top:1px dashed #000 !important; }' +
        '</style>';

    var doc = frame.contentWindow.document;
    doc.open();
    doc.write('<!DOCTYPE html><html><head><meta charset="utf-8">' + head +
        '</head><body>' + el.outerHTML + '</body></html>');
    doc.close();

    var printed = false;
    function go() {
        if (printed) { return; }
        printed = true;
        setTimeout(function() {
            frame.contentWindow.focus();
            frame.contentWindow.print();
        }, 100);
    }

    // wait for the stylesheets before printing
    var links = doc.querySelectorAll('link[rel="stylesheet"]');
    var pending = links.length;
    if (pending === 0) {
        go();
    } else {
        links.forEach(function(l) {
            var done = function() { pending--; if (pending <= 0) { go(); } };
            l.addEventListener('load', done);
            l.addEventListener('error', done);
        });
        setTimeout(go, 1500); // safety fallback
    }
};