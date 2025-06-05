window.carouselSelectImage = (index) => {
    document.querySelectorAll('.carousel img').forEach((img, i) => {
        if (i === index)
            img.classList.add('opaque');
        else
            img.classList.remove('opaque');
    });
}
