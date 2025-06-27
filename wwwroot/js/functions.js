// JavaScript functions for SIPOTEK

// Download file function
window.downloadFile = (fileName, contentType, content) => {
    const blob = new Blob([content], { type: contentType });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
};

// Print report function
window.printReport = (title) => {
    const printWindow = window.open('', '_blank');
    const content = document.documentElement.outerHTML;

    printWindow.document.write(`
        <html>
        <head>
            <title>${title}</title>
            <style>
                body { font-family: Arial, sans-serif; margin: 20px; }
                .no-print { display: none !important; }
                .mud-card { border: 1px solid #ddd; margin-bottom: 20px; padding: 15px; }
                .mud-table { width: 100%; border-collapse: collapse; }
                .mud-table th, .mud-table td { border: 1px solid #ddd; padding: 8px; text-align: left; }
                .mud-table th { background-color: #f5f5f5; font-weight: bold; }
                .mud-chip { padding: 2px 8px; border-radius: 4px; font-size: 12px; }
                @media print {
                    .no-print, .mud-button, .mud-icon-button, .mud-fab { display: none !important; }
                    .mud-card { page-break-inside: avoid; }
                }
            </style>
        </head>
        <body>
            ${content}
            <script>
                window.onload = function() {
                    window.print();
                    window.close();
                };
            </script>
        </body>
        </html>
    `);

    printWindow.document.close();
};