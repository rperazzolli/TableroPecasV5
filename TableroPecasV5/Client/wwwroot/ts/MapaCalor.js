"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var CanvasOv = require("CanvasOverlayModule.js");
var mapColor = null;
function UbicarMapaLibre() {
    var i;
    for (i = 0; i < mapColor.length; i++) {
        if (mapColor[i] == null) {
            return i;
        }
    }
    return mapColor.length - 1;
}
// https://docs.microsoft.com/en-us/bingmaps/v8-web-control/map-control-concepts/custom-overlays/dynamic-canvas-overlay
function LiberarMapaCalor(Posicion) {
    if (Posicion >= 0 && Posicion < mapColor.length) {
        mapColor[Posicion] = null;
    }
}
function TextoHexa(Valor) {
    var Respuesta = Valor.toString(16);
    if (Respuesta.length == 1) {
        return "0" + Respuesta;
    }
    else {
        return Respuesta;
    }
}
function ColorRelleno(Posicion, Total, Red, Green, Blue) {
    if (Red < 0 && Green < 0 && Blue < 0) {
        if (Posicion <= (Total / 2)) {
            var ValRed = Math.floor(510 * Posicion / Total);
            return '#' + TextoHexa(ValRed) + TextoHexa(ValRed) + TextoHexa(255 - ValRed);
        }
        else {
            var ValGreen = Math.floor(510 * (Total - Posicion) / Total);
            return '#FF,' + TextoHexa(ValGreen) + "00";
        }
    }
    else {
        return "rgba(" + Red.toString() + "," + Green.toString() + "," + Blue.toString() + "," + (255 * Posicion / Total).toFixed() + ")";
    }
}
function DibujarPoligonoCanvas(Contexto, Puntos) {
    Contexto.beginPath();
    var i;
    Contexto.moveTo(Puntos[Puntos.length - 1].x, Puntos[Puntos.length - 1].y);
    for (i = 0; i < Puntos.length; i++) {
        Contexto.lineTo(Puntos[i].x, Puntos[i].y);
    }
    Contexto.closePath();
    Contexto.fill();
}
function ObtenerFraccion(Val1, Val2, Buscado) {
    if (((Val1 - Buscado) * (Val2 - Buscado)) <= 0) {
        return (Buscado - Val1) / (Val2 - Val1);
    }
    else {
        return -1;
    }
}
function PuntoSobreBorde(P1, P2, Fraccion) {
    return new Microsoft.Maps.Point(P1.x + (P2.x - P1.x) * Fraccion, P1.y + (P2.y - P1.y) * Fraccion);
}
function DibujarValoresTrianguloCanvas(Contexto, Punto1, Val1, Punto2, Val2, Punto3, Val3, Escala1, Escala2) {
    if (Val1 < Escala1 && Val2 < Escala1 && Val3 < Escala1) {
        return;
    }
    if (Val1 >= Escala2 && Val2 >= Escala2 && Val3 >= Escala2) {
        return;
    }
    var Puntos;
    // Ubicar puntos interseccion.
    // Si hay que llenar el triangulo
    if (Val1 >= Escala1 && Val2 >= Escala1 && Val3 >= Escala1 && Val1 <= Escala2 && Val2 <= Escala2 && Val3 <= Escala2) {
        Puntos = [Punto1, Punto2, Punto3];
        DibujarPoligonoCanvas(Contexto, Puntos);
        return;
    }
    var IntersV1 = new Microsoft.Maps.Point[2];
    var PosIntersV1 = [-1, -1];
    var Fraccion112 = ObtenerFraccion(Val1, Val2, Escala1);
    var Fraccion123 = ObtenerFraccion(Val2, Val3, Escala1);
    var Fraccion113 = ObtenerFraccion(Val1, Val3, Escala1);
    var Fraccion212 = ObtenerFraccion(Val1, Val2, Escala2);
    var Fraccion223 = ObtenerFraccion(Val2, Val3, Escala2);
    var Fraccion213 = ObtenerFraccion(Val1, Val3, Escala2);
    if (Fraccion112 < 0 && Fraccion123 < 0 && Fraccion113 < 0) {
        // no intersecta el valor Escala1.
        if (Fraccion212 >= 0 && Fraccion223 >= 0) {
            if (Val2 < Escala2) {
                Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion212), Punto2, PuntoSobreBorde(Punto2, Punto3, Fraccion223)];
            }
            else {
                Puntos = [Punto1, PuntoSobreBorde(Punto1, Punto2, Fraccion212), PuntoSobreBorde(Punto2, Punto3, Fraccion223), Punto3];
            }
        }
        else {
            if (Fraccion212 >= 0 && Fraccion213 >= 0) {
                if (Val1 < Escala2) {
                    Puntos = [Punto2, PuntoSobreBorde(Punto1, Punto2, Fraccion212), PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                }
                else {
                    Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion212), Punto2, Punto3, PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                }
            }
            else {
                if (Fraccion223 >= 0 && Fraccion213 >= 0) {
                    if (Val3 < Escala2) {
                        Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion223), Punto3, PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                    }
                    else {
                        Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion223), PuntoSobreBorde(Punto1, Punto3, Fraccion213), Punto1, Punto2];
                    }
                }
            }
        }
    }
    else {
        if (Fraccion212 < 0 && Fraccion223 < 0 && Fraccion213 < 0) {
            // el valor Escala2 no intersecta.
            if (Fraccion112 >= 0 && Fraccion123 >= 0) {
                if (Val2 > Escala1) {
                    Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion212), Punto2, PuntoSobreBorde(Punto2, Punto3, Fraccion223)];
                }
                else {
                    Puntos = [Punto1, PuntoSobreBorde(Punto1, Punto2, Fraccion212), PuntoSobreBorde(Punto2, Punto3, Fraccion223), Punto3];
                }
            }
            else {
                if (Fraccion112 >= 0 && Fraccion113 >= 0) {
                    if (Val1 > Escala1) {
                        Puntos = [Punto2, PuntoSobreBorde(Punto1, Punto2, Fraccion212), PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                    }
                    else {
                        Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion212), Punto2, Punto3, PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                    }
                }
                else {
                    if (Fraccion123 >= 0 && Fraccion113 >= 0) {
                        if (Val3 > Escala1) {
                            Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion223), Punto3, PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                        }
                        else {
                            Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion223), PuntoSobreBorde(Punto1, Punto3, Fraccion213), Punto1, Punto2];
                        }
                    }
                }
            }
        }
        else {
            // Aca hay intersecciones por todas partes (2 intersecciones por valor de escala.)
            if (Fraccion112 >= 0 && Fraccion123 >= 0) {
                if (Fraccion212 >= 0 && Fraccion223 > 0) {
                    if (Fraccion112 < Fraccion212) {
                        Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto1, Punto2, Fraccion212),
                            PuntoSobreBorde(Punto2, Punto3, Fraccion223), PuntoSobreBorde(Punto2, Punto3, Fraccion123)];
                    }
                    else {
                        Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto2, Punto3, Fraccion123),
                            PuntoSobreBorde(Punto2, Punto3, Fraccion223), PuntoSobreBorde(Punto2, Punto3, Fraccion212)];
                    }
                }
                else {
                    if (Fraccion223 >= 0 && Fraccion213 >= 0) {
                        Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto2, Punto3, Fraccion123),
                            PuntoSobreBorde(Punto2, Punto3, Fraccion223), PuntoSobreBorde(Punto1, Punto3, Fraccion213)];
                    }
                    else {
                        if (Fraccion212 >= 0 && Fraccion213 >= 0) {
                            Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto2, Punto3, Fraccion123),
                                PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto2, Fraccion212)];
                        }
                    }
                }
            }
            else {
                if (Fraccion112 >= 0 && Fraccion113 >= 0) {
                    if (Fraccion212 >= 0 && Fraccion213 > 0) {
                        if (Fraccion112 < Fraccion212) {
                            Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto1, Punto2, Fraccion212),
                                PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto3, Fraccion113)];
                        }
                        else {
                            Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto1, Punto3, Fraccion113),
                                PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto3, Fraccion113)];
                        }
                    }
                    else {
                        if (Fraccion223 >= 0 && Fraccion213 >= 0) {
                            Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto2, Punto3, Fraccion223),
                                PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto3, Fraccion113)];
                        }
                        else {
                            if (Fraccion212 >= 0 && Fraccion223 >= 0) {
                                Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto1, Punto2, Fraccion212),
                                    PuntoSobreBorde(Punto2, Punto3, Fraccion223), PuntoSobreBorde(Punto1, Punto3, Fraccion113)];
                            }
                        }
                    }
                }
                else {
                    if (Fraccion123 >= 0 && Fraccion113 >= 0) {
                        if (Fraccion223 >= 0 && Fraccion213 > 0) {
                            if (Fraccion123 < Fraccion223) {
                                Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion123), PuntoSobreBorde(Punto1, Punto3, Fraccion113),
                                    PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto3, Fraccion113)];
                            }
                            else {
                                Puntos = [PuntoSobreBorde(Punto1, Punto2, Fraccion112), PuntoSobreBorde(Punto2, Punto3, Fraccion223),
                                    PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto3, Fraccion113)];
                            }
                        }
                        else {
                            if (Fraccion212 >= 0 && Fraccion213 >= 0) {
                                Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion123), PuntoSobreBorde(Punto1, Punto3, Fraccion113),
                                    PuntoSobreBorde(Punto1, Punto3, Fraccion213), PuntoSobreBorde(Punto1, Punto2, Fraccion212)];
                            }
                            else {
                                if (Fraccion212 >= 0 && Fraccion223 >= 0) {
                                    Puntos = [PuntoSobreBorde(Punto2, Punto3, Fraccion123), PuntoSobreBorde(Punto1, Punto1, Fraccion113),
                                        PuntoSobreBorde(Punto1, Punto2, Fraccion212), PuntoSobreBorde(Punto2, Punto3, Fraccion223)];
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    DibujarPoligonoCanvas(Contexto, Puntos);
}
function DibujarValoresRectanguloCanvas(Contexto, Punto1, Val1, Punto2, Val2, Punto3, Val3, Punto4, Val4, Escala1, Escala2) {
    if (Val1 < Escala1 && Val2 < Escala1 && Val3 < Escala1 && Val4 < Escala1) {
        return;
    }
    if (Val1 >= Escala2 && Val2 >= Escala2 && Val3 >= Escala2 && Val4 >= Escala2) {
        return;
    }
    var PuntoCentro = new Microsoft.Maps.Point((Punto1.x + Punto2.x) / 2, (Punto2.y + Punto4.y) / 2);
    var ValCentro = (Val1 + Val2 + Val3 + Val4) / 4;
    DibujarValoresTrianguloCanvas(Contexto, Punto1, Val1, Punto2, Val2, PuntoCentro, ValCentro, Escala1, Escala2);
    DibujarValoresTrianguloCanvas(Contexto, Punto2, Val2, Punto4, Val4, PuntoCentro, ValCentro, Escala1, Escala2);
    DibujarValoresTrianguloCanvas(Contexto, Punto4, Val4, Punto3, Val3, PuntoCentro, ValCentro, Escala1, Escala2);
    DibujarValoresTrianguloCanvas(Contexto, Punto3, Val3, Punto1, Val1, PuntoCentro, ValCentro, Escala1, Escala2);
}
function GetMapCalor(Posicion, NombreCanvas, Filas, Columnas, Abscisas, Ordenadas, Valores, LatCentro, LngCentro, NivelZoom, Escala, Red, Green, Blue) {
    if (mapColor == null) {
        mapColor = [null, null, null, null, null, null, null, null, null, null];
    }
    var ValoresNecesarios = (Filas + 1) * (Columnas + 1);
    if (Abscisas.length != ValoresNecesarios || Ordenadas.length != ValoresNecesarios || Valores.length != ValoresNecesarios) {
        alert("Los valores de abscisas, ordenadas o valores no corresponden a filas y columnas");
        return "-1";
    }
    if (Escala.length < 2) {
        alert("Se requieren al menos 2 puntos de escala");
        return "-1";
    }
    if (Red < 0 && (Blue * Green * Red) >= 0) {
        alert("Color incorrecto");
        return "-1";
    }
    if (Posicion < 0) {
        Posicion = UbicarMapaLibre();
    }
    mapColor[Posicion] = new Microsoft.Maps.Map(NombreCanvas, {});
    if (mapColor[Posicion] != null) {
        mapColor[Posicion].setView({
            center: new Microsoft.Maps.Location(LatCentro, LngCentro),
            zoom: NivelZoom
        });
    }
    var Ubicaciones;
    Ubicaciones = new Array(ValoresNecesarios);
    var i;
    for (i = 0; i < ValoresNecesarios; i++) {
        Ubicaciones[i] = new Microsoft.Maps.Location(Ordenadas[i], Abscisas[i]);
    }
    var Puntos = mapColor[Posicion].tryLocationToPixel(Ubicaciones, Microsoft.Maps.PixelReference.control);
    //Register the custom module.
    Microsoft.Maps.registerModule('CanvasOverlayModule', 'CanvasOverlayModule.js');
    //Load the module.
    Microsoft.Maps.loadModule('CanvasOverlayModule', function () {
        //Implement the new custom overlay class.
        var overlay = new CanvasOv.CanvasOverlay(function (canvas) {
            var ctx = canvas.getContext("2d");
            var Fila;
            var Columna;
            var PosColor;
            for (PosColor = 0; PosColor < (Escala.length - 1); PosColor++) {
                ctx.fillStyle = ColorRelleno(PosColor + 0.5, Escala.length, Red, Green, Blue);
                for (Fila = 0; Fila < Filas; Fila++) {
                    for (Columna = 0; Columna < Columnas; Columna++) {
                        var Pos0 = Fila * (Columnas + 1) + Columna;
                        var Pos2 = Pos0 + Columnas + 1;
                        DibujarValoresRectanguloCanvas(ctx, Puntos[Pos0], Valores[Pos0], Puntos[Pos0 + 1], Valores[Pos0 + 1], Puntos[Pos2], Valores[Pos2], Puntos[Pos2 + 1], Valores[Pos2 + 1], Escala[PosColor], Escala[PosColor + 1]);
                    }
                }
            }
        });
        //Add the custom overlay to the map.
        mapColor[Posicion].layers.insert(overlay);
    });
}
//# sourceMappingURL=MapaCalor.js.map