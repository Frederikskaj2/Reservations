window.fragmentNavigation = {
    scrollIntoView: function (elementId) {
        const element = document.getElementById(elementId);
        if (element) {
            element.scrollIntoView();
            window.location.hash = elementId;
        }
    }
}
