
/// <reference path="types/MicrosoftMaps/Microsoft.Maps.All.d.ts" />

let bingMap: BingMap[] = [null, null, null, null, null, null, null, null, null, null];
let EventoViewChanged: boolean[] = [false, false, false, false, false, false, false, false, false, false];
let EventoMouseUp: boolean[] = [false, false, false, false, false, false, false, false, false, false];


class BingMap {

    map: Microsoft.Maps.Map;



    constructor(Direccion: string) {

 //       this.map = new Microsoft.Maps.Map('#ContenedorMapa', {

          this.map = new Microsoft.Maps.Map(Direccion, {
            center: new Microsoft.Maps.Location(-40,-72),

            mapTypeId: Microsoft.Maps.MapTypeId.road,

            zoom: 9

        });

    }

}

function UbicarPosicionLibre() {
    var i: number;
    for (i = 0; i < bingMap.length; i++) {
        if (bingMap[i] == null) {
            return i;
        }
    }
    return bingMap.length - 1;
}

function LiberarPushpins(Posicion: number) : string {
    if (Posicion >= 0 && Posicion < bingMap.length) {
        if (bingMap[Posicion] != null) {
            if (bingMap[Posicion].map != null) {
                if (bingMap[Posicion].map.entities != null) {
                    bingMap[Posicion].map.entities.clear();
                }
            }
        }
    }
    return "";
}

function LiberarMap(Posicion: number): string {
    try {
        if (bingMap != null && Posicion >= 0 && Posicion < bingMap.length && bingMap[Posicion] != null) {
            var i: number;
            for (i = 0; i < bingMap[Posicion].map.layers.length; i++) {
                var Capa: Microsoft.Maps.Layer = bingMap[Posicion].map.layers[i];
                Capa.clear();
            }
            bingMap[Posicion].map.entities.clear();
            bingMap[Posicion].map.layers.clear();
            bingMap[Posicion].map.dispose();
            bingMap[Posicion] = null;
            return "";
        }
        else {
            return "";
        }
    }
    catch (exc) {
        return exc.message;
    }
}

//function loadMap(Direccion: string, LatCentro: number, LngCentro: number, NivelZoom: number): void {

//    try {
//        bingMap = new BingMap(Direccion);
//        if (bingMap != null) {
//            bingMap.map.setView({
//                center: new Microsoft.Maps.Location(LatCentro, LngCentro),
//                zoom: NivelZoom
//            });
//        }
//    }
//    catch (exc) {
//        //        alert(exc.message);
//        bingMap = null;
//    }
//}

function loadMapRetPos(Posicion: number, Direccion: string, LatCentro: number, LngCentro: number, NivelZoom: number,
    EventoViewChange: boolean, EventoMouseUp: boolean): string {

    if (bingMap == null) {
        return 'bingMap es nulo';
    }

    try {
        let Eventos: boolean = false;
        if (Posicion < 0) {
            Posicion = UbicarPosicionLibre();
            Eventos = true;
            bingMap[Posicion] = new BingMap(Direccion);
        }
        if (Posicion < 0) {
            return "-1";
        }
        //        bingMap[Posicion] = new BingMap(Direccion);
        if (bingMap[Posicion] != null) {
            bingMap[Posicion].map.setView({
                center: new Microsoft.Maps.Location(LatCentro, LngCentro),
                zoom: NivelZoom
            });
            //           Microsoft.Maps.registerModule('RutinasJS', 'RutinasJS.js');
            //Microsoft.Maps.Events.addHandler(bingMap[Posicion].map, 'viewchangeend', function () {
            //    window['FuncionesJS'].RefrescarMapa(Direccion)
            //       });
            if (Eventos) {
                if (EventoViewChange) {
                    Microsoft.Maps.Events.addHandler(bingMap[Posicion].map, 'viewchangeend', function () {
                        window['FuncionesJS'].ReposicionarMapa(Posicion, bingMap[Posicion].map.getZoom());
                    });
                }
                if (EventoMouseUp) {
                    Microsoft.Maps.Events.addHandler(bingMap[Posicion].map, 'click', function (e: Microsoft.Maps.IMouseEventArgs) {
                        window['FuncionesJS'].ClickEnMapa(e.location.latitude.toString(), e.location.longitude.toString())
                    });
                }
            }
        }
    }
    catch (exc) {
        alert(exc.message);
        if (Posicion > 0) {
            bingMap[Posicion] = null;
        }
    }
    return Posicion.toString();
}

