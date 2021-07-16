using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazor.Extensions.Canvas.Model;
using Blazorise;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Plantillas;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaGrafico : CBaseGrafico, IDisposable
  {

    //public enum ClaseGrafico
    //{
    //  Tortas = 1,
    //  Barras = 2,
    //  BarrasMultiples = 3,
    //  Histograma = 4,
    //  Puntos = 5,
    //  NoDefinido = -1
    //}

    public event CLogicaFiltroTextos.FncEventoCrearGrafDependiente AlCrearGraficoDependiente;
    public event CLogicaFiltroTextos.FncEventoTextoBool AlCerrarGrafico;
    public event FncSeleccionarGrafico EvSeleccionarGrafico;

    public CLogicaGrafico()
    {
      mszNombreElemento = "ELEM_GRAF_" + (gNroGrafico++).ToString();
    }

    public CLogicaGrafico(ClaseGrafico Clase0, CColumnaBase ColValor, CColumnaBase ColAbscisas,
        CColumnaBase ColSexo, CProveedorComprimido Prov0)
    {
      mszNombreElemento = "ELEM_GRAF_" + (gNroGrafico++).ToString();
      Clase = Clase0;
      ColumnaOrdenadas = ColValor;
      ColumnaAbscisas = ColAbscisas;
      ColumnaSexo = ColSexo;
      Proveedor = Prov0;
      ValoresSeleccionados = new List<string>();
    }

    [CascadingParameter]
    Logicas.CLogicaIndicador Pagina { get; set; }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public ClaseGrafico Clase { get; set; }

    [Parameter]
    public CColumnaBase ColumnaAbscisas { get; set; } = null;

    [Parameter]
    public CColumnaBase ColumnaOrdenadas { get; set; } = null;

    [Parameter]
    public CColumnaBase ColumnaSexo { get; set; } = null;

    [Parameter]
    public List<string> ValoresSeleccionados { get; set; } = new List<string>();

    [Parameter]
    public List<CLogicaGrafico> GraficosDependientes { get; set; } = new List<CLogicaGrafico>();

    private CFiltradorPasos mFiltroPasos = null;
    private List<CPorcionTorta> mPorciones = null;
    private List<CPorcionTorta> PorcionesTorta
    {
      get { return mPorciones; }
      set { mPorciones = value; }
    }
    private List<CPorcionTorta> mPorcionesDibujadas = new List<CPorcionTorta>();
    private CPorcionTorta mPorcion20 = null;
    private static Int32 gNroGrafico = 1;

    private List<DatosBarraApilada> mDatosApilados;
    private List<ElementoPila> mPilas;

    private string mszNombreElemento;

    public void Cerrando(Blazorise.ModalClosingEventArgs e)
    {
      switch (e.CloseReason)
      {
        case CloseReason.EscapeClosing:
        case CloseReason.FocusLostClosing:
          e.Cancel = true;
          break;
      }
    }

    public string NombreElemento
    {
      get { return mszNombreElemento; }
    }

    [Parameter]
    public long AnchoCanvas { get; set; }

    [Parameter]
    public long AltoCanvas { get; set; }

    [Parameter]
    public long AltoCanvasDer { get; set; }

    [Parameter]
    public long AnchoGraficoApilado { get; set; }

    [Parameter]
    public long AnchoGraficoApiladoDer { get; set; }

    [Parameter]
    public CLinkGrafico Link { get; set; }

    [Parameter]
    public bool PuedeCrearGraficoDependiente { get; set; } = true;

    private long mAnchoGrafico = -1;

    public long AnchoGrafico
    {
      get { return (long)mAnchoGrafico; }
      set
      {
        if (value != mAnchoGrafico)
        {
          mAnchoGrafico = value;
          AnchoCanvas = value;
          AnchoGraficoApilado = Math.Max(2, 67 * value / 100 - 1);
          AnchoGraficoApiladoDer = Math.Max(2, Math.Min(value - AnchoGraficoApilado - 2, 33 * value / 100));
        }
      }
    }

    public void RecibirDrop(Microsoft.AspNetCore.Components.Web.DragEventArgs e)
    {
      if (Pagina.LineaDrag != null)
      {
        if (Pagina.LineaDrag.Columna.Orden >= 0)
        {
          if (AlCrearGraficoDependiente != null)
          {
            AlCrearGraficoDependiente(this, Pagina.LineaDrag.Columna.Orden);
          }
          CerrarVentanaGraficoDependiente();
        }
        Pagina.LineaDrag = null;
      }
    }

    public long AltoGrafico
    {
      get { return (long)Alto; }
      set
      {
        if (value != Alto)
        {
          Alto = value;
        }
      }
    }

    public long AnchoGraficoTotal { get; set; }
    public long AltoGraficoTotal { get; set; }

    public async Task<bool> FncObtenerPosicionDimensiones()
    {
      object[] Args = new object[1];
      Args[0] = mszNombreElemento;
      Rectangulo Medidas = new Rectangulo(await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args));
      return (((Int32)Medidas.width) != Ancho || ((Int32)Medidas.height) != Alto ||
            ((Int32)Medidas.left) != Abscisa || ((Int32)Medidas.top) != Ordenada);
    }

    public async Task AlCompletar()
    {
      await AjustarDimensionesGlobalesAsync("#TendenciaUnica");
      StateHasChanged();
    }

    public async Task AjustarDimensionesGlobalesAsync(string Nombre)
    {
      if (await base.AjustarDimensionesAsync(Nombre))
      {
        AjustarDimensionesGrafico();
      }
    }

    private void AjustarDimensionesGrafico()
    {
      switch (Clase)
      {
        case ClaseGrafico.Torta:
        case ClaseGrafico.Puntos:
          if (AnchoGraficoTotal != (Ancho - 4))
          {
            AnchoGraficoTotal = (long)Ancho - 4;
          }
          if (AltoGraficoTotal != (Alto - 32))
          {
            AltoGraficoTotal = (long)Alto - 32;
          }
          break;
        case ClaseGrafico.Barras:
          AnchoGraficoTotal = Math.Max((long)Ancho - 4, (long)(PorcionesTorta.Count *
              (CTendencias.ANCHO_BARRA + CTendencias.SEP_ENTRE_BARRAS) - CTendencias.SEP_ENTRE_BARRAS));
          AltoGraficoTotal = (AnchoGraficoTotal > ((long)Ancho - 4) ? ((long)Alto - 50) : ((long)Alto - 32));
          break;
        case ClaseGrafico.Histograma:
          AnchoGraficoTotal = Math.Max((long)Ancho - 4, (long)(mDatosHistograma.Count *
              (CTendencias.ANCHO_BARRA + CTendencias.SEP_ENTRE_BARRAS) - CTendencias.SEP_ENTRE_BARRAS));
          AltoGraficoTotal = (AnchoGraficoTotal > ((long)Ancho - 4) ? ((long)Alto - 50) : ((long)Alto - 32));
          break;
      }
    }

    public static Int32 CantidadBarrasHistograma(Int32 CantidadElementos)
    {
      if (CantidadElementos >= 1000)
      {
        return 10 + (Int32)Math.Floor(Math.Log(CantidadElementos / 1000));
      }
      else
      {
        if (CantidadElementos >= 500)
        {
          return 10;
        }
        else
        {
          if (CantidadElementos >= 200)
          {
            return 9;
          }
          else
          {
            if (CantidadElementos >= 100)
            {
              return 8;
            }
            else
            {
              if (CantidadElementos >= 50)
              {
                return 7;
              }
              else
              {
                return Math.Min(6, CantidadElementos);
              }
            }
          }
        }
      }
    }
    public void ImponerClase(ClaseGrafico Clase0)
    {
      Clase = Clase0;
    }

    public void ImponerModoAgrupar(ModoAgruparDependiente Modo)
    {
      if (Modo != AgrupamientoDependiente)
      {
        AgrupamientoDependiente = Modo;
      }
    }

    public void ImponerCodigoUnico(Int32 Cod0)
    {
      if (Cod0 != CodigoUnico)
      {
        CodigoUnico = Cod0;
      }
    }

    public void ImponerColumnaAbscisas(CColumnaBase Col0)
    {
      if (Col0 != ColumnaAbscisas)
      {
        ColumnaAbscisas = Col0;
      }
    }

    public void ImponerColumnaOrdenadas(CColumnaBase Col0)
    {
      if (Col0 != ColumnaOrdenadas)
      {
        ColumnaOrdenadas = Col0;
      }
    }

    public void ImponerColumnaSexo(CColumnaBase Col0)
    {
      if (Col0 != ColumnaOrdenadas)
      {
        ColumnaSexo = Col0;
      }
    }

    public void ImponerValoresSeleccionados(List<string> Valores)
    {
      ValoresSeleccionados = Valores;
    }

    public void ImponerGraficosDependientes(List<CLogicaGrafico> Valores)
    {
      GraficosDependientes = Valores;
    }

    public void AbrirVentanaGraficoAsociado()
    {
      if (ModalGraficoDependiente != null)
      {
        VariableIndependienteGraficoDependiente = -1;
        ModalGraficoDependiente.Show();
      }
    }

    private string ScrollAbscisas
    {
      get
      {
        switch (Clase)
        {
          case ClaseGrafico.Barras:
          case ClaseGrafico.BarrasH:
          case ClaseGrafico.Histograma:
            return (AnchoCanvas > Ancho ? "auto" : "hidden");
          default:
            return "hidden";
        }
      }
    }

    private string ScrollOrdenadas
    {
      get
      {
        switch (Clase)
        {
          case ClaseGrafico.BarrasH:
            return "auto";
          default:
            return "hidden";
        }
      }
    }

    public string Estilo
    {
      get
      {
        return "width:100%; height: calc(100% - 32px); overflow-x: " + (AnchoGrafico > Ancho ? "auto" : "hidden") +
          "; overflow-y: " + ScrollOrdenadas + "; position: relative; margin: 30px 0px 0px 0px; background-color: white;";
      }
    }

    public void ImponerDetallado(bool B)
    {
      Detallado = B;
    }

    [Parameter]
    public bool Detallado { get; set; }

    public string EstiloCanvasApiladas
    {
      get
      {
        return "width: " + (Detallado ? "100" : "67") + "%; height: calc(100% - 32px); overflow-x: hidden; display: inline-block; float: left; " +
          " overflow-y: auto; position: relative; margin: 0px 0px 0px 0px; background-color: white;";
      }
    }

    public string EstiloCanvasApiladasDer
    {
      get
      {
        return "width:33%; height: calc(100% - 32px); overflow-x: hidden; display: inline-block;" +
          " overflow-y: auto; position: relative; margin: 0px 0px 0px 0px; background-color: white;";
      }
    }

    public string Nombre
    {
      get { return mszNombreElemento; }
    }

    public void Cerrar()
    {
      if (AlCerrarGrafico != null)
      {
        AlCerrarGrafico(mszNombreElemento, false);
      }
    }

    public void Maximizar()
    {
      if (AlCerrarGrafico != null && !Ampliado)
      {
        mAnchoLabelsIzq = -1;
        Pagina.PonerElementoEncima(false, false, false, -1, this.CodigoUnico);
        Pagina.ReposicionarGrafico(this.CodigoUnico, 2, 2,
            Contenedores.CContenedorDatos.AnchoPantallaAmpliada - 4,
            Contenedores.CContenedorDatos.AltoPantallaAmpliada - 4, true);
        Ampliado = true;
        AlCerrarGrafico("", true);
      }
    }

    public void Reducir()
    {
      if (AlCerrarGrafico != null && Ampliado)
      {
        Pagina.PonerElementoEncima(false, false, false, -1, -1);
        Ampliado = false;
        mAnchoLabelsIzq = -1;
        Pagina.ReposicionarGrafico(this.CodigoUnico, -999999, -999999, -999999, -999999, true);
        AlCerrarGrafico("", true);
      }
    }

    public void FncScroll()
    {
      if (mRefrescador == null)
      {
        mRefrescador = new Clases.CTimerRefresco();
        mRefrescador.AlRefrescar += RefrescarInterfase;
      }
      mRefrescador.RefrescarPedido();
    }

    private Clases.CTimerRefresco mRefrescador = null;

    private void RefrescarInterfase()
    {
      try
      {
        InvokeAsync(() => { StateHasChanged(); });
      }
      catch (Exception)
      {
      }
    }

    private async Task<Rectangulo> ObtenerRectanguloAsync()
    {
      object[] Args = new object[1];
      Args[0] = mszNombreElemento;
      return new Rectangulo(await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args));
    }

    private void AgregarCondicion(ref CCondiciones Condiciones,
        CDatosTorta Dato)
    {
      CCondicion Condicion = new CCondicion();
      if (Clase == ClaseGrafico.Histograma)
      {
        Condicion.Clase = ColumnaOrdenadas.Clase;
        Condicion.ColumnaCondicion = ColumnaOrdenadas.Orden;
        Condicion.Modo = ModoFiltrar.PorRango;
        Condicion.ValorMinimo = Dato.MinimoRango;
        Condicion.ValorMaximo = Dato.MaximoRango;
      }
      else
      {
        Condicion.Clase = ColumnaAbscisas.Clase;
        Condicion.ColumnaCondicion = ColumnaAbscisas.Orden;
        if (AgrupamientoIndependiente == ModoAgruparIndependiente.Todos)
        {
          Condicion.Modo = ModoFiltrar.PorValor;
          Condicion.ValorIgual = Dato.NombreOriginal;
        }
        else
        {
          Condicion.Modo = ModoFiltrar.PorRango;
          Condicion.ValorMinimo = Dato.MinimoRango;
          Condicion.ValorMaximo = Dato.MaximoRango;
        }
      }
      Condiciones.AgregarCondicion(Condicion);
    }

    /// <summary>
    /// Arma una lista de las condiciones seleccionadas para el grafico (elementos seleccionados).
    /// </summary>
    /// <returns></returns>
    private List<CCondiciones> ExtraerCondicionesSeleccionadas()
    {
      List<CCondiciones> Respuesta = new List<CCondiciones>();
      foreach (CDatosTorta Dato in mListaSeleccion)
      {
        CCondiciones Condicion = new CCondiciones();
        Condicion.IncluyeCondiciones = true;
        AgregarCondicion(ref Condicion, Dato);
        Respuesta.Add(Condicion);
      }

      return Respuesta;
    }

    private List<CLineaComprimida> ObtenerDatosDependientes()
    {
      if (mListaSeleccion.Count == 0)
      {
        return Filtrador.Datos;
      }
      else
      {
        List<CLineaDatos> Respuesta = new List<CLineaDatos>();
        CFiltradorStep Paso = new CFiltradorStep();
        Paso.AgregarCondiciones(ExtraerCondicionesSeleccionadas());
        Paso.CumplirTodas = false;
        Paso.FiltrarDatos(Filtrador.Datos, Filtrador.Proveedor.Columnas);
        return Paso.Datos;
      }
    }

    public CLogicaGrafico Superior { get; set; } = null;

    private List<CDatosTorta> mListaSeleccion = new List<CDatosTorta>();

    private bool mbMouseEnProceso = false;

    public int AbscisaAbajo { get; set; }
    public int OrdenadaAbajo { get; set; }

    public void PonerArriba(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
    }

    public void EventoMouseAbajo(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      Pagina.PonerElementoEncima(false, false, false, -1, CodigoUnico);
      AbscisaAbajo = (int)e.ScreenX;
      OrdenadaAbajo = (int)e.ScreenY;
      Pagina.Refrescar();
    }

    public void EventoMouseUp(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      lock (OBJ_LOCK)
      {
        if (mbMouseEnProceso)
        {
          return;
        }
        else
        {
          mbMouseEnProceso = true;
        }
      }

      try
      {
        if (e != null)
        {
          if (Clase == ClaseGrafico.Barras)
          {
            BuscarPorcionAbajo(mPorcionesBarras, e);
          }
          else
          {
            if (Clase == ClaseGrafico.Histograma)
            {
              BuscarPorcionAbajo((from D in mDatosHistograma
                                             select D.Datos).ToList(), e);
            }
            else
            {
              if (mPorcionesDibujadas != null)
              {
                BuscarPorcionAbajo(mPorcionesDibujadas, e);
              }
            }
          }
        }
      }
      finally
      {
        mbMouseEnProceso = false;
      }
    }

    private void BuscarPorcionAbajo(List<CPorcionTorta> Porciones, Microsoft.AspNetCore.Components.Web.MouseEventArgs e) {
      if (PorcionesTorta != null)
      {
        //Rectangulo Rect0 = await ObtenerRectanguloAsync();
        //e.ClientX -= Rect0.left;
        //e.ClientY -= Rect0.top;
        Int32 Pos = 0;
        foreach (CPorcionTorta Porcion in Porciones)
        {
          if (Porcion.PuntoEncima(e.OffsetX, e.OffsetY))
          {
            Porcion.Seleccionado = !Porcion.Seleccionado;
            if (Porcion == mPorcion20 || Porcion.Datos == null)
            {
              mbPorcion20Seleccionada = Porcion.Seleccionado;
            }
            if (Porcion.Seleccionado)
            {
              ValoresSeleccionados.Add(Porcion.Datos.Nombre);
            }
            else
            {
              ValoresSeleccionados.Remove(Porcion.Datos.Nombre);
            }

            mListaSeleccion = (from P in Porciones
                               where P.Seleccionado
                               select P.Datos).ToList();

            AjustarGraficosDependientes(); acá hay que buscar por qué se limpia el proveedor.

            StateHasChanged();
            break;
          }
          Pos++;
        }
      }
    }

    public BECanvas CanvasGrafico;
    private Canvas2DContext mContexto;

    public BECanvas CanvasGraficoDer;
    private Canvas2DContext mContextoDer;

    [Parameter]
    public ModoAgruparIndependiente AgrupamientoIndependiente { get; set; } = ModoAgruparIndependiente.NoDefinido;

    [Parameter]
    public ModoAgruparDependiente AgrupamientoDependiente { get; set; } = ModoAgruparDependiente.NoDefinido;

    public Int32 MaxGajos { get; set; } = 20;

    public void ImponerAgrupamientoIndependiente(ModoAgruparIndependiente Modo)
    {
      AgrupamientoIndependiente = Modo;
    }

    public void ImponerAgrupamientoDependiente(ModoAgruparDependiente Modo)
    {
      AgrupamientoDependiente = Modo;
    }

    private ModoAgruparIndependiente ModoAgruparSegunFechasExtremas()
    {
      // determinar diferencia de fechas (en dias).
      if (ColumnaAbscisas.Nombre == CProveedorComprimido.PERIODO_DATOS_DATASET)
      {
        return ModoAgruparIndependiente.Todos;
      }
      else
      {
        return CRutinas.AgrupamientoSegunRangoDias(ColumnaAbscisas.RangoDeFechas());
      }
    }

    public Int32 AbscisaGrafico
    {
      get { return Abscisa; }
      set
      {
        if (value != Abscisa)
        {
          Abscisa = value;
        }
      }
    }

    public Int32 OrdenadaGrafico
    {
      get { return Ordenada; }
      set
      {
        if (value != Ordenada)
        {
          Ordenada = value;
        }
      }
    }

    public void Dispose()
    {
      if (mFiltroPasos != null && mFiltroPasos.Proveedor != null)
      {
        mFiltroPasos.Proveedor.AlAjustarDependientes -= Proveedor_AlAjustarDependientes;
      }
    }

    private void AjustarAgrupamiento()
    {
      if (AgrupamientoIndependiente == ModoAgruparIndependiente.NoDefinido)
      {
        AgrupamientoIndependiente = ColumnaAbscisas.AjustarAgrupamientoIndependientePorDefecto;
      }
      if (AgrupamientoDependiente == ModoAgruparDependiente.NoDefinido)
      {
        AgrupamientoDependiente = ColumnaOrdenadas.AjustarAgrupamientoDependientePorDefecto;
      }
    }

    public CFiltradorPasos Filtrador
    {
      get { return mFiltroPasos; }
    }

    public CProveedorComprimido Proveedor
    {
      get
      {
        return (mFiltroPasos == null ? null : mFiltroPasos.Proveedor);
      }
      set
      {
        if (mFiltroPasos == null || mFiltroPasos.Proveedor != value)
        {
          if (mFiltroPasos == null)
          {
            mFiltroPasos = new CFiltradorPasos(ClaseElemento.NoDefinida, -1);
          }
          else
          {
            if (mFiltroPasos.Proveedor != null)
            {
              mFiltroPasos.Proveedor.AlAjustarDependientes -= Proveedor_AlAjustarDependientes;
            }
          }
          mFiltroPasos.ClaseOrigen = value.ClaseOrigen;
          mFiltroPasos.CodigoOrigen = value.CodigoOrigen;
          mFiltroPasos.Proveedor = value;
          if (mFiltroPasos.Proveedor != null)
          {
            mFiltroPasos.Proveedor.AlAjustarDependientes += Proveedor_AlAjustarDependientes;
          }
        }
      }
    }

    public Modal ModalCfgHistograma { get; set; }

    public void AbrirVentanaCfgHistograma()
    {
      if (ModalCfgHistograma != null)
      {
        ModalCfgHistograma.Show();
      }
    }

    public void CerrarVentanaCfgHistograma()
    {
      if (ModalCfgHistograma != null)
      {
        ModalCfgHistograma.Hide();
      }
    }

    public Modal ModalGraficoDependiente { get; set; }

    public void AbrirVentanaGraficoDependiente()
    {
      if (VariableIndependienteGraficoDependiente >= 0)
      {
        if (AlCrearGraficoDependiente != null)
        {
          AlCrearGraficoDependiente(this, VariableIndependienteGraficoDependiente);
        }
        CerrarVentanaGraficoDependiente();
      }
    }

    public void CerrarVentanaGraficoDependiente()
    {
      if (ModalGraficoDependiente != null)
      {
        ModalGraficoDependiente.Hide();
      }
    }

    /// <summary>
    /// Agrega las condiciones que impone externamente al grafico.
    /// Se trata de las condiciones que impone en grafico anterior.
    /// </summary>
    /// <param name="Condiciones"></param>
    public void AjustarFiltroInicial(List<CCondiciones> Condiciones)
    {

      if (Filtrador.TieneFiltrosExternos)
      {
        Filtrador.Pasos.RemoveAt(1);
      }

      if (Condiciones.Count > 0)
      {
        CFiltradorStep Paso = new CFiltradorStep();
        Paso.AgregarCondiciones(Condiciones);
        Paso.CumplirTodas = false;
        Paso.ImpuestoExternamente = true;
        Filtrador.Pasos.Insert(1, Paso);
      }
    }

    private List<CDatosTorta> CopiarListaSeleccionados(List<CDatosTorta> Anterior)
    {
      List<CDatosTorta> Respuesta = new List<CDatosTorta>();
      foreach (CDatosTorta Dato in Anterior)
      {
        Respuesta.Add(Dato);
      }
      return Respuesta;
    }

    public void ActualizarDatosGrafico(List<CLineaComprimida> Datos)
    {
      //      GuardarSeleccion(); // guarda los elementos seleccionados.
      //      mbActualizaSuperior = true;
      try
      {
        List<CDatosTorta> SeleccionAnterior = CopiarListaSeleccionados(mListaSeleccion);
        Filtrador.Proveedor.Columnas = Proveedor.Columnas;
        Filtrador.Proveedor.Datos = Datos;
        Filtrador.AjustarDatosIniciales();
        //        RefrescarDatosDesdeProveedorLineas();
        RefrescarDatosTortaDesdeProveedor();
        StateHasChanged();
        mListaSeleccion = SeleccionAnterior;

        AjustarGraficosDependientes();

      }
      finally
      {
        //        mbActualizaSuperior = false;
      }

    }

    public void AjustarGraficosDependientes()
    {
      /*      if (Referencia == null)
            {
              mListaSeleccion.Clear(); // mientras haya datos en lista no dispara refrescos al modificarse.
            } */

      if (GraficosDependientes != null && GraficosDependientes.Count > 0) // && !mbCreandoGrafico)
      {
        List<CLineaComprimida> DatosParaDependientes =
            ObtenerDatosDependientes();
        foreach (CLogicaGrafico Graf0 in GraficosDependientes)
        {
          // Al grafico dependiente le impone el filtro externo (que sale de las condiciones actuales).
          Graf0.AjustarFiltroInicial(ExtraerCondicionesSeleccionadas());
          // Actualiza el grafico con los datos que correspondan con el filtro actualizado.
          Graf0.ActualizarDatosGrafico(Filtrador.Datos);
        }
      }
    }

    public Int32 VariableIndependienteGraficoDependiente { get; set; } = -1;

    public List<CColumnaBase> VariablesIndependientesGraficoDependiente
    {
      get
      {
        return (Filtrador == null ? new List<CColumnaBase>() : (from C in Filtrador.Proveedor.Columnas
                                                                where C.Orden != ColumnaOrdenadas.Orden &&
                                                                (Clase == ClaseGrafico.Histograma || C.Orden != ColumnaAbscisas.Orden) &&
                                                                (Clase != ClaseGrafico.Histograma || C.Clase == ClaseVariable.Real || C.Clase == ClaseVariable.Entero)
                                                                orderby C.Nombre
                                                                select C).ToList());
      }
    }

    public void AjustarHistograma()
    {
      if (!double.IsNaN(mdSaltoHistograma))
      {
        CerrarVentanaCfgHistograma();
        RefrescarDatosDesdeProveedorHistograma();
        StateHasChanged();
      }
    }

    private Int32 mCantidadHistograma = 10;
    public string SaltoHistograma
    {
      get { return (double.IsNaN(mdSaltoHistograma) ? "" : mdSaltoHistograma.ToString()); }
      set
      {
        double R;
        if (double.TryParse(value, out R))
        {
          mdSaltoHistograma = R;
        }
        else
        {
          mdSaltoHistograma = double.NaN;
        }
      }
    }

    private double mdSaltoHistograma = double.NaN;
    public void Proveedor_AlAjustarDependientes(object sender)
    {
      switch (Clase)
      {
        case ClaseGrafico.Torta:
        case ClaseGrafico.Barras:
        case ClaseGrafico.BarrasH:
        case ClaseGrafico.Histograma:
        case ClaseGrafico.Puntos:
          RefrescarDatosTortaDesdeProveedor();
          break;
      }
    }

    private double mAcumulado;

    private void PonerColoresPorciones()
    {
      List<string> Colores = CRutinas.ArmarListaColores(PorcionesTorta.Count);
      Int32 i = 0;
      foreach (CPorcionTorta Porcion in PorcionesTorta)
      {
        Porcion.ColorRelleno = Colores[i++];
      }
    }

    private double mMinimoHistograma = double.NaN;
    private double mMaximoHistograma = double.NaN;
    private double mMaximoEscalaHistograma = double.NaN;
    private List<DatosTortaColor> mDatosHistograma = new List<DatosTortaColor>();
    private List<CDatosPuntoReal> mPuntosLineas = new List<CDatosPuntoReal>();

    private void SumarDatosHistograma(List<CDatosTorta> Originales)
    {
      foreach (CDatosTorta Elemento in Originales)
      {
        Int32 Posicion = Math.Min(
              (Int32)Math.Floor((Elemento.Valor - mMinimoHistograma) / mdSaltoHistograma), mDatosHistograma.Count - 1);
        //        mDatosHistograma[Posicion].Datos.Datos.Cantidad++;
        mDatosHistograma[Posicion].Datos.Valor++;
      }
    }

    private double MaximoEscalaHistograma()
    {
      double Respuesta = 0;
      foreach (DatosTortaColor Dato in mDatosHistograma)
      {
        Respuesta = Math.Max(Respuesta, Dato.Datos.Valor);
      }
      return (Respuesta > 0 ? CRutinas.BuscarValorEscala(Respuesta / 1.25) : Respuesta);
    }

    private void ArmarDatosLineas(List<CDatosTorta> Originales)
    {

      mDatosHistograma.Clear();

      if (Originales.Count == 0)
      {
        return;
      }

      if (double.IsNaN(mMinimoHistograma))
      {
        if (!DeterminarSaltosEscalaHistograma(ColumnaOrdenadas.Orden))
        {
          return;
        }
      }

      if (double.IsNaN(mMinimoHistograma))
      {
        return;
      }

      // Crear datos limpios.
      mDatosHistograma = CFiltradorPasos.ArmarGajosHistogramaSinDatos(
          mMinimoHistograma, mMaximoHistograma, mdSaltoHistograma);

      SumarDatosHistograma(Originales);

      mMaximoEscalaHistograma = MaximoEscalaHistograma();

    }

    private void ArmarDatosHistograma(List<CDatosTorta> Originales)
    {

      mDatosHistograma = new List<DatosTortaColor>();

      if (Originales.Count == 0)
      {
        return;
      }

      if (double.IsNaN(mMinimoHistograma))
      {
        if (!DeterminarSaltosEscalaHistograma(ColumnaOrdenadas.Orden))
        {
          return;
        }
      }

      if (double.IsNaN(mMinimoHistograma))
      {
        return;
      }

      // Crear datos limpios.
      mDatosHistograma = CFiltradorPasos.ArmarGajosHistogramaSinDatos(
          mMinimoHistograma, mMaximoHistograma, mdSaltoHistograma);

      SumarDatosHistograma(Originales);

      mMaximoEscalaHistograma = MaximoEscalaHistograma();

    }

    public bool DeterminarSaltosEscalaHistograma(Int32 Orden)
    {
      mMinimoHistograma = mMaximoHistograma = 0;
      return mFiltroPasos.DeterminarRangosHistograma(Orden, ref mMinimoHistograma, ref mMaximoHistograma,
            ref mdSaltoHistograma, mdSaltoHistograma, ref mCantidadHistograma);
    }

    private void RefrescarDatosDesdeProveedorLineas(List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {
      if (CondicionesAdicionales != null)
      {
        if (CondicionesAdicionales.Count > 0)
        {
          Filtrador.InicializarProcesoCondicionesAdicionales(CondicionesAdicionales);
        }
        else
        {
          CondicionesAdicionales = null;
        }
      }

      mPuntosLineas.Clear();
      foreach (CLineaComprimida Dato in mFiltroPasos.Datos)
      {
        if (Dato.Vigente)
        {
          if (CondicionesAdicionales == null ||
              Filtrador.VerificaCondicionesFiltrador(Dato, CondicionesAdicionales))
          {
            mPuntosLineas.Add(new CDatosPuntoReal()
            {
              Abscisa = ColumnaAbscisas.ValorReal(Dato.Codigos[ColumnaAbscisas.Orden]),
              Valor = ColumnaOrdenadas.ValorReal(Dato.Codigos[ColumnaOrdenadas.Orden])
            });
          }
        }
      }

      mPuntosLineas = (from M in mPuntosLineas
                       orderby M.Abscisa
                       select M).ToList();

    }

    private void RefrescarDatosDesdeProveedorHistograma(List<CCondicionFiltradorCN> CondicionesAdicionales = null)
    {
      //Int32 Orden=mContenedor.RelojDatos().ColumnaDesdeNombre(mContenedor.NombreColumnaOrdenada).Orden;
      Int32 Orden = ColumnaOrdenadas.Orden;
      if (double.IsNaN(mMinimoHistograma) ||
          double.IsNaN(mdSaltoHistograma))
      {
        DeterminarSaltosEscalaHistograma(Orden);
      }

      List<CDatosTorta> Originales = mFiltroPasos.ArmarSerieTodosLosDatos(Orden, CondicionesAdicionales);

      if (Originales.Count == 0)
      {
        mDatosHistograma = new List<DatosTortaColor>();
      }
      else
      {
        ArmarDatosHistograma(Originales);
      }

    }

    public void RefrescarDatosTortaDesdeProveedor()
		{
      RefrescarDatosTortaDesdeProveedor(null);
		}

    public void RefrescarDatosTortaDesdeProveedor(List<CCondicionFiltradorCN> CondicionesAdicionales)
    {

      mFiltroPasos.AjustarDatosIniciales();

      if (Clase == ClaseGrafico.BarrasH)
      {
        if (mPilas == null)
        {
          mPilas = new List<ElementoPila>();
        }
        else
        {
          mPilas.Clear();
        }
        mDatosApilados = mFiltroPasos.ArmarSerieApilada(ColumnaAbscisas.Orden, ColumnaSexo.Orden,
          ColumnaOrdenadas.Orden, AgrupamientoDependiente,
          mPilas, CondicionesAdicionales);
      }
      else
      {
        if (Clase == ClaseGrafico.Histograma)
        {
          RefrescarDatosDesdeProveedorHistograma(CondicionesAdicionales);
        }
        else
        {
          if (Clase == ClaseGrafico.Puntos)
          {
            RefrescarDatosDesdeProveedorLineas(CondicionesAdicionales);
          }
          else
          {
            AjustarAgrupamiento();

            List<CDatosTorta> Originales = new List<CDatosTorta>();

            switch (AgrupamientoIndependiente)
            {
              case ModoAgruparIndependiente.Todos:
              case ModoAgruparIndependiente.Rangos:
                Originales = mFiltroPasos.ArmarSerieTodasFechas(
                    ColumnaAbscisas.Orden,
                    ColumnaOrdenadas.Orden,
                    AgrupamientoIndependiente,
                    AgrupamientoDependiente, MaxGajos,
                    CondicionesAdicionales);
                break;
              default:
                Originales = mFiltroPasos.ArmarSerieDatosPorPeriodo(
                    ColumnaAbscisas.Orden,
                    ColumnaOrdenadas.Orden,
                    AgrupamientoIndependiente,
                    AgrupamientoDependiente, MaxGajos,
                    CondicionesAdicionales);
                break;
            }

            PorcionesTorta = new List<CPorcionTorta>();

            int i = 0;
            double Parcial = 0;
            foreach (CDatosTorta Elemento in Originales)
            {
              if (double.IsNaN(Elemento.Valor))
              {
                Elemento.Valor = 0;
              }
              CPorcionTorta Porcion = new CPorcionTorta();
              Porcion.Texto = Elemento.Nombre;
              Porcion.Valor = Elemento.Valor;
              Porcion.Datos = Elemento;
              Porcion.Seleccionado = false;
              Porcion.AcumuladoAnterior = Parcial;
              Parcial += Math.Abs(Elemento.Valor);
              Porcion.Codigo = i;
              i++;
              PorcionesTorta.Add(Porcion);
            }

            mAcumulado = Parcial;

            PonerColoresPorciones();

          }
        }
      }
      StateHasChanged();

    }

    //public void RefrescarGrafico()
    //{

    //  switch (Clase)
    //  {
    //    case ClaseGrafico.Tortas:
    //      if (ColumnaAbscisas != null && ColumnaOrdenadas != null)
    //      {
    //        RefrescarTorta();
    //      }
    //      break;
    //  }
    //}

    private bool mbPorcion20Seleccionada = false;

    private void CrearTortaResto(double Parcial, double Anterior)
    {
      mPorcion20 = new CPorcionTorta();
      mPorcion20.Datos = null;
      mPorcion20.Texto = CRutinas.CTE_OTROS;
      mPorcion20.Valor = Parcial;
      mPorcion20.AcumuladoAnterior = Anterior;
      mPorcion20.ColorRelleno = "lightgray";
      mPorcion20.Seleccionado = mbPorcion20Seleccionada;
    }

    private async Task DibujarTortaEnCanvasAsync()
    {

      mPorcion20 = null;

      Int32 CantGajosMaximos = CantidadElementosPorDibujoLabels();

      Int32 YaDibujadas = 0;
      double HastaAhora = 0;
      mPorcionesDibujadas.Clear();
      foreach (CPorcionTorta Porcion in PorcionesTorta)
      {
        YaDibujadas++;
        if (YaDibujadas >= CantGajosMaximos && YaDibujadas < PorcionesTorta.Count)
        {
          // Armar mPorcion20.
          CrearTortaResto(mAcumulado - HastaAhora, HastaAhora);
          await mPorcion20.DibujarSobreCanvasAsync(AnchoGraficoTotal, AltoGraficoTotal, mAcumulado, mContexto);
          mPorcionesDibujadas.Add(mPorcion20);
          break;
        }
        else
        {
          await Porcion.DibujarSobreCanvasAsync(AnchoGraficoTotal, AltoGraficoTotal, mAcumulado, mContexto);
          HastaAhora += Porcion.Valor;
          mPorcionesDibujadas.Add(Porcion);
        }
      }

      await AgregarLabelsTortaAsync(Math.Min(CantGajosMaximos, PorcionesTorta.Count));

    }

    private Int32 mLabelsDer;
    private Int32 mLabelsIzq;
    private Int32 mCantLabels;

    private double AltoLabel
    {
      get { return 2 * mDimensionCaracter + 6; }
    }

    private Int32 CantidadElementosPorDibujoLabels()
    {

      mCantLabels = (Int32)Math.Floor(AltoGraficoTotal / AltoLabel);

      mLabelsDer = 0;
      mLabelsIzq = 0;
      foreach (CPorcionTorta Porcion in PorcionesTorta)
      {
        if (Porcion.AnguloCentral(mAcumulado) <= Math.PI)
        {
          mLabelsDer++;
        }
        else
        {
          mLabelsIzq++;
        }
      }

      mLabelsDer = Math.Min(mLabelsDer, mCantLabels);
      mLabelsIzq = Math.Min(mLabelsIzq, mCantLabels);

      // mCantLabels es la cantidad que entran en una vertical.
      return mLabelsDer + mLabelsIzq;

    }

    private double mAnchoLabelsIzq = -1;

    public const double FRACCION_REFERENCIAS = 0.75; // 0.5625;
    private async Task GraficarReferenciaAsync(bool Derecha, Int32 Posicion, Int32 Cantidad, CPorcionTorta Porcion,
          double AltoLinea, double Dimension, double AltoTotal, double AnchoTextoMaxIzq)
    {
      double AnguloMedio = Porcion.AnguloCentral(mAcumulado);
      double Separacion = Math.Max(0, (AltoTotal - Cantidad * AltoLinea) / (Cantidad + 1));
      double Ordenada = Separacion + (Derecha ? Posicion : (mLabelsIzq - Posicion)) * (Separacion + AltoLinea) + mDimensionCaracter;
      double Abscisa = (Derecha ? (AnchoGraficoTotal / 2 + Dimension) :
          Math.Max(AnchoGraficoTotal / 2 - Dimension - AnchoTextoMaxIzq, 0));
      // Evita poner sobre los botones.

      string Texto = Porcion.Texto;
      string Texto2 = CRutinas.ValorATexto(Porcion.Valor) +
            " - " + string.Format("{0:0.00}", Porcion.Valor * 100 / mAcumulado) + " %";


      double AbscLinea;
      double AnchoTexto = 0;

      await mContexto.SetFillStyleAsync("black");

      if (Derecha)
      {
        await mContexto.FillTextAsync(Texto, Abscisa, Ordenada);
        await mContexto.FillTextAsync(Texto2, Abscisa, Ordenada + mDimensionCaracter + 2);
        AbscLinea = Abscisa - 5;
      }
      else
      {
        TextMetrics MedidasMin = await mContexto.MeasureTextAsync(Texto);
        // Admite como ancho maximo
        AbscLinea = AnchoGraficoTotal / 2 - Dimension - Abscisa;
        if (AbscLinea < MedidasMin.Width)
        {
          // Elimina caracteres que sobran (aproximadamente).
          Texto = Texto.Substring(0, (Int32)Math.Truncate(Texto.Length * AbscLinea / MedidasMin.Width));
        }

        AnchoTexto = Math.Min(MedidasMin.Width, AbscLinea);

        await mContexto.FillTextAsync(Texto, Abscisa, Ordenada, AnchoTexto);
        await mContexto.FillTextAsync(Texto2, Abscisa, Ordenada + mDimensionCaracter + 2);

        AbscLinea = AnchoGraficoTotal / 2 - Dimension;

      }

      await mContexto.BeginPathAsync();
      await mContexto.MoveToAsync(AnchoGraficoTotal / 2 + 0.77 * Dimension * Math.Sin(AnguloMedio),
        AltoGraficoTotal / 2 - 0.77 * Dimension * Math.Cos(AnguloMedio));
      await mContexto.LineToAsync(AbscLinea, Ordenada);
      //await mContexto.LineToAsync(AnchoGrafico / 2 + 0.80 * Dimension * Math.Sin(AnguloMedio),
      //  AltoGraficoTotal / 2 - 0.80 * Dimension * Math.Cos(AnguloMedio));
      if (Derecha)
      {
        await mContexto.LineToAsync(Abscisa, Ordenada);
      }
      else
      {
        if (AbscLinea > AnchoTexto)
        {
          await mContexto.LineToAsync(Abscisa + AnchoTexto, Ordenada);
        }
      }
      await mContexto.SetLineWidthAsync(1);
      await mContexto.SetStrokeStyleAsync("gray");
      await mContexto.StrokeAsync();
      await mContexto.ClosePathAsync();

    }

    private double mDimensionCaracter = 8;

    private string FmtTexto
    {
      get
      {
        return (AnchoGrafico > 500 && AltoGrafico > 400 ? "12" : "10") + "px serif";
      }
    }

    private async Task AgregarLabelsTortaAsync(Int32 CantGajosMaxima)
    {
      Int32 YaDibujadas = 0;

      if (PorcionesTorta != null && PorcionesTorta.Count > 0)
      {
        await mContexto.SetFontAsync(FmtTexto);
        // Determina ancho necesario izq.
        if (mAnchoLabelsIzq < 0)
        {

          mAnchoLabelsIzq = 0;
          for (Int32 i = mLabelsDer; i < (CantGajosMaxima - (mPorcion20 == null ? 0 : 1)); i++)
          {
            TextMetrics Medida = await mContexto.MeasureTextAsync(PorcionesTorta[i].Texto);
            mAnchoLabelsIzq = Math.Max(mAnchoLabelsIzq, Medida.Width);
          }
          if (mPorcion20 != null)
          {
            TextMetrics Medida = await mContexto.MeasureTextAsync(mPorcion20.Texto);
            mAnchoLabelsIzq = Math.Max(mAnchoLabelsIzq, Medida.Width);
          }
        }
        // mCantDibujadas es la cantidad de gajos dibujados.
        mLabelsDer = Math.Min(PorcionesTorta.Count, Math.Min(mLabelsDer, mCantLabels));
        double Dimension = Math.Min(AltoGraficoTotal, AnchoGraficoTotal) * 0.4; // 80% de la mitad.
        double AltoTotal = AltoGraficoTotal;
        double Alto = AltoLabel;
        Int32 CantGajosADibujar = CantGajosMaxima - (mPorcion20 == null ? 0 : 1);

        foreach (CPorcionTorta Porcion in PorcionesTorta)
        {
          YaDibujadas++;
          if (YaDibujadas <= CantGajosADibujar)
          {
            if (YaDibujadas <= mLabelsDer)
            {
              await GraficarReferenciaAsync(true, YaDibujadas - 1, mLabelsDer, Porcion, Alto,
                  Dimension, AltoTotal, mAnchoLabelsIzq);
            }
            else
            {
              await GraficarReferenciaAsync(false, YaDibujadas - mLabelsDer,
                  mLabelsIzq, Porcion, Alto, Dimension, AltoTotal, mAnchoLabelsIzq);
            }
          }
          else
          {
            break;
          }
        }

        if (mPorcion20 != null)
        {
          await GraficarReferenciaAsync(false, mLabelsIzq,
              mLabelsIzq, mPorcion20, Alto, Dimension, AltoTotal, mAnchoLabelsIzq);
        }
      }
    }

    private object OBJ_LOCK = new object();
    private bool mbGraficando = false;

    private List<CPorcionTorta> ExtraerPorcionesBarras()
    {
      List<CPorcionTorta> Respuesta = new List<CPorcionTorta>();
      if (PorcionesTorta.Count > MaxGajos)
      {
        for (Int32 i = 0; i < MaxGajos; i++)
        {
          Respuesta.Add(PorcionesTorta[i]);
          Respuesta.Add(mPorcion20);
        }
      }
      else
      {
        Respuesta.AddRange(PorcionesTorta);
      }
      return Respuesta;
    }

    private bool AjustarDimensionesBarrasApiladas()
    {
      AltoGraficoTotal = (long)Alto - 32;
      bool Respuesta = false;
      if (Detallado)
      {
        long AltoCanvasNecesario = (long)Math.Max(AltoGraficoTotal, CGraficoBarrasApiladas.AltoNecesario(mDatosApilados));
        if (AltoCanvasNecesario != AltoCanvas)
        {
          AltoCanvas = AltoCanvasNecesario;
          Respuesta = true;
        }
        if (AnchoGraficoApilado != ((long)Ancho - 1))
        {
          AnchoGraficoApilado = (long)Ancho - 1;
          Respuesta = true;
        }
        AltoCanvasDer = 0;
        AnchoGraficoApiladoDer = 0;
      }
      else
      {
        long AltoCanvasNecesario = (long)Math.Max(AltoGraficoTotal, CGraficoBarrasApiladas.AltoNecesario(mDatosApilados.Count));
        long Antes = AltoCanvas;
        if (AltoCanvasNecesario != AltoCanvas)
        {
          AltoCanvas = AltoCanvasNecesario;
        }
        Respuesta = (AltoCanvas != Antes);

        long AltoCanvasNecesarioDer = (long)Math.Max(AltoGraficoTotal, CGraficoBarrasApiladas.AltoNecesario(mPilas.Count));
        Antes = AltoCanvasDer;
        if (AltoCanvasNecesarioDer != AltoCanvasDer)
        {
          AltoCanvasDer = AltoCanvasNecesarioDer;
        }
        Respuesta |= (AltoCanvasDer != Antes);

        Antes = AnchoGraficoApilado;
        if (AnchoGraficoApilado != (67 * (long)Ancho / 100 - 1))
        {
          AnchoGraficoApilado = 67 * (long)Ancho / 100 - 1;
        }
        Respuesta |= (AnchoGraficoApilado != Antes);

        Antes = AnchoGraficoApiladoDer;
        if (AnchoGraficoApiladoDer != (33 * (long)Ancho / 100 - 1))
        {
          AnchoGraficoApiladoDer = 33 * (long)Ancho / 100 - 1;
        }
        Respuesta |= (AnchoGraficoApiladoDer != Antes);
      }

      return Respuesta;

    }

    private async Task DibujarBarrasApiladasAsync()
    {
      if (AjustarDimensionesBarrasApiladas())
      {
        StateHasChanged();
      }
      else
      {
        mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGrafico);
        await mContexto.BeginBatchAsync();

        try
        {

          await mContexto.ClearRectAsync(0, 0, AnchoGrafico, AltoGrafico);
          await mContexto.SetFillStyleAsync("white");
          await mContexto.FillRectAsync(0, 0, AnchoGrafico, AltoGrafico);
          CGraficoBarrasApiladas GrBarrasApi = new CGraficoBarrasApiladas();

          if (CanvasGraficoDer != null)
          {
            mContextoDer = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGraficoDer);
            CGraficoBarrasApiladas GrApiladas = new CGraficoBarrasApiladas();
            await GrApiladas.HacerGraficoBarrasAsync(mContexto, mContextoDer, Ancho, AltoGraficoTotal,
                mDatosApilados, mPilas, Detallado);
          }
        }
        finally
        {
          await mContexto.EndBatchAsync();
        }
      }
    }

    public void AjustarDetallado()
    {
      Detallado = !Detallado;
      StateHasChanged();
    }

    public static string NombreClaseGrafico(ClaseGrafico Clase)
    {
      switch (Clase)
      {
        case ClaseGrafico.Barras: return "Barras";
        case ClaseGrafico.BarrasH: return "Barras múltiples";
        case ClaseGrafico.Grilla: return "Grilla";
        case ClaseGrafico.Histograma: return "Histograma";
        case ClaseGrafico.Mimico: return "Mímico";
        case ClaseGrafico.Pareto: return "Pareto";
        case ClaseGrafico.Piramide: return "Pirámide";
        case ClaseGrafico.Puntos: return "Puntos";
        case ClaseGrafico.SobreGIS: return "GIS";
        case ClaseGrafico.Torta: return "Torta";
        default: return "No definido";
      }
    }

    private CGrafV2DatosContenedorBlock ObtenerGrafV2()
    {
      List<CFiltradorStep> Pasos = new List<CFiltradorStep>();

      CLogicaGrafico Anterior = Superior;
      while (Anterior != null)
      {
        CFiltradorStep Paso = new CFiltradorStep();
        Paso.AgregarCondiciones(Anterior.ExtraerCondicionesSeleccionadas());
        Paso.CumplirTodas = false;
        Pasos.Insert(0, Paso);
      }

      // Armar datos y poner en lista.
      CGrafV2DatosContenedorBlock Datos = new CGrafV2DatosContenedorBlock();

      Datos.ClaseOrigen = (Pagina.PeriodoEnProceso < 0 ? ClaseElemento.SubConsulta : ClaseElemento.Indicador);
      Datos.Indicador = Pagina.Indicador.Codigo;
      Datos.CodigoDimension = Pagina.Indicador.Dimension;
      Datos.CodigoElementoDimension = Pagina.CodigoElementoDimension;
      Datos.Nombre = Pagina.NombreIndicador;
      Datos.AgrupIndep = AgrupamientoIndependiente;
      Datos.Agrupacion = AgrupamientoDependiente;
      Datos.Clase = Clase; // (Clase == ClaseGrafico.Barras && BarrasH ? ClaseGrafico.BarrasH : Clase);
      Datos.ClaseElemento = CGrafV2DatosContenedorBlock.ClaseBlock.Grafico;
      Datos.ColumnaAbscisas = (ColumnaAbscisas == null ? "" : ColumnaAbscisas.Nombre);
      Datos.ColumnaOrdenadas = (ColumnaOrdenadas == null ? "" : ColumnaOrdenadas.Nombre);
      Datos.ColumnaSexo = (ColumnaSexo == null ? "" : ColumnaSexo.Nombre);
      Datos.SaltoHistograma = mdSaltoHistograma;

      Datos.FiltrosBlock.AddRange(Proveedor.ObtenerFiltrosBlockDesdeAsociaciones());

      foreach (CFiltradorStep Paso in Pasos)
      {
        CPasoCondicionesBlock PasoLocal = new CPasoCondicionesBlock();
        PasoLocal.CumplirTodas = Paso.CumplirTodas;
        foreach (CCondiciones Cnd in Paso.Condiciones)
        {
          CGrupoCondicionesBlock Grupo = new CGrupoCondicionesBlock();
          Grupo.CumplirTodas = Cnd.TodasLasCondiciones;
          Grupo.Incluye = Cnd.IncluyeCondiciones;
          foreach (CCondicion CndElemental in Cnd.Condiciones)
          {
            CCondicionBlock CndLocal = new CCondicionBlock();
            CndLocal.Columna = Proveedor.Columnas[CndElemental.ColumnaCondicion].Nombre;
            CndLocal.Modo = CndElemental.Modo;
            switch (CndLocal.Modo)
            {
              case ModoFiltrar.PorRango:
                CndLocal.Valor = CRutinas.FloatVStr(CndElemental.ValorMinimo);
                CndLocal.ValorMaximo = CRutinas.FloatVStr(CndElemental.ValorMaximo);
                break;
              default:
                CndLocal.Valor = CndElemental.ValorIgual;
                break;
            }
            Grupo.Condiciones.Add(CndLocal);
          }
          PasoLocal.Grupos.Add(Grupo);
        }
        Datos.FiltrosBlock.Add(PasoLocal);
      }

      return Datos;

    }

    public void GuardarXML()
    {
      Contenedores.CContenedorDatos.gElementosXML.Add(
          new CElementoXML()
          {
            Clase = CClaseElementoXML.Grafico,
            Nombre = NombreClaseGrafico(Clase) + " " + Pagina.NombreIndicador +
              " <" + ColumnaOrdenadas.Nombre +
              (ColumnaAbscisas != null ? ("/" + ColumnaAbscisas.Nombre) : "") +
              (ColumnaSexo != null ? ("/" + ColumnaSexo.Nombre) : "") + ">",
            GrafV2 = ObtenerGrafV2()
          });
    }

    private void AjustarSeleccionadoPorciones()
    {
      if (ValoresSeleccionados == null)
      {
        ValoresSeleccionados = new List<string>();
      }
      foreach (CPorcionTorta Porcion in mPorciones)
      {
        Porcion.Seleccionado = ValoresSeleccionados.Contains(Porcion.Datos.Nombre);
      }
    }

    private List<CPorcionTorta> mPorcionesBarras;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

      bool bRedibujar = false;

      while (true)
      {
        lock (OBJ_LOCK)
        {
          if (mbGraficando)
          {
            return; // System.Threading.Thread.Sleep(100);
          }
          else
          {
            mbGraficando = true;
            break;
          }
        }
      }

      try
      {

        if (PorcionesTorta == null && mDatosApilados == null && mDatosHistograma == null)
        {
          RefrescarDatosTortaDesdeProveedor();
          bRedibujar = true;
          return;
        }
        else
        {

          object[] Args = new object[1];
          Args[0] = "IDGrafico" + CodigoUnico.ToString();
          string Dimensiones = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
          List<double> Valores = CRutinas.ListaAReales(Dimensiones);
          Ancho = Valores[2];
          Alto = Valores[3];
          Link.Ancho = (long)Ancho;
          Link.Alto = (long)Alto;

          if (Clase == ClaseGrafico.BarrasH)
          {
            await DibujarBarrasApiladasAsync();
          }
          else
          {
            if (CanvasGrafico != null)
            {
              if (CanvasGrafico.Height != AltoGrafico)
              {
                bRedibujar = true;
                return;
              }
              try
              {

                mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGrafico);

                await mContexto.BeginBatchAsync();

                try
                {

                  await mContexto.ClearRectAsync(0, 0, AnchoGrafico, AltoGrafico);
                  await mContexto.SetFillStyleAsync("white");
                  await mContexto.FillRectAsync(0, 0, AnchoGrafico, AltoGrafico);

                  AjustarDimensionesGrafico();

                  await mContexto.SetFontAsync(FmtTexto);
                  TextMetrics Medida = await mContexto.MeasureTextAsync("H");
                  mDimensionCaracter = Medida.Width + 2;

                  switch (Clase)
                  {
                    case ClaseGrafico.Torta:
                      AjustarSeleccionadoPorciones();
                      await DibujarTortaEnCanvasAsync();
                      break;
                    case ClaseGrafico.Barras:
                      CGraficoBarras GrBarras = new CGraficoBarras();
                      mPorcionesBarras = ExtraerPorcionesBarras();
                      long AnchoCanvasNecesario = await GrBarras.DeterminarAnchoNecesarioAsync(mContexto, mPorcionesBarras, AltoGraficoTotal);
                      if (AnchoCanvasNecesario > AnchoGrafico)
                      {
                        AnchoGrafico = AnchoCanvasNecesario;
                        bRedibujar = true;
                        return;
                      }
                      AnchoGrafico = Math.Max(AnchoCanvasNecesario, (long)Ancho);
                      if (AnchoGrafico > (Ancho - 4))
                      {
                        AltoGraficoTotal = (long)Alto - 47;
                      }
                      else
                      {
                        AltoGraficoTotal = (long)Alto - 32;
                      }
                      AnchoCanvas = AnchoGrafico;
                      await GrBarras.HacerGraficoBarrasAsync(mContexto, Ancho, AltoGraficoTotal);
                      break;
                    case ClaseGrafico.Histograma:
                      CGraficoHistograma GrHisto = new CGraficoHistograma();
                      long AnchoCanvasNecesarioH = await GrHisto.DeterminarAnchoNecesarioAsync(mContexto, mDatosHistograma,
                          AnchoGraficoTotal, AltoGraficoTotal);
                      if (AnchoCanvasNecesarioH > AnchoGrafico)
											{
                        AnchoGrafico = AnchoCanvasNecesarioH;
                        bRedibujar = true;
                        return;
											}
                      AnchoGrafico = Math.Max(AnchoCanvasNecesarioH, (long)Ancho);
                      if (AnchoGrafico > (Ancho - 4))
                      {
                        AltoGraficoTotal = (long)Alto - 47;
                      }
                      else
                      {
                        AltoGraficoTotal = (long)Alto - 32;
                      }
                      AnchoCanvas = AnchoGrafico;
                      await GrHisto.HacerGraficoHistogramaAsync(mContexto, Ancho, AltoGraficoTotal);
                      break;
                    case ClaseGrafico.Puntos:
                      CGraficoLineas GrLineas = new CGraficoLineas();
                      long AnchoCanvasNecesarioL = await GrLineas.DeterminarAnchoNecesarioAsync(mContexto, mPuntosLineas,
                          AnchoGraficoTotal, AltoGraficoTotal);
                      if (AnchoCanvasNecesarioL > AnchoGrafico)
                      {
                        AnchoGrafico = AnchoCanvasNecesarioL;
                        bRedibujar = true;
                        return;
                      }
                      AnchoGrafico = Math.Max(AnchoCanvasNecesarioL, (long)Ancho);
                      if (AnchoGrafico > (Ancho - 4))
                      {
                        AltoGraficoTotal = (long)Alto - 47;
                      }
                      else
                      {
                        AltoGraficoTotal = (long)Alto - 32;
                      }
                      AnchoCanvas = AnchoGrafico;
                      await GrLineas.HacerGraficoLineasAsync(mContexto, Ancho, AltoGraficoTotal);
                      break;
                  }
                }
                finally
                {
                  await mContexto.EndBatchAsync();
                  if (firstRender && Superior != null && EvSeleccionarGrafico != null)
                  {
                    EvSeleccionarGrafico(this);
                  }
                }

              }
              catch (Exception ex)
              {
                CRutinas.DesplegarMsg(ex);
              }
            }
          }
        }
      }
      finally
      {
//        await base.OnAfterRenderAsync(firstRender);
        mbGraficando = false;
        if (bRedibujar)
				{
          StateHasChanged();
				}
      }
    }

  }

  //public class CPorcionTorta
  //{
  //  public bool Seleccionado;
  //  public Int32 Codigo;
  //  public double AcumuladoAnterior;
  //  public double Valor;
  //  public string Texto;
  //  public string Texto2;
  //  public string ColorRelleno;
  //  public CDatosTorta Datos;

  //  public string Denominacion
  //  {
  //    get
  //    {
  //      return Texto;
  //    }
  //  }

  //  public async Task DibujarSobreCanvasAsync(double AnchoTotal, double AltoTotal, double AcumuladoTotal, Canvas2DContext Contexto)
  //  {
  //    double AbscCentro = AnchoTotal / 2;
  //    double OrdCentro = AltoTotal / 2;
  //    double Tamanio = CLogicaGrafico.FRACCION_REFERENCIAS * Math.Min(AbscCentro, OrdCentro);

  //    if (Seleccionado)
  //    {
  //      double AnguloMedio = 2 * Math.PI * (AcumuladoAnterior + Math.Abs(Valor) / 2) /
  //          AcumuladoTotal;
  //      AbscCentro += Tamanio * Math.Sin(AnguloMedio) / 4;
  //      OrdCentro -= Tamanio * Math.Cos(AnguloMedio) / 4;
  //    }

  //    await CrearSectorAsync(AbscCentro, OrdCentro, Tamanio, AcumuladoTotal, Contexto);

  //  }

  //  private double AnguloInicial(double AcumuladoTotal)
  //  {
  //    return 2 * Math.PI * AcumuladoAnterior / AcumuladoTotal;
  //  }

  //  public double AnguloCentral(double AcumuladoTotal)
  //  {
  //    //return 2 * Math.PI * (AcumuladoAnterior + Valor / 2) /
  //    //      Superior.Acumulado;
  //    return 2 * Math.PI * AcumuladoAnterior / AcumuladoTotal +
  //        Math.PI * Math.Abs(Valor) / AcumuladoTotal;
  //  }

  //  public CPunto PuntoContornoEtiqueta(double AcumuladoTotal, double Ancho, double Alto)
  //  {
  //    double AngCentro = AnguloCentral(AcumuladoTotal);
  //    double AbscCentro = Ancho / 2;
  //    double OrdCentro = Alto / 2;
  //    double Tamanio = 0.85 * Math.Min(AbscCentro, OrdCentro);

  //    return new CPunto(AbscCentro + Tamanio * Math.Sin(AngCentro),
  //        OrdCentro - Tamanio * Math.Cos(AngCentro));
  //  }

  //  private CPunto PuntoDesplazado(double AbscCentro, double OrdCentro, double Angulo, double Distancia)
  //  {
  //    return new CPunto(AbscCentro + Math.Sin(Angulo) * Distancia,
  //        OrdCentro - Math.Cos(Angulo) * Distancia);
  //  }

  //  public void ImponerContornoRectangular(double Abscisa, double Ordenada, double Ancho, double Alto)
  //  {
  //    Contorno.Clear();
  //    Contorno.Add(new CPunto(Abscisa, Ordenada));
  //    Contorno.Add(new CPunto(Abscisa + Ancho, Ordenada));
  //    Contorno.Add(new CPunto(Abscisa + Ancho, Ordenada + Alto));
  //    Contorno.Add(new CPunto(Abscisa, Ordenada + Alto));
  //  }

  //  //public double AnguloCentro()
  //  //{
  //  //  return 2 * Math.PI * AcumuladoAnterior /
  //  //      Superior.Acumulado +
  //  //      ((Valor == Superior.Acumulado ?
  //  //      2 * Math.PI - 0.0001 :
  //  //      2 * Math.PI * (AcumuladoAnterior + Valor) /
  //  //      Superior.Acumulado)) / 2;
  //  //}

  //  private List<CPunto> Contorno = new List<CPunto>();

  //  private CPunto ObtenerPuntoArco(double Angulo, double AbscC, double OrdC, double Tamanio)
  //  {
  //    return new CPunto(AbscC + Tamanio * Math.Sin(Angulo), OrdC - Tamanio * Math.Cos(Angulo));
  //  }

  //  private async Task CrearSectorAsync(double AbscCentro, double OrdCentro,
  //        double Tamanio, double AcumuladoTotal, Canvas2DContext Contexto)
  //  {

  //    Contorno.Clear();

  //    double AngIni = AnguloInicial(AcumuladoTotal);

  //    await Contexto.BeginPathAsync();
  //    await Contexto.SetLineWidthAsync(1);
  //    await Contexto.SetStrokeStyleAsync("black");
  //    float[] Saltos = new float[0];
  //    await Contexto.SetLineDashAsync(Saltos);
  //    await Contexto.SetFillStyleAsync(ColorRelleno);

  //    CPunto Inicio = PuntoDesplazado(AbscCentro, OrdCentro, AngIni, Tamanio / 2);
  //    await Contexto.MoveToAsync(Inicio.Abscisa, Inicio.Ordenada);
  //    Contorno.Add(Inicio);

  //    Inicio = PuntoDesplazado(AbscCentro, OrdCentro, AngIni, Tamanio);
  //    await Contexto.LineToAsync(Inicio.Abscisa, Inicio.Ordenada);
  //    Contorno.Add(Inicio);

  //    double AnguloH = (Math.Abs(Valor + AcumuladoAnterior) >= AcumuladoTotal ?
  //      2 * Math.PI - 0.0001 :
  //      2 * Math.PI * Math.Abs(AcumuladoAnterior + Valor) / AcumuladoTotal);

  //    await Contexto.ArcAsync(AbscCentro, OrdCentro, Tamanio, CRutinas.PonerEnRango(AngIni - Math.PI / 2),
  //      CRutinas.PonerEnRango(AnguloH - Math.PI / 2), false);
  //    double AnguloMax = (AngIni > AnguloH ? (AnguloH + 2 * Math.PI) : AnguloH);
  //    for (Int32 i = 1; i <= 5; i++)
  //    {
  //      Contorno.Add(ObtenerPuntoArco(AngIni + (AnguloMax - AngIni) * i / 5, AbscCentro, OrdCentro, Tamanio));
  //    }

  //    Inicio = PuntoDesplazado(AbscCentro, OrdCentro, AnguloH, Tamanio / 2);
  //    await Contexto.LineToAsync(Inicio.Abscisa, Inicio.Ordenada);
  //    Contorno.Add(Inicio);

  //    await Contexto.ArcAsync(AbscCentro, OrdCentro, Tamanio / 2, CRutinas.PonerEnRango(AnguloH - Math.PI / 2),
  //      CRutinas.PonerEnRango(AngIni - Math.PI / 2), true);
  //    AnguloMax -= 2 * Math.PI;
  //    for (Int32 i = 1; i <= 5; i++)
  //    {
  //      Contorno.Add(ObtenerPuntoArco(AnguloMax + (AngIni - AnguloMax) * i / 5, AbscCentro, OrdCentro, Tamanio / 2));
  //    }

  //    await Contexto.StrokeAsync();
  //    await Contexto.FillAsync();

  //    await Contexto.ClosePathAsync();

  //  }

  //  public bool PuntoEncima(double Abscisa, double Ordenada)
  //  {
  //    // Usa el contorno.
  //    Int32 Cantidad = 0;
  //    for (Int32 i = 0; i < Contorno.Count; i++)
  //    {
  //      Int32 Otro = (i == 0 ? Contorno.Count - 1 : i - 1);
  //      if (((Ordenada - Contorno[i].Ordenada) * (Ordenada - Contorno[Otro].Ordenada)) <= 0)
  //      {
  //        if (Ordenada == Contorno[i].Ordenada)
  //        {
  //          if (Abscisa >= Contorno[i].Abscisa || Abscisa >= Contorno[Otro].Abscisa)
  //          {
  //            Cantidad++;
  //          }
  //        }
  //        else
  //        {
  //          double AbscInterseccion = Contorno[Otro].Abscisa + (Contorno[i].Abscisa - Contorno[Otro].Abscisa) *
  //              (Ordenada - Contorno[Otro].Ordenada) / (Contorno[i].Ordenada - Contorno[Otro].Ordenada);
  //          if (AbscInterseccion <= Abscisa)
  //          {
  //            Cantidad++;
  //          }
  //        }
  //      }
  //    }
  //    return ((Cantidad % 2) != 0);
  //  }

  //  public void Seleccionar()
  //  {
  //    Seleccionado = true;
  //  }

  //}

  public class Rectangulo
  {
    public double height { get; set; }
    public double left { get; set; }
    public double top { get; set; }
    public double width { get; set; }

    public Rectangulo(string Texto)
    {
      string[] Elementos = Texto.Split(new char[] { ';' });
      left = CRutinas.StrVFloat(Elementos[0]);
      top = CRutinas.StrVFloat(Elementos[1]);
      width = CRutinas.StrVFloat(Elementos[2]);
      height = CRutinas.StrVFloat(Elementos[3]);
    }
  }


}
