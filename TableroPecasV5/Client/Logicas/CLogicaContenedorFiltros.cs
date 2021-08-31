using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Blazorise;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaContenedorFiltros : CBaseGrafico, IDisposable
  {

    private List<CLinkFiltros> mLinks;
    private List<CLinkFiltros> mLinksAnteriores = null;
    private CLinkGrilla mGrilla = null;

    public delegate void FncAjustarGraficos();

    public event FncAjustarGraficos EvAjustarGraficosDependientes;
    public event FncSeleccionarGrafico EvSeleccionarGrafico;

    private List<CLinkGrafico> mGraficos;

    [Parameter]
    public List<LineaFiltro> Lineas { get; set; }

    private CProveedorComprimido mProveedor = null;

    public event Rutinas.CRutinas.FncRefrescar AlAjustarVentanasFlotantes;

    [CascadingParameter]
    Logicas.CLogicaIndicador Pagina { get; set; }

    private Plantillas.CContenedorBlocks mBlocks = null;

    [CascadingParameter]
    public Plantillas.CContenedorBlocks Blocks
    {
      get { return mBlocks; }
      set
      {
        if (mBlocks != value)
        {
          if (mBlocks != null)
          {
            AlAjustarVentanasFlotantes -= mBlocks.Refrescar;
          }
          mBlocks = value;
          if (mBlocks != null)
          {
            AlAjustarVentanasFlotantes += mBlocks.Refrescar;
          }
        }
      }
    }


    public void AbrirGISPinsLL()
    {
      OrdenValor = -1;
      OrdenLat = -1;
      OrdenLng = -1;
      Agrupados = true;
      CapaPins = true;
      ModalPinsLL.Show();
    }

    public void AbrirTortasLL()
    {
      OrdenValor = -1;
      OrdenLat = -1;
      OrdenLng = -1;
      OrdenAgrupador = -1;
      CapaPins = false;
      ModalPinsLL.Show();
    }

    public void AbrirTortasManual()
    {
      OrdenValor = -1;
      OrdenAgrupador = -1;
      OrdenPosicionador = -1;
      mSolicitud = DatosSolicitados.TortasManual;
      ModalTortasGIS.Show();
    }

    public void AbrirTortasCapa()
    {
      OrdenValor = -1;
      OrdenAgrupador = -1;
      OrdenPosicionador = -1;
      mSolicitud = DatosSolicitados.TortasGIS;
      ModalTortasGIS.Show();
    }

    [Inject]
    public NavigationManager Navegador { get; set; }

    public void AbrirGISWSS()
    {
      Pagina.AbrirWSS();
      //CLogicaBingWSS.gColumnas = Proveedor.Columnas;
      //CLogicaBingWSS.gLineas = Proveedor.DatosVigentes;
      //CLogicaBingWSS.gClaseElemento = Pagina.ClaseOrigen;
      //CLogicaBingWSS.gCodigoElemento = Pagina.Codigo;
      //CLogicaBingWSS.gCodigoElementoDimension = Pagina.CodigoElementoDimension;
      //Navegador.NavigateTo("PagBingWSS");
    }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    public CLogicaContenedorFiltros()
    {
      mLinks = new List<CLinkFiltros>();
      mGraficos = new List<CLinkGrafico>();
      Abscisa = 15;
    }

		protected override async Task OnInitializedAsync()
		{
      await Rutinas.CRutinas.AsegurarDimensionFuenteAsync(JSRuntime, "Microsoft Sans Serif", 11);
      await base.OnInitializedAsync();
		}

		public string OpcionCantidad
    {
      get { return (EsPuntos ? "Puntos" : "Cantidad"); }
    }

    public CLinkGrilla Grilla
    {
      get
      {
        return mGrilla;
      }
      set { mGrilla = value; }
    }

    public List<CLinkFiltros> Links
    {
      get { return mLinks; }
      set { mLinks = value; }
    }

    public List<CLinkGrafico> Graficos
    {
      get { return mGraficos; }
      set { mGraficos = value; }
    }

    public Modal ModalCrearGrafico { get; set; }

    public Modal ModalPinsLL { get; set; }
    public Modal ModalTortasGIS { get; set; }

    public List<CLinkFiltros> LinksAnteriores
    {
      get { return mLinksAnteriores; }
      set { mLinksAnteriores = value; }
    }

    public void Dispose()
    {
      if (mProveedor != null)
      {
        mProveedor.AlAjustarDependientes -= MProveedor_AlAjustarDependientes;
      }
    }

    public void CerrarVentanaGrafico()
    {
      if (ModalCrearGrafico != null)
      {
        ModalCrearGrafico.Hide();
      }
      if (ModalPinsLL != null)
      {
        ModalPinsLL.Hide();
      }
      if (ModalTortasGIS != null)
      {
        ModalTortasGIS.Hide();
      }
    }

    public void AgregarPinsLL()
    {
      Contenedor.ColumnaDatosTorta = Proveedor.Columnas[OrdenValor];
      Contenedor.ColumnaLatTorta = Proveedor.Columnas[OrdenLat];
      Contenedor.ColumnaLngTorta = Proveedor.Columnas[OrdenLng];
      if (mLinks != null && mLinks.Count > 0)
      {
        Contenedor.LineasTorta = mLinks[0].Filtrador.DatosFiltrados;
      }
      else
      {
        Contenedor.LineasTorta = Proveedor.Datos;
      }

      if (CapaPins)
      {
        Contenedor.ColumnaAgrupadoraTorta = (OrdenAgrupador >= 0 ? Proveedor.Columnas[OrdenAgrupador] : null);
        Contenedor.PinesAgrupados = Agrupados;
        Contenedor.AbrirPinsLL();
      }
      else
      {
        Contenedor.ColumnaAgrupadoraTorta = Proveedor.Columnas[OrdenAgrupador];
        Contenedor.SolicitudTorta = DatosSolicitados.TortasLL;
        Contenedor.AbrirTortasGIS();
      }
      CerrarVentanaGrafico();
    }

    public void AgregarTortasGIS()
    {
      CerrarVentanaGrafico();
      if (mLinks != null && mLinks.Count > 0)
      {
        Pagina.LineasTorta = mLinks[0].Filtrador.DatosFiltrados;
      }
      else
      {
        Pagina.LineasTorta = Proveedor.Datos;
      }
      Pagina.ColumnaDatosTorta = Proveedor.Columnas[OrdenValor];
      Pagina.ColumnaAgrupadoraTorta = Proveedor.Columnas[OrdenAgrupador];
      Pagina.ColumnaPosicionadoraTorta = Proveedor.Columnas[OrdenPosicionador];
      Pagina.SolicitudTorta = mSolicitud;
      Pagina.AbrirTortasGIS();
    }

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

    private ClaseGrafico mClaseGrafico = ClaseGrafico.Torta;
    public Int32 ClaseGraficoElegido
    {
      get
      {
        return (Int32)mClaseGrafico;
      }
      set
      {
        mClaseGrafico = (ClaseGrafico)value;
        StateHasChanged();
      }
    }

    public bool EsTortas
    {
      get { return (mClaseGrafico == ClaseGrafico.Torta); }
    }

    public bool EsBarras
    {
      get { return (mClaseGrafico == ClaseGrafico.Barras); }
    }

    public bool EsHistograma
    {
      get { return (mClaseGrafico == ClaseGrafico.Histograma); }
    }

    public bool EsBarrasApiladas
    {
      get { return (mClaseGrafico == ClaseGrafico.BarrasH); }
    }

    public bool EsPuntos
    {
      get { return (mClaseGrafico == ClaseGrafico.Puntos); }
    }

    public Int32 OrdenColumnaValor { get; set; } = -1;

    public Int32 OrdenColumnaAbscisa { get; set; } = -1;

    public Int32 OrdenColumnaSexo { get; set; } = -1;

    public List<CColumnaBase> Columnas
    {
      get
      {
        return (mProveedor == null ? null : mProveedor.Columnas);
      }
    }

    private ClaseGrafico ObtenerClaseGrafico()
    {
      if (EsTortas)
      {
        return ClaseGrafico.Torta;
      }
      else
      {
        if (EsBarras)
        {
          return ClaseGrafico.Barras;
        }
        else
        {
          if (EsHistograma)
          {
            return ClaseGrafico.Histograma;
          }
          else
          {
            if (EsBarrasApiladas)
            {
              return ClaseGrafico.BarrasH;
            }
            else
            {
              if (EsPuntos)
              {
                return ClaseGrafico.Puntos;
              }
              else
              {
                return ClaseGrafico.NoDefinido;
              }
            }
          }
        }
      }
    }

    public void AbrirGrafico()
    {
      if (FncReposicionarArriba != null)
      {
        FncReposicionarArriba();
      }
      if (ModalCrearGrafico != null)
      {
        ModalCrearGrafico.Show();
      }
    }

    public bool CumpleCondicionReal(ClaseVariable ClaseV)
    {
      if (EsHistograma || EsPuntos)
      {
        return true; // (ClaseV == ClaseVariable.Real || ClaseV == ClaseVariable.Entero);
      }
      else
      {
        return true;
      }
    }

    private ModoAgruparDependiente ConvertirModoAgrupar(Int32 Opcion)
    {
      switch (Opcion)
      {
        case 1: return ModoAgruparDependiente.Acumulado;
        case 2: return ModoAgruparDependiente.Media;
        case 4: return ModoAgruparDependiente.Minimo;
        case 5: return ModoAgruparDependiente.Maximo;
        default: return ModoAgruparDependiente.Cantidad;
      }
    }

    public void AjustarBotones()
    {
      StateHasChanged();
    }

    public Blazorise.Color ColorBoton(Int32 Clase)
    {
      CColumnaBase ColRefe = (from C in Proveedor.Columnas
                              where C.Orden == OrdenColumnaValor
                              select C).FirstOrDefault();
      if (ColRefe == null)
      {
        return Color.Light;
      }
      switch (ColRefe.Clase)
      {
        case ClaseVariable.Entero:
        case ClaseVariable.Real:
          if (EsHistograma || EsPuntos)
          {
            return (Clase == 3 ? Color.Primary : Color.Light);
          }
          else
          {
            return Color.Primary;
          }
        default: return (Clase == 3 ? Color.Primary : Color.Light);
      }
    }

    public string GraficoSeleccionado
    {
      get
      {
        switch (mClaseGrafico)
        {
          case ClaseGrafico.Torta: return "Torta";
          case ClaseGrafico.Barras: return "Barras";
          case ClaseGrafico.Histograma: return "Histograma";
          case ClaseGrafico.BarrasH: return "Barras apiladas";
          case ClaseGrafico.Puntos: return "Puntos";
          default: return "No definido";
        }
      }
    }

    public DatosSolicitados mSolicitud;
    public Int32 OrdenPosicionador { get; set; }
    public Int32 OrdenAgrupador { get; set; }
    public Int32 OrdenValor { get; set; }
    public Int32 OrdenLat { get; set; }
    public Int32 OrdenLng { get; set; }
    public bool Agrupados { get; set; } = true;
    public bool CapaPins { get; set; } = true;

    public bool NoHayPinsLL
		{
      get
			{
        return (OrdenValor < 0 || OrdenLat < 0 || OrdenLng < 0 || (!CapaPins && OrdenAgrupador < 0));
			}
		}

    public bool NoHayTortasGIS
    {
      get
      {
        return (OrdenValor < 0 || OrdenAgrupador < 0 || OrdenPosicionador < 0);
      }
    }

    public string EstiloBoton(Int32 Clase, Int32 Ancho = 50)
    {
      CColumnaBase ColRefe = (from C in Proveedor.Columnas
                              where C.Orden == OrdenColumnaValor
                              select C).FirstOrDefault();
      string Base = "width: " + Ancho.ToString() + "px; ";
      if (ColRefe == null)
      {
        return Base + "pointer-events: none;";
      }
      switch (ColRefe.Clase)
      {
        case ClaseVariable.Entero:
        case ClaseVariable.Real:
          if (EsHistograma || EsPuntos)
          {
            return Base + (Clase == 3 ? "" : "pointer-events: none;");
          }
          else
          {
            return Base;
          }
        default: return Base + (Clase == 3 ? "" : "pointer-events: none;");
      }
    }

    private void PosicionarGrafico(ref CLinkGrafico Grafico)
    {
      // Ubicar la posicion de la tendencia.
      int AbscisaMinima = (int)Logicas.CLogicaIndicador.AnchoTendenciasDefault + Logicas.CLogicaIndicador.ABSCISA_INI_TENDENCIAS +
          Logicas.CLogicaIndicador.SEPARACION;
      Grafico.Ancho = Contenedores.CContenedorDatos.AnchoPantallaIndicadores - Logicas.CLogicaIndicador.SEPARACION - AbscisaMinima;
      Grafico.Abscisa = (Grafico.Superior == null ? (AbscisaMinima - 25 * mGraficos.Count) : Grafico.Superior.Abscisa);
      Grafico.Alto = Logicas.CLogicaIndicador.AltoTendenciaDefault;
      Grafico.Ordenada = (Grafico.Superior == null ? (5 + 25 * mGraficos.Count) :
          (Int32)Math.Floor(Math.Min(Grafico.Superior.Ordenada + Grafico.Superior.Alto + SEP_FILTROS,
            Contenedores.CContenedorDatos.AltoPantalla - Grafico.Superior.Alto - SEP_FILTROS)));
    }

    public void AgregarGrafico(Int32 Opcion)
    {
      try
      {
        Pagina.PonerElementoEncima(true, false, false, -1, -1);
        ClaseGrafico Clase = ObtenerClaseGrafico();
        if (Clase == ClaseGrafico.NoDefinido)
        {
          throw new Exception("Definir el gráfico");
        }

        if (OrdenColumnaValor < 0)
        {
          throw new Exception("Seleccionar campo de valor");
        }

        if (Clase == ClaseGrafico.Histograma || Clase == ClaseGrafico.Puntos)
        {
          if (Proveedor.Columnas[OrdenColumnaValor].Clase != ClaseVariable.Real &&
              Proveedor.Columnas[OrdenColumnaValor].Clase != ClaseVariable.Entero)
          {
            throw new Exception("Los valores deben ser reales o enteros");
          }
        }

        if (Clase != ClaseGrafico.Histograma)
        {
          if (OrdenColumnaAbscisa < 0)
          {
            throw new Exception("Seleccionar campo agrupador");
          }
        }

        if (Clase == ClaseGrafico.Puntos)
        {
          if (Proveedor.Columnas[OrdenColumnaAbscisa].Clase != ClaseVariable.Real &&
              Proveedor.Columnas[OrdenColumnaAbscisa].Clase != ClaseVariable.Entero)
          {
            throw new Exception("La columna agrupadora debe ser real o entera");
          }
        }

        if (Clase == ClaseGrafico.BarrasH)
        {
          if (OrdenColumnaAbscisa < 0)
          {
            throw new Exception("Seleccionar agrupador líneas");
          }
        }

        CLinkGrafico Link = new CLinkGrafico()
        {
          Proveedor = mProveedor,
          Componente = null,
          FncCrearGraficoDependiente = AgregarGraficoDependiente,
          ColumnaAbscisas = (OrdenColumnaAbscisa >= 0 ? Proveedor.Columnas[OrdenColumnaAbscisa] : null),
          ColumnaOrdenadas = Proveedor.Columnas[OrdenColumnaValor],
          ColumnaSexo = (OrdenColumnaSexo >= 0 ? Proveedor.Columnas[OrdenColumnaSexo] : null),
          AgrupamientoDependiente = ConvertirModoAgrupar(Opcion),
          Clase = Clase,
          CodigoUnico = CLinkFiltros.gCodigoUnico++,
          FncCerrar = Agregado_AlCerrarGrafico
        };

        PosicionarGrafico(ref Link);

        mGraficos.Add(Link);

        CerrarVentanaGrafico();

        AlAjustarVentanasFlotantes();

      }
      catch (Exception ex)
      {
        Rutinas.CRutinas.DesplegarMsg(ex);
      }
    }

    private void AgregarGraficoDependiente(CLogicaGrafico Superior, Int32 ColumnaIndependiente)
    {
      CLinkGrafico Link = new CLinkGrafico()
      {
        Proveedor = mProveedor,
        Componente = null,
        FncCrearGraficoDependiente = AgregarGraficoDependiente,
        FncSeleccionarGrafico = SeleccionarGrafico,
        ColumnaAbscisas = (Superior.Clase == ClaseGrafico.Histograma ? null :
          (ColumnaIndependiente >= 0 ? Proveedor.Columnas[ColumnaIndependiente] : null)),
        ColumnaOrdenadas = (Superior.Clase == ClaseGrafico.Histograma ?
            (ColumnaIndependiente >= 0 ? Proveedor.Columnas[ColumnaIndependiente] : null) : Superior.ColumnaOrdenadas),
        ColumnaSexo = null,
        AgrupamientoDependiente = Superior.AgrupamientoDependiente,
        Clase = Superior.Clase,
        Superior = Superior,
        CodigoUnico = CLinkFiltros.gCodigoUnico++,
        FncCerrar = Agregado_AlCerrarGrafico
      };

      PosicionarGrafico(ref Link);

      mGraficos.Add(Link);

      AlAjustarVentanasFlotantes();

    }

    public void AbrirGrilla()
    {
      if (mGrilla == null)
      {
        try
        {
          mGrilla = new CLinkGrilla()
          {
            Proveedor = mProveedor,
            Componente = null,
          };

          PosicionarGrilla(Pagina.ClaseOrigen);

          AlAjustarVentanasFlotantes();

        }
        catch (Exception ex)
        {
          Rutinas.CRutinas.DesplegarMsg(ex);
        }
      }
    }

    private void PosicionarGrilla(ClaseElemento Clase)
    {
      mGrilla.Abscisa = (Clase == ClaseElemento.SubConsulta ? 8 : 270 + SEP_FILTROS);
      mGrilla.Ordenada = Contenedores.CContenedorDatos.AltoPantallaIndicadores - 250 - SEP_FILTROS;
      mGrilla.Ancho = Contenedores.CContenedorDatos.AnchoPantallaIndicadores - mGrilla.Abscisa - SEP_FILTROS;
      mGrilla.Alto = 250;
    }

    private void SeleccionarGrafico(CBaseGrafico Grafico)
    {
      if (EvSeleccionarGrafico != null)
      {
        EvSeleccionarGrafico(Grafico);
      }
    }

    private void CerrarDependientes(CLogicaGrafico Graf)
    {
      foreach (CLogicaGrafico Dep in Graf.GraficosDependientes)
      {
        CLinkGrafico LinkGraf = (from G in mGraficos
                                 where G.Componente != null && G.Componente.Nombre == Dep.Nombre
                                 select G).FirstOrDefault();
        if (LinkGraf != null)
        {
          mGraficos.Remove(LinkGraf);
          CerrarDependientes(LinkGraf.Componente);
        }
      }
    }

    private void Agregado_AlCerrarGrafico(string Nombre, bool B)
    {
      if (!B)
      {
        CLinkGrafico Graf = (from G in mGraficos
                             where G.Componente != null && G.Componente.Nombre == Nombre
                             select G).FirstOrDefault();
        if (Graf != null)
        {
          EvAjustarGraficosDependientes -= Graf.Componente.RefrescarDatosTortaDesdeProveedor;
          mGraficos.Remove(Graf);
          CerrarDependientes(Graf.Componente);
          if (AlAjustarVentanasFlotantes != null)
          {
            AlAjustarVentanasFlotantes();
          }
        }
      }
      else
      {
        AlAjustarVentanasFlotantes();
      }
    }

    public CProveedorComprimido ProveedorImpuesto
    {
      set
      {
        Proveedor = value;
      }
    }

    [Parameter]
    public CProveedorComprimido Proveedor
    {
      get { return mProveedor; }
      set
      {
        if (mProveedor != value)
        {
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes -= MProveedor_AlAjustarDependientes;
          }
          mProveedor = value;
          if (mProveedor != null)
          {
            mProveedor.AlAjustarDependientes += MProveedor_AlAjustarDependientes;
          }
          foreach (CLinkFiltros Link in mLinks)
          {
            Link.Filtrador.Proveedor = mProveedor;
          }
        }
        if (mProveedor != null && Lineas == null)
        {
          CrearLineas();
          RefrescarSuperior();
        }
      }
    }

    public override void RefrescarSuperior()
    {
      if (Blocks != null)
      {
        Blocks.Refrescar();
      }
      else
      {
        base.RefrescarSuperior();
      }
    }

    public void CrearLineas(bool BloquearRefresco = false)
    {
      if (Lineas == null)
      {
        Lineas = CrearLineasGlobal(mProveedor, this);
        if (Lineas != null && !BloquearRefresco)
        {
          mProveedor.RefrescarDependientes();
        }
      }
    }

    public static List<LineaFiltro> CrearLineasGlobal(CProveedorComprimido Proveedor, CLogicaContenedorFiltros Otro)
    {

      List<LineaFiltro> Respuesta = null;
      if (Otro != null && Otro.Lineas != null)
      {
        return Otro.Lineas;
      }

      if (Proveedor != null && Otro != null)
      {

        Respuesta = new List<LineaFiltro>();
        foreach (CColumnaBase Columna in Proveedor.Columnas)
        {
          bool HayCondicion = false;
          if (Otro.Links != null)
          {
            CLinkFiltros LinkRefe = (from Link in Otro.Links
                                     where Link.Filtrador != null &&
                                         Link.Filtrador.Proveedor.Orden == Proveedor.Orden &&
                                         Link.Filtrador.Columna.Orden == Columna.Orden
                                     select Link).FirstOrDefault();
            if (LinkRefe != null)
            {
              HayCondicion = LinkRefe.Filtrador.HayCondicion;
            }
          }
          Respuesta.Add(new LineaFiltro(Columna, HayCondicion, Otro));
        }

      }

      return Respuesta;

    }

    public void MProveedor_AlAjustarDependientes(object sender)
    {
      if (EvAjustarGraficosDependientes != null)
      {
        EvAjustarGraficosDependientes();
      }
      Lineas = null;
      CrearLineas(true);
      StateHasChanged();
      //      AlAjustarVentanasFlotantes();
    }

    public double DimensionCaracter { get; set; }

    //    private CInformacionAlarmaCN mPuntoLeido = null;

    public CInformacionAlarmaCN PuntoSeleccionado { get; set; }

    public string EstiloColumna(CColumnaBase Columna)
    {
      return "height: 25px; width: 100%; background-color: white;";
    }

    private bool mbMostrarAguarda = true;

    public void Fnc2()
    {
      return;
    }

    private Int32 PosicionMaximaOcupada()
    {
      Int32 PosEnPantalla = -1;
      foreach (CLinkFiltros Link in mLinks)
      {
        if (Link.Componente != null)
        {
          PosEnPantalla = Math.Max(PosEnPantalla, Link.PosicionEnPantalla);
        }
      }
      return PosEnPantalla;
    }

    private void FncAjustarAnchos(object Enviador)
    {
      foreach (CLinkFiltros Link in mLinks)
      {
        if (Link.Componente != null && Link.Componente is CLogicaFiltroTextos)
        {
          Link.Ancho = (Int32)((CLogicaFiltroTextos)Link.Componente).Ancho;
        }
      }
      AlAjustarVentanasFlotantes();
    }

    public void FiltrarDataset()
    {
      if (mLinks != null && mLinks.Count > 0)
      {
        mLinks[0].Filtrador.FiltrarDataset();
      }
    }

    private void FncAjustarListasValor(Datos.CFiltrador Filtro, List<Int32> Codigos)
    {
      foreach (CLinkFiltros Link in mLinks)
      {
        if (Link.Componente != null && Link.Componente is CLogicaFiltroTextos)
        {
          CLogicaFiltroTextos FTextos = (CLogicaFiltroTextos)Link.Componente;
          if (FTextos.Filtrador.Columna.Nombre == Filtro.Columna.Nombre)
          {
            FTextos.ImponerFiltrosAsociados(Codigos);
          }
        }
      }
      AlAjustarVentanasFlotantes();
    }

    private void CrearPantallaFiltro(Int32 Orden, Int32 Abscisa, Int32 Ordenada)
    {
      CColumnaBase Columna = (from C in Columnas
                              where C.Orden == Orden
                              select C).FirstOrDefault();
      if (Columna != null)
      {
        CLinkFiltros Agregado = new CLinkFiltros()
        {
          Componente = null,
          Abscisa = Abscisa,
          Ordenada = Ordenada,
          FncRefresco = FncAjustarAnchos,
          FncCerrar = FncCerrarFiltro,
          FncAjustarListasValor = FncAjustarListasValor,
          PosicionEnPantalla = PosicionMaximaOcupada() + 1,
          Filtrador = Proveedor.ObtenerFiltroColumna(Columna.Nombre)
        };
        Agregado.Filtrador.CrearInformacionFilas();
        mLinks.Add(Agregado);
        AlAjustarVentanasFlotantes();
      }
    }

    private const double ANCHO_FILTROS = 180;
    private const Int32 SEP_FILTROS = 10;

    private void FncCerrarFiltro(string NombreColumna, bool Eliminar)
    {
      //      ActualizarDatosDataset(mProveedor);
      for (Int32 i = 0; i < mLinks.Count; i++)
      {
        CLogicaFiltroTextos Filtro = mLinks[i].Componente as CLogicaFiltroTextos;
        if (Filtro != null && Filtro.Filtrador.Columna.Nombre == NombreColumna)
        {

          Filtro.Filtrador.LimpiarCondiciones();
          Filtro.Filtrador.ModoAgrupar = ModoAgruparDependiente.NoDefinido;
          Filtro.Filtrador.ColumnaValor = null;

          for (Int32 ii = i; ii < (mLinks.Count - 1); ii++)
          {
            mLinks[ii] = mLinks[ii + 1];
          }
          mLinks.RemoveAt(mLinks.Count - 1);
          Filtro.Filtrador.Proveedor.FiltrarPorAsociaciones();
          break;
        }
      }

      Int32 Pos = 0;
      foreach (CLinkFiltros L in mLinks)
      {
        L.PosicionUnica = CLinkFiltros.gCodigoUnico++;
        if (L.PosicionEnPantalla >= 0)
        {
          L.PosicionEnPantalla = Pos++;
        }
      }

      mLinksAnteriores = new List<CLinkFiltros>();
      mLinksAnteriores.AddRange(mLinks);
      mLinks.Clear();

      CrearLineas();

      if (AlAjustarVentanasFlotantes != null)
      {
        AlAjustarVentanasFlotantes();
      }

    }

    private void RecrearPantallaFiltro(CLinkFiltros Link)
    {
      if (Link.PosicionEnPantalla < 0)
      {
        Link.PosicionEnPantalla = PosicionMaximaOcupada() + 1;
        AlAjustarVentanasFlotantes();
      }
    }


    public void FncSeleccionFila(Int32 Orden, Int32 Abscisa, Int32 Ordenada)
    {
      if (Orden < 0)
      {
        return;
      }

      CLinkFiltros Link = (from L in mLinks
                           where L.Filtrador != null && L.Filtrador.Columna != null && L.Filtrador.Columna.Orden == Orden
                           select L).FirstOrDefault();
      if (Link == null)
      {
        // No hay filtro, ni siquiera minimizado.
        CrearPantallaFiltro(Orden, Abscisa, Ordenada);
      }
      else
      {
        // si PosicionEnPantalla<0, indica que se cerro.
        if (Link.PosicionEnPantalla < 0)
        {
          RecrearPantallaFiltro(Link);
        }
      }

    }

    [Parameter]
    public CDatoIndicador Indicador { get; set; }

    public string EstiloDivAguarda
    {
      get
      {
        return "width: 100%; height: 100%; display: block; overflow-y: hidden; overflow-x: hidden; position: absolute; margin: 0px; " +
          "visible: " + (mbMostrarAguarda ? "visible;" : "collapse;");
      }
    }

    public string EstiloLista
    {
      get
      {
        return "padding-left: 15px; padding-top: 10px; width: 100 %; bottom: 0px; height: " + (Alto - 45).ToString() +
          "px; overflow: auto; display: block; box-sizing: inherit; text-align: left; ";
      }
    }

    public string EstiloImagen = "display: inline; width: 20px; height: 20px; cursor: pointer;";

    public string EstiloLinea(LineaFiltro Linea)
    {
      return "background-color: " + Linea.ColorBase + ";";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
      if (Lineas == null)
      {
        CrearLineas();
        if (Lineas != null)
        {
          StateHasChanged();
        }
      }
      await base.OnAfterRenderAsync(firstRender);
    }

    public void ActualizarDatosDataset(CProveedorComprimido ProvDatos)
    {
      Proveedor = ProvDatos;
      CrearLineas();
      foreach (CLinkFiltros Link in mLinks)
      {
        Link.Filtrador.Proveedor = Proveedor;
        if (Link.Componente != null)
        {
          Link.Componente.Filtrador = Link.Filtrador;
        }
      }

      foreach (CLinkGrafico Link in mGraficos)
      {
        Link.Proveedor = Proveedor;
        if (Link.Componente != null)
        {
          Link.Componente.Proveedor = Proveedor;
          Link.Componente.RefrescarDatosTortaDesdeProveedor();
          Link.Componente.Redibujar();
        }
      }

      if (Grilla != null)
      {
        Grilla.Proveedor = Proveedor;
        Grilla.Componente.Proveedor = Proveedor;
        Grilla.Componente.Redibujar();
      }

      StateHasChanged();
    }

  }

  public class CLinkGrafico

  {

    [CascadingParameter]
    public CLogicaIndicador Contenedor { get; set; }

    public CLogicaFiltroTextos.FncEventoCrearGrafDependiente FncCrearGraficoDependiente = null;
    public CBaseGrafico.FncSeleccionarGrafico FncSeleccionarGrafico = null;

    //public CLogicaGrafico ComponenteAnterior { get; set; } = null;

    private CLogicaGrafico mComponente;
    public CLogicaGrafico Componente
    {
      get { return mComponente; }
      set
      {
        if (mComponente != value)
        {
          if (mComponente != null)
          {
            mComponente.AlCerrarGrafico -= FncCerrar;
            if (FncCrearGraficoDependiente != null)
            {
              mComponente.AlCrearGraficoDependiente -= FncCrearGraficoDependiente;
            }
            if (FncSeleccionarGrafico != null)
            {
              mComponente.EvSeleccionarGrafico -= FncSeleccionarGrafico;
            }

            if (Superior != null)
            {
              Superior.GraficosDependientes.Remove(mComponente);
            }
            mComponente.Dispose();
          }
          if (mComponente != value)
          {
            mComponente = value;
            if (value != null)
            {
              value.AbscisaGrafico = Abscisa;
              value.OrdenadaGrafico = Ordenada;
              value.AnchoGrafico = Ancho;
              value.AltoGrafico = Alto;
              value.ImponerCodigoUnico(CodigoUnico);
              value.ImponerColumnaAbscisas(ColumnaAbscisas);
              value.ImponerColumnaOrdenadas(ColumnaOrdenadas);
              value.ImponerColumnaSexo(ColumnaSexo);
              value.ImponerGraficosDependientes(GraficosDependientes);
              value.ImponerValoresSeleccionados(ValoresSeleccionados);
              value.ImponerClase(Clase);
              value.ImponerModoAgrupar(AgrupamientoDependiente);
              value.OrdenadaGrafico = Ordenada;
              value.Proveedor = Proveedor;
              value.ImponerDetallado(mbDetallado);
              value.AlCerrarGrafico += FncCerrar;
              if (FncCrearGraficoDependiente != null)
              {
                value.AlCrearGraficoDependiente += FncCrearGraficoDependiente;
              }
              if (Superior != null)
              {
                Superior.GraficosDependientes.Add(value);
                value.Superior = Superior;
                if (FncSeleccionarGrafico != null)
                {
                  value.EvSeleccionarGrafico += FncSeleccionarGrafico;
                }
              }
       //       else
							//{
       //         if (ComponenteAnterior != null)
							//	{
       //           if (ComponenteAnterior.Superior != null)
							//		{
       //             CLinkGrafico LnkSuperior = (from L in Contenedor.ComponenteFiltros.Graficos
       //                                            where L.CodigoUnico == ComponenteAnterior.Superior.CodigoUnico
       //                                            select L).FirstOrDefault();
       //             if (LnkSuperior != null)
							//			{
       //               LnkSuperior.GraficosDependientes.Add(value);
       //               value.Superior = LnkSuperior.Componente;
							//			}
							//		}
							//	}
							//}
              value.RefrescarDatosTortaDesdeProveedor(); // Carga inicial de datos.
            }
          }
        }
      }

    }

    public void GuardarPosicion()
    {
      AbscisaAnterior = Abscisa;
      OrdenadaAnterior = Ordenada;
      AnchoAnterior = Ancho;
      AltoAnterior = Alto;
    }

    public void RecuperarPosicion()
    {
      Abscisa = AbscisaAnterior;
      Ordenada = OrdenadaAnterior;
      Ancho = AnchoAnterior;
      Alto = AltoAnterior;
    }

    public Int32 AbscisaAnterior = -999999;
    public Int32 OrdenadaAnterior = -999999;
    public long AnchoAnterior = -999999;
    public long AltoAnterior = -999999;

    private bool mbAmpliado = false;

    public bool Ampliado
    {
      get
      {
        return mbAmpliado;
      }
      set
      {
        mbAmpliado = value;
      }
    }

    public bool Encima { get; set; }

    private bool mbDetallado = false;
    public bool Detallado
    {
      get { return (Componente == null ? mbDetallado : mComponente.Detallado); }
      set
      {
        mbDetallado = value;
        if (mComponente != null)
        {
          mComponente.ImponerDetallado(value);
        }
      }
    }

    public Int32 CodigoUnico { get; set; }

    private Int32 mAbscisa = -999999;
    public Int32 Abscisa
    {
      get
      {
        return (mAbscisa > -999999 || mComponente == null ? mAbscisa : mComponente.AbscisaGrafico);
      }
      set
      {
        mAbscisa = value;
        if (mComponente != null)
        {
          mComponente.AbscisaGrafico = value;
        }
      }
    }

    private Int32 mOrdenada = -999999;
    public Int32 Ordenada
    {
      get
      {
        return (mOrdenada > -999999 || mComponente == null ? mOrdenada : mComponente.OrdenadaGrafico);
      }
      set
      {
        mOrdenada = value;
        if (mComponente != null)
        {
          mComponente.OrdenadaGrafico = value;
        }
      }
    }

    private long mAlto = -999999;
    public long Alto
    {
      get
      {
        return (mAlto > -999998 ? mAlto : mComponente.AltoGrafico);
      }
      set
      {
        mAlto = value;
        if (mComponente != null)
        {
          mComponente.AltoGrafico = value;
        }
      }
    }

    private long mAncho = -999999;
    public long Ancho
    {
      get
      {
        return (mAncho > -999998 ? mAncho : mComponente.AnchoGrafico + 4);
      }
      set
      {
        mAncho = value;
        if (mComponente != null)
        {
          mComponente.AnchoGrafico = Math.Max(mComponente.AnchoGrafico, value - 4);
        }
      }
    }

    private ClaseGrafico mClase;
    public ClaseGrafico Clase
    {
      get
      {
        return (mComponente == null ? mClase : mComponente.Clase);
      }
      set
      {
        mClase = value;
        if (mComponente != null)
        {
          mComponente.ImponerClase(value);
        }
      }
    }

    public CProveedorComprimido Proveedor { get; set; }

    private CColumnaBase mColumnaAbscisas = null;
    public CColumnaBase ColumnaAbscisas
    {
      get { return (mComponente == null ? mColumnaAbscisas : mComponente.ColumnaAbscisas); }
      set
      {
        mColumnaAbscisas = value;
        if (mComponente != null)
        {
          mComponente.ImponerColumnaAbscisas(value);
        }
      }
    }

    private CColumnaBase mColumnaOrdenadas = null;
    public CColumnaBase ColumnaOrdenadas
    {
      get { return (mComponente == null ? mColumnaOrdenadas : mComponente.ColumnaOrdenadas); }
      set
      {
        mColumnaOrdenadas = value;
        if (mComponente != null)
        {
          mComponente.ImponerColumnaOrdenadas(value);
        }
      }
    }

    private CColumnaBase mColumnaSexo = null;
    public CColumnaBase ColumnaSexo
    {
      get { return (mComponente == null ? mColumnaSexo : mComponente.ColumnaSexo); }
      set
      {
        mColumnaSexo = value;
        if (mComponente != null)
        {
          mComponente.ImponerColumnaSexo(value);
        }
      }
    }

    private List<string> mszValoresSeleccionados = new List<string>();
    public List<string> ValoresSeleccionados
    {
      get { return (mComponente == null ? mszValoresSeleccionados : mComponente.ValoresSeleccionados); }
      set
      {
        mszValoresSeleccionados = value;
        if (mComponente != null)
        {
          mComponente.ImponerValoresSeleccionados(value);
        }
      }
    }

    private List<CLogicaGrafico> mGraficosDependientes = new List<CLogicaGrafico>();
    public List<CLogicaGrafico> GraficosDependientes
    {
      get { return (mComponente == null ? mGraficosDependientes : mComponente.GraficosDependientes); }
      set
      {
        mGraficosDependientes = value;
        if (mComponente != null)
        {
          mComponente.ImponerGraficosDependientes(value);
        }
      }
    }

    private ModoAgruparDependiente mModoAgrupar = ModoAgruparDependiente.Cantidad;
    public ModoAgruparDependiente AgrupamientoDependiente
    {
      get { return (mComponente == null || mComponente.AgrupamientoDependiente== ModoAgruparDependiente.NoDefinido
            ? mModoAgrupar : mComponente.AgrupamientoDependiente); }
      set
      {
        mModoAgrupar = value;
        if (mComponente != null)
        {
          mComponente.ImponerModoAgrupar(value);
        }
      }
    }

    //public IndicadoresV2.Datos.CFiltradorPasos Filtrador { get; set; }
    public CLogicaFiltroTextos.FncEventoRefresco FncRefresco { get; set; }
    public CLogicaFiltroTextos.FncEventoTextoBool FncCerrar { get; set; }
    public FncAjustarDependientes FncAjustarGraficos { get; set; }
    public static Int32 gCodigoUnico = 1;

    public CLogicaGrafico Superior { get; set; } = null;

    public CLinkGrafico()
    {
      Componente = null;
      GraficosDependientes = new List<CLogicaGrafico>();
      //Filtrador = null;
      //CodigoUnico = gCodigoUnico++;
    }

  }

  public class CLinkFiltros
  {

    public ModoAgruparDependiente ModoAgrupar { get; set; }

    private Client.Componentes.FiltroTextos mComponente;
    public Client.Componentes.FiltroTextos Componente
    {
      get { return mComponente; }
      set
      {
        if (mComponente != value)
        {
          if (mComponente != null)
          {
            mComponente.AlCambiarAncho -= FncRefresco;
            mComponente.AlCerrarFiltro -= FncCerrar;
            if (mComponente.Filtrador != null)
            {
              mComponente.Filtrador.AlImponerFiltrosAsociados -= FncAjustarListasValor;
            }
          }
        }
        mComponente = value;
        if (mComponente != null)
        {
          mComponente.Filtrador = Filtrador;
          mComponente.ImponerAbscisa(mAbscisa);
          mComponente.ImponerOrdenada(mOrdenada);
          mComponente.AlCambiarAncho += FncRefresco;
          mComponente.AlCerrarFiltro += FncCerrar;
          mComponente.Filtrador.AlImponerFiltrosAsociados += FncAjustarListasValor;
        }
      }
    }

    public bool Encima { get; set; }
    public Int32 PosicionEnPantalla { get; set; }
    public Int32 PosicionUnica { get; set; }
    public Int32 Ancho { get; set; } = 185;

    private Int32 mAbscisa = -999;
    public Int32 Abscisa
    {
      get { return (mComponente == null || mAbscisa > -998 ? mAbscisa : mComponente.Abscisa); }
      set
      {
        mAbscisa = value;
        if (mComponente != null && mComponente.Abscisa != value)
        {
          mComponente.ImponerAbscisa(value);
        }
      }
    }

    private Int32 mOrdenada = -999;
    public Int32 Ordenada
    {
      get { return (mComponente == null || mOrdenada > -998 ? mOrdenada : mComponente.Ordenada); }
      set
      {
        mOrdenada = value;
        if (mComponente != null && mComponente.Ordenada != value)
        {
          mComponente.ImponerOrdenada(value);
        }
      }
    }

    public CFiltrador Filtrador { get; set; }
    public CLogicaFiltroTextos.FncEventoRefresco FncRefresco { get; set; }
    public CLogicaFiltroTextos.FncEventoTextoBool FncCerrar { get; set; }
    public CFiltrador.FncEventoEnteros FncAjustarListasValor { get; set; }
    public static Int32 gCodigoUnico = 1;

    public CLinkFiltros()
    {
      PosicionUnica = gCodigoUnico++;
      Componente = null;
      PosicionEnPantalla = -1;
      Filtrador = null;
      ModoAgrupar = ModoAgruparDependiente.NoDefinido;
    }

  }

  public class LineaFiltro
  {
    public CLogicaContenedorFiltros Filtro { get; set; }
    public string ColorBase { get; set; }
    public CColumnaBase Columna { get; set; }

    public LineaFiltro(CColumnaBase Col0, bool Hay0, CLogicaContenedorFiltros Filtro0 = null)
    {
      Columna = Col0;
      ColorBase = (Hay0 ? "yellow" : "white");
      Filtro = Filtro0;
    }
  }

  public class CLinkGrilla
  {

    private TableroPecasV5.Client.Componentes.CGrillaDatos mComponente;
    public TableroPecasV5.Client.Componentes.CGrillaDatos Componente
    {
      get { return mComponente; }
      set
      {
        if (mComponente != value)
        {
        }
        mComponente = value;
        if (mComponente != null)
        {
          mComponente.Proveedor = Proveedor;
          mComponente.ImponerAbscisa(mAbscisa);
          mComponente.ImponerOrdenada(mOrdenada);
        }
      }
    }

    public bool Encima
    {
      get { return (mComponente == null ? false : mComponente.Encima); }
      set
      {
        if (value != Encima && mComponente != null)
        {
          mComponente.ImponerEncima(value);
        }
      }
    }

    public Int32 PosicionEnPantalla { get; set; }
    public Int32 PosicionUnica { get; set; }
    public Int32 Ancho { get; set; } = 185;
    public Int32 Alto { get; set; } = 185;
    private Int32 mAbscisa = -999;
    public Int32 Abscisa
    {
      get { return (mAbscisa > -998 || mComponente == null ? mAbscisa : mComponente.Abscisa); }
      set
      {
        mAbscisa = value;
        if (mComponente != null && mComponente.Abscisa != value)
        {
          mComponente.ImponerAbscisa(value);
        }
      }
    }

    private Int32 mOrdenada = -999;
    public Int32 Ordenada
    {
      get { return (mOrdenada > -998 || mComponente == null ? mOrdenada : mComponente.Ordenada); }
      set
      {
        mOrdenada = value;
        if (mComponente != null && mComponente.Ordenada != value)
        {
          mComponente.ImponerOrdenada(value);
        }
      }
    }

    public CProveedorComprimido Proveedor { get; set; }
    public static Int32 gCodigoUnico = 1;

    public CLinkGrilla()
    {
      PosicionUnica = gCodigoUnico++;
      Componente = null;
      PosicionEnPantalla = -1;
      Proveedor = null;
    }

  }

  public enum DatosSolicitados
	{
    TortasLL,
    TortasManual,
    TortasGIS
	}

}