function ObtenerBordes(Posicion: number): string {
    if (Posicion >= 0 && Posicion <= bingMap.length && bingMap[Posicion] != null) {
        let Rect0: Microsoft.Maps.LocationRect = bingMap[Posicion].map.getBounds();
        return Rect0.getWest().toString() + ";" + Rect0.getNorth().toString() + ";" +
            Rect0.getEast().toString() + ";" + Rect0.getSouth().toString();
    }
    else {
        return "-1;-1;-1;-1";
    }
}

function ObtenerZoom(Posicion: number): string {
    if (Posicion >= 0 && Posicion <= bingMap.length && bingMap[Posicion] != null) {
        return bingMap[Posicion].map.getZoom().toString();
    }
    else {
        return "-1";
    }
}

function AgregarLayerWMS(Posicion: number, UrlWMS: string, LatNorte: number, LngOeste: number, LatSur: number, LngEste: number): void {
//    uriConstructor: 'http://idpgis.ncep.noaa.gov/arcgis/services/NWS_Observations/radar_base_reflectivity/MapServer/WmsServer?REQUEST=GetMap&SERVICE=WMS&VERSION=1.3.0&LAYERS=1&STYLES=default&FORMAT=image/png&TRANSPARENT=TRUE&CRS=CRS:84&BBOX={bbox}&WIDTH=256&HEIGHT=256',
    if (bingMap != null) {
        if (UrlWMS != null && UrlWMS.length > 0) {
            var CapaWMS = new Microsoft.Maps.TileLayer({
                mercator: new Microsoft.Maps.TileSource({
                    uriConstructor: UrlWMS + '&LAYERS=1&STYLES=default&FORMAT=image/png&TRANSPARENT=TRUE&CRS=CRS:84&BBOX={bbox}&WIDTH=256&HEIGHT=256',
                    minZoom: 3,
                    maxZoom: 10,
                    bounds: Microsoft.Maps.LocationRect.fromEdges(LatNorte, LngOeste, LatSur, LngEste)
                })
            });

            bingMap[Posicion].map.layers.insert(CapaWMS);
        }

        var Capa = new Microsoft.Maps.Layer();
        //    Microsoft.Maps.Events.addHandler(Capa, 'mousedown', function () { highlight('mousedown'); });

        bingMap[Posicion].map.layers.insert(Capa);
    }
}

function AgregarPushpin(Posicion: number, Abscisa: number, Ordenada: number, Color: string, Texto: string, Texto2: string, Referencia: string): void {

    if (bingMap != null) {

        if (Texto != null && Texto.length > 0) {
            var pushpinLocal = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(Ordenada, Abscisa), {

                icon: this.CrearIcono(Color, Texto),

                title: Texto,

                subTitle: Texto2,

                anchor: new Microsoft.Maps.Point(12.5, 40.5)

            });
        }
        else {
            var pushpinLocal = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(Ordenada, Abscisa), {

                icon: this.CrearIcono(Color, Texto),

                anchor: new Microsoft.Maps.Point(12.5, 40.5)

            });
        }

        pushpinLocal.metadata = Referencia;

        if (Referencia.length > 0) {
            Microsoft.Maps.Events.addHandler(pushpinLocal, 'click', pushpinClicked);
        }

        bingMap[Posicion].map.entities.push(pushpinLocal);
    }

}

function AgregarPushpinGrande(Posicion: number, Abscisa: number, Ordenada: number, Color: string, Texto: string, Texto2: string, Referencia: string): void {

    if (bingMap != null) {

        var Factor: number = 1.25;

        if (Texto != null && Texto.length > 0) {
            var pushpinLocal = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(Ordenada, Abscisa), {

                icon: this.CrearIconoFactor(Color, Texto, Factor),

                title: Texto,

                subTitle: Texto2,

                anchor: new Microsoft.Maps.Point(12.5 * Factor, 40.5 * Factor)

            });
        }
        else {
            var pushpinLocal = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(Ordenada, Abscisa), {

                icon: this.CrearIconoFactor(Color, Texto, Factor),

                anchor: new Microsoft.Maps.Point(12.5 * Factor, 40.5 * Factor)

            });
        }

        pushpinLocal.metadata = Referencia;

        if (Referencia.length > 0) {
            Microsoft.Maps.Events.addHandler(pushpinLocal, 'click', pushpinClicked);
        }

        bingMap[Posicion].map.entities.push(pushpinLocal);
    }

}

