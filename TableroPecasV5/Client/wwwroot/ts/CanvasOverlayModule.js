//Define a custom overlay class that inherts from the CustomOverlay class.
CanvasOverlay.prototype = new Microsoft.Maps.CustomOverlay();

//Define the Canvas Overlay constructor which takes in a callback function which draws on the canvas.
//The callback function will recieve a reference to the canvas element.
function CanvasOverlay(drawCallback) {
    //Create a canvas for rendering.
    this.canvas = document.createElement('canvas');
    this.canvas.style.position = 'absolute';
    this.canvas.style.left = '0px';
    this.canvas.style.top = '0px';

    //Variables to track view change events.
    this.viewChangeEvent;
    this.viewChangeEndEvent;

    //Simple function for updating the CSS position and dimensions of the canvas.
    this._updatePosition = function (x, y, w, h) {
        //Update CSS position.
        this.canvas.style.left = x + 'px';
        this.canvas.style.top = y + 'px';

        //Update CSS dimensions.
        this.canvas.style.width = w + 'px';
        this.canvas.style.height = h + 'px';
    };

    //This function is triggered when the canvas needs to be rerendered.
    this._redraw = function () {
        //Clear canvas by updating dimensions. This also ensures canvas stays the same size as the map.
        this.canvas.width = this.getMap().getWidth();
        this.canvas.height = this.getMap().getHeight();

        //Call the defined drawing callback function.
        drawCallback(this.canvas);
    };
}

//Implement the onAdd method to set up DOM elements, and use setHtmlElement to bind it with the overlay.
CanvasOverlay.prototype.onAdd = function () {
    //Add the canvas to the overlay.            
    this.setHtmlElement(this.canvas);
};

//Implement the onLoad method to perform custom operations after adding the overlay to the map.
CanvasOverlay.prototype.onLoad = function () {
    var self = this;
    var map = self.getMap();

    //Get the current map view information.
    var zoomStart = map.getZoom();
    var centerStart = map.getCenter();

    //Redraw the canvas.
    self._redraw();

    self.viewChangeEvent = Microsoft.Maps.Events.addHandler(map, 'viewchange', function (e) {
        if (map.getMapTypeId() == Microsoft.Maps.MapTypeId.streetside) {
            //Don't show the canvas if the map is in Streetside mode.
            self.canvas.style.display = 'none';
        } else {
            //Re-drawing the canvas as it moves would be too slow. Instead, scale and translate canvas element.
            var zoomCurrent = map.getZoom();
            var centerCurrent = map.getCenter();

            //Calculate map scale based on zoom level difference.
            var scale = Math.pow(2, zoomCurrent - zoomStart);

            //Calculate the scaled dimensions of the canvas.
            var newWidth = map.getWidth() * scale;
            var newHeight = map.getHeight() * scale;

            //Calculate offset of canvas based on zoom and center offsets.
            var pixelPoints = map.tryLocationToPixel([centerStart, centerCurrent], Microsoft.Maps.PixelReference.control);
            var centerOffsetX = pixelPoints[1].x - pixelPoints[0].x;
            var centerOffsetY = pixelPoints[1].y - pixelPoints[0].y;
            var x = (-(newWidth - map.getWidth()) / 2) - centerOffsetX;
            var y = (-(newHeight - map.getHeight()) / 2) - centerOffsetY;

            //Update the canvas CSS position and dimensions.
            self._updatePosition(x, y, newWidth, newHeight);
        }
    });

    self.viewChangeEndEvent = Microsoft.Maps.Events.addHandler(map, 'viewchangeend', function (e) {
        //Only render the canvas if it isn't in streetside mode.
        if (map.getMapTypeId() != Microsoft.Maps.MapTypeId.streetside) {
            self.canvas.style.display = '';

            //Reset CSS position and dimensions of canvas.
            self._updatePosition(0, 0, map.getWidth(), map.getHeight());

            //Redraw the canvas.
            self._redraw();

            //Get the current map view information.
            zoomStart = map.getZoom();
            centerStart = map.getCenter();
        }
    });
};

CanvasOverlay.prototype.onRemove = function () {
    //Remove all event handlers from the map.
    Microsoft.Maps.Events.removeHandler(this.viewChangeEvent);
    Microsoft.Maps.Events.removeHandler(this.viewChangeEndEvent);
};

//Call the module loaded function.
Microsoft.Maps.moduleLoaded('CanvasOverlayModule');