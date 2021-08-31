using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Blazor.Extensions.Canvas;
using Blazor.Extensions.Canvas.Canvas2D;
using Blazorise;
//using Blazor.Extensions.Canvas.WebGL;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Shared;


namespace TableroPecasV5.Client.Logicas
{
  public class CLogicaTendencias : CBaseGrafico
  {

    public delegate Task FncCambiarPunto();
    public event FncCambiarPunto AlCambiarPunto;

//    private Semaphore mSemaforo;

    public CLogicaTendencias()
    {
      Ancho = Logicas.CLogicaIndicador.AnchoTendenciasDefault;
      Alto = Logicas.CLogicaIndicador.AltoTendenciaDefault;
      MesesTendencia = -1;
      AjustarDimensionesGrafico();
      Abscisa = 0;
      Ordenada = 0;
      Indicador = null;
      Alarmas = null;
      FechaHasta = DateTime.Now;
      FechaDesde = FechaHasta.AddMonths(-1);
//      mSemaforo = new Semaphore(0, 1);
    }

    [Inject]
    public IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public Int32 MesesTendencia { get; set; }

    public bool MostrarAguarda
		{
      get { return mbMostrarAguarda; }
      set
			{
        if (value != mbMostrarAguarda)
				{
          mbMostrarAguarda = value;
          StateHasChanged();
				}
			}
		}

		public override void ImponerPosicion(int Absc0, int Ord0, int Ancho0, int Alto0)
		{
			base.ImponerPosicion(Absc0, Ord0, Ancho0, Alto0);
      Pagina.AbscisaTendencia = Absc0;
      Pagina.OrdenadaTendencia = Ordenada;
      Pagina.AnchoTendencia = Ancho0;
      Pagina.AltoTendencia = Alto0;
		}

		public void OpcionBarras(Int32 Frecuencia)
    {
      MesesTendencia = Frecuencia;
      if (MesesTendencia > 0)
      {
        mPeriodos = ObtenerDatosPorPeriodo();
      }
      AjustarDimensionesGrafico();
      StateHasChanged();
      OcultarMenus();
    }