function EliminarPushpin(Posicion: number, Referencia: string) : string {
    let Eliminar: Microsoft.Maps.Pushpin = null;
    try {
        for (var i = 0; i < bingMap[Posicion].map.entities.getLength(); i++) {

            if (bingMap[Posicion].map.entities.get(i) instanceof Microsoft.Maps.Pushpin) {
                let EnCiclo: Microsoft.Maps.Pushpin = <Microsoft.Maps.Pushpin>bingMap[Posicion].map.entities.get(i);
                if (Referencia.length > 0 && EnCiclo.metadata == Referencia) {
                    Eliminar = EnCiclo;
                    break;
                }
            }
        }
        if (Eliminar != null) {
            bingMap[Posicion].map.entities.remove(Eliminar);
            return "";
        }
        else {
            return "";
        }
    }
    catch (exc) {
        return exc.message;
    }
}

function EliminarPoligono(Posicion: number, Referencia: string): string {
    try {
        let Eliminar: Microsoft.Maps.Polygon = null;
        for (var i = 0; i < bingMap[Posicion].map.entities.getLength(); i++) {

            if (bingMap[Posicion].map.entities.get(i) instanceof Microsoft.Maps.Polygon) {
                let EnCiclo: Microsoft.Maps.Polygon = <Microsoft.Maps.Polygon>bingMap[Posicion].map.entities.get(i);
                if (Referencia.length > 0 && EnCiclo.metadata == Referencia) {
                    Eliminar = EnCiclo;
                    break;
                }
            }
        }
        if (Eliminar != null) {
            bingMap[Posicion].map.entities.remove(Eliminar);
            return "";
        }
        else {
            return "";
        }
    }
    catch (exc) {
        return exc.message;
    }
}

function pushpinClicked(e) {

    if (e.target.metadata) {
        window['FuncionesJS'].AbrirMenuBingMaps(e.target.metadata);
    }
    else {
        alert("Entra");
    }
}

var IconoAzul = null;
var IconoRojo = null;
var IconoVerde = null;
var IconoAmarillo = null;
var IconoGrande = null;

function CrearIcono(Color: string, Texto: string): string {
    switch (Color) {
        case "blue":
            if (IconoAzul == null) {
                IconoAzul = CrearIconoColor(Color, "");
            }
            return IconoAzul;
        case "red":
            if (IconoRojo == null) {
                IconoRojo = CrearIconoColor(Color, "");
            }
            return IconoRojo;
        case "green":
            if (IconoVerde == null) {
                IconoVerde = CrearIconoColor(Color, "");
            }
            return IconoVerde;
        case "yellow":
            if (IconoAmarillo == null) {
                IconoAmarillo = CrearIconoColor(Color, "");
            }
            return IconoAmarillo;
        default:
            return CrearIconoColor(Color, Texto);
    }
}

function CrearIconoFactor(Color: string, Texto: string, Factor: number): string {
    if (IconoGrande == null) {
        IconoGrande = CrearIconoColorFactor(Color, Texto, Factor);
    }
    return IconoGrande;
}

function CrearIconoColor(Color: string, Texto: string): string {
    return CrearIconoColorFactor(Color, Texto, 1);
}

function CrearIconoColorFactor(Color: string, Texto: string, Factor: number): string {
    var CanvasIc = document.createElement('canvas');
    CanvasIc.width = 25 * Factor;
    CanvasIc.height = 41 * Factor;

    var Contexto = CanvasIc.getContext('2d');

    //Draw a path in the shape of an arrow.

    //    Contexto.translate(-12, -40);

    Contexto.beginPath();
    Contexto.fillStyle = "lightgray";
    Contexto.strokeStyle = "gray";
    Contexto.lineWidth = 1;
    Contexto.moveTo(1.657905 * Factor, 17.642857 * Factor);
    Contexto.arc(12.5 * Factor, 12.5 * Factor, 12 * Factor, 2.698682, 0.442911, true);
    Contexto.lineTo(12.5 * Factor, 40.5 * Factor);
    Contexto.lineTo(1.6157905 * Factor, 17.642857 * Factor);
    Contexto.closePath();
    Contexto.fill();
    Contexto.stroke();

    Contexto.beginPath();
    Contexto.fillStyle = Color;
    Contexto.strokeStyle = "gray";
    Contexto.arc(12.5 * Factor, 12.5 * Factor, 12 * Factor, 0, 6.283185, false);
    Contexto.closePath();
    Contexto.fill();
    Contexto.stroke();

    //Generate the base64 image URL from the canvas.
    return CanvasIc.toDataURL();
}

function PosicionarMapa(Posicion: number, AbscCentro: number, OrdCentro: number, Zoom: number): void {
    if (bingMap[Posicion] != null) {
        bingMap[Posicion].map.setView({
            mapTypeId: Microsoft.Maps.MapTypeId.aerial,
            center: new Microsoft.Maps.Location(OrdCentro, AbscCentro),
            zoom: Zoom
        });
    }
}

