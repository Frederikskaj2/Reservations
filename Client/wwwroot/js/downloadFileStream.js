window.downloadFileFromStream = async (fileName, mediaType, contentStreamReference) => {
    const arrayBuffer = await contentStreamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], {type: mediaType});
    console.log("Blob: " + blob.size);
    const url = URL.createObjectURL(blob);
    console.log("URL: " + url.length);
    const anchorElement = document.createElement('a');
    anchorElement.href = url;
    anchorElement.download = fileName ?? '';
    anchorElement.click();
    anchorElement.remove();
    URL.revokeObjectURL(url);
}
