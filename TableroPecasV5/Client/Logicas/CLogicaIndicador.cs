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
          try
          {
            List<CInformacionAlarmaCN> Alarmas = (from A in Contenedores.CContenedorDatos.gAlarmasIndicador
                                                  where A.CodigoIndicador == mComponenteTendRed.Indicador.Codigo &&
                                                    A.ElementoDimension == mComponenteTendRed.CodigoElementoDimension
                                                  orderby A.Periodo
                                                  select A).ToList();
            if (Alarmas != null && Alarmas.Count > 0)
            {
              return Alarmas.Last().Periodo;
            }
          }
          catch (Exception)
          {
            //
          }
        }

        return -1;

      }
    }

    public bool AguardandoReloj { get; set; } = !Contenedores.CContenedorDatos.SiempreTendencia;

    public bool AguardandoFiltros { get; set; } = true;

    [Inject]
    IJSRuntime JSRuntime { get; set; }

    [Parameter]
    public Int32 NivelReloj
    {
      get { return (ComponenteReloj == null ? 1 : ComponenteReloj.NivelFlotante); }
      set
      {
        if (value!=NivelReloj && ComponenteReloj != null)
        {
          ComponenteReloj.ImponerNivelFlotante(value);
        }
      }
    }

    public bool TendenciasAmpliadas { get; set; } = false;

    [Parameter]
    public Int32 NivelTendencias { get; set; }

    public Int32 NivelGrilla = 1;

    [Parameter]
    public Int32 NivelFiltros
    {
      get { return (ComponenteFiltros == null || HayGraficoAmpliado ? 1 : ComponenteFiltros.NivelFlotante); }
      set
      {
        if (value != NivelFiltros && ComponenteFiltros != null)
        {
          ComponenteFiltros.ImponerNivelFlotante(value);
        }
      }
    }

    protected bool mbLeyo = false;

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

    public Int32 CodigoElementoDimension { get; set; } = -1;

    public static string Ayuda { get; set; } = "AA";

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
    public ObservableCollection<Clases.CCurvaTendencia> Curvas { get; set; } = new ObservableCollection<Clases.CCurvaTendencia>();

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
      NivelTendencias = 2;
      NivelFiltros = 2;

      if (ComponenteFiltros != null)
      {
        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
        {
          Link.NivelFlotante = 2;
        }

        foreach (CLinkGrafico Lnk in ComponenteFiltros.Graficos)
        {
          Lnk.NivelFlotante = 2;
          Lnk.Componente.DeSeleccionar();
        }
      }

		}

    public void PonerTendenciaArriba()
    {
      if (NivelTendencias < 8)
      {
        PonerElementoEncima(false, true, false, -1, -1);
        StateHasChanged();
      }
    }
    public void PonerContenedorFiltroArriba()
		{
      if (NivelFiltros < 8)
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

    private long mAnchoTendencia = AnchoTendenciasDefault;
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

    private long mAltoTendencia = AltoTendenciaDefault;

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

    private Int32 mAbscisaTendencia = ABSCISA_INI_TENDENCIAS;

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

    private Int32 mOrdenadaTendencia = 5;
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
            NivelReloj.ToString() + ";";
      }
    }


    public string EstiloTendencias
    {
      get
      {
        return "width: " + AnchoTendencia.ToString() + "px; height: " +
          AltoTendencia.ToString() +
          "px; z-index: " + NivelTendencias.ToString() +
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
          OrdenadaContenedorFiltros.ToString() + "px; position: absolute; text-align: center; z-index: " +
          NivelFiltros.ToString() + ";";
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
        "px; position: absolute; text-align: center; z-index: " + Lnk.NivelFlotante.ToString() +
        "; resize: horizontal; overflow: hidden;";
    }

    public string EstiloGrafico(CLinkGrafico Lnk)
    {
      return "width: " + Lnk.Ancho.ToString() + "px; height: " +
        Lnk.Alto.ToString() +
        "px; margin-left: " + Lnk.Abscisa.ToString() +
        "px; margin-top: " + Lnk.Ordenada.ToString() + "px; position: absolute; text-align: " +
        (Lnk.Clase == ClaseGrafico.BarrasH ? "left" : "center") +
        "; z-index: " + Lnk.NivelFlotante.ToString() + ";" +
        (Lnk.Clase == ClaseGrafico.BarrasH ? " overflow: hidden;" : "");
    }

    public string EstiloGrilla
    {
      get
      {
        return "width: " + Grilla.Ancho.ToString() + "px; height: " +
          Grilla.Alto.ToString() +
          "px; margin-left: " + Grilla.Abscisa.ToString() +
          "px; margin-top: " + Grilla.Ordenada.ToString() + "px; position: absolute; text-align: center" +
          "; z-index: " + NivelGrilla.ToString() + "; overflow: hidden; font-size: 11px;";
      }
    }

    public void CrearTendencias()
    {
      HayTendencias = true;
      StateHasChanged();
      //AguardandoReloj = true;
      //StateHasChanged();
      //try
      //{
      //  if (ComponenteReloj.Alarmas == null || ComponenteReloj.Alarmas.Count < 2)
      //  {
      //    ComponenteReloj.ImponerAlarmas(null);
      //    await LeerAlarmasAsync(true);
      //    HayTendencias = true;
      //  }
      //}
      //finally
      //{
      //  AguardandoReloj = false;
      //  StateHasChanged();
      //}
    }

    private Int32 mPeriodoDataset = -1;
    private List<CColumnaBase> mColumnasDataset;
    private CProveedorComprimido mProveedor = null;

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

    public bool VerDetalleIndicador { get; set; } = false;

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
          Periodo = PeriodoAlarmaTendRed;
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

    public ClaseElemento ClaseOrigen { get; set; } = ClaseElemento.NoDefinida;
    public Int32 CodigoOrigen { get; set; } = -1;

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

    protected List<CInformacionAlarmaCN> mAlarmasImpuestas = null;

    public CInformacionAlarmaCN UltimaAlarma
		{
      get
			{
        return (Alarmas != null && Alarmas.Count > 0 ? Alarmas.Last() : null);
			}
		}

    protected List<CInformacionAlarmaCN> Alarmas
		{
      get
			{
        return (mAlarmasImpuestas == null ?
            (ComponenteTendencias == null ? ComponenteTendRed.Alarmas : ComponenteTendencias.Alarmas) :
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

    private List<BlockDatosZip> mBlocksDatos = new List<BlockDatosZip>();
    
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

    private Int32 mAbscisaTendenciaAnterior = -999999;
    private Int32 mOrdenadaTendenciaAnterior = -999999;
    private long mAnchoTendenciaAnterior = -999999;
    private long mAltoTendenciaAnterior = -999999;

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
          NivelTendencias = 8;
          PonerElementoEncima(false, true, false, -1, -1);
				}
        else
				{
          AbscisaTendencia = mAbscisaTendenciaAnterior;
          OrdenadaTendencia = mOrdenadaTendenciaAnterior;
          AnchoTendencia = mAnchoTendenciaAnterior;
          AltoTendencia = mAltoTendenciaAnterior;
          NivelTendencias = 1;
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
      NivelFiltros = (Filtros ? 8 : 1);
      NivelTendencias = (Tendencia ? 8 : 1);
      NivelGrilla = (Grilla ? 8 : 1);
      if (ComponenteFiltros != null)
      {
        foreach (CLinkFiltros Link in ComponenteFiltros.Links)
        {
           if (Link.Componente != null)
					{
            Link.NivelFlotante = (Link.Componente.Filtrador.Columna.Orden == OrdenFiltro ? 8 : 1);
					}
           else
					{
            Link.NivelFlotante = 1;
					}
        }

        foreach (CLinkGrafico Graf in ComponenteFiltros.Graficos)
				{
          if (Graf.Componente != null)
					{
            Graf.NivelFlotante = (Graf.Componente.CodigoUnico == CodigoGrafico ? 8 : 1);
					}
          else
					{
            Graf.NivelFlotante = 1;
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
            Graf.NivelFlotante = 1;
          }
          if (Abscisa > -999998)
          {
            Graf.GuardarPosicion();
            Graf.NivelFlotante = (Encima ? 8 : 1);
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

    public bool HayTendencias { get; set; }
    public bool HayFiltroDatos { get; set; } = false;

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

    private CReloj mComponenteReloj = null;
    private CTendRed mComponenteTendRed = null;

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

    private bool mbAlarmaReducida = false;
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
          StateHasChanged();
				}
			}
		}

    public CTendRed ComponenteTendRed
    {
      get { return mComponenteTendRed; }
      set
      {
        mComponenteTendRed = value;
        ComponenteTendRed.ImponerPosicion(10, 5, 350, 180);
        HayAlarmaReducida = (PeriodoAlarmaTendRed > -1);
      }
    }

    private CComponenteTendencias mComponenteTendencias = null;
    public CComponenteTendencias ComponenteTendencias
    {
      get { return mComponenteTendencias; }
      set
      {
        mComponenteTendencias = value;
        mComponenteTendencias.FncReposicionarArriba = PonerTendenciaArriba;
        CLogicaTendencias.DimensionCaracter = mDimensionCaracter;
        ComponenteTendencias.ImponerPosicion(ABSCISA_INI_TENDENCIAS, 5, (Int32)AnchoTendenciasDefault, (Int32)AltoTendenciaDefault);
        mComponenteTendencias.EjecutarRefresco = FncRefrescar;
        mComponenteTendencias.AlCambiarPunto += ReposicionarDatasetAsync;
      }
    }

    public void EliminarVariosArriba()
		{
      NivelTendencias = 2;
      NivelFiltros = 2;
		}

    private void FncRefrescar()
    {
      StateHasChanged();
    }

    private async Task ReposicionarDatasetAsync()
    {
      PosicionPuntoTendencia = ComponenteTendencias.PosicionPuntoSeleccionado;
      if (ComponenteFiltros != null)
      {
        await CrearFiltroDatosAsync();
        ComponenteFiltros.Proveedor.RefrescarDependientes();
//        ComponenteFiltros.Proveedor.FiltrarPorAsociaciones();
        ComponenteTendencias.MostrarAguarda = false;
        StateHasChanged();
      }
    }

    private CContenedorFiltros mComponenteFiltros = null;
    public CContenedorFiltros ComponenteFiltros
    {
      get { return mComponenteFiltros; }
      set
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

    public void EliminarGrilla()
    {
      Grilla = null;
      StateHasChanged();
    }

    private CLinkGrafico mGraficoDrag = null;
    private CLinkFiltros mFiltroDrag = null;
    private LineaFiltro mLineaDrag = null;
    private CLinkGrilla mGrillaDrag = null;

    public LineaFiltro LineaDrag
		{
      get { return mLineaDrag; }
      set { mLineaDrag = value; }
		}

    public void IniciarDragGrafico(CLinkGrafico Grafico)
    {
      mFiltroDrag = null;
      mLineaDrag = null;
      mGrillaDrag = null;
      mGraficoDrag = Grafico;
    }

    public void IniciarDragFiltro(CLinkFiltros Filtro)
    {
      CambioMedidas(Filtro);
      mFiltroDrag = Filtro;
      mLineaDrag = null;
      mGrillaDrag = null;
      mGraficoDrag = null;
    }

    public async void CambioMedidas(CLinkFiltros Filtro)
    {
      object[] Args = new object[1];
      Args[0] = IdFiltro(Filtro);
      string Dimensiones = await JSRuntime.InvokeAsync<string>("FuncionesJS.getRectangulo", Args);
      List<double> Valores = CRutinas.ListaAReales(Dimensiones);
      Filtro.Ancho = (Int32)Valores[2];
    }

    public void IniciarDragLinea(LineaFiltro Linea)
    {
      mFiltroDrag = null;
      mLineaDrag = Linea;
      mGrillaDrag = null;
      mGraficoDrag = null;
    }

    public void IniciarDragGrilla(CLinkGrilla Grilla)
		{
      mFiltroDrag = null;
      mLineaDrag = null;
      mGrillaDrag = Grilla;
      mGraficoDrag = null;       
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
          Int32 Diferencia = (int)e.ScreenX - mGraficoDrag.Componente.AbscisaAbajo;
          Link.Abscisa += Diferencia;
          Diferencia = (int)e.ScreenY - mGraficoDrag.Componente.OrdenadaAbajo;
          Link.Ordenada += Diferencia;
          StateHasChanged();
        }
      }
      if (mFiltroDrag != null)
      {
        CLinkFiltros Link = (from G in ComponenteFiltros.Links
                             where G.Componente.CodigoUnico == mFiltroDrag.Componente.CodigoUnico
                             select G).FirstOrDefault();
        if (Link != null)
        {
          Int32 Diferencia = (int)e.ScreenX - mFiltroDrag.Componente.AbscisaAbajo;
          Link.Abscisa += Diferencia;
          Diferencia = (int)e.ScreenY - mFiltroDrag.Componente.OrdenadaAbajo;
          Link.Ordenada += Diferencia;
          StateHasChanged();
        }
      }
      if (mLineaDrag != null)
			{
        ComponenteFiltros.FncSeleccionFila(mLineaDrag.Columna.Orden, (int)e.ClientX,
            (int)e.ClientY);
      }
      if (mGrillaDrag != null)
			{
        if (ComponenteFiltros != null && ComponenteFiltros.Grilla != null)
        {
          Int32 Abscisa = ComponenteFiltros.Grilla.Abscisa + (int)e.ScreenX - ComponenteFiltros.Grilla.Componente.AbscisaAbajo;
          Int32 Ordenada = ComponenteFiltros.Grilla.Ordenada + (int)e.ScreenY - ComponenteFiltros.Grilla.Componente.OrdenadaAbajo;
          ReposicionarGrilla(Abscisa, Ordenada, (int)ComponenteFiltros.Grilla.Componente.Ancho,
              (int)ComponenteFiltros.Grilla.Componente.Alto);
          StateHasChanged();
        }
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

    private double mDimensionCaracter = -1;
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

    private string mbSinTendencia = "disabled";
    public string SinTendencia
    {
      get { return mbSinTendencia; }
      set { mbSinTendencia = value; }
    }

    private CDatoIndicador mIndicador = null;
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

    private List<CInformacionAlarmaCN> mAlarmas = null;

    public string Valor()
    {

      if (Indicador == null)
      {
        return "";
      }

      if (mAlarmas == null)
			{
        _ = LeerAlarmasAsync(false);
        return "";
			}

      if (mAlarmas.Count == 0)
      {
        return "";
      }
      else
      {
        return CRutinas.ValorATexto(mAlarmas.Last().Valor, Indicador.Decimales);
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

    private CBaseGrafico mGraficoSeleccionadoImpuesto = null;

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
      if (mProveedor != null)
      {
        mProveedor = null;
      }
    }

  }

}
