using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaIndicador : ComponentBase, IDisposable
  {

    protected static CLogicaIndicador gPuntero = null;

    private Int32 mCodigo = -1;

    public static CLogicaIndicador gDetalle = null;

    public void FncResizeEvento(string Datos)
    {
      if (gPuntero != null)
      {
        Ayuda = "QQQQ";
        StateHasChanged();
      }
    }

    public Int32 PeriodoAlarmaTendRed
    {
      get
      {
        if (ComponenteTendRed != null)
        {
          return ComponenteTendRed.PeriodoSeleccionado;
          //try
          //{
          //  List<CInformacionAlarmaCN> Alarmas = (from A in Contenedores.CContenedorDatos.gAlarmasIndicador
          //                                        where A.CodigoIndicador == mComponenteTendRed.Indicador.Codigo &&
          //                                          A.ElementoDimension == mComponenteTendRed.CodigoElementoDimension
          //                                        orderby A.Periodo
          //                                        select A).ToList();
          //  if (Alarmas != null && Alarmas.Count > 0)
          //  {
          //    return Alarmas.Last().Periodo;
          //  }
          //}
          //catch (Exception)
          //{
          //  //
          //}
        }

        return -1;

      }
    }

    public bool HayAlarmaTendRed
    {
      get
      {
        if (ComponenteTendRed != null)
        {
          try
          {
            List<CInformacionAlarmaCN> Alarmas = (from A in Contenedores.CContenedorDatos.gAlarmasIndicador
                                                  where A.CodigoIndicador == mComponenteTendRed.Indicador.Codigo &&
                                                    A.ElementoDimension == mComponenteTendRed.CodigoElementoDimension
                                                  orderby A.Periodo
                                                  select A).ToList();
            return (Alarmas != null && Alarmas.Count > 0);
          }
          catch (Exception)
          {
            //
          }
        }

        return false;

      }
    }

    public Int32 PosicionPuntoTendencia
    {
      get
      {
        return mPosicionPuntoTendencia;
      }
      set
      {
        mPosicionPuntoTendencia = value;
      }
    }

    private static void CopiarDatosEntreLogicaIndicadores(CLogicaIndicador L1, CLogicaIndicador L2)
		{
      L2.mCodigo = L1.mCodigo;
      L2.mPosicionPuntoTendencia = L1.mPosicionPuntoTendencia;
      L2.AguardandoReloj = L1.AguardandoReloj;
      L2.AguardandoFiltros = L1.AguardandoFiltros;
      L2.RelojEncima = L1.RelojEncima;
      L2.TendenciasAmpliadas = L1.TendenciasAmpliadas;
      L2.TendenciasEncima = L1.TendenciasEncima;
      L2.GrillaEncima = L1.GrillaEncima;
      L2.ComponenteFiltrosEncima = L1.ComponenteFiltrosEncima;
      L2.mAlarmas = L1.mAlarmas;
      L2.mbLeyo = L1.mbLeyo;
      L2.mPosicionPuntoTendencia = L1.mPosicionPuntoTendencia;
      L2.CodigoElementoDimension = L1.CodigoElementoDimension;
      L2.Curvas = L1.Curvas;
      L2.mBlocksDatos = L1.mBlocksDatos;
      L2.mAbscisaTendenciaAnterior = L1.mAbscisaTendenciaAnterior;
      L2.mAbscisaTendencia = L1.mAbscisaTendencia;
      L2.mOrdenadaTendencia = L1.mOrdenadaTendencia;
      L2.mOrdenadaTendenciaAnterior = L1.mOrdenadaTendenciaAnterior;
      L2.mAnchoTendencia = L1.mAnchoTendencia;
      L2.mAnchoTendenciaAnterior = L1.mAnchoTendenciaAnterior;
      L2.mAltoTendencia = L1.mAltoTendencia;
      L2.mAltoTendenciaAnterior = L1.mAltoTendenciaAnterior;
      L2.HayAlarmaReducida = L1.HayAlarmaReducida;
      L2.HayFiltroDatos = L1.HayFiltroDatos;
      L2.HayTendencias = L1.HayTendencias;
      L2.mComponenteReloj = L1.mComponenteReloj;
      L2.mComponenteTendencias = L1.mComponenteTendencias;
      L2.mComponenteTendRed = L1.mComponenteTendRed;
      L2.mDimensionCaracter = L1.mDimensionCaracter;
      L2.Indicador = L1.Indicador;
      L2.VerDetalleIndicador = L1.VerDetalleIndicador;
      L2.ClaseOrigen = L1.ClaseOrigen;
      L2.CodigoOrigen = L1.CodigoOrigen;
      L2.mColumnasDataset = L1.mColumnasDataset;
      L2.mPeriodoDataset = L1.mPeriodoDataset;

      //L2.mProveedor = new CProveedorComprimido(L1.mProveedor.ClaseOrigen, L1.mProveedor.CodigoOrigen)
      //{
      //  Columnas = L1.mProveedor.Columnas,
      //  Datos = L1.mProveedor.Datos
      //};

      L2.mProveedor = L1.mProveedor;

      if (L1.ComponenteFiltros != null)
			{
        L2.ComponenteFiltros = L1.ComponenteFiltros;
			}
		}

    private void EliminarRelacionesComponentes(CLogicaIndicador L)
		{
			L.mComponenteFiltros.Contenedor = this;
			if (L.mComponenteFiltros.Grilla != null)
			{
				L.mComponenteFiltros.Grilla.Componente = null;
			}
			foreach (CLinkFiltros Lnk in L.mComponenteFiltros.Links)
			{
				Lnk.Componente = null;
			}
			foreach (CLinkGrafico Lnk in L.mComponenteFiltros.Graficos)
			{
				Lnk.Componente = null;
			}
		}

		#region Datos

		public bool AguardandoReloj { get; set; } = !Contenedores.CContenedorDatos.SiempreTendencia;

    public bool AguardandoFiltros { get; set; } = true;

    public bool Procesando { get; set; } = false;

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public bool RelojEncima
    {
      get { return (ComponenteReloj == null ? false : ComponenteReloj.Encima); }
      set
      {
        if (value != RelojEncima && ComponenteReloj != null)
        {
          ComponenteReloj.ImponerEncima(value);
        }
      }
    }

    public bool TendenciasAmpliadas { get; set; } = false;

    [Parameter]
    public bool TendenciasEncima { get; set; } = false;

    public bool GrillaEncima { get; set; } = false;

    [Parameter]
    public bool ComponenteFiltrosEncima { get; set; } = false;

    private List<CInformacionAlarmaCN> mAlarmas = null;

    protected bool mbLeyo = false;

    private bool mbRetrocediendo = false; // cuando se recuperan los datos de memoria.

    [Parameter]
    public Int32 Codigo
    {
      get { return mCodigo; }
      set
      {
        if (mCodigo != value)
        {
          mbLeyo = false;
          mCodigo = value;
          try
          {
            HayFiltroDatos = false;
            ComponenteFiltros = null;
          }
          catch (Exception)
					{
            //
					}
          Indicador = (from I in Contenedores.CContenedorDatos.ListaIndicadores
                       where I.Codigo == mCodigo
                       select I).FirstOrDefault();
          ClaseOrigen = ClaseElemento.Indicador;
          CodigoOrigen = Codigo;
          StateHasChanged();
        }
      }
    }

    private Int32 mPosicionPuntoTendencia = -1;
    public Int32 CodigoElementoDimension { get; set; } = -1;

    public static string Ayuda { get; set; } = "AA";

    public ObservableCollection<Clases.CCurvaTendencia> Curvas { get; set; } = new ObservableCollection<Clases.CCurvaTendencia>();

    private long mAnchoTendencia = AnchoTendenciasDefault;

    private long mAltoTendencia = AltoTendenciaDefault;

    private Int32 mAbscisaTendencia = ABSCISA_INI_TENDENCIAS;

    private Int32 mOrdenadaTendencia = 5;

    public bool DatosLeidos
		{
      get { return mPeriodoDataset > 0; }
		}

    private Int32 mPeriodoDataset = -1;
    private List<CColumnaBase> mColumnasDataset;
    private CProveedorComprimido mProveedor = null;

    public bool VerDetalleIndicador { get; set; } = false;

    public ClaseElemento ClaseOrigen { get; set; } = ClaseElemento.NoDefinida;
    public Int32 CodigoOrigen { get; set; } = -1;

    protected List<CInformacionAlarmaCN> mAlarmasImpuestas = null;

    private List<BlockDatosZip> mBlocksDatos = new List<BlockDatosZip>();

    private Int32 mAbscisaTendenciaAnterior = -999999;
    private Int32 mOrdenadaTendenciaAnterior = -999999;
    private long mAnchoTendenciaAnterior = -999999;
    private long mAltoTendenciaAnterior = -999999;

    public bool HayTendencias { get; set; }
    public bool HayFiltroDatos { get; set; } = false;

    private CReloj mComponenteReloj = null;
    private CLogicaTrendRed mComponenteTendRed = null;

    private bool mbAlarmaReducida = false;
    private CLogicaTendencias mComponenteTendencias = null;

    private CLogicaContenedorFiltros mComponenteFiltros = null;
    private CLinkGrafico mGraficoDrag = null;
    private CLinkFiltros mFiltroDrag = null;
    private LineaFiltro mLineaDrag = null;
    private CLinkGrilla mGrillaDrag = null;
    private double mDimensionCaracter = -1;
    private string mbSinTendencia = "disabled";
    private CBaseGrafico mGraficoSeleccionadoImpuesto = null;
    private CDatoIndicador mIndicador = null;

    #endregion

    [JSInvokable]
    public static Task FncAyudaAsync()
    {
      Ayuda = "Entro";

      if (gPuntero != null)
      {
        gPuntero.StateHasChanged();
      }

      return new Task(null);

    }

    private async Task LeerAlarmasAsync(bool Forzadas)
    {
      if (ComponenteReloj != null)
      {
        if (ComponenteReloj.Alarmas == null || Forzadas)
        {
          ComponenteReloj.AlarmasLeidas = await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(Http, Indicador, -1, Forzadas);
          if (!Forzadas && ComponenteReloj.Alarmas != null && ComponenteReloj.Alarmas.Count < 2)
          {
            SinTendencia = "disabled";
            _ = LeerAlarmasAsync(true);
          }
          else
          {
            SinTendencia = (ComponenteReloj.Alarmas == null || ComponenteReloj.Alarmas.Count == 0 ? "disabled" : "enabled");
          }
          AguardandoReloj = false;
          StateHasChanged();
        }
      }
    }

    public static long AnchoTendenciasDefault
    {
      get
      {
        return Math.Max(400, (int)Math.Floor(Contenedores.CContenedorDatos.AnchoPantallaIndicadores / 2.5));
      }
    }

    private void InicializarZIndex()
		{
      TendenciasEncima = false;
      ComponenteFiltrosEncima = false;

      if (ComponenteFiltros != null)
      {
        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
        {
          Link.Encima=false;
        }

        foreach (CLinkGrafico Lnk in ComponenteFiltros.Graficos)
        {
          Lnk.Encima = false;
          Lnk.Componente.DeSeleccionar();
        }
      }

		}

    public void PonerTendenciaArriba()
    {
      if (!TendenciasEncima)
      {
        PonerElementoEncima(false, true, false, -1, -1);
        StateHasChanged();
      }
    }

    public void PonerContenedorFiltroArriba()
		{
      if (!ComponenteFiltrosEncima)
      {
        PonerElementoEncima(true, false, false, -1, -1);
        StateHasChanged();
      }
		}

    public static Int32 AbscisaReloj
    {
      get
      {
        if (gPuntero == null || gPuntero.ComponenteReloj == null)
        {
          return 10;
        }
        else
        {
          return gPuntero.ComponenteReloj.Abscisa;
        }
      }
    }

    public static Int32 OrdenadaReloj
    {
      get
      {
        if (gPuntero == null || gPuntero.ComponenteReloj == null)
        {
          return 5;
        }
        else
        {
          return gPuntero.ComponenteReloj.Ordenada;
        }
      }
    }

    private static long ConvertirDoubleLng(double R)
    {
      return (long)Math.Floor(R + 0.5);
    }

    public long AnchoTendencia
    {
      get
      {
        return mAnchoTendencia;
      }
      set
			{
        mAnchoTendencia = value;
        if (ComponenteTendencias != null)
				{
          ComponenteTendencias.ImponerAncho(value);
          ComponenteTendencias.AnchoGrafico = value;
				}
			}
    }

    public static long AltoTendenciaDefault
    {
      get { return Math.Max(250, Contenedores.CContenedorDatos.AltoPantallaIndicadores / 3); }
    }

    public long AltoTendencia
    {
      get
      {
          return mAltoTendencia;
      }
      set
			{
        mAltoTendencia = value;
        if (ComponenteTendencias != null)
        {
          ComponenteTendencias.ImponerAlto(value);
        }
      }
    }

    public Int32 AbscisaTendencia
    {
      get
      {
        return mAbscisaTendencia;
      }
      set
      {
        mAbscisaTendencia = value;
      }
    }

    public Int32 OrdenadaTendencia
    {
      get
      {
        return mOrdenadaTendencia;
      }
      set
			{
        mOrdenadaTendencia = value;
			}
    }

    public static Int32 AbscisaContenedorFiltros
    {
      get
      {
        if (gPuntero == null || gPuntero.ComponenteFiltros == null)
        {
          return 10;
        }
        else
        {
          return gPuntero.ComponenteFiltros.Abscisa;
        }
      }
    }

    public static Int32 OrdenadaContenedorFiltros
    {
      get
      {
        if (gPuntero != null && gPuntero.ComponenteTendRed != null)
        {
          return 305;
        }
        else
        {
          if (gPuntero != null && gPuntero.ClaseOrigen == ClaseElemento.SubConsulta)
          {
            return 4;
          }
          else
          {
            if (gPuntero == null || gPuntero.ComponenteFiltros == null)
            {
              return 215;
            }
            else
            {
              return gPuntero.ComponenteFiltros.Ordenada;
            }
          }
        }
      }
    }

    public static int ANCHO_RELOJ = 260;
    public static int ALTO_RELOJ = 200;
    public static int SEPARACION = 10;
    public static int ABSCISA_INI_TENDENCIAS = 280;

    public string EstiloAguardando
		{
      get
			{
        return "position: absolute; left: 0px; top: 0px; background: #BEC9E7; opacity: 1; width: 100%; height: 100%;";
			}
		}

    public string EstiloDetalleAguardando
    {
      get
      {
        return "position: absolute; left: 0px; top: 0px; width: 100%; height: 100%;";
      }
    }

    public string TextoAguardando
		{
      get
			{
        return "Aguarde, por favor";
			}
		}

    public static Int32 ANCHO_PANTALLA_RELOJ
    {
      get
      {
        return (Contenedores.CContenedorDatos.SiempreTendencia ? 370 : 260);
      }
    }

    public static Int32 ALTO_PANTALLA_RELOJ
    {
      get
      {
        return (Contenedores.CContenedorDatos.SiempreTendencia ? 290 : 200);
      }
    }

    public string EstiloReloj
    {
      get
      {
        return "width: " + ANCHO_PANTALLA_RELOJ.ToString() + "px; height: " + ALTO_PANTALLA_RELOJ.ToString() +
            "px; margin-left: " + AbscisaReloj.ToString() +
            "px; margin-top: " + OrdenadaReloj.ToString() + "px; position: absolute; text-align: center; z-index: " +
            RelojEncima.ToString() + ";";
      }
    }


    public string EstiloTendencias
    {
      get
      {
        return "width: " + AnchoTendencia.ToString() + "px; height: " +
          AltoTendencia.ToString() +
          "px; z-index: " + TendenciasEncima.ToString() +
          "; margin-left: " + AbscisaTendencia.ToString() +
          "px; margin-top: "+OrdenadaTendencia.ToString()+"px; position: absolute; text-align: center; overflow: hidden;";
      }
    }

    public Int32 AltoContenedor
    {
      get
      {
        if (ClaseOrigen == ClaseElemento.SubConsulta)
        {
          return Contenedores.CContenedorDatos.AltoPantalla / 2 - 8;
        }
        else
        {
          if (Contenedores.CContenedorDatos.SiempreTendencia)
          {
            return Math.Max(180, Contenedores.CContenedorDatos.AltoPantalla - 338);
          }
          else
          {
            return Math.Max(180, Contenedores.CContenedorDatos.AltoPantalla - 248);
          }
        }
      }
    }

    public static Int32 AltoFiltro
    {
      get
      {
        return Math.Max(180, Contenedores.CContenedorDatos.AltoPantallaIndicadores - 2 * Logicas.CLogicaIndicador.SEPARACION - 5 -
        (Int32)Logicas.CLogicaIndicador.AltoTendenciaDefault);
      }
    }

    public string EstiloContenedorFiltros
    {
      get
      {
        return "width: 260px; height: " +
          AltoContenedor.ToString() +
          "px; margin-left: " + AbscisaContenedorFiltros.ToString() + "px; margin-top: " +
          OrdenadaContenedorFiltros.ToString() + "px; position: absolute; text-align: center;";
      }
    }

    private Int32 AbscisaFiltro(CLinkFiltros Lnk)
    {
      if (Lnk.Abscisa < -998)
      {
        Lnk.Abscisa = 280 + (from L in ComponenteFiltros.Links
                             where L.PosicionEnPantalla >= 0 && L.PosicionEnPantalla < Lnk.PosicionEnPantalla
                             select (L.Ancho + 10)).Sum();
      }
      return Lnk.Abscisa;
    }

    private Int32 OrdenadaFiltro(CLinkFiltros Lnk)
    {
      if (Lnk.Ordenada < -998)
      {
        Lnk.Ordenada = (Int32)Logicas.CLogicaIndicador.AltoTendenciaDefault + Logicas.CLogicaIndicador.SEPARACION + 5;
      }
      return Lnk.Ordenada;
    }

    public string EstiloFiltro(CLinkFiltros Lnk)
    {
      Int32 AnchoLocal = Math.Max(Lnk.Ancho, Math.Max(185, (from L in ComponenteFiltros.Links
                                                            where L.PosicionUnica == Lnk.PosicionUnica
                                                            select L.Ancho).FirstOrDefault()));

      return "width: " + AnchoLocal.ToString() + "px; height: " +
        AltoFiltro.ToString() +
        "px; margin-left: " + AbscisaFiltro(Lnk).ToString() +
        "px; margin-top: " + OrdenadaFiltro(Lnk).ToString() +
        "px; position: absolute; text-align: center; resize: horizontal; overflow: hidden;";
    }

    public string EstiloGrafico(CLinkGrafico Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: " +
        Lnk.Alto.ToString() +
        "px; margin-left: " + Lnk.Abscisa.ToString() +
        "px; margin-top: " + Lnk.Ordenada.ToString() + "px; position: absolute; text-align: " +
        (Lnk.Clase == ClaseGrafico.BarrasH ? "left" : "center") +
        (Lnk.Clase == ClaseGrafico.BarrasH ? "; overflow: hidden;" : ";")+
        " resize: both; overflow: hidden;";
    }

    public string EstiloGrilla
    {
      get
      {
        return "width: " + Grilla.Ancho.ToString() + "px; height: " +
          Grilla.Alto.ToString() +
          "px; margin-left: " + Grilla.Abscisa.ToString() +
          "px; margin-top: " + Grilla.Ordenada.ToString() + "px; position: absolute; text-align: center;" +
          " overflow: hidden; font-size: 11px;";
      }
    }

    public void CrearTendencias()
    {
      HayTendencias = true;
      StateHasChanged();
    }

    private BlockDatosZip DatosYaLeidos(Int32 Periodo)
    {
      foreach (BlockDatosZip Block in mBlocksDatos)
      {
        if (Block.Periodo == Periodo)
        {
          return Block;
        }
      }
      return null;
    }

    private void RefrescarFiltradores()
		{
      if (ComponenteFiltros != null)
			{
        ComponenteFiltros.FiltrarDataset();
//        ComponenteFiltros_AlAjustarVentanasFlotantes();
        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
				{
          Link.Componente.Filtrar();
				}
			}
		}

    private void CopiarFiltrosDesdeProveedorVentana(CProveedorComprimido ProveedorAnterior)
    {
      BlockDatosZip BlockAhora = BlockDatosPeriodo(mPeriodoDataset);
      if (BlockAhora != null)
      {
        CProveedorComprimido ProveedorAhora = BlockAhora.Proveedor;
        if (ProveedorAhora != null && ProveedorAnterior != null && ProveedorAnterior != ProveedorAhora)
        {
          ProveedorAhora.CopiarFiltros(ProveedorAnterior);
          mProveedor = ProveedorAhora;
          ProveedorAhora.FiltrarPorAsociaciones();
//          RefrescarFiltradores();
        }
      }
    }

    private void CopiarFiltrosProveedor()
    {
      if (ComponenteFiltros != null && ComponenteFiltros.Proveedor != null)
      {
        CopiarFiltrosDesdeProveedorVentana(ComponenteFiltros.Proveedor);
      }
    }

    private bool ProcesoDatosYaLeidos(Int32 Periodo)
    {
      try
      {

        if (mbRetrocediendo && mProveedor != null)
        {
          mbRetrocediendo = false;
          ActualizarDatosVentanas(false);
          return true;
        }
        BlockDatosZip Block = DatosYaLeidos(Periodo);
        if (Block == null)
        {
          return false;
        }

        mPeriodoDataset = Periodo;

//        CrearTextoRangoFechas(Periodo);

        mColumnasDataset = Block.Proveedor.Columnas;

        CopiarFiltrosProveedor();

//        bool ContinuaProcesando = false;

        ActualizarDatosVentanas(false);

//        ProcesarDatosDataset(ref ContinuaProcesando);
        //
        //        CargarDatosDataset(Respuesta);

        //        mPeriodoDataset = mPeriodoBase;

        //        ProcesarDatosDataset(ref ContinuaProcesando);
        //
        return true;
      }
      catch (Exception ex)
      {
        Contenedores.CContenedorDatos.MostrarMensaje(Rutinas.CRutinas.MostrarMensajeError(ex));
        return false;
      }
    }

    [Inject]
    public HttpClient Http { get; set; }

    public void VerDetalles()
		{
      VerDetalleIndicador = !VerDetalleIndicador;
      StateHasChanged();
		}

    public async Task CrearFiltroDatosAsync()
    {
      Int32 Periodo = -1;
      if (ComponenteTendencias != null && ComponenteTendencias.PuntoSeleccionado != null)
      {
        Periodo = ComponenteTendencias.PuntoSeleccionado.Periodo;
      }
      else
      {
        if (ComponenteTendRed != null)
        {
          Periodo = ComponenteTendRed.PeriodoSeleccionado;
        }
      }

      if (Periodo > 0)
      {

        // intenta procesar datos que ya estan en memoria.
        if (ProcesoDatosYaLeidos(Periodo))
        {
          return;
        }

        AguardandoFiltros = true;
        HayFiltroDatos = true;
        StateHasChanged();


        try
        {

          RespuestaDatasetBin Respuesta = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
              "api/Dataset/GetDataset?URL=" + Contenedores.CContenedorDatos.UrlBPI +
              "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
              "&Indicador=" + mIndicador.Codigo.ToString() +
              "&Dimension=" + mIndicador.Dimension.ToString() +
              "&ElementoDimension=" + CodigoElementoDimension.ToString() +
              "&Periodo=" + Periodo.ToString() +
              "&UnicamenteColumnas=false");
          if (Respuesta.RespuestaOK)
          {
            ProcesarRespuestaDataset(Respuesta);
          }
          else
          {
            throw new Exception(Respuesta.MsgErr);
          }

          //          CrearTextoRangoFechas(Periodo);
          //          HabilitarBotonesGraficos(false);
          //          MainPage.MostrarAguarda("Leyendo detalle", FncAbortar);
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
        finally
        {
          AguardandoFiltros = false;
          StateHasChanged();
        }
      }
    }

    private BlockDatosZip BlockDatosPeriodo(Int32 Periodo)
    {
      foreach (BlockDatosZip Block in mBlocksDatos)
      {
        if (Block.Periodo == Periodo)
        {
          return Block;
        }
      }
      return null;
    }

    private bool HayInformacionBlockPeriodo(Int32 Periodo)
    {
      return (BlockDatosPeriodo(Periodo) != null);
    }

    public CInformacionAlarmaCN UltimaAlarma
		{
      get
			{
        return Contenedores.CContenedorDatos.AlarmaIndicadorDesdeGlobal(Indicador.Codigo, CodigoElementoDimension); // (Alarmas != null && Alarmas.Count > 0 ? Alarmas.Last() : null);
			}
		}

    protected List<CInformacionAlarmaCN> Alarmas
		{
      get
			{
        return (mAlarmasImpuestas == null ?
            (ComponenteTendencias == null ?
            (ComponenteTendRed == null ? null : ComponenteTendRed.Alarmas) : ComponenteTendencias.Alarmas) :
            mAlarmasImpuestas);
			}
		}

    private DateTime FechaHoraPeriodo(Int32 Periodo)
    {
      foreach (CInformacionAlarmaCN Dato in Alarmas)
      {
        if (Dato.Periodo == Periodo)
        {
          return (mIndicador == null ? DateTime.Now : CRutinas.FechaGraficarPeriodo(mIndicador.Frecuencia, Dato));
        }
      }
      return new DateTime(1800, 1, 1);
    }

    private void ReordenarBlocks()
    {
      mBlocksDatos.Sort(delegate (BlockDatosZip B1, BlockDatosZip B2)
      {
        return (B1.FechaHora.CompareTo(B2.FechaHora));
      }
      );
    }

    private CInformacionAlarmaCN PeriodoDesdeCodigo(Int32 Codigo)
    {
      foreach (CInformacionAlarmaCN Punto in Alarmas)
      {
        if (Punto.Periodo == Codigo)
        {
          return Punto;
        }
      }
      return null;
    }

    private void AsociarProveedorConVentanas(CProveedorComprimido ProvAhora)
    {
      if (ComponenteFiltros != null)
      {
        ComponenteFiltros.ProveedorImpuesto = ProvAhora;
      }
    }

    private void AgregarInformacionBlocksBin(Int32 Periodo,
        byte[] DatosBinarios)
    {
      if (!HayInformacionBlockPeriodo(Periodo))
      {
        BlockDatosZip Block = new BlockDatosZip(ClaseOrigen, CodigoOrigen);
        Block.Periodo = Periodo;
        Block.FechaHora = FechaHoraPeriodo(Periodo);
        Block.Proveedor.ClaseOrigen = ClaseOrigen;
        Block.Proveedor.CodigoOrigen = CodigoOrigen;
        if (DatosBinarios != null)
        {
          if (ClaseOrigen == ClaseElemento.SubConsulta)
          {
            Block.Proveedor.ProcesarDatasetBinarioUnicode(DatosBinarios, false);
          }
          else
          {
            Block.Proveedor.ProcesarDatasetBinario(DatosBinarios, false);
          }
        }
        AsociarProveedorConVentanas(Block.Proveedor);
//        Block.Proveedor.AjustarDependientes = FncRefrescarDatosDataset;
        Block.Proveedor.Periodo = PeriodoDesdeCodigo(Periodo);
        mBlocksDatos.Add(Block);
        ReordenarBlocks();
        if (Block.Proveedor.Columnas.Count == 0)
        {
          Contenedores.CContenedorDatos.MostrarMensaje("El detalle no está definido");
        }
        else
        {
          if (Block.Proveedor.Datos.Count == 0)
          {
            Contenedores.CContenedorDatos.MostrarMensaje("No hay datos para el período");
          }
        }
      }
    }

    private void CargarDatosDatasetBin(RespuestaDatasetBin Tabla)
    {
      try
      {
        if (mBlocksDatos != null)
        {
          if (Tabla.Datos == null)
          {
            AgregarInformacionBlocksBin(Tabla.Periodo, null);
            Contenedores.CContenedorDatos.MostrarMensaje("No hay detalle disponible para el indicador en el período solicitado");
            // para bloquear nuevas lecturas.
            return;
          }

          // mColumnasDataset.Clear();
          // modificar cuando se modifique el metodo del WCF que traiga el dataset.
          AgregarInformacionBlocksBin(Tabla.Periodo, Tabla.Datos);

          BlockDatosZip Block = BlockDatosPeriodo(Tabla.Periodo);

          if (Tabla.Periodo == mPeriodoDataset)
          {
            mColumnasDataset = Block.Proveedor.Columnas;
            if (mProveedor != null)
            {
              CopiarFiltrosDesdeProveedorVentana(mProveedor);
            }
            AsociarProveedorConVentanas(Block.Proveedor);
            mProveedor = Block.Proveedor;
            if (ComponenteFiltros != null)
						{
              ComponenteFiltros.ProveedorImpuesto = mProveedor;
						}
          }

        }
      }
      catch (Exception ex)
      {
        throw new Exception("CDI " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
      }

    }

    //private void ProcesarDatosDataset(ref bool ContinuaProcesando)
    //{
    //  try
    //  {
    //    switch (mAccion)
    //    {
    //      case AccionAlLeerDataset.AbrirGrilla:
    //        AjustarPorFiltros();
    //        MostrarDatosEnFiltro();
    //        break;
    //      case AccionAlLeerDataset.MostrarBarraGraficos:
    //        AjustarPorFiltros();
    //        MostrarBarraGraficos();
    //        break;
    //      case AccionAlLeerDataset.AcumularParaTendencia:
    //        PonerDatosDisponiblesEnTendencia(true);
    //        ContinuaProcesando = IntentarCargarProximoBlock();
    //        return;
    //      case AccionAlLeerDataset.NoDefinida:
    //        AjustarPorFiltros();
    //        return;
    //    }

    //    if (this == mPagina.RelojSeleccionado)
    //    {
    //      //        CopiarFiltrosProveedor();
    //      //        mDatos = BlockDatosPeriodo(mPeriodoBase).Proveedor.Datos;
    //      ActualizarDatosVentanas();
    //      HabilitarBotones(true);
    //    }
    //  }
    //  catch (Exception ex)
    //  {
    //    throw new Exception("PDD " + Environment.NewLine + CRutinas.MostrarMensajeError(ex));
    //  }

    //}

    public CProveedorComprimido ProveedorComprimido()
    {
      BlockDatosZip Block = BlockDatosPeriodo(mPeriodoDataset);
      return (Block == null ? null : Block.Proveedor);
    }

    private void ActualizarDatosVentanas(bool IncluirTendencia = true)
    {

      CProveedorComprimido Prov = ProveedorComprimido();
      if (Prov != null)
      {
        mColumnasDataset = Prov.Columnas;
        if (mColumnasDataset == null)
        {
          return;
        }

        if (ComponenteFiltros != null)
        {
          ComponenteFiltros.ActualizarDatosDataset(Prov);
        }
      }
      else
      {
        return;
      }

    }

    public Int32 PeriodoEnProceso
		{
      get
			{
        return (ComponenteTendencias == null ? PeriodoAlarmaTendRed : ComponenteTendencias.PuntoSeleccionado.Periodo);
			}
		}

    protected void ProcesarRespuestaDataset(
        RespuestaDatasetBin Respuesta)
    {

      //      bool ContinuaProcesando = false;

      try
      {
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }

        mPeriodoDataset = PeriodoEnProceso;

        Contenedores.CContenedorDatos.AjustarHoraHasta();

        CargarDatosDatasetBin(Respuesta);

        //              mPeriodoDataset = mPeriodoBase;

        //              ProcesarDatosDataset(ref ContinuaProcesando);
        ActualizarDatosVentanas(false);

        //              CmdWSS.Visibility = Visibility.Visible;

      }
      catch (Exception ex)
      {
        //        ContinuaProcesando = false;
        Contenedores.CContenedorDatos.MostrarMensaje("Error recibiendo dataset4" +
          Environment.NewLine +
          CRutinas.MostrarMensajeError(ex));
				throw;
			}
    }

    public void ReposicionarTendencias(int Abscisa, int Ordenada, int Ancho, int Alto, bool Encima)
    {
      if (ComponenteTendencias != null)
      {
        TendenciasAmpliadas = Encima;
        if (Encima)
				{
          mAbscisaTendenciaAnterior = AbscisaTendencia;
          mOrdenadaTendenciaAnterior = OrdenadaTendencia;
          mAnchoTendenciaAnterior = AnchoTendencia;
          mAltoTendenciaAnterior = AltoTendencia;
          AbscisaTendencia = Abscisa;
          OrdenadaTendencia = Ordenada;
          AnchoTendencia = Ancho;
          AltoTendencia = Alto;
          TendenciasEncima = true;
          PonerElementoEncima(false, true, false, -1, -1);
				}
        else
				{
          AbscisaTendencia = mAbscisaTendenciaAnterior;
          OrdenadaTendencia = mOrdenadaTendenciaAnterior;
          AnchoTendencia = mAnchoTendenciaAnterior;
          AltoTendencia = mAltoTendenciaAnterior;
          TendenciasEncima = false;
        }
      }
    }

    public void ReposicionarGrilla(int Abscisa, int Ordenada, int Ancho, int Alto)
    {
      if (ComponenteFiltros != null && ComponenteFiltros.Grilla != null)
      {
        CLinkGrilla Grilla = ComponenteFiltros.Grilla;
        Grilla.Abscisa = Abscisa;
        Grilla.Ordenada = Ordenada;
        Grilla.Ancho = Ancho;
        Grilla.Alto = Alto;
      }
    }

    public void PonerElementoEncima(bool Filtros, bool Tendencia, bool Grilla, Int32 OrdenFiltro,
        Int32 CodigoGrafico)
    {

      ComponenteFiltrosEncima = Filtros;
      TendenciasEncima = Tendencia;
      GrillaEncima = Grilla;

      if (ComponenteFiltros != null)
      {
        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
        {
           if (Link.Componente != null)
					{
            Link.Encima = (Link.Componente.Filtrador.Columna.Orden == OrdenFiltro);
					}
           else
					{
            Link.Encima = false;
					}
        }

        foreach (CLinkGrafico Graf in ComponenteFiltros.Graficos)
				{
          if (Graf.Componente != null)
					{
            Graf.Encima = (Graf.Componente.CodigoUnico == CodigoGrafico);
					}
          else
					{
            Graf.Encima = false;
					}
				}
      }
    }

    public void ReposicionarGrafico(int CodigoUnico, int Abscisa, int Ordenada, long Ancho, long Alto, bool Encima)
    {
      if (ComponenteFiltros != null)
      {
        CLinkGrafico Graf = (from G in ComponenteFiltros.Graficos
                             where G.Componente != null && G.Componente.CodigoUnico == CodigoUnico
                             select G).FirstOrDefault();
        if (Graf != null)
				{
          if (Encima)
          {
            PonerElementoEncima(false, false, false, -1, Graf.CodigoUnico);
          }
          else
          {
            Graf.Encima = false;
          }
          if (Abscisa > -999998)
          {
            Graf.GuardarPosicion();
            Graf.Encima = Encima;
            Graf.Abscisa = Abscisa;
            Graf.Ordenada = Ordenada;
            Graf.Ancho = Ancho;
            Graf.Componente.AnchoGrafico = Ancho;
            Graf.Alto = Alto;
            Graf.Componente.AltoGrafico = Alto;
            Graf.Ampliado = Encima;
          }
          else
					{
            Graf.RecuperarPosicion();
            Graf.Ampliado = false;
					}
				}
      }
    }

    public void Refrescar()
		{
      StateHasChanged();
		}

    public CLinkGrilla Grilla
    {
      get
      {
        return (ComponenteFiltros == null ? null : ComponenteFiltros.Grilla);
      }
      set
			{
        if (ComponenteFiltros != null)
				{
          ComponenteFiltros.Grilla = value;
				}
			}
    }

    public CReloj ComponenteReloj
    {
      get { return mComponenteReloj; }
      set
      {
        mComponenteReloj = value;
        AguardandoReloj = false;
        ComponenteReloj.ImponerPosicion(10, 5, 260, 200);
      }
    }

    public bool HayAlarmaReducida
		{
      get
			{
        return mbAlarmaReducida;
			}
      set
			{
        if (value != mbAlarmaReducida)
				{
          mbAlarmaReducida = value;
				}
			}
		}

    public CLogicaTrendRed ComponenteTendRed
    {
      get { return mComponenteTendRed; }
      set
      {
        if (ComponenteTendRed != value)
        {
          if (ComponenteTendRed != null)
          {
            ComponenteTendRed.AlCambiarPunto -= ReposicionarDatasetAsync;
          }
          mComponenteTendRed = value;
          ComponenteTendRed.ImponerPosicion(10, 5, 350, 180);
          ComponenteTendRed.AlCambiarPunto += ReposicionarDatasetAsync;
          HayAlarmaReducida = HayAlarmaTendRed;
        }
      }
    }

    private void CopiarComponenteTendencias(CLogicaTendencias C1, CLogicaTendencias C2)
		{
      C2.AltoGrafico = C1.AltoGrafico;
      C2.AltoGraficoTotal = C1.AltoGraficoTotal;
      C2.AnchoGrafico = C1.AnchoGrafico;
      C2.Contenedor = this;
      C2.Curvas = C1.Curvas;
      C2.DetalleMensaje = C1.DetalleMensaje;
      C2.EscalaDerecha = C1.EscalaDerecha;
      C2.FechaDesde = C1.FechaDesde;
      C2.FechaHasta = C1.FechaHasta;
      C2.FechaHastaImpuesta = C1.FechaHastaImpuesta;
      C2.FechaImpuesta = C1.FechaImpuesta;
      C2.FechaInicialImpuesta = C1.FechaInicialImpuesta;
      C2.ImponerAbscisa(C1.Abscisa);
      C2.ImponerAlto(C1.Alto);
      C2.ImponerAmpliado(C1.Ampliado);
      C2.ImponerAncho(C1.Ancho);
      C2.ImponerEncima(C1.Encima);
      C2.IndicadoresAdicionales = C1.IndicadoresAdicionales;
		}

    public CLogicaTendencias ComponenteTendencias
    {
      get { return mComponenteTendencias; }
      set
      {
        mComponenteTendencias = value;
        mComponenteTendencias.FncReposicionarArriba = PonerTendenciaArriba;
        CLogicaTendencias.DimensionCaracter = mDimensionCaracter;
        ComponenteTendencias.ImponerPosicion(ABSCISA_INI_TENDENCIAS, 5, (Int32)AnchoTendenciasDefault, (Int32)AltoTendenciaDefault);
        mComponenteTendencias.EjecutarRefresco = Refrescar;
        mComponenteTendencias.AlCambiarPunto += ReposicionarDatasetAsync;
      }
    }

    public void EliminarVariosArriba()
		{
      TendenciasEncima = false;
      ComponenteFiltrosEncima = false;
		}

    private async Task ReposicionarDatasetAsync()
    {
      if (ComponenteTendencias != null)
      {
        PosicionPuntoTendencia = ComponenteTendencias.PosicionPuntoSeleccionado;
      }
      else
      {
        PosicionPuntoTendencia = ComponenteTendRed.PosicionAlarmaSeleccionada;
      }
      if (ComponenteFiltros != null)
      {
        await CrearFiltroDatosAsync();
        ComponenteFiltros.Proveedor.RefrescarDependientes();
        //        ComponenteFiltros.Proveedor.FiltrarPorAsociaciones();
        ComponenteTendencias.MostrarAguarda = false;
        StateHasChanged();
      }
    }

    public CLogicaContenedorFiltros ComponenteFiltros
    {
      get { return mComponenteFiltros; }
      set
      {
        if (mComponenteFiltros != value)
        {
          mComponenteFiltros = value;
          ComponenteFiltros.AlAjustarVentanasFlotantes += ComponenteFiltros_AlAjustarVentanasFlotantes;
          ComponenteFiltros.FncReposicionarArriba = PonerContenedorFiltroArriba;
          ComponenteFiltros.EvSeleccionarGrafico += FncSeleccionarGrafico;
          mComponenteFiltros.DimensionCaracter = mDimensionCaracter;
          mComponenteFiltros.PuntoSeleccionado = (mComponenteTendencias == null ? null : mComponenteTendencias.AlarmaParaDatos);
          mComponenteFiltros.ImponerPosicion(10, 215, 260, AltoContenedor);
          mComponenteFiltros.ProveedorImpuesto = ProveedorComprimido();
        }
      }
    }

    public void EliminarGrilla()
    {
      Grilla = null;
      StateHasChanged();
    }

    public LineaFiltro LineaDrag
		{
      get { return mLineaDrag; }
      set { mLineaDrag = value; }
		}

    public bool HayWSS { get; set; } = false;

    public CLogicaBingWSS PaginaWSS { get; set; }

    public List<CColumnaBase> Columnas
		{
      get { return mProveedor.Columnas; }
		}

    public List<CLineaComprimida> LineasVigentes
		{
      get { return mProveedor.DatosVigentes; }
		}

    public void AbrirWSS()
		{
      HayWSS = true;
      StateHasChanged();
    }

    public void CerrarWSS()
		{
      HayWSS = false;
      StateHasChanged();
		}

    public CLogicaPagTortasGIS PaginaTortasGIS { get; set; }

    public bool HayTortasGIS { get; set; } = false;

    public CColumnaBase ColumnaDatosTorta { get; set; }
    public CColumnaBase ColumnaAgrupadoraTorta { get; set; }
    public CColumnaBase ColumnaPosicionadoraTorta { get; set; }
    public CColumnaBase ColumnaLatTorta { get; set; }
    public CColumnaBase ColumnaLngTorta { get; set; }
    public DatosSolicitados SolicitudTorta { get; set; }
    public List<CLineaComprimida> LineasTorta { get; set; }
    public bool PinesAgrupados { get; set; }


    public void AbrirTortasGIS()
    {
      HayTortasGIS = true;
      StateHasChanged();
    }

    public void CerrarTortasGIS()
    {
      HayTortasGIS = false;
      StateHasChanged();
    }

    public bool HayPinesLL { get; set; } = false;

    public CLogicaPagPinsLL PaginaPinsLL { get; set; }

    public void AbrirPinsLL()
    {
      HayPinesLL = true;
      StateHasChanged();
    }

    public void CerrarPinesLL()
    {
      HayPinesLL = false;
      StateHasChanged();
    }

    public void PonerGraficoArriba(Microsoft.AspNetCore.Components.Web.MouseEventArgs e, CLinkGrafico Grafico)
    {
      PonerElementoEncima(false, false, false, -1, Grafico.CodigoUnico);
    }

    public void IniciarDragGrafico(Microsoft.AspNetCore.Components.Web.DragEventArgs e, CLinkGrafico Grafico)
    {
      mOffsetAbsc = (int)e.ScreenX; // e.OffsetX;
      mOffsetOrd = (int)e.ScreenY; // e.OffsetY;
      mFiltroDrag = null;
      mLineaDrag = null;
      mGrillaDrag = null;
      mGraficoDrag = Grafico;
      PonerElementoEncima(false, false, false, -1, Grafico.CodigoUnico);
    }

    public void IniciarDragFiltro(Microsoft.AspNetCore.Components.Web.DragEventArgs e, CLinkFiltros Filtro)
    {
      mOffsetAbsc = (int)e.ScreenX; // e.OffsetX;
      mOffsetOrd = (int)e.ScreenY; // e.OffsetY;
      CambioMedidas(Filtro);
      mFiltroDrag = Filtro;
      mLineaDrag = null;
      mGrillaDrag = null;
      mGraficoDrag = null;
      PonerElementoEncima(false, false, false, Filtro.Filtrador.Columna.Orden, -1);
    }

    public async void CambioMedidas(CLinkFiltros Filtro)
    {
      object[] Args = new object[1];
      Args[0] = IdFiltro(Filtro);
      string Dimensiones = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
      List<double> Valores = CRutinas.ListaAReales(Dimensiones);
      Filtro.Ancho = (Int32)Valores[2];
    }

    public async void CambioMedidasGrafico(CLinkGrafico Grafico)
    {
//      Grafico.Componente.BloquearGrafico();
      object[] Args = new object[1];
      Args[0] = IdGrafico(Grafico.CodigoUnico);
      string Dimensiones = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
      List<double> Valores = CRutinas.ListaAReales(Dimensiones);
      Grafico.Ancho = (Int32)Valores[2];
      Grafico.Alto = (Int32)Valores[3];
//      Grafico.Componente.Redibujar();
    }

    private int mOffsetAbsc;
    private int mOffsetOrd;

    public void IniciarDragLinea(int OffsetAbsc, int OffsetOrd, LineaFiltro Linea)
    {
      mOffsetAbsc = OffsetAbsc;
      mOffsetOrd = OffsetOrd;
      mFiltroDrag = null;
      mLineaDrag = Linea;
      mGrillaDrag = null;
      mGraficoDrag = null;
      PonerElementoEncima(true, false, false, -1, -1);
    }

    public void IniciarDragGrilla(Microsoft.AspNetCore.Components.Web.DragEventArgs e, CLinkGrilla Grilla)
		{
      mOffsetAbsc = (int)e.ScreenX;
      mOffsetOrd = (int)e.ScreenY;
      mFiltroDrag = null;
      mLineaDrag = null;
      mGrillaDrag = Grilla;
      mGraficoDrag = null;
      PonerElementoEncima(false, false, true, -1, -1);
    }

    public void RecibirDrop(Microsoft.AspNetCore.Components.Web.DragEventArgs e)
    {
      if (mGraficoDrag != null)
      {
        CLinkGrafico Link = (from G in ComponenteFiltros.Graficos
                             where G.CodigoUnico == mGraficoDrag.CodigoUnico
                             select G).FirstOrDefault();
        if (Link != null)
        {
          Int32 Diferencia = (int)e.ScreenX - (int)mOffsetAbsc;
          Link.Abscisa += Diferencia;
          Diferencia = (int)e.ScreenY - (int)mOffsetOrd;
          Link.Ordenada += Diferencia;
          mGraficoDrag = null;
          StateHasChanged();
        }
      }
      if (mFiltroDrag != null && mFiltroDrag.Componente != null)
      {
        CLinkFiltros Link = (from G in ComponenteFiltros.Links
                             where G.Componente != null && G.Componente.CodigoUnico == mFiltroDrag.Componente.CodigoUnico
                             select G).FirstOrDefault();
        if (Link != null)
        {
          Int32 Diferencia = (int)e.ScreenX - (int)mOffsetAbsc;
          Link.Abscisa += Diferencia;
          Diferencia = (int)e.ScreenY - (int)mOffsetOrd;
          Link.Ordenada += Diferencia;
          mFiltroDrag = null;
          StateHasChanged();
        }
      }
      if (mLineaDrag != null)
      {
        ComponenteFiltros.FncSeleccionFila(mLineaDrag.Columna.Orden, (int)e.OffsetX - 40,
            (int)e.OffsetY - 29);
        mLineaDrag = null;
      }
      if (mGrillaDrag != null)
      {
        if (ComponenteFiltros != null && ComponenteFiltros.Grilla != null)
        {
          Int32 Abscisa = ComponenteFiltros.Grilla.Abscisa + (int)e.ScreenX - (int)mOffsetAbsc;
          Int32 Ordenada = ComponenteFiltros.Grilla.Ordenada + (int)e.ScreenY - (int)mOffsetOrd;
          ReposicionarGrilla(Abscisa, Ordenada, (int)ComponenteFiltros.Grilla.Componente.Ancho,
              (int)ComponenteFiltros.Grilla.Componente.Alto);
          StateHasChanged();
        }
        mGrillaDrag = null;
      }
    }

    public static string IdFiltro(CLinkFiltros Lnk)
    {
      return "IDFiltro" + Lnk.PosicionEnPantalla.ToString();
    }

    public string IdGrafico(Int32 Pos) 
    {
      return "IDGrafico" + Pos.ToString();
    }

    private void ComponenteFiltros_AlAjustarVentanasFlotantes()
    {
      if (ComponenteFiltros != null && (ComponenteFiltros.Links != null ||
          ComponenteFiltros.Graficos != null))
      {
        StateHasChanged();
      }
    }

    [JSInvokable]
    public static Task<string> AjustarDimensionesPantallaAsync(string Nombre)
    {
      
      string Msg = "";
      try
      {
        Logicas.CLogicaIndicador.Redimensionar(Nombre);
      }
      catch (Exception ex)
      {
        Msg = ex.Message;
      }
      return Task.FromResult(Msg);
    }

    [JSInvokable]
    public static Task<string> CambioPosicionElementoAsync(string Nombre, string Posicion)
    {
      string Msg = "";
      try
      {
        if (gPuntero != null)
        {
          CBaseGrafico Grafico = gPuntero.UbicarGraficoFlotante(Nombre);
          if (Grafico != null)
          {
            string[] Posiciones = Posicion.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (Posiciones.Length == 4)
            {
              Grafico.ImponerPosicion(Int32.Parse(Posiciones[0]) - Contenedores.CContenedorDatos.DefasajeAbscisasPantallaIndicadores,
                    Int32.Parse(Posiciones[1]) - Contenedores.CContenedorDatos.DefasajeOrdenadasPantallaIndicadores,
                    Int32.Parse(Posiciones[2]), Int32.Parse(Posiciones[3]));
            }
          }
        }
        Msg = "";
      }
      catch (Exception ex)
      {
        Msg = ex.Message;
      }
      return Task.FromResult(Msg);
    }

    private void PonerMobil()
    {
//      JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerDesplazable", "#RelojPropio");
      if (HayTendencias)
      {
//        JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerResizable", "#TendenciaUnica");        //JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerResizable", "#TendenciaUnica");
        //        JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerDesplazable", "#TendenciaUnica");
        //object[] Args = new object[1];
        //Args[0] = "#ContenedorTend";
        //JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerMouseUp", Args);
      }

      if (HayFiltroDatos)
      {
//        JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerDesplazable", "#ContenedorFiltrosUnico");

        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
        {
          if (Link.PosicionEnPantalla >= 0)
          {
            JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerResizable", "#"+ IdFiltro(Link));
          }
        }

        foreach (CLinkGrafico GraficoL in ComponenteFiltros.Graficos)
        {
          JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerResizable", "#" + IdGrafico(GraficoL.CodigoUnico));
        }

        if (Grilla != null)
        {
          JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerResizable", "#DivGrilla");
        }

      }
    }

    private void PonerEventoClick()
    {
      JSRuntime.InvokeAsync<Int32>("FuncionesJS.FncPonerEventoClick", "#svgclock");
    }

    public void FncDragEnd(ChangeEventArgs e)
    {
      if (this != null)
      {
        return;
      }
    }

    private async Task BuscarDimensionCaracter()
    {
      if (mDimensionCaracter < 0)
      {
        object[] Prms = new object[3];
        Prms[0] = "H";
        Prms[1] = "Arial";
        Prms[2] = "9px";
        mDimensionCaracter = await JSRuntime.InvokeAsync<double>("FuncionesJS.ObtenerDimensionTexto", Prms);
      }
    }

    public async Task AjustarDimensionesIndicador(string Nombre)
    {
      object[] Args = new object[1];
      Args[0] = Nombre;
      string Texto = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
      Rectangulo Medidas = new Rectangulo(Texto);
      Contenedores.CContenedorDatos.DefasajeAbscisasPantallaIndicadores = (Int32)Medidas.left;
      Contenedores.CContenedorDatos.DefasajeOrdenadasPantallaIndicadores = (Int32)Medidas.top + 27;
      Contenedores.CContenedorDatos.AnchoPantallaIndicadores = (Int32)Medidas.width;
      Contenedores.CContenedorDatos.AltoPantallaIndicadores = (Int32)Medidas.height - 25;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

      gPuntero = this;

      if (Contenedores.CContenedorDatos.AnchoPantallaIndicadores < 0)
      {
        await AjustarDimensionesIndicador("BASE_INDICADOR");
      }

      await BuscarDimensionCaracter();
      if (!Contenedores.CContenedorDatos.SiempreTendencia)
      {
        _ = LeerAlarmasAsync(false);
      }
      //this.PonerEventoClick();
      //Task Respuesta = base.OnAfterRenderAsync(firstRender);
      //if (ComponenteFiltros != null && ComponenteFiltros.LinksAnteriores != null)
      //{
      //  ComponenteFiltros.Links.AddRange(ComponenteFiltros.LinksAnteriores);
      //  ComponenteFiltros.LinksAnteriores = null;
      //  StateHasChanged();
      //}
      await base.OnAfterRenderAsync(firstRender);
    }

    public void FncClick(EventArgs e)
    {
      if (e != null)
      {
        return;
      }
    }

    public string SinTendencia
    {
      get { return mbSinTendencia; }
      set { mbSinTendencia = value; }
    }

    public CDatoIndicador Indicador
    {
      get { return mIndicador; }
      set {
        mIndicador = value;
      }
    }


   public virtual string NombreIndicador
    {
      get { return (Indicador == null ? "No definido" : Indicador.Descripcion); }
    }

    public string Unidades
    {
      get { return (Indicador == null ? "" : Indicador.Unidades); }
    }

    public string Valor()
    {

      if (Indicador == null || mComponenteReloj == null)
      {
        return "--";
      }

      if (ComponenteReloj.AlarmasLeidas == null)
			{
        _ = LeerAlarmasAsync(false);
        return "--";
			}

      if (ComponenteReloj.AlarmasLeidas.Count == 0)
      {
        return "--";
      }
      else
      {
        return CRutinas.ValorATexto(ComponenteReloj.AlarmasLeidas.Last().Valor, Indicador.Decimales);
      }
    }

    public async void AjustarDimensiones(string Nombre)
    {
      if (ComponenteTendencias!=null && Nombre == "#TendenciaUnica")
      {
        await ComponenteTendencias.AjustarDimensionesGlobalesAsync(Nombre);
        StateHasChanged();
      }
    }

    //public List<Elementos.Reloj> Relojes { get; set; }

    public static void Redimensionar(string Nombre)
    {
      try
      {
        if (gDetalle == null)
        {
          throw new Exception("No hay objeto global");
        }
        else
        {
          gDetalle.AjustarDimensiones(Nombre);
        }
      }
      catch (Exception ex)
      {
        gDetalle.Indicador.Descripcion = ex.Message;
      }
      gDetalle.StateHasChanged();
    }

    public CBaseGrafico UbicarGraficoFlotante(string Nombre)
    {
      switch (Nombre)
      {
        case "RelojPropio": return ComponenteReloj;
        case "TendenciaUnica": return ComponenteTendencias;
        case "ContenedorFiltrosUnico": return ComponenteFiltros;
        case "DivGrilla": return Grilla.Componente;
        default:
          foreach (CLinkFiltros Link in ComponenteFiltros.Links)
          {
            if (Link.PosicionEnPantalla >= 0)
            {
              if (IdFiltro(Link) == Nombre)
              {
                return (CBaseGrafico)Link.Componente;
              }
            }
          }

          foreach (CLinkGrafico Grafico in ComponenteFiltros.Graficos)
          {
            if (Nombre == IdGrafico(Grafico.CodigoUnico))
            {
              return (CBaseGrafico)Grafico.Componente;
            }
          }
          return null;
      }
    }

    public bool HayGraficoAmpliado
    {
      get
      {
        if (ComponenteFiltros == null)
        {
          return false;
        }
        else
        {
          return (from C in ComponenteFiltros.Graficos
                  where C.Componente != null && C.Componente.Ampliado
                  select C).FirstOrDefault() != null;
        }
      }
    }

    public CLinkGrafico GraficoAmpliado
		{
      get
			{
        return (from C in ComponenteFiltros.Graficos
                where C.Componente.Ampliado
                select C).FirstOrDefault();
      }
		}

    private void FncSeleccionarGrafico(CBaseGrafico Grafico)
    {
      SeleccionarGrafico(Grafico);
    }

    public bool SeleccionarGrafico(CBaseGrafico Grafico)
    {
      bool Respuesta = false;

      if (mGraficoSeleccionadoImpuesto != null)
      {
        Grafico = mGraficoSeleccionadoImpuesto;
        mGraficoSeleccionadoImpuesto = null;
      }

      if (ComponenteReloj != null)
      {
        Respuesta |= ComponenteReloj.Seleccionar(Grafico);
      }

      if (ComponenteTendencias != null)
      {
        Respuesta |= ComponenteTendencias.Seleccionar(Grafico);
      }

      if (ComponenteFiltros != null)
      {

        Respuesta |= ComponenteFiltros.Seleccionar(Grafico);

        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
        {
          if (Link.PosicionEnPantalla >= 0)
          {
            Respuesta |= ((CBaseGrafico)Link.Componente).Seleccionar(Grafico);
          }
        }

        foreach (CLinkGrafico GraficoL in ComponenteFiltros.Graficos)
        {
          Respuesta |= ((CBaseGrafico)GraficoL.Componente).Seleccionar(Grafico);
        }
      }

      return Respuesta;

    }

    [JSInvokable]
    public static Task<string> RecibirClickAsync(string Nombre)
    {
      string Msg = "";
      try
      {
        if (gPuntero != null)
        {
          CBaseGrafico Grafico = gPuntero.UbicarGraficoFlotante(Nombre);
          if (Grafico != null)
          {
            bool Modifico = gPuntero.SeleccionarGrafico(Grafico);
            //if (Grafico is Componentes.CComponenteTendencias)
            //{
            //  // Obtener dimensiones y si corresponde refrescar.
            //  string Posicion = await gPuntero.JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", "TendenciaUnica");
            //  Grafico.ImponerPosicion(Posicion);
            //}
            if (Modifico)
            {
              gPuntero.StateHasChanged();
            }
          }
        }
      }
      catch (Exception ex)
      {
        Msg = ex.Message;
      }
      return Task.FromResult(Msg);
    }

    public CLogicaIndicador()
    {
      CBaseGrafico.BordeSuperior = 0;
      gDetalle = this;
      CLogicaTendencias.gAnchoEscalaTendencias = -1;
      HayTendencias = false;
      //Relojes = new List<Elementos.Reloj>();
      //Relojes.Add(new Elementos.Reloj());
      TableroPecasV5.CEventosJS.OnResize += FncResizeEvento;
    }

    public void Dispose()
    {
      TableroPecasV5.CEventosJS.OnResize -= FncResizeEvento;
    }

  }

}