function ExtremosMapa(Posicion: number): string {
    if (Posicion >= 0 && Posicion < bingMap.length && bingMap[Posicion] != null) {
        var PosicionMapa = bingMap[Posicion].map.getBounds();
        return PosicionMapa.getWest().toString() + ";" +
            PosicionMapa.getNorth().toString() + ";" +
            PosicionMapa.getEast().toString() + ";" +
            PosicionMapa.getSouth().toString();
    }
    else {
        return "0;0;-1;-1";
    }
}

function AgregarIconoCentro(Posicion: number, Lat: number, Lng: number, Texto1: string, Texto2: string, Referencia: string,
    Capa: Microsoft.Maps.Layer): void {

    if (bingMap[Posicion] != null) {
        var pushpinLocal = new Microsoft.Maps.Pushpin(new Microsoft.Maps.Location(Lat, Lng), {

            icon: this.CrearIconoCentroArea(),

            title: Texto1,

            subTitle: Texto2,

            anchor: new Microsoft.Maps.Point(4.5, 8.5)

        });

        pushpinLocal.metadata = Referencia;

        Microsoft.Maps.Events.addHandler(pushpinLocal, 'click', pushpinClicked);

        if (Capa == null) {
            bingMap[Posicion].map.entities.push(pushpinLocal);
        }
        else {
            Capa.add(pushpinLocal);
        }
    }
}

function CrearIconoCentroArea(): string {
    var CanvasIc = document.createElement('canvas');
    CanvasIc.width = 9;
    CanvasIc.height = 9;

    var Contexto = CanvasIc.getContext('2d');

    //Draw a path in the shape of an arrow.

    //    Contexto.translate(-12, -40);

    Contexto.beginPath();
    Contexto.fillStyle = "transparent";
    Contexto.strokeStyle = "black";
    Contexto.lineWidth = 1;
    Contexto.arc(4.5, 4.5, 4, 0, 2 * Math.PI, false);
    Contexto.closePath();
    Contexto.stroke();

    //Generate the base64 image URL from the canvas.
    return CanvasIc.toDataURL();
}

function DibujarPoligono(Posicion: number, Abscisas: number[], Ordenadas: number[],
    AbscCentro: number, OrdCentro: number,
    Color: string, Texto1: string, Texto2: string,
    Referencia: string, AnchoBorde: number): void {

    if (bingMap[Posicion] != null) {
        var Capa: Microsoft.Maps.Layer;

        if (bingMap[Posicion].map.layers.length == 2) {
            Capa = bingMap[Posicion].map.layers[1];
        }
        else {
            if (bingMap[Posicion].map.layers.length == 1) {
                Capa = bingMap[Posicion].map.layers[0];
            }
            else {
                Capa = null;
            }
        }

        try {

            //        var arr_names: number[] = new Array(4);

            var Coords: Microsoft.Maps.Location[] = new Array(Abscisas.length);
            var i;
            for (i = 0; i < Abscisas.length; i++) {
                Coords[i] = new Microsoft.Maps.Location(Ordenadas[i], Abscisas[i]);
            }

 //           Microsoft.Maps.CustomOverlay
            var Poligono = new Microsoft.Maps.Polygon(Coords, {
                strokeColor: 'gray',
                fillColor: Color,
                strokeThickness: AnchoBorde
            });

            Poligono.metadata = Referencia;
            Microsoft.Maps.Events.addHandler(Poligono, 'click', pushpinClicked);

            //    Capa.add(Poligono);
            if (Capa == null) {
                bingMap[Posicion].map.entities.push(Poligono);
            }
            else {
                Capa.add(Poligono);
            }

            if (AbscCentro > -998) {
                AgregarIconoCentro(Posicion, OrdCentro, AbscCentro, Texto1, Texto2, Referencia, Capa);
            }

        }
        catch (exc) {
            alert(exc.message);
        }
    }

}

