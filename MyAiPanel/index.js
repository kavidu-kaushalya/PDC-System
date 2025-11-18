function loadJSX(fileName) {
    var csInterface = new CSInterface();
    var extensionRoot = csInterface.getSystemPath(SystemPath.EXTENSION) + "/";

    csInterface.evalScript('$.evalFile("' + extensionRoot + fileName + '")');
}

window.onload = function() {
    var csInterface = new CSInterface();
    loadJSX("hostscript.jsx");

    document.getElementById("createRect").onclick = function() {
        csInterface.evalScript('createRectangle()');
    };
};
