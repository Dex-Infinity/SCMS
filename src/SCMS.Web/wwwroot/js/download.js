// Saves bytes from .NET as a file download. Imported on demand from the analytics page.
export function downloadFile(fileName, contentType, bytes) {
    const blob = new Blob([bytes], { type: contentType });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    link.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}
