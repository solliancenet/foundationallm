function scrollToLastEntry(divElement)
{
    if (document.getElementById(divElement)) {
        var elem = document.getElementById(divElement);
        elem.scrollTop = elem.scrollHeight;
        return true;
    }
    return false;
}