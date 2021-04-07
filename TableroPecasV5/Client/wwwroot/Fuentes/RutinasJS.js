window.FuncionesJS = {
    getInnerHeight: function () {
        try {
        return window.innerHeight;
        }
        catch (exc) {
            return 0;
        }
    },
    getInnerWidth: function () {
        try {
        return window.innerWidth;
        }
        catch (exc) {
            return 0;
        }
    },
    getAbscisa: function (Nombre) {
        try {
        return document.getElementById(Nombre).getBoundingClientRect().left;
        }
        catch (exc) {
            return 0;
        }
    },
    getOrdenada: function (Nombre) {
        try {
        return document.getElementById(Nombre).getBoundingClientRect().top;
        }
        catch (exc) {
            return 0;
        }
    },
    getRectangulo: function (Nombre) {
        try {
        Rectangulo = document.getElementById(Nombre).getBoundingClientRect();
        return Rectangulo.left + ";" + Rectangulo.top + ";" + Rectangulo.width + ";" + Rectangulo.height;
        }
        catch (exc) {
            return "0;0;0;0";
        }
    },
    getInnerWidthElemento: function (Nombre) {
        try {
            return document.getElementById(Nombre).clientWidth;
        }
        catch(exc) {
            return 0;
        }
    },
    getInnerHeightElemento: function (Nombre) {
        try {
        return document.getElementById(Nombre).clientHeight;
        }
        catch (exc) {
            return 0;
        }
    },
    registerResizeCallback: function () {
        window.addEventListener("resize", browserResize.resized);
    },
    resized: function () {
        //DotNet.invokeMethod("BrowserResize", 'OnBrowserResize');
        DotNet.invokeMethodAsync("BrowserResize", 'OnBrowserResize');
    },
    ReposicionarMapa: function (Posicion) {
        DotNet.invokeMethodAsync('TableroPecasV5.Client', 'RefrescarZoomAsync', Posicion.toString());
    },
    ClickEnMapa: function (Lat, Lng) {
        DotNet.invokeMethodAsync('TableroPecasV5.Client', 'ClickEnMapaAsync', Lat + ";" + Lng);
    },
    AbrirMenuBingMaps: function (Referencia) {
        DotNet.invokeMethodAsync('TableroPecasV5.Client', 'AbrirMenuBingMapsAsync', Referencia);
    },
    FncClickElemento: function () {
        alert("Entra");
    },
    FncPonerDesplazable: function (Nombre) {
        try {
            $(Nombre).draggable({
                drag: function (event, ui) {
                    FuncionesJS.CambioPosicionEv(event, ui);
                }
            });
            FuncionesJS.FncPonerMouseUp(Nombre);
        }
        catch (exc) {
            alert(exc.message);
        }
        return 0;
    },
    FncPonerMouseUp: function (Nombre) {
        $(Nombre).click(function (ev) {
            FuncionesJS.EjecutoClick(ev);
        })
        return 0;
    },
    IniciarResize: function (event) {
        Nombre = event.currentTarget.id;
        var element = document.getElementById(Nombre);
        element.addEventListener('mousemove', 'Redimensionar', false);
        element.addEventListener('mouseup', 'CerrarRedimensionado', false);
        DotNet.invokeMethodAsync('TableroPecasV5.Client', 'IniciaRedimensionadoAsync', Nombre,
              element.offsetLeft.toString() + ';' + element.offsetTop.toString());
    },
    Redimensionar: async function (e) {
        await DotNet.invokeMethodAsync('TableroPecasV5.Client', 'AjustarDimensionAsync', Nombre,
            e.clientX.toString() + ';' + e.clientY.toString());
    },
    CerrarRedimensionado: async function (event) {
        Nombre = event.currentTarget.id;
        var element = document.getElementById(Nombre);
        element.removeEventListener('mousemove', 'Redimensionar', false);
        element.removeEventListener('mouseup', 'CerrarRedimensionado', false);
        await DotNet.invokeMethodAsync('TableroPecasV5.Client', 'RefrescarGraficoAsync', Nombre);
    },
    AgregarEventoMouseDown: function (Nombre) {
        try {
        var element = document.getElementById(Nombre);
        element.addEventListener('mousedown', 'FuncionesJS.IniciarResize', false);
        return 0;
        }
        catch (exc) {
            alert(exc.message);
        }
    },
    FncPonerResizable: function (Nombre) {
        try {
            //$(Nombre).draggable({
            //    drag: function (event, ui) {
            //        FuncionesJS.CambioPosicionEv(event, ui);
            //    }
            //});
            //$(Nombre).resizable({
            //    resize: function () {
            //        FuncionesJS.CambioDimensionEv();
            //    }
            //});
            //$(Nombre).resizable();
            ////$(Nombre).ready(function () {
            // $(window).resize(function () {
            //        FuncionesJS.CambioDimensionEv();
            //    });
            ////});
            FuncionesJS.FncPonerMouseUp(Nombre);
        }
        catch (exc) {
            alert(exc.message);
        }
        return 0;
    },
    returnArrayAsyncJs: function () {
        DotNet.invokeMethodAsync('TableroPecasV5.Client', 'ReturnArrayAsync')
            .then(data => {
                data.push(4);
                console.log(data);
            });
    },
    FncPonerEventoClick: function (Nombre) {
        $(Nombre).click(function () {
            FuncionesJS.FncClickElemento();
        });
        return 0;
    },
    MostrarMsg: function (Msg) {
        alert(Msg);
        return 0;
    },
    CambioPosicionEv: async function (ev, ui) {
        try {
            Nombre = ui.helper.context.id;
            Respuesta = await DotNet.invokeMethodAsync('TableroPecasV5.Client', 'RecibirClickAsync', Nombre);
            if (Respuesta.length > 0) {
                alert(Respuesta);
            }
            else {
                // Buscar la posicion.
                Respuesta = await DotNet.invokeMethodAsync('TableroPecasV5.Client', 'CambioPosicionElementoAsync', Nombre, this.getRectangulo(Nombre));
                if (Respuesta.length > 0) {
                    alert(Respuesta);
                }
            }
        }
        catch (exc) {
            alert(exc.message);
        }
        return 0;
    },
    CambioDimensionEv: function () {
        try {
            RDotNet.invokeMethodAsync('TableroPecasV5.Client', 'RecibirClickAsync', 'qwqw');
                // Buscar la posicion.
            DotNet.invokeMethodAsync('TableroPecasV5.Client', 'CambioPosicionElementoAsync', Nombre, this.getRectangulo(Nombre));
        }
        catch (exc) {
            alert(exc.message);
        }
        return 0;
    },
    RefrescarMapa: async function (Nombre) {
        try {
            Respuesta = await DotNet.invokeMethodAsync('TableroPecasV5.Client', 'ProcesarPedidoRefrescoAsync', Nombre);
            if (Respuesta.length > 0) {
                alert(Respuesta);
            }
        }
        catch (exc) {
            alert(exc.message);
        }
    },
    EjecutoClick: async function (ev) {
        try {
            Respuesta = await DotNet.invokeMethodAsync('TableroPecasV5.Client', 'RecibirClickAsync', ev.currentTarget.id);
            if (Respuesta.length > 0) {
                alert(Respuesta);
            }
        }
        catch (exc) {
            alert(exc.message);
        }
        return 0;
    },
    AjustarDatosPosicion: function(Nombre) {
        alert("Cambio posicion");
        DotNet.invokeMethodAsync('BlazorSample', 'AjustarDimensionesPantallaAsync', Nombre);
    },
    ObtenerDimensionTexto: function (Texto, Fuente, Tamanio) {
        try {
            var div = document.createElement('div');
            div.style.position = 'absolute';
            div.style.visibility = 'hidden';
            div.style.height = 'auto';
            div.style.width = 'auto';
            div.style.whiteSpace = 'nowrap';
            div.style.fontFamily = Fuente;
            div.style.fontSize = Tamanio;
            div.style.border = "1px solid blue"; // for convenience when visible

            div.innerHTML = Texto;
            document.body.appendChild(div);

            //        var offsetWidth = div.offsetWidth;
            var clientWidth = div.clientWidth;

            document.body.removeChild(div);

            return clientWidth;
        }
        catch (exc) {
            return 10;
        }
    },
    PonerImagen: function (NombreImg, Datos) {
        // var arrayBufferView = new Uint8Array(Datos);
        //var blob = new Blob([arrayBufferView], { type: "image/png" });
        //var urlCreator = window.URL || window.webkitURL;
        //var imageUrl = urlCreator.createObjectURL(blob);
        var img = document.querySelector(NombreImg);
        //img.src = imageUrl;
        img.imageUrl = "data:image/png;base64," + Datos; // Convert.ToBase64String(arrayBufferView, 0, arrayBufferView.Length);
        return img.width + "," + img.height;
    },
    RegistrarDatosLocalStorage: function (key, value)
    {
      localStorage.setItem(key, value);
    },
    LeerDatosLocalStorage: function (key) {
      return localStorage.getItem(key);
    },
    ObtenerDimensiones: function (Tamanio, Fuente, LargoBytes) {
        return LargoBytes * Math.floor(this.ObtenerDimensionTexto("ABCDEFGHIJKLMNOPQRSTUVWXYZ", Fuente, Tamanio) / 26 + 0.5);
    }
}