    public void FncGral(object e)
    {
      return;
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

    public DateTime FechaDesde { get; set; }
    public DateTime FechaHasta { get; set; }

    private CDatoIndicador mIndicador = null;
    [Parameter]
    public CDatoIndicador Indicador
    {
      get { return mIndicador; }
      set { mIndicador = value; }
    }

    private Modal mModal = null;
    public Modal ModalIndicadorAdicional
    {
      get { return mModal; }
      set { mModal = value; }
    }

    protected override void IntentarRefrescarGraficoPorResize()
    {
      RefrescoConTimer();
    }

    public Modal ModalRangoFechas { get; set; }

    public void AbrirVentanaAgregarIndicador()
    {
      if (FncReposicionarArriba != null)
      {
        FncReposicionarArriba();
      }
      ModalIndicadorAdicional.Show();
    }

    public Int32 CodigoIndicadorAdicional { get; set; }

    public void CerrarVentanaAgregarIndicador()
    {
      ModalIndicadorAdicional.Hide();
    }

    public DateTime? FechaInicialImpuesta { get; set; }
    public DateTime? FechaHastaImpuesta { get; set; }
    public bool FechaImpuesta { get; set; } = false;

    //public Visibility VerMenuModos { get; set; }

    //public void CambiarMenuModos()
    //{
    //  VerMenuModos = (VerMenuModos == Blazorise.Visibility.Always ? Blazorise.Visibility.Never : Blazorise.Visibility.Always);
    //}

    private object LOCK_TIMER = new object();
    private void RefrescoConTimer()
    {
      lock (LOCK_TIMER)
      {
        if (mRefrescador == null)
        {
          mRefrescador = new Clases.CTimerRefresco();
          mRefrescador.AlRefrescar += RefrescarInterfase;
        }
      }
      mRefrescador.RefrescarPedido();
    }

    private Clases.CTimerRefresco mRefrescador = null;

    private void RefrescarInterfase()
    {
      InvokeAsync(() => { StateHasChanged(); });
    }

    public void FncScroll()
    {
      RefrescoConTimer();
    }

    public void AbrirVentanaRangoFechas()
    {
      if (FncReposicionarArriba != null)
      {
        FncReposicionarArriba();
      }
      FechaInicialImpuesta = FechaDesde;
      FechaHastaImpuesta = FechaHasta;
      ModalRangoFechas.Show();
    }

    [Parameter]
    public Int32 CodigoElementoDimension { get; set; }

    public async void ImponerRangoFechasAsync()
    {
      FechaDesde = (DateTime)FechaInicialImpuesta;
      FechaHasta = (DateTime)FechaHastaImpuesta;
      FechaImpuesta = true;

      ModalRangoFechas.Hide();
      mbMostrarAguarda = true;
      StateHasChanged();

      try
      {
        foreach (Clases.CCurvaTendencia Curva in Curvas)
        {
          Curva.Alarmas = await CContenedorDatos.ObtenerAlarmasIndicadorEntreFechasAsync(
              Http, Curva.Indicador,
              FechaDesde, FechaHasta);
        }
      }
      catch (Exception ex)
      {
        MensajeBase = "Al leer tendencia";
        DetalleMensaje = Rutinas.CRutinas.TextoMsg(ex);
        ModalMsg.Show();
      }

      mbMostrarAguarda = false;
      StateHasChanged();

    }

    [Inject]
    public HttpClient Http { get; set; }

    public Modal ModalMsg { get; set; }

    public string MensajeBase { get; set; } = "";
    public string DetalleMensaje { get; set; } = "";

    public void CerrarVentanaMsg() { ModalMsg.Hide(); }

    public void CerrarVentanaRangoFechas()
    {
      ModalRangoFechas.Hide();
    }

    public ObservableCollection<Clases.CCurvaTendencia> Curvas = new ObservableCollection<Clases.CCurvaTendencia>();

    public string OpcionIndicadores { get; set; }

    public void CambioIndicador(ChangeEventArgs e)
    {
      IndicadorAgregado = null;
      if (e.Value != null)
      {
        Int32 CodigoLocal;
        if (Int32.TryParse(e.Value.ToString(), out CodigoLocal))
        {
          IndicadorAgregado = (from I in Contenedores.CContenedorDatos.ListaIndicadores
                               where I.Codigo == CodigoLocal
                               select I).FirstOrDefault();
          OpcionIndicadores = (IndicadoresAdicionales.Contains(IndicadorAgregado) ? "Eliminar" : "Agregar");
        }
      }
    }

    public string EstiloContenedor
    {
      get
      {
        return "width: 100%; height: " + AltoGraficoTotal.ToString() +
          "px; overflow-x: auto; overflow-y: hidden; position: relative; margin: 36px 0px 0px 0px; background-color: white; ";
      }
    }

    private bool AjustarDimensionesGrafico()
    {
      bool Cambio = false;
      if (MesesTendencia < 0)
      {
        if (AnchoGrafico != (Ancho - 4))
        {
          Cambio = true;
          AnchoGrafico = (long)Math.Max(Ancho - 4, 10);
        }
        if (AltoGrafico != (Alto - 40))
        {
          AltoGrafico = (long)Math.Max(Alto - 40, 10);
          AltoGraficoTotal = AltoGrafico;
          Cambio = true;
        }
      }
      else
      {
        long AnchoAhora = Math.Max((long)Ancho - 4, (long)(mPeriodos.Count *
            ((Curvas.Count * (CTendencias.ANCHO_BARRA + CTendencias.SEP_ENTRE_BARRAS) - CTendencias.SEP_ENTRE_BARRAS) +
              CTendencias.SEP_GRUPOS_BARRAS) + CTendencias.SEP_GRUPOS_BARRAS + CTendencias.ANCHO_PREVISTO_ESCALA));
        if (AnchoAhora != AnchoGrafico)
        {
          AnchoGrafico = Math.Max(AnchoAhora, 10);
          Cambio = true;
        }

        long AltoAhora = (AltoGrafico > ((long)Alto - 4) ? ((long)Alto - 40 - Rutinas.CRutinas.ALTO_BARRA_SCROLL) : ((long)Alto - 40));
        if (AltoAhora != AltoGrafico)
        {
          AltoGrafico = Math.Max(AltoAhora, 10);
          Cambio = true;
        }

        if (AltoGraficoTotal != (Alto - 40))
        {
          AltoGraficoTotal = (long)Alto - 40;
          Cambio = true;
        }
      }
      return Cambio;
    }

    public List<CDatoIndicador> IndicadoresAdicionales = new List<CDatoIndicador>();

    public CDatoIndicador IndicadorAgregado { get; set; } = null;

    public bool EscalaDerecha { get; set; } = false;

    [Parameter]
    public bool MostrarSubMenu { get; set; } = false;

    [Parameter]
    public bool MostrarComparativo { get; set; } = false;

    [Parameter]
    public bool MostrarRepresentar { get; set; } = false;

    [Parameter]
    public bool MostrarBarras { get; set; } = false;

    [Parameter]
    public bool MostrarFechas { get; set; } = false;

    [Parameter]
    public bool VerReferencias { get; set; } = false;

    [Parameter]
    public bool VerEtiquetas { get; set; } = false;

    public void CambiarMostrarSubMenu()
    {
      MostrarSubMenu = !MostrarSubMenu;
      MostrarComparativo = false;
      MostrarRepresentar = false;
      MostrarBarras = false;
      MostrarFechas = false;
      OpcionIndicadores = "Agregar";
    }

    private List<Clases.CPeriodoT> mPeriodos;

    private Clases.CPeriodoT ObtenerPeriodo(DateTime Fecha, ref List<Clases.CPeriodoT> Periodos)
    {
      switch (MesesTendencia)
      {
        case 12:
          DateTime Inicio = Rutinas.CRutinas.FechaInicioAnio(Fecha);
          foreach (Clases.CPeriodoT Periodo in Periodos)
          {
            if (Periodo.Desde.Year == Inicio.Year)
            {
              return Periodo;
            }
          }
          Clases.CPeriodoT PeriodoL = new Clases.CPeriodoT()
          {
            Desde = Rutinas.CRutinas.FechaInicioAnio(Fecha),
            Hasta = Rutinas.CRutinas.FechaInicioAnio(Fecha).AddYears(1).AddSeconds(-1)
          };
          Periodos.Add(PeriodoL);
          return PeriodoL;
        default:
          foreach (Clases.CPeriodoT Periodo in Periodos)
          {
            if (Periodo.Desde <= Fecha && Periodo.Hasta >= Fecha)
            {
              return Periodo;
            }
          }
          DateTime FechaInicio = Rutinas.CRutinas.FechaInicioMes(Fecha);
          Int32 Mes = FechaInicio.Month - (FechaInicio.Month - 1) % MesesTendencia;
          Clases.CPeriodoT PeriodoL2 = new Clases.CPeriodoT()
          {
            Desde = new DateTime(FechaInicio.Year, Mes, 1, 0, 0, 0),
            Hasta = new DateTime(FechaInicio.Year, Mes, 1, 0, 0, 0).AddMonths(MesesTendencia).AddSeconds(-1)
          };
          Periodos.Add(PeriodoL2);
          return PeriodoL2;
      }
    }

    private List<Clases.CPeriodoT> ObtenerDatosPorPeriodo()
    {
      List<Clases.CPeriodoT> Respuesta = new List<Clases.CPeriodoT>();
      Int32 Pos = 0;
      foreach (Clases.CCurvaTendencia Curva in Curvas)
      {
        foreach (CInformacionAlarmaCN Dato in Curva.Alarmas)
        {
          Clases.CPeriodoT Punto = ObtenerPeriodo(Dato.FechaInicial, ref Respuesta);
          Punto.AgregarValor(Pos, Dato.Valor);
        }
        Pos++;
      }

      Respuesta.Sort(delegate (Clases.CPeriodoT P1, Clases.CPeriodoT P2)
      {
        return P1.Desde.CompareTo(P2.Desde);
      });

      return Respuesta;
    }
    public async Task AjustarDimensionesGlobalesAsync(string Nombre)
    {
      if (await base.AjustarDimensionesAsync(Nombre))
      {
        AjustarDimensionesGrafico();
      }
    }
    public void CambiarMostrarComparativo()
    {
      MostrarComparativo = !MostrarComparativo;
      MostrarRepresentar = false;
      MostrarBarras = false;
      MostrarFechas = false;
    }

    public void CambiarMostrarRepresentar()
    {
      MostrarRepresentar = !MostrarRepresentar;
      MostrarComparativo = false;
      MostrarBarras = false;
      MostrarFechas = false;
    }

    public void CambiarReferencias()
    {
      VerReferencias = !VerReferencias;
      OcultarMenus();
      StateHasChanged();
    }

    public void CambiarEtiquetas()
    {
      VerEtiquetas = !VerEtiquetas;
      OcultarMenus();
      StateHasChanged();
    }

    //private CTendencias mTendencias = null;
    //public CTendencias ReferenciaGrafico
    //{
    //  get { return mTendencias; }
    //  set
    //  {
    //    if (mTendencias != value)
    //    {
    //      mTendencias = value;
    //    }
    //  }
    //}

    public long AnchoGrafico { get; set; } = 100;
    public long AltoGrafico { get; set; } = 100;
    public long AltoGraficoTotal { get; set; } = 100;

    private List<CInformacionAlarmaCN> mAlarmas = null;

    [Parameter]
    public List<CInformacionAlarmaCN> Alarmas
    {
      get { return mAlarmas; }
      set
      {
        mAlarmas = value;
        if (mAlarmas != null)
        {
          FechaDesde = (from M in mAlarmas
                        select M.FechaInicial).Min();
          FechaHasta = (from M in mAlarmas
                        select M.FechaInicial).Max();
          Clases.CCurvaTendencia Curva = new Clases.CCurvaTendencia()
          {
            IndicadorBase = true,
            Indicador = this.Indicador,
            Alarmas = mAlarmas
          };
          if (Curvas.Count == 0)
          {
            Curvas.Add(Curva);
          }
          else
          {
            Curvas[0] = Curva;
          }
        }
      }
    }

    public static double DimensionCaracter { get; set; }

    private void OcultarMenus()
    {
      MostrarSubMenu = false;
      MostrarComparativo = false;
      MostrarRepresentar = false;
      MostrarBarras = false;
      MostrarFechas = false;
      StateHasChanged();
    }

//    public event CLogicaFiltroTextos.FncEventoTextoBool AlCerrarGrafico;

    public void Maximizar()
    {
      if (!Ampliado)
      {
        Ampliado = true;
        Pagina.ReposicionarTendencias(2, 2, AnchoPosible - 4, AltoPosible - 4, true);
        EjecutarRefresco();
      }
    }

    private Int32 AnchoPosible
    {
      get
      {
        return (Contenedores.CContenedorDatos.AnchoPantallaIndicadores > 0 ? Contenedores.CContenedorDatos.AnchoPantallaIndicadores :
            Contenedores.CContenedorDatos.AnchoPantalla);
      }
    }

    private Int32 AltoPosible
    {
      get
      {
        return (Contenedores.CContenedorDatos.AltoPantallaIndicadores > 0 ? Contenedores.CContenedorDatos.AltoPantallaIndicadores :
            Contenedores.CContenedorDatos.AltoPantalla);
      }
    }

    public void Reducir()
    {
      if (Ampliado)
      {
        Ampliado = false;
        Pagina.ReposicionarTendencias(-1, -1, -1, -1, false);
        EjecutarRefresco();
      }
    }

    //public string EstiloRedimensionador
    //{
    //  get
    //  {
    //    return "width: 10px; height: 10px; overflow-x: hidden; overflow-y: hidden; position: relative; " +
    //        "margin-left: 14px; background-color: red; margin-top: -14px; cursor: se-resize;";
    //  }
    //}

    public async Task AlCompletar()
    {
      await AjustarDimensionesGlobalesAsync("#TendenciaUnica");
      StateHasChanged();
    }

    private BECanvas mCanvasGrafico = null;

    public BECanvas CanvasGrafico
		{
      get { return mCanvasGrafico; }
      set { mCanvasGrafico = value; }
		}

    //    private WebGLContext mContextoWGL;

    private Canvas2DContext mContexto;

    private List<CPunto> PuntosTendencia = null;

    [CascadingParameter]
    Logicas.CLogicaIndicador Pagina { get; set; }

    public void EventoMouseAbajo(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      Pagina.PonerElementoEncima(false, true, false, -1, -1);
      Pagina.Refrescar();
    }

    public static double gAnchoEscalaTendencias = -1;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {

//      mSemaforo.WaitOne();

      try
      {

        if (CanvasGrafico == null)
				{
          return;
				}

        if (AjustarDimensionesGrafico())
        {
          EjecutarRefresco();
          RefrescarSuperior();
          return;
        }

        if (CanvasGrafico != null)
        {
          if (CanvasGrafico.Height != AltoGrafico)
          {
            if (EjecutarRefresco != null)
            {
              EjecutarRefresco();
            }
            RefrescarSuperior();
            return;
          }
        }

        CTendencias Tendencias = new CTendencias();
        Tendencias.Curvas = Curvas;
        Tendencias.FechaMinima = FechaDesde;
        Tendencias.FechaMaxima = FechaHasta;
        Tendencias.MostrarEtiquetas = VerEtiquetas;
        Tendencias.MostrarReferencias = VerReferencias;
        Tendencias.MesesAgrupacionBarras = MesesTendencia;
        Tendencias.PuntoSeleccionado = mPosPuntoSeleccionado;
        Tendencias.Periodos = mPeriodos;
        Tendencias.Ancho = AnchoGrafico;
        Tendencias.AnchoContenedor = Ancho;
        Tendencias.Alto = AltoGrafico;

        mContexto = await Blazor.Extensions.CanvasContextExtensions.CreateCanvas2DAsync(CanvasGrafico);
        await mContexto.BeginBatchAsync();

        try
        {

          await mContexto.SetFillStyleAsync("yellow");
          await mContexto.FillRectAsync(0, 0, AnchoGrafico, AltoGrafico);

          gAnchoEscalaTendencias = await Tendencias.DibujarTendenciasAsync(mContexto, gAnchoEscalaTendencias);

          PuntosTendencia = Tendencias.PuntosTendencia;

        }
        finally
        {
          await mContexto.EndBatchAsync();
        }

      }
      catch (Exception)
      {
        PuntosTendencia = null;
      }
      finally
      {
 //       mSemaforo.Release();
      }


    }

    private bool mbMostrarAguarda = false;

    public string EstiloAguarda
    {
      get
      {
        return "width:100%; height: 100%; position: absolute; margin: 0px; background-color: #c0c0c0c0; visibility: " +
            (mbMostrarAguarda ? "visible;" : "hidden;");
      }
    }

    private void AjustarRangoFechas()
    {

      if (!FechaImpuesta)
      {
        FechaDesde = Rutinas.CRutinas.FechaMaxima();
        FechaHasta = Rutinas.CRutinas.FechaMinima();

        foreach (Clases.CCurvaTendencia Curva in Curvas)
        {
          DateTime FechaInicial = (from A in Curva.Alarmas
                                   select A.FechaInicial).Min();
          DateTime FechaMaxima = (from A in Curva.Alarmas
                                  select A.FechaInicial).Max();
          if (FechaInicial < FechaDesde)
          {
            FechaDesde = FechaInicial;
          }
          if (FechaMaxima > FechaHasta)
          {
            FechaHasta = FechaMaxima;
          }
        }
      }

    }

    public async void AjustarIndicadorAdicionalAsync()
    {
      if (CodigoIndicadorAdicional>0 && Curvas != null && Curvas.Count > 0 && Curvas[0].Alarmas.Count>0)
      {
        IndicadorAgregado = (from I in Contenedores.CContenedorDatos.ListaIndicadores
                             where I.Codigo == CodigoIndicadorAdicional
                             select I).FirstOrDefault();

        IndicadoresAdicionales.Add(IndicadorAgregado);
        mbMostrarAguarda = true;
        StateHasChanged();
        List<CInformacionAlarmaCN>  Alarmas = await Contenedores.CContenedorDatos.ObtenerAlarmasIndicadorAsync(
          Http, IndicadorAgregado);
        if (Alarmas.Count > 0)
        {
          Clases.CCurvaTendencia Curva = new Clases.CCurvaTendencia();
          Curva.Indicador = IndicadorAgregado;
          Curva.Alarmas = Alarmas;
          Curva.EscalaDerecha = EscalaDerecha;
          Curvas.Add(Curva);
          mbMostrarAguarda = false;
          AjustarDimensionesGrafico();
          OcultarMenus();
          ModalIndicadorAdicional.Hide();
          AjustarRangoFechas();
          StateHasChanged();
        }
      }
    }

    public void CerrarVentanaEliminarIndicador()
    {
      if (CodigoIndicadorAdicional > 0 && Curvas != null && Curvas.Count > 0 && Curvas[0].Alarmas.Count > 0)
      {
        CDatoIndicador IndicadorEliminado = (from I in Contenedores.CContenedorDatos.ListaIndicadores
                             where I.Codigo == CodigoIndicadorAdicional
                             select I).FirstOrDefault();

        if (IndicadorEliminado.Codigo == Indicador.Codigo)
        {
          ModalIndicadorAdicional.Hide();
          return;
        }

        IndicadoresAdicionales.Remove(IndicadorEliminado);
        // Eliminar curva.
        Clases.CCurvaTendencia CurvaBorrada = (from Curva in Curvas
                                               where Curva.Indicador.Codigo == IndicadorEliminado.Codigo
                                               select Curva).FirstOrDefault();
        if (CurvaBorrada != null)
        {
          Curvas.Remove(CurvaBorrada);
          if (Curvas.Count == 1)
          {
            MesesTendencia = -1;
          }
        }
        mbMostrarAguarda = false;
        AjustarDimensionesGrafico();
        OcultarMenus();
        ModalIndicadorAdicional.Hide();
        AjustarRangoFechas();
        StateHasChanged();
      }
    }

    public void FncClickTendencias(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      if (e != null)
      {
        return;
      }
    }

    [Parameter]
    public Int32 PosicionPuntoSeleccionado
    {
      get { return mPosPuntoSeleccionado; }
      set { mPosPuntoSeleccionado = value; }
    }

    private Int32 mPosPuntoSeleccionado = -1;
    public CInformacionAlarmaCN PuntoSeleccionado
    {
      get
      {
        if (Alarmas != null && Alarmas.Count > 0) {
          if (mPosPuntoSeleccionado < 0 || mPosPuntoSeleccionado >= Alarmas.Count)
          {
            mPosPuntoSeleccionado = Alarmas.Count - 1;
          }
        }
        else
        {
          mPosPuntoSeleccionado = -1;
        }
        return (mPosPuntoSeleccionado < 0 ? null : Alarmas[mPosPuntoSeleccionado]);
      }
    }

    public async void ClickSobreTendencias(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      OcultarMenus();
      if (e != null)
      {
        if (PuntosTendencia != null && MesesTendencia < 0)
        {
          Int32 Pos = 0;
          foreach (CPunto Punto in PuntosTendencia)
          {
            if ((Punto.Abscisa - e.OffsetX) * (Punto.Abscisa - e.OffsetX) +
                (Punto.Ordenada - e.OffsetY) * (Punto.Ordenada - e.OffsetY) <=
                  CTendencias.SEMIDIAMETRO2)
            {
              PosicionPuntoSeleccionado = Pos;
              mbMostrarAguarda = true;
              StateHasChanged();
              _ = AlCambiarPunto?.Invoke();
              return;
            }
            Pos++;
          }
        }
      }
    }

    public CInformacionAlarmaCN AlarmaParaDatos
    {
      get {
        return (mAlarmas == null || mPosPuntoSeleccionado < 0 || mPosPuntoSeleccionado >= Alarmas.Count ? null :
          Alarmas[mPosPuntoSeleccionado]);
      }
    }

    public void Picar(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
    {
      if (e != null)
      {
        return;
      }
    }

  }
}
