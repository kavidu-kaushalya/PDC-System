// hostscript.jsx
function createRectangle() {
    if (app.documents.length === 0) {
        var doc = app.documents.add();
    } else {
        var doc = app.activeDocument;
    }
    
    var rect = doc.pathItems.rectangle(-100, 100, 200, 100);
    rect.filled = true;
    rect.fillColor = app.getActiveDocument().swatches[0].color;
    
    var redColor = new RGBColor();
    redColor.red = 255;
    redColor.green = 0;
    redColor.blue = 0;
    rect.fillColor = redColor;
}