function DibujarPoligonoOpaco(Posicion: number, Abscisas: number[], Ordenadas: number[],
    AbscCentro: number, OrdCentro: number,
    A: number, R: number, G: number, B: number, Texto1: string, Texto2: string,
    Referencia: string, AnchoBorde: number): void {

    if (bingMap[Posicion] != null) {
        var Capa: Microsoft.Maps.Layer;

        if (bingMap[Posicion].map.layers.length == 2) {
            Capa = bingMap[Posicion].map.layers[1];
        }
        else {
            if (bingMap[Posicion].map.layers.length == 1) {
                Capa = bingMap[Posicion].map.layers[0];
            }
            else {
                Capa = null;
            }
        }

        try {

            //        var arr_names: number[] = new Array(4);

            var Coords: Microsoft.Maps.Location[] = new Array(Abscisas.length);
            var i;
            for (i = 0; i < Abscisas.length; i++) {
                Coords[i] = new Microsoft.Maps.Location(Ordenadas[i], Abscisas[i]);
            }

            var Color = new Microsoft.Maps.Color(A, R, G, B);

            //           Microsoft.Maps.CustomOverlay
            var Poligono = new Microsoft.Maps.Polygon(Coords, {
                strokeColor: 'gray',
                fillColor: Color,
                strokeThickness: AnchoBorde
            });

            Poligono.metadata = Referencia;
            Microsoft.Maps.Events.addHandler(Poligono, 'click', pushpinClicked);

            //    Capa.add(Poligono);
            if (Capa == null) {
                bingMap[Posicion].map.entities.push(Poligono);
            }
            else {
                Capa.add(Poligono);
            }

            if (AbscCentro > -998) {
                AgregarIconoCentro(Posicion, OrdCentro, AbscCentro, Texto1, Texto2, Referencia, Capa);
            }

        }
        catch (exc) {
            alert(exc.message);
        }
    }

}

function DibujarPoligonoHueco(Posicion: number, Abscisas: number[], Ordenadas: number[],
    AbscAdentro: number[], OrdsAdentro: number[],
    Color: string, Texto1: string, Texto2: string, Referencia: string): void {

    if (bingMap[Posicion] != null) {
        var Capa: Microsoft.Maps.Layer;

        if (bingMap[Posicion].map.layers.length == 2) {
            Capa = bingMap[Posicion].map.layers[1];
        }
        else {
            if (bingMap[Posicion].map.layers.length == 1) {
                Capa = bingMap[Posicion].map.layers[0];
            }
            else {
                Capa = null;
            }
        }

        try {

            //        var arr_names: number[] = new Array(4);

            var Coords: Microsoft.Maps.Location[] = new Array(Abscisas.length);
            var i;
            for (i = 0; i < Abscisas.length; i++) {
                Coords[i] = new Microsoft.Maps.Location(Ordenadas[i], Abscisas[i]);
            }

            var CoordsAdentro: Microsoft.Maps.Location[] = new Array(AbscAdentro.length);
            for (i = 0; i < AbscAdentro.length; i++) {
                CoordsAdentro[i] = new Microsoft.Maps.Location(OrdsAdentro[i], AbscAdentro[i]);
            }

            var Contornos: Microsoft.Maps.Location[][] = [Coords, CoordsAdentro];


            Microsoft.Maps.CustomOverlay
            var Poligono = new Microsoft.Maps.Polygon(Contornos, {
                strokeColor: "gray",
                fillColor: Color,
                strokeThickness: 0
            });

            Poligono.metadata = Referencia;
            Microsoft.Maps.Events.addHandler(Poligono, 'click', pushpinClicked);

            //    Capa.add(Poligono);
            if (Capa == null) {
                bingMap[Posicion].map.entities.push(Poligono);
            }
            else {
                Capa.add(Poligono);
            }

        }
        catch (exc) {
            alert(exc.message);
        }
    }

}

function DibujarLinea(Posicion: number, Abscisas: number[], Ordenadas: number[], AbscCentro: number, OrdCentro: number,
    Color: string, Texto1: string, Texto2: string, Referencia: string): void {

    if (bingMap[Posicion] != null) {
        let Capa: Microsoft.Maps.Layer;

        if (bingMap[Posicion].map.layers.length == 2) {
            Capa = bingMap[Posicion].map.layers[1];
        }
        else {
            if (bingMap[Posicion].map.layers.length == 1) {
                Capa = bingMap[Posicion].map.layers[0];
            }
            else {
                Capa = null;
            }
        }

        var Coords = new Microsoft.Maps.Location[Abscisas.length];
        var i;
        for (i = 0; i < Abscisas.length; i++) {
            Coords[i] = new Microsoft.Maps.Location(Ordenadas[i], Abscisas[i]);
        }
        var Linea = new Microsoft.Maps.Polyline(Coords, {
            strokeColor: Color,
            strokeThickness: 1
        });

        Linea.metadata = Referencia;
        Microsoft.Maps.Events.addHandler(Linea, 'click', pushpinClicked);

        if (Capa == null) {
            bingMap[Posicion].map.entities.push(Linea);
        }
        else {
            Capa.add(Linea);
        }

        AgregarIconoCentro(Posicion, OrdCentro, AbscCentro, Texto1, Texto2, Referencia, Capa);

    }
}
