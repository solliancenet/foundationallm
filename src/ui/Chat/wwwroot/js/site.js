function scrollToLastEntry(divElement)
{
    if (document.getElementById(divElement)) {
        var elem = document.getElementById(divElement);
        elem.scrollTop = elem.scrollHeight;
        return true;
    }
    return false;
}

function updateFavIcon(newIconUrl) {
    document.querySelector("link[rel='icon']").href = newIconUrl;
}

function updateCSSVariables(primaryColor, secondaryColor, accentColor, backgroundColor, primaryText, secondaryText) {
    document.documentElement.style.setProperty('--primary-color', primaryColor);
    document.documentElement.style.setProperty('--secondary-color', secondaryColor);
    document.documentElement.style.setProperty('--accent-color', accentColor);
    document.documentElement.style.setProperty('--primary-bg', backgroundColor);
    document.documentElement.style.setProperty('--primary-text', primaryText);
    document.documentElement.style.setProperty('--secondary-text', secondaryText);
}