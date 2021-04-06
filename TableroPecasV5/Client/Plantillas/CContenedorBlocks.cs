using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.JSInterop;
using System.Xml.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Logicas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Plantillas
{
  /// <summary>
  /// Es el componente que hace toda la funcionalidad del despliegue y registro de los blocks en las solapas y tarjetas.
  /// </summary>
  public class CContenedorBlocks : ComponentBase
  {
    public delegate void FncCompletar();

    public CContenedorBlocks()
    {
      // bloquea la lectura del dataset con los prms de la subconsulta desde el menu.
      CLogicaSubconsulta.gParametros = null;
      mRelojes = new List<CLinkReloj>();
      mFichas = new List<CLinkFicha>();
      mTendencias = new List<CLinkTendencia>();
      mFiltros = new List<CLinkFiltros>();
      mContenedoresFiltros = new List<CLinkContenedorFiltros>();
      mGraficos = new List<CLinkGraficoCnt>();
      mMapas = new List<CLinkMapa>();
      mOtros = new List<CLinkOtro>();
      mGrillas = new List<CLinkOtro>();
    }

    private List<CLinkReloj> mRelojes; // relojes, tendencias y contenedores de filtros.
    private List<CLinkFicha> mFichas;
    private List<CLinkTendencia> mTendencias;
    private List<CLinkFiltros> mFiltros;
    private List<CLinkContenedorFiltros> mContenedoresFiltros = new List<CLinkContenedorFiltros>();
    private List<CLinkGraficoCnt> mGraficos; // Incluye graficos y conjuntos.
    private List<CLinkMapa> mMapas;
    private List<CLinkOtro> mOtros;
    private List<CLinkOtro> mGrillas;

    public List<CLinkReloj> Relojes
    {
      get { return mRelojes; }
    }

    public List<CLinkFicha> Fichas
    {
      get { return mFichas; }
    }

    public List<CLinkTendencia> Tendencias
    {
      get { return mTendencias; }
    }

    public List<CLinkFiltros> Filtros
    {
      get { return mFiltros; }
    }

    public List<CLinkContenedorFiltros> ContenedoresFiltros
    {
      get { return mContenedoresFiltros; }
    }

    public List<CLinkGraficoCnt> Graficos
    {
      get { return mGraficos; }
    }

    public List<CLinkMapa> Mapas
    {
      get { return mMapas; }
    }

    public List<CLinkOtro> Otros
    {
      get { return mOtros; }
    }

    public List<CLinkOtro> Grillas
    {
      get { return mGrillas; }
    }

    public const string NOMBRE_ELEMENTO = "BASE_CONTENEDOR_BLOCKS";

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    private Rectangulo mRectContenedor = null;
    public async Task<Rectangulo> RectanguloContenedor()
		{
      if (mRectContenedor == null)
			{
        mRectContenedor = await CRutinas.ObtenerRectanguloElementoAsync(JSRuntime, NOMBRE_ELEMENTO);
			}
      return mRectContenedor;
		}

    private LineaFiltro mLineaDrag = null;
    private CLinkFiltros mFiltroDrag = null;

    public LineaFiltro LineaDrag
    {
      get { return mLineaDrag; }
      set { mLineaDrag = value; }
    }

    public void IniciarDragLinea(LineaFiltro Linea)
    {
      mFiltroDrag = null;
      mLineaDrag = Linea;
    }

    public void IniciarDragFiltro(CLinkFiltros Filtro)
    {
      mFiltroDrag = Filtro;
      mLineaDrag = null;
    }

    public string IdFiltro(CLinkFiltros Lnk)
    {
      return "IDFiltro" + Lnk.PosicionEnPantalla.ToString();
    }

    public string EstiloFiltro(CLinkFiltros Lnk, CLogicaContenedorFiltros ComponenteFiltros)
    {
      Int32 AnchoLocal = Math.Max(185, (from L in ComponenteFiltros.Links
                                        where L.PosicionUnica == Lnk.PosicionUnica
                                        select L.Ancho).FirstOrDefault());

      return "width: " + AnchoLocal.ToString() + "px; height: 180" + //Logicas.CDetalleIndicador.AltoFiltro.ToString() +
        "px; margin-left: " + Lnk.Abscisa.ToString() +
        "px; margin-top: " + Lnk.Ordenada.ToString() +
        "px; position: absolute; text-align: center; z-index: 100;";
    }

    public void Refrescar()
		{
      StateHasChanged();
		}

    public void RecibirDrop(Microsoft.AspNetCore.Components.Web.DragEventArgs e)
    {
      if (mFiltroDrag != null)
      {
        CLinkFiltros Link = null;
        foreach (CLinkOtro O in Otros)
        {
          if (O.ComponenteFiltros != null)
          {
            foreach (CLinkFiltros G in O.ComponenteFiltros.Links)
            {
              if (G.Componente.CodigoUnico == mFiltroDrag.Componente.CodigoUnico)
              {
                Link = G;
                break;
              }
            }
          }
        }

        if (Link != null)
        {
          Int32 Diferencia = (int)e.OffsetX - mFiltroDrag.Componente.AbscisaAbajo;
          Link.Abscisa += Diferencia;
          Diferencia = (int)e.OffsetY - mFiltroDrag.Componente.OrdenadaAbajo;
          Link.Ordenada += Diferencia;
          StateHasChanged();
        }
      }
      if (mLineaDrag != null)
      {
        mLineaDrag.Filtro.FncSeleccionFila(mLineaDrag.Columna.Orden, (int)e.OffsetX,
            (int)e.OffsetY);
      }
    }

    public string EstiloContenedorMapa(CLinkMapa Lnk)
    {
      return "id: 'MAPA_" + Lnk.Datos.Codigo.ToString() +
        "'; margin-left: 0px; margin-top: 25px;  width: " +
        Lnk.Datos.Ancho.ToString() + "px; height: " + (Lnk.Datos.Alto - 25).ToString() +
        "px; text-align: center; overflow: hidden; position: absolute;";
    }

    public void ReducirMapa(CLinkMapa Mapa)
    {
      Mapa.Reducir();
      StateHasChanged();
    }

    public async void MaximizarMapa(CLinkMapa Mapa)
    {
      Mapa.Maximizar(await RectanguloContenedor());
      StateHasChanged();
    }

    public string EstiloDivBotonOtro(CLinkOtro Otro)
    {
      return "height: 25px; width: 25px; position: absolute; margin-left: " + Math.Floor(Otro.Datos.Ancho - 27).ToString() +
          "px; margin-top: -38px; ";
    }

    public void ReducirOtro(CLinkOtro Otro)
    {
      Otro.Reducir();
      StateHasChanged();
    }

    public void MaximizarOtro(CLinkOtro Otro)
    {
      Otro.Maximizar();
      StateHasChanged();
    }

    public static string EstiloLinkBase(CLinkBase Grafico)
    {
      return "width: " + ((Int32)Math.Floor(Grafico.Ancho + 0.5)).ToString() + "px; height: " +
        ((Int32)Math.Floor(Grafico.Alto + 0.5)).ToString() +
        "px; position: absolute; margin-left: " + (Grafico.Ampliado ? "0" : Grafico.Abscisa.ToString()) +
        "px; margin-top: " + (Grafico.Ampliado ? "0" : Grafico.Ordenada.ToString()) + "px; background: white; overflow: auto;";
    }

    public static string EstiloRelojLink(CLinkReloj Reloj)
    {
      return "width: " + Reloj.Ancho.ToString() + "px; height: " + Reloj.Alto.ToString() +
        "px; position: absolute; margin-left: " + Reloj.Abscisa.ToString() +
        "px; margin-top: " + Reloj.Ordenada.ToString() + "px; background: red;";
    }

    public static string EstiloBlock(CLinkBase Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: " + Lnk.Alto.ToString() +
        "px; position: absolute; margin-left: " + Lnk.Abscisa.ToString() +
        "px; margin-top: " + Lnk.Ordenada.ToString() + "px; background: gray;";
    }

    public string EstiloGrilla(CLinkOtro Grilla)
    {
      return "width: " + Math.Floor(Grilla.Ancho).ToString() + "px; height: " +
        Math.Floor(Grilla.Alto).ToString() +
        "px; margin-left: " + Grilla.Abscisa.ToString() +
        "px; margin-top: " + Grilla.Ordenada.ToString() + "px; position: absolute; text-align: center;" +
        " z-index: " + Grilla.NivelFlotante.ToString() + "; background-color: white; overflow: hidden; font-size: 11px;";
    }

    public static string EstiloTarjetaLink(CLinkFicha Pregunta)
    {
      return "width: " + CLogicaTarjeta.ANCHO_TARJETA.ToString() + "px; height: " + CLogicaTarjeta.ALTO_TARJETA.ToString() +
        "px; position: absolute; margin-left: " + Math.Floor(Pregunta.Abscisa + 0.5).ToString() +
        "px; margin-top: " + Math.Floor(Pregunta.Ordenada + 0.5).ToString() + "px;";
    }

    public static string IdGrafico(Int32 Pos)
    {
      return "IDGrafico" + Pos.ToString();
    }

    public string EstiloGrafico(CLinkGraficoCnt Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: " +
        Lnk.Alto.ToString() +
        "px; margin-left: " + Lnk.Abscisa.ToString() +
        "px; margin-top: " + Lnk.Ordenada.ToString() + "px; position: absolute; text-align: " +
        (Lnk.Clase == ClaseGrafico.BarrasH ? "left" : "center") +
        "; z-index: " + Lnk.NivelFlotante.ToString() + ";" +
        (Lnk.Clase == ClaseGrafico.BarrasH ? " overflow: hidden;" : "");
    }

    public string EstiloGraficoSuperior(CLinkGraficoCnt Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: 25px; text-align: center; overflow: hidden;";
    }

    public string EstiloContenedorGrafico(CLinkGraficoCnt Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: " + (Lnk.Alto - 25).ToString() +
        "px; text-align: center; overflow: hidden;";
    }

    public string EstiloOtro(CLinkOtro Lnk)
    {
      return "width: " + Math.Floor(Lnk.Datos.Ancho).ToString() + "px; height: " +
        Math.Floor(Lnk.Datos.Alto).ToString() +
        "px; margin-left: " + Math.Floor(Lnk.Datos.Posicion.X).ToString() +
        "px; margin-top: " + Math.Floor(Lnk.Datos.Posicion.Y).ToString() + "px; position: absolute; text-align: center" +
        "; z-index: " + Lnk.NivelFlotantePropio.ToString() + "; overflow: hidden;";
    }

    public string EstiloOtroSuperior(CLinkOtro Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: 25px; text-align: center; overflow: hidden;";
    }

    public string TransformacionMimico(CLinkOtro Mimico)
    {
      if (Mimico.ComponenteMimico == null || Mimico.ComponenteMimico.AnchoNecesario < 0.001 ||
          Mimico.ComponenteMimico.AltoNecesario < 0.001)
      {
        return "scale(0.25 , 0.25);";
      }
      else
      {
        double Factor = Math.Max(Mimico.ComponenteMimico.AnchoNecesario / Mimico.Datos.Ancho,
            Mimico.ComponenteMimico.AltoNecesario / Mimico.Datos.Alto);
        return "scale(" + Rutinas.CRutinas.FloatVStr(Factor) + " , " + Rutinas.CRutinas.FloatVStr(Factor) + ");";
      }
    }

    public string EstiloContenedorOtro(CLinkOtro Lnk)
    {
      return "width: " + Math.Floor(Lnk.Datos.Ancho).ToString() + "px; height: " + Math.Floor(Lnk.Datos.Alto - 25).ToString() +
        "px; text-align: center; overflow: hidden;";
    }

    public string IdMapa(Int32 Pos)
    {
      return "IDMapa" + Pos.ToString();
    }

    public string EstiloMapa(CLinkMapa Lnk)
    {
      return "width: " + Math.Floor(Lnk.Datos.Ancho).ToString() + "px; height: " +
        Math.Floor(Lnk.Datos.Alto).ToString() +
        "px; margin-left: " + Math.Floor(Lnk.Datos.Posicion.X).ToString() +
        "px; margin-top: " + Math.Floor(Lnk.Datos.Posicion.Y).ToString() + "px; position: absolute; text-align: center; z-index: " +
        Lnk.NivelFlotantePropio.ToString() + "; overflow: hidden; background-color: white;";
    }

    public string EstiloMapaSuperior(CLinkMapa Lnk)
    {
      return "width: " + Math.Floor(Lnk.Datos.Ancho).ToString() +
          "px; height: 25px; text-align: center; overflow: hidden;";
    }

    [Inject]
    NavigationManager NavigationManager { get; set; }

    public void MoverseAFicha(Int32 Codigo)
    {
      NavigationManager.NavigateTo("Tarjeta/" + Codigo.ToString(), false);
    }

    private string mszXML = "";
    [Parameter]
    public string XML
    {
      set
      {
        mszXML = value;
        if (mszXML.Length > 0)
        {
          PrecargarXML();
        }
      }
    }

    public string EstiloDivBotonMapa(CLinkMapa Mapa)
    {
      return "height: 25px; width: 25px; position: absolute; margin-left: " + Math.Floor(Mapa.Datos.Ancho - 27).ToString() +
          "px; margin-top: -38px; ";
    }

    public FncCompletar AlCompletarPedido { get; set; }

    public CSolapaCN Solapa { get; set; }
    public CPreguntaCN Pregunta { get; set; }

    public async Task<CProveedorComprimido> CrearProveedorAsync(HttpClient Http, 
        Int32 Indicador, Int32 Dimension, Int32 Elemento, ClaseElemento Clase, string TextoPrms)
    {
      try
      {
        switch (Clase)
        {
          case ClaseElemento.Indicador:
            RespuestaDatasetBin Respuesta = await Contenedores.CContenedorDatos.LeerDetalleDatasetAsync(Http,
                Indicador, Dimension, Elemento, -1);
            if (Respuesta.RespuestaOK)
            {
              CProveedorComprimido Proveedor = new CProveedorComprimido(ClaseElemento.NoDefinida, -1);
              Proveedor.ProcesarDatasetBinario(Respuesta.Datos, false);
              return Proveedor;
            }
            else
            {
              throw new Exception(Respuesta.MsgErr);
            }
          case ClaseElemento.SubConsulta:
            RespuestaDatasetBin RespuestaSC = await Contenedores.CContenedorDatos.LeerDetalleSubconsultaAsync(
                Http, Indicador, TextoPrms);
            if (RespuestaSC.RespuestaOK)
            {
              CProveedorComprimido Proveedor = new CProveedorComprimido(ClaseElemento.SubConsulta, Indicador);
              Proveedor.ProcesarDatasetBinarioUnicode(RespuestaSC.Datos, false);
              return Proveedor;
            }
            else
            {
              throw new Exception(RespuestaSC.MsgErr);
            }
          default:
            throw new Exception("Clase reloj no soportada");
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return null;
      }
    }

    private CLinkContenedorFiltros BuscarContenedorFiltros(Int32 Codigo, Int32 CodigoElementoDimension,
        ClaseElemento Clase = ClaseElemento.Indicador)
    {
      foreach (CLinkContenedorFiltros Filtro in mContenedoresFiltros)
      {
        if (Filtro.Indicador.Codigo == Codigo && Filtro.CodigoElemento == CodigoElementoDimension &&
            Filtro.Clase == Clase)
        {
          return Filtro;
        }
      }
      return null;
    }

    public static string XMLLimpia()
    {
      XDocument Documento = new XDocument(
        new XElement(CRutinas.CTE_INICIO));
      System.Text.StringBuilder Escritor = new System.Text.StringBuilder();
      Escritor.Append(Documento);
      return Escritor.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Indicador"></param>
    /// <param name="CodigoElementoDimension"></param>
    public CLinkReloj AgregarReloj(CDatoIndicador Indicador, Int32 CodigoElementoDimension, bool Visible,
        double Abscisa, double Ordenada, double Ancho, double Alto,
        ClaseElemento Clase = ClaseElemento.Indicador, string Prms = "")
    {

      CLinkReloj Reloj = new CLinkReloj();
      Reloj.Clase = Clase;
      Reloj.ParametrosSC = Prms;
      Reloj.Abscisa = (Int32)Abscisa;
      Reloj.Ordenada = (Int32)Ordenada;
      Reloj.Ancho = Ancho;
      Reloj.Alto = Alto;
      Reloj.Indicador = Indicador;
      Reloj.CodigoElemento = CodigoElementoDimension;
      Reloj.Visible = Visible;
      mRelojes.Add(Reloj);

      return Reloj;

    }

    public CLinkFicha AgregarFicha(CPreguntaCN Pregunta, double Abscisa, double Ordenada, double Ancho, double Alto)
    {
      CLinkFicha Ficha = new CLinkFicha();
      Ficha.Abscisa = (Int32)Abscisa;
      Ficha.Ordenada = (Int32)Ordenada;
      Ficha.Ancho = Ancho;
      Ficha.Alto = Alto;
      Ficha.Pregunta = Pregunta;
      mFichas.Add(Ficha);
      return Ficha;
    }

    private CLinkReloj BuscarReloj(Int32 Codigo, Int32 CodigoElemento, ClaseElemento Clase)
    {
      return (from L in mRelojes
              where L.Indicador.Codigo == Codigo && L.CodigoElemento == CodigoElemento &&
              L.Clase == Clase
              select L).FirstOrDefault();
    }

    private async Task<CLinkReloj> BuscarOCrearRelojAsync(HttpClient Http, Int32 Codigo, Int32 CodigoElemento,
          bool ConAlarmas = false, ClaseElemento Clase = ClaseElemento.Indicador,
          string Prms="")
    {

      CDatoIndicador Indicador = null;

      if (Clase == ClaseElemento.Indicador)
      {
        Contenedores.CContenedorDatos.IndicadorDesdeCodigo(Codigo);
        if (Indicador == null)
        {
          return null;
        }
      }
      else
			{
        Indicador = new CDatoIndicador()
        {
          Codigo = Codigo,
          Dimension = -1
        };
			}

      CLinkReloj Respuesta = BuscarReloj(Codigo, CodigoElemento, Clase);
      if (Respuesta == null)
      {
        Respuesta = AgregarReloj(Indicador, CodigoElemento, false, 0, 0, 100, 100, Clase, Prms);
      }

      if (Respuesta.Alarmas == null && ConAlarmas && Clase == ClaseElemento.Indicador)
      {
        try
        {
          Respuesta.Alarmas =
             await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(Http, Indicador, CodigoElemento);
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
      }

      return Respuesta;
    }

    public async Task AgregarTendenciaAsync(HttpClient Http, CDatoIndicador Indicador, Int32 CodigoElementoDimension, double Abscisa, double Ordenada,
        double Ancho, double Alto, string Adicionales="")
    {
      // Crea el elemento a incluir.
      CLinkTendencia Tendencia = new CLinkTendencia();
      Tendencia.Abscisa = (Int32)Abscisa;
      Tendencia.Ordenada = (Int32)Ordenada;
      Tendencia.Ancho = (Int32)Ancho;
      Tendencia.Alto = (Int32)Alto;
      Tendencia.Reloj = await BuscarOCrearRelojAsync(Http, Indicador.Codigo, CodigoElementoDimension, true);
      Tendencia.Reloj.HayVentanasDependientes = true;
      if (Adicionales.Length > 0)
      {
        try
        {
          Tendencia.MostrarReferencias = (Adicionales.Substring(0, 1) == "S");
          Tendencia.MostrarBarras = (Adicionales.Substring(1, 1) == "S");
          Tendencia.MostrarAcumulado = (Adicionales.Substring(2, 1) == "S");
          Tendencia.MostrarPromedio = (Adicionales.Substring(3, 1) == "S");
          Tendencia.MostrarEvolutivo = (Adicionales.Substring(4, 1) == "S");
          Adicionales = Adicionales.Substring(5);
          string[] DatosLocales = Adicionales.Split(CRutinas.CTE_SEPARADOR.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
          Int32 CantAdic = Int32.Parse(DatosLocales[0]);
          Int32 Pos = 1;
          for (Int32 i = 0; i < CantAdic && Pos < DatosLocales.Length; i++)
          {
            Int32 CodIndi = Int32.Parse(DatosLocales[Pos++]);
            Int32 CodElem = Int32.Parse(DatosLocales[Pos++]);
            Tendencia.IndicadoresAsociados.Add(CodIndi);
            Tendencia.CodigosElementosAsociados.Add(CodElem);
            Tendencia.EscalasDerechas.Add(DatosLocales[Pos] == "S");
            Pos++;
          }
        mTendencias.Add(Tendencia);
        }
        catch (Exception)
        {
          //
        }
      }
    }

    [Inject]
    public HttpClient Http { get; set; }

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
      if (mszXML.Length!=0)
			{
        _ = CargarXMLAsync(Http, mszXML);
        mszXML = "";
			}
			return base.OnAfterRenderAsync(firstRender);
		}

		public async Task<CLinkContenedorFiltros> AgregarContenedorFiltrosAsync(HttpClient Http,
          CDatoIndicador Indicador, Int32 CodigoElemento, ClaseElemento Clase, string Prms)
    {
      CLinkContenedorFiltros Filtro = new CLinkContenedorFiltros()
      {
        Indicador = Indicador,
        CodigoElemento = CodigoElemento,
        Clase = Clase
      };
      mContenedoresFiltros.Add(Filtro);
      await Filtro.CargarDatosFiltradorAsync(Http, this, Clase, Prms);
      return Filtro;
    }

    private async Task<CLinkContenedorFiltros> BuscarOCrearContenedorFiltrosAsync(HttpClient Http,
        Int32 CodigoIndicador, Int32 CodigoElementoDimension, ClaseElemento Clase = ClaseElemento.Indicador,
        string Prms="")
    {
      CLinkContenedorFiltros Filtros = BuscarContenedorFiltros(CodigoIndicador, CodigoElementoDimension,
            Clase);
      if (Filtros == null)
      {
        CDatoIndicador DefIndicador = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(CodigoIndicador);
        if (DefIndicador == null)
        {
          if (Clase == ClaseElemento.SubConsulta)
          {
            DefIndicador = new CDatoIndicador()
            {
              Codigo = CodigoIndicador,
              Dimension=Int32.MaxValue
            };
          }
          else
          {
            return null;
          }
        }
        Filtros = await AgregarContenedorFiltrosAsync(Http, DefIndicador, CodigoElementoDimension, Clase, Prms);
      }
      return Filtros;
    }

    public async Task AgregarGraficoAsync(HttpClient Http, CDatosGrafLista DatosGraf)
    {
      CGrafV2DatosContenedorBlock Datos = DatosGraf.Datos;

      CLinkContenedorFiltros Filtro = await BuscarOCrearContenedorFiltrosAsync(Http, Datos.Indicador, Datos.CodigoElementoDimension);

      if (Filtro != null)
      {
        CLinkGraficoCnt Grafico = new CLinkGraficoCnt()
        {
          Datos = DatosGraf,
          Filtros = Filtro,
          Reloj = await BuscarOCrearRelojAsync(Http, Datos.Indicador, Datos.CodigoElementoDimension, false),
          Abscisa = (Int32)Datos.Posicion.X,
          Ordenada = (Int32)Datos.Posicion.Y,
          Ancho = (Int32)Datos.Ancho,
          Alto = (Int32)Datos.Alto
        };
        mGraficos.Add(Grafico);
        if (Grafico.ComponentePropio != null)
        {
          Grafico.ComponentePropio.Proveedor = Filtro.Filtrador.Proveedor;
        }
      }

    }

    public async Task AgregarConjuntoAsync(HttpClient Http, CDatosGrafLista DatosGraf)
    {

      await AgregarGraficoAsync(Http, DatosGraf);

    }

    public List<CColumnaBase> ColumnasDataset(Int32 Indicador)
    {
      if (mContenedoresFiltros != null)
      {
        foreach (CLinkContenedorFiltros Lnk in mContenedoresFiltros)
        {
          if (Lnk.Indicador.Codigo == Indicador && Lnk.Filtrador != null && Lnk.Filtrador.Proveedor != null)
          {
            return Lnk.Filtrador.Proveedor.Columnas;
          }
        }
      }
      return new List<CColumnaBase>();
    }

    public List<CLineaComprimida> DatosSinFiltro(Int32 Indicador, Int32 Elemento)
    {
      foreach (CLinkContenedorFiltros Lnk in mContenedoresFiltros)
      {
        if (Lnk.Indicador.Codigo == Indicador && Lnk.CodigoElemento == Elemento)
        {
          return Lnk.Filtrador.DatosFiltrados;
        }
      }
      return new List<CLineaComprimida>();
    }

    public async Task AgregarMapaAsync(HttpClient Http, CDatosMapaLista Datos)
    {
      CLinkContenedorFiltros Filtros = await BuscarOCrearContenedorFiltrosAsync(Http, Datos.CodigoIndicador,
          Datos.CodigoElementoDimension);

      CLinkMapa Mapa = new CLinkMapa()
      {
        Datos = Datos,
        Filtros = Filtros
      };
      mMapas.Add(Mapa);
      StateHasChanged();
    }

    public void AgregarMimico(CDatosOtroLista Datos)
    {

      CLinkOtro Otro = new CLinkOtro()
      {
        Datos = Datos,
        Filtros = null
      };
      mOtros.Add(Otro);

    }

    public async Task AgregarGrilla(HttpClient Http,  CDatosOtroLista Datos)
    {

      CLinkContenedorFiltros Filtros = await BuscarOCrearContenedorFiltrosAsync( Http, Datos.CodigoPropio, Datos.CodigoElementoDimension);

      if (Filtros != null)
      {
        CLinkOtro Otro = new CLinkOtro()
        {
          Datos = Datos,
          Filtros = Filtros
        };
        mGrillas.Add(Otro);
      }

      StateHasChanged();

    }

    private List<CFiltradorStep> ExtraerFiltros(List<CPasoCondicionesBlock> Filtros, List<CColumnaBase> Columnas)
    {
      List<CFiltradorStep> Respuesta = new List<CFiltradorStep>();
      foreach (CPasoCondicionesBlock Paso in Filtros)
      {
        Respuesta.Add(ExtraerFiltradorStepDesdePasoCondiciones(Paso, Columnas));
      }
      return Respuesta;
    }

    private CFiltradorStep ExtraerFiltradorStepDesdePasoCondiciones(CPasoCondicionesBlock Cnd, List<CColumnaBase> Columnas)
    {
      CFiltradorStep Paso = new CFiltradorStep();
      Paso.CumplirTodas = false;
      foreach (CGrupoCondicionesBlock Grupo in Cnd.Grupos)
      {
        CCondiciones CndsLocal = new CCondiciones();
        CndsLocal.TodasLasCondiciones = false;
        CndsLocal.IncluyeCondiciones = true;
        foreach (CCondicionBlock CndBlock in Grupo.Condiciones)
        {
          CCondicion CndLocal = new CCondicion();
          CColumnaBase Columna = (from C in Columnas
                                  where C.Nombre == CndBlock.Columna
                                  select C).FirstOrDefault();
          if (Columna != null)
          {
            CndLocal.Clase = Columna.Clase;
            CndLocal.Modo = CndBlock.Modo;
            CndLocal.ValorIgual = CndBlock.Valor;
            CndLocal.ColumnaCondicion = Columna.Orden;
            switch (CndLocal.Modo)
            {
              case ModoFiltrar.Igual:
                break;
              default:
                if (Columna.Clase == ClaseVariable.Fecha)
                {
                  CndLocal.ValorMinimo = CRutinas.DecodificarFechaHora(CndBlock.Valor).ToOADate();
                  CndLocal.ValorMaximo = CRutinas.DecodificarFechaHora(CndBlock.ValorMaximo).ToOADate();
                }
                else
                {
                  CndLocal.ValorMinimo = CRutinas.StrVFloat(CndBlock.Valor);
                  CndLocal.ValorMaximo = CRutinas.StrVFloat(CndBlock.ValorMaximo);
                }
                break;
            }
            CndLocal.AjustarIndices(Columnas);
            CndsLocal.AgregarCondicion(CndLocal);
          }
        }
        Paso.AgregarCondicion(CndsLocal);
      }
      return Paso;
    }

    private CFiltradorStep ExtraerFiltro(CGrafV2DatosContenedorBlock Datos, List<CColumnaBase> Columnas)
    {
      if (Datos.Filtro != null)
      {
        if (Datos.Filtro.Grupos.Count != 1 || Datos.Filtro.Grupos[0].Condiciones.Count == 0 ||
            Datos.Filtro.Grupos[0].Condiciones[0].Columna != Datos.ColumnaAbscisas)
        {
          return ExtraerFiltradorStepDesdePasoCondiciones(Datos.Filtro, Columnas);
        }
      }
      return null;
    }

    private List<CLineaComprimida> FiltrarDatosInicialesBlock(List<CColumnaBase> ColsIni, List<CLineaComprimida> LinsIni,
          List<CFiltradorStep> FiltrosBlocks)
    {
      if (FiltrosBlocks != null && FiltrosBlocks.Count > 0)
      {
        for (Int32 i = 0; i < FiltrosBlocks.Count; i++)
        {
          FiltrosBlocks[i].FiltrarDatos((i == 0 ? LinsIni : FiltrosBlocks[i - 1].Datos), ColsIni);
        }
        return FiltrosBlocks.Last().Datos;
      }
      else
      {
        return LinsIni;
      }
    }

    public static Point ObtenerPosicion(XElement Elemento, out double Ancho, out double Alto)
    {
      double Absc=CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ABSCISA).Value);
      double Ord = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ORDENADA).Value);
      Ancho = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ANCHO).Value);
      Alto = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_ALTO).Value);
      return new Point(Absc, Ord);
    }

    private void CargarIndicador(XElement Elemento)
    {

      Int32 CodigoIndicador=Int32.Parse( Elemento.Attribute(CRutinas.CTE_CODIGO).Value);
      Int32 CodigoElementoDimension = Int32.Parse(Elemento.Attribute(CRutinas.CTE_ELEMENTO_DIMENSION).Value);

      double Ancho;
      double Alto;
      Point Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);

      CDatoIndicador Datos=Contenedores.CContenedorDatos.IndicadorDesdeCodigo(CodigoIndicador);

      if (Datos != null)
      {
        System.Xml.Linq.XAttribute Atr = Elemento.Attribute(CRutinas.CTE_VISIBLE);
        bool Visible = (Atr == null ? true : (Atr.Value != "N"));
        AgregarReloj(Datos, CodigoElementoDimension, Visible, Posicion.X, Posicion.Y, Ancho, Alto);
      }

    }

    private async Task CargarTendenciaAsync(HttpClient Http, XElement Elemento)
    {

      Int32 CodigoIndicador = Int32.Parse(Elemento.Attribute(CRutinas.CTE_CODIGO).Value);
      Int32 CodigoElementoDimension = Int32.Parse(Elemento.Attribute(CRutinas.CTE_ELEMENTO_DIMENSION).Value);

      double Ancho;
      double Alto;
      Point Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);

      CDatoIndicador Datos = Contenedores.CContenedorDatos.IndicadorDesdeCodigo(CodigoIndicador);

      if (Datos != null)
      {
        XAttribute Atr = Elemento.Attribute(CRutinas.CTE_CONDICIONES);
        string Adicionales = (Atr == null ? "" : Elemento.Attribute(CRutinas.CTE_CONDICIONES).Value);

        Atr = Elemento.Attribute(CRutinas.CTE_VISIBLE);
        bool Visible = (Atr == null ? true : (Atr.Value != "N"));
        await AgregarTendenciaAsync(Http, Datos, CodigoElementoDimension, Posicion.X, Posicion.Y, Ancho, Alto, Adicionales);
      }

    }

    private void CargarFicha(XElement Elemento)
    {

      Int32 CodigoFicha = Int32.Parse(Elemento.Attribute(CRutinas.CTE_CODIGO).Value);

      CPreguntaCN DatosFicha = Contenedores.CContenedorDatos.PreguntaDesdeCodigo(CodigoFicha);
      if (DatosFicha != null)
      {
        double Ancho;
        double Alto;
        Point Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);
        AgregarFicha(DatosFicha, Posicion.X, Posicion.Y, Ancho, Alto);
      }

    }

    private async Task CargarGraficoAsync(HttpClient Http, XElement Elemento)
    {

      CGrafV2DatosContenedorBlock Graf = new CGrafV2DatosContenedorBlock();

      Graf.ClaseElemento = (CGrafV2DatosContenedorBlock.ClaseBlock)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE_BLOCK);
      Graf.Clase = (ClaseGrafico)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CLASE);

      Graf.Nombre = Elemento.Attribute(CRutinas.CTE_NOMBRE).Value;

      double Ancho;
      double Alto;
      Graf.Posicion = ObtenerPosicion(Elemento, out Ancho, out Alto);
      Graf.Ancho = Ancho;
      Graf.Alto = Alto;

      Graf.Indicador = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_CODIGO);
      Graf.CodigoElementoDimension = CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_ELEMENTO_DIMENSION);

      Graf.AgrupIndep = (ModoAgruparIndependiente)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_AGRUPACION_IND);
      Graf.Agrupacion = (ModoAgruparDependiente)CRutinas.ExtraerAtributoEntero(Elemento, CRutinas.CTE_AGRUPACION_DEP);

      Graf.ColumnaAbscisas = Elemento.Attribute(CRutinas.CTE_COLUMNA_ABSC).Value;
      Graf.ColumnaOrdenadas = Elemento.Attribute(CRutinas.CTE_COLUMNA_ORD).Value;
      Graf.ColumnaSexo = Elemento.Attribute(CRutinas.CTE_COLUMNA_SEXO).Value;

      Graf.SaltoHistograma = CRutinas.StrVFloat(Elemento.Attribute(CRutinas.CTE_SALTO_HISTOGRAMA).Value);

      Graf.Visible = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_VISIBLE);
      Graf.UsaFiltroPropio = CRutinas.ExtraerAtributoBooleano(Elemento, CRutinas.CTE_FILTRO_PROPIO);

      XElement ElFiltro = Elemento.Element(CRutinas.CTE_FILTRO);
      if (ElFiltro != null)
      {
        Graf.Filtro = new CPasoCondicionesBlock();
        Graf.Filtro.CargarDesdeXML(ElFiltro);
      }

      ElFiltro = Elemento.Element(CRutinas.CTE_FILTROS);
      if (ElFiltro != null)
      {
        XElement Filtros = new XElement(CRutinas.CTE_FILTROS);
        foreach (XElement ElFiltroLocal in ElFiltro.Elements(CRutinas.CTE_FILTRO))
        {
          CPasoCondicionesBlock FLocal = new CPasoCondicionesBlock();
          FLocal.CargarDesdeXML(ElFiltroLocal);
          Graf.FiltrosBlock.Add(FLocal);
        }
      }

      switch (Graf.ClaseElemento)
      {
        case CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
          await AgregarConjuntoAsync(Http, new CDatosGrafLista(Graf));
          break;
        default:
          await AgregarGraficoAsync(Http, new CDatosGrafLista(Graf));
          break;
      }

    }

    private void CargarMapa(HttpClient Http, XElement Elemento)
    {

      CDatosMapaLista Datos = new CDatosMapaLista();
      Datos.CargarDesdeXML(Elemento);

      _ = AgregarMapaAsync(Http, Datos);

    }

    private async Task<CLinkContenedorFiltros> ProcesarDatasetAsync(Int32 CodigoSC, byte[] Datos, CDatosOtroLista DatosSC)
    {
      CLinkContenedorFiltros Filtro = await BuscarOCrearContenedorFiltrosAsync(Http, CodigoSC, -1,
            ClaseElemento.SubConsulta, DatosSC.Parametros);
      CProveedorComprimido ProvDatos = new Datos.CProveedorComprimido(ClaseElemento.Consulta, CodigoSC);
      ProvDatos.ProcesarDatasetBinarioUnicode(Datos, false);
      CFiltrador Filtrador = new CFiltrador()
      {
        Proveedor = ProvDatos
      };
      Filtro.Filtrador = Filtrador;
      return Filtro;
    }

    private async Task AgregarSubconsultaAsync(CDatosOtroLista Datos)
    {
      try
      {

        CLinkContenedorFiltros Filtro = await BuscarOCrearContenedorFiltrosAsync(Http, Datos.Codigo, -1,  ClaseElemento.SubConsulta, Datos.Parametros);

        CLinkOtro SC = new CLinkOtro()
        {
          Datos = Datos,
          Filtros = Filtro
        };
        mOtros.Add(SC);

        StateHasChanged();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

    private void CargarOtroLista(HttpClient Http, XElement Elemento)
    {

      try
      {

        CDatosOtroLista Datos = new CDatosOtroLista();
        Datos.CargarDesdeXML(Elemento);

        switch (Datos.Clase)
        {
          case CGrafV2DatosContenedorBlock.ClaseBlock.Grilla:
            _ = AgregarGrilla(Http, Datos);
            break;
          case CGrafV2DatosContenedorBlock.ClaseBlock.Mimico:
            AgregarMimico(Datos);
            break;
          case CGrafV2DatosContenedorBlock.ClaseBlock.Subconsulta:
            _ = AgregarSubconsultaAsync(Datos);
            break;
        }
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
				throw;
			}

    }

    public bool ACargar { get; set; }

    public List<CLinkBase> LinksEnBlock { get; set; } = null;

    private void PrecargarXML()
		{
      LinksEnBlock = new List<CLinkBase>();
      XDocument Documento = XDocument.Parse(mszXML);
      XElement Inicio = Documento.Element(CRutinas.CTE_INICIO);
      // Mantener una lista de los datos cargados.
      foreach (XElement Elemento in Inicio.Elements(CRutinas.CTE_ELEMENTO))
      {
        double AnchoLocal;
        double AltoLocal;
        Point Posicion = CContenedorBlocks.ObtenerPosicion(Elemento, out AnchoLocal, out AltoLocal);
        LinksEnBlock.Add(new CLinkBase()
        {
          Abscisa = (Int32)Posicion.X,
          Ordenada = (Int32)Posicion.Y,
          Ancho = (Int32)AnchoLocal,
          Alto = (Int32)AltoLocal
        });
      }

    }

    public async Task CargarXMLAsync(HttpClient Http, string XML)
    {
      try
      {
//        Contenedor.CloseAllWindows();
        XDocument Documento = XDocument.Parse(XML);
        XElement Inicio = Documento.Element(CRutinas.CTE_INICIO);
        // Mantener una lista de los datos cargados.
        foreach (XElement Elemento in Inicio.Elements(CRutinas.CTE_ELEMENTO))
        {
          XAttribute AtrClase = Elemento.Attribute(CRutinas.CTE_CLASE_BLOCK);
          switch ((CGrafV2DatosContenedorBlock.ClaseBlock)Int32.Parse(AtrClase.Value))
          {
            case CGrafV2DatosContenedorBlock.ClaseBlock.Indicador:
              CargarIndicador(Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Tendencia:
              await CargarTendenciaAsync(Http, Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Grafico:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Conjunto:
              await CargarGraficoAsync(Http, Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Grilla:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Mimico:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Subconsulta:
              CargarOtroLista(Http, Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaCalor:
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaControl:
            case CGrafV2DatosContenedorBlock.ClaseBlock.MapaGradientes:
            case CGrafV2DatosContenedorBlock.ClaseBlock.Pines:
              CargarMapa(Http, Elemento);
              break;
            case CGrafV2DatosContenedorBlock.ClaseBlock.Ficha:
              CargarFicha(Elemento);
              break;
          }
        }

        ACargar = false;

        foreach (CLinkReloj Reloj in mRelojes)
				{
          _ = Reloj.CargarDatosProveedorAsync(Http, this);
				}

//        LinksEnBlock = null;

        StateHasChanged();

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
      }
    }

  }

}
