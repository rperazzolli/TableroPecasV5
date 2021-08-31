using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Datos
{
  public class CContenedorComprimido
  {
    // funcion que se llama cuando se cambia el periodo o se unifican varios datasets.
    public delegate void FncPosicionarsePeriodo(CContenedorComprimido sender);
    public delegate void FncCargoDatasets(bool CompletoCarga, string MsgError, string MsgProgreso);

    private ClaseElemento mClaseOrigen;
    private Int32 mCodigoOrigen;
    private Int32 mPeriodoDatos;
    private List<CInformacionAlarmaCN> mPeriodos;
    private List<CProveedorComprimido> mProveedores;
    private CInformacionAlarmaCN mPeriodoEnProceso; // para datos multiples.
    private CProveedorComprimido mProveedorSeleccionado;
    private FncPosicionarsePeriodo mFncPosicionarse;
    private FncCargoDatasets mFncCargo = null;

    public CContenedorComprimido()
    {
      mProveedores = new List<CProveedorComprimido>();
      mPeriodos = new List<CInformacionAlarmaCN>();
      mProveedorSeleccionado = null;
      mClaseOrigen = ClaseElemento.NoDefinida;
      mPeriodoEnProceso=null;
      mCodigoOrigen = -1;
      mPeriodoDatos = -1111;
    }

    public ClaseElemento ClaseOrigen
    {
      get { return mClaseOrigen; }
      set { mClaseOrigen = value; }
    }

    public Int32 CodigoOrigen
    {
      get { return mCodigoOrigen; }
      set { mCodigoOrigen = value; }
    }

    public List<CInformacionAlarmaCN> Periodos
    {
      get { return mPeriodos; }
    }

    public FncPosicionarsePeriodo FuncionPosicionarse
    {
      get { return mFncPosicionarse; }
      set { mFncPosicionarse = value; }
    }

    public Int32 PeriodoDatos
    {
      get { return mPeriodoDatos; }
      set { mPeriodoDatos = value; }
    }

    public Int32 PeriodoDataset
    {
      get { return mPeriodoDatos; }
      set { mPeriodoDatos = value; }
    }

    public bool DatasetMultiple
    {
      get
      {
        return (mProveedorSeleccionado == null ? false :
          mProveedorSeleccionado.ColumnaNombre(CProveedorComprimido.PERIODO_DATOS_DATASET) != null);
      }
    }

    public CProveedorComprimido ProveedorSeleccionado
    {
      get { return mProveedorSeleccionado; }
      set
      {
        if (mProveedorSeleccionado != null && value != null)
        {
          value.CopiarFiltros(mProveedorSeleccionado);
        }
        if (!mProveedores.Contains(value))
        {
          mProveedores.Add(value);
        }
        mProveedorSeleccionado = value;
        if (mFncPosicionarse != null)
        {
          mFncPosicionarse(this);
        }
      }
    }

    public List<CProveedorComprimido> Proveedores
    {
      get { return mProveedores; }
      set
      {
        mProveedores = value;
        if (mProveedorSeleccionado != null && !Proveedores.Contains(mProveedorSeleccionado))
        {
          mProveedorSeleccionado = null;
        }
      }
    }

    private List<DateTime> ObtenerDistintasFechas(Int32 OrdenFecha)
    {
      List<DateTime> Respuesta = new List<DateTime>();
      foreach (Datos.CLineaComprimida Linea in mProveedorSeleccionado.Datos)
      {
        bool HayDatos = false;
        DateTime Fecha = CRutinas.DecodificarFecha(
            Linea.Codigos[OrdenFecha]<0?"":
            (string)mProveedorSeleccionado.Columnas[OrdenFecha].Valores[Linea.Codigos[OrdenFecha]],
            ref HayDatos);
        if (HayDatos && !Respuesta.Contains(Fecha))
        {
          Respuesta.Add(Fecha);
        }
      }

      Respuesta.Sort(delegate(DateTime F1, DateTime F2)
      {
        return F1.CompareTo(F2);
      });

      return Respuesta;
    }

    private CProveedorComprimido ArmarProveedorFechas(DateTime Fecha, DateTime FechaH, Int32 Periodo)
    {
      CProveedorComprimido Respuesta = new CProveedorComprimido(mClaseOrigen, mCodigoOrigen);
      Respuesta.Periodo = new CInformacionAlarmaCN();
      Respuesta.Periodo.Periodo = Periodo + 1;
      Respuesta.Periodo.FechaInicial = Fecha;
      Respuesta.Periodo.FechaFinal = FechaH.AddSeconds(-1);
      Respuesta.Columnas = mProveedorSeleccionado.Columnas;
      return Respuesta;
    }

    public void ParticionarPorFechas(CColumnaBase ColumnaFecha)
    {
      if (mPeriodos.Count == 0)
      {
        // obtener distintas fechas.
        List<DateTime> Fechas = ObtenerDistintasFechas(ColumnaFecha.Orden);

        // armar los periodos.
        for (Int32 i = 0; i < Fechas.Count; i++)
        {
          DateTime FechaH=(i < (Fechas.Count - 1) ? Fechas[i + 1] :
            (Fechas.Count > 1 ? Fechas[i].AddDays(Fechas[1].ToOADate() - Fechas[0].ToOADate()) :
                Fechas[i].AddDays(1)));
          mProveedores.Add(ArmarProveedorFechas(Fechas[i],FechaH, i+1));
          CInformacionAlarmaCN Periodo = new CInformacionAlarmaCN();
          Periodo.CodigoIndicador = mCodigoOrigen;
          Periodo.Color = "AMARILLO";
          Periodo.Comentarios = new List<CComentarioCN>();
          Periodo.DatosParaFecha = true;
          Periodo.Dimension = -1;
          Periodo.ElementoDimension = -1;
          Periodo.FechaInicial = Fechas[i];
          Periodo.FechaFinal = FechaH;
          Periodo.Periodo = i + 1;
          mPeriodos.Add(Periodo);
        }

        // armar los datasets.
        SubdividirDatasetFechas(ColumnaFecha.Orden);

      }
    }

    private void SubdividirDatasetFechas(Int32 Columna)
    {
      foreach (Datos.CLineaComprimida Linea in mProveedorSeleccionado.Datos)
      {
        bool HayDatos = false;
        DateTime Fecha = CRutinas.DecodificarFecha(Linea.Codigos[Columna] < 0 ? "" :
              (string)mProveedorSeleccionado.Columnas[Columna].Valores[Linea.Codigos[Columna]], ref HayDatos);
        if (HayDatos)
        {
          foreach (CProveedorComprimido ProvLocal in mProveedores)
          {
            if (ProvLocal.Periodo != null && ProvLocal.Periodo.FechaInicial == Fecha)
            {
              ProvLocal.Datos.Add(Linea);
              break;
            }
          }
        }
      }
    }

    public string TextoPeriodo(DateTime FechaPosicionador)
    {
      CInformacionAlarmaCN Periodo = PeriodoParaFecha(FechaPosicionador);
      return (Periodo == null ? "" : (CRutinas.FormatearFecha(Periodo.FechaInicial) + " a " +
                CRutinas.FormatearFecha(Periodo.FechaFinal)));
    }

    public CInformacionAlarmaCN PeriodoParaFecha(DateTime Fecha)
    {
      foreach (CProveedorComprimido ProvLocal in mProveedores)
      {
        if (ProvLocal.Periodo.FechaInicial <= Fecha && ProvLocal.Periodo.FechaFinal > Fecha)
        {
          return ProvLocal.Periodo;
        }
      }
      return null;
    }

    private CInformacionAlarmaCN UbicarPeriodoDesdeCodigo(Int32 Codigo)
    {
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        if (Periodo.Periodo == Codigo)
        {
          return Periodo;
        }
      }
      return null;
    }

    private CProveedorComprimido UbicarProveedorDesdePeriodo(Int32 Periodo)
    {
      foreach (CProveedorComprimido Proveedor in mProveedores)
      {
        if (Proveedor.Periodo.Periodo == Periodo)
        {
          return Proveedor;
        }
      }
      return null;
    }

    private void EliminarPeriodosNoEnInstancias(List<CInformacionAlarmaCN> Instancias)
    {
      if (mPeriodos == null)
      {
        mPeriodos = new List<CInformacionAlarmaCN>();
      }

      // si hay un periodo leido y no incluido, lo agrega.
      if (mPeriodoDatos >= 0 && !InstanciaEnPeriodos(mPeriodoDatos))
      {
        AgregarPeriodoEnProcesoAPeriodos(Instancias);
      }

      for (Int32 i = mPeriodos.Count - 1; i >= 0; i--)
      {
        if (!PeriodoEnPeriodos(mPeriodos[i].Periodo, Instancias))
        {
          mPeriodos.RemoveAt(i);
        }
      }
    }

    private void AgregarPeriodoEnProcesoAPeriodos(List<CInformacionAlarmaCN> Instancias)
    {
      if (mPeriodoDatos >= 0)
      {
        foreach (CInformacionAlarmaCN Punto in Instancias)
        {
          if (Punto.Periodo == mPeriodoDatos)
          {
            mPeriodos.Add(Punto);
          }
        }
      }
    }

    private bool PeriodoEnPeriodos(Int32 Periodo, List<CInformacionAlarmaCN> Instancias)
    {
      foreach (CInformacionAlarmaCN Instancia in Instancias)
      {
        if (Instancia.Periodo == Periodo)
        {
          return true;
        }
      }
      return false;
    }

    private bool InstanciaEnPeriodos(Int32 PeriodoInstancia)
    {
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        if (Periodo.Periodo == PeriodoInstancia)
        {
          return true;
        }
      }
      return false;
    }

    public void MoverseAPeriodo(Int32 Periodo)
    {
      CProveedorComprimido ProvLocal = UbicarProveedorDesdePeriodo(Periodo);
      if (ProvLocal != null) // encontro el proveedor. quiere decir que tenia los datos leidos.
      {
        if (ProvLocal != mProveedorSeleccionado)
        {
          if (ProvLocal != mProveedorSeleccionado && mProveedorSeleccionado != null)
          {
            ProvLocal.CopiarFiltros(mProveedorSeleccionado);
          }
          mProveedorSeleccionado = ProvLocal;
        }
      }
      else // no lo encontro.
      {
        mPeriodoDatos = -1;
        mPeriodoEnProceso = null;
        CProveedorComprimido ProvL = new CProveedorComprimido(mClaseOrigen, mCodigoOrigen);
        if (mProveedorSeleccionado != null)
        {
          ProvL.Columnas = CProveedorComprimido.CopiarColumnas(mProveedorSeleccionado.Columnas);
          ProvL.CopiarFiltros(mProveedorSeleccionado);
        }
        mProveedorSeleccionado = ProvL;

      }
    }

    public async Task CargarDatosPeriodoAsync(HttpClient Http, CInformacionAlarmaCN Periodo)
    {

      try
      {

        mPeriodoEnProceso = Periodo;

        RespuestaDatasetBin Respuesta = await Contenedores.CContenedorDatos.LeerDetalleDatasetAsync(
              Http, Periodo.CodigoIndicador,
              Periodo.Dimension, Periodo.ElementoDimension, Periodo.Periodo);
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }
        mPeriodoEnProceso.DatosParaFecha = true;
        SumarDatosPeriodo(Periodo.Periodo, ClaseElemento.Indicador, Periodo.CodigoIndicador,
              Respuesta.Datos);
        await CargarProximoPeriodoAsync(Http);
      }
      catch (Exception ex)
			{
        CRutinas.DesplegarMsg(ex);
			}

      // cargar el listado de puntos de tendencia.
      //WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
      //try
      //{
      //  CRespuestaDatasetBin Respuesta=await Cliente.ObtenerDetalleIndicadorBinAsync(
      //        TableroPecasV5.Client.Contenedores.CContenedorDatos.Ticket, new Guid().ToString(),
      //        Periodo.CodigoIndicador, Periodo.Periodo, Periodo.Dimension, Periodo.ElementoDimension,
      //        false);
      //  if (!Respuesta.RespuestaOK)
      //  {
      //    throw new Exception(Respuesta.MensajeError);
      //  }
      //  switch (Respuesta.Situacion)
      //  {
      //    case SituacionPedido.EnMarcha:
      //      await CargarDatosPeriodoAsync(mPeriodoEnProceso);
      //      break;
      //    case SituacionPedido.Abortado:
      //      CopiarFiltros();
      //      mFncCargo(true, "Abortado", "");
      //      break;
      //    default:
      //      mPeriodoEnProceso.DatosParaFecha = true;
      //      SumarDatosPeriodo(Respuesta);
      //      await CargarProximoPeriodoAsync();
      //      break;
      //  }
      //}
      //catch (Exception ex)
      //{
      //  CopiarFiltros();
      //  mFncCargo(true, CRutinas.MostrarMensajeError(ex), "");
      //}
      //finally
      //{
      //  await Cliente.CloseAsync();
      //}

    }

    private string MensajeProgresoPeriodos()
    {
      Int32 Importados = 1; // se usa para el proximo.
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        Importados += (Periodo.DatosParaFecha == true ? 1 : 0);
      }
      return "Importando " + Importados.ToString() + " de " + mPeriodos.Count.ToString();
    }

    private async Task CargarProximoPeriodoAsync(HttpClient Http)
    {
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        if (!Periodo.DatosParaFecha)
        {
          mFncCargo(false, "", MensajeProgresoPeriodos()); // para saber que se completo la lectura de los periodos.
          await CargarDatosPeriodoAsync(Http, Periodo);
          return;
        }
      }

      // ya se completo la lectura de los periodos. Ordena por fecha.
      mPeriodos.Sort(delegate(CInformacionAlarmaCN P1, CInformacionAlarmaCN P2)
      {
        return P1.FechaInicial.CompareTo(P2.FechaInicial);
      });

      CopiarFiltros();

      mFncCargo(true, "", "");

    }

    private CProveedorComprimido AgregarProveedorLimpio()
    {
      CProveedorComprimido ProvL = new CProveedorComprimido(mClaseOrigen, mCodigoOrigen);
      mProveedores.Add(ProvL);
      return ProvL;
    }

    private void SumarDatosPeriodo(Int32 PeriodoDatos, ClaseElemento ClaseOrigen, Int32 CodigoOrigen,
        byte[] Datos)
    {
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        if (Periodo.Periodo == PeriodoDatos)
        {
          Periodo.DatosParaFecha = true;
          CProveedorComprimido ProvLocal = UbicarProveedorDesdePeriodo(PeriodoDatos);
          if (ProvLocal == null)
          {
            ProvLocal = AgregarProveedorLimpio();
          }
          ProvLocal.ClaseOrigen = ClaseOrigen;
          ProvLocal.CodigoOrigen = CodigoOrigen;
          ProvLocal.Periodo = Periodo;
          ProvLocal.ProcesarDatasetBinario(Datos, false);
          return;
        }
      }

    }

    private void CopiarFiltros()
    {
      if (mPeriodoEnProceso != null)
      {
        CProveedorComprimido ProveedorL = UbicarProveedorDesdePeriodo(mPeriodoEnProceso.Periodo);
        if (ProveedorL != null && ProveedorL != mProveedorSeleccionado &&
              mProveedorSeleccionado.Filtros.Count > 0)
        {
          ProveedorL.CopiarFiltros(mProveedorSeleccionado);
        }
      }
    }

    public void FechasExtremas(ref DateTime FechaMinima, ref DateTime FechaMaxima)
    {
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        if (Periodo.FechaInicial < FechaMinima)
        {
          FechaMinima = Periodo.FechaInicial;
        }
        if (Periodo.FechaInicial > FechaMaxima)
        {
          FechaMaxima = Periodo.FechaInicial;
        }
      }
    }

    public DateTime FechaInicioSaltoAnterior(DateTime Fecha)
    {
      for (Int32 i = mPeriodos.Count - 1; i >= 0; i--)
      {
        if (mPeriodos[i].FechaInicial < Fecha)
        {
          return mPeriodos[i].FechaInicial;
        }
      }
      return CRutinas.FechaMinima();
    }

    public DateTime FechaInicioSaltoPosterior(DateTime Fecha)
    {
      foreach (CInformacionAlarmaCN Periodo in mPeriodos)
      {
        if (Periodo.FechaInicial > Fecha)
        {
          return Periodo.FechaInicial;
        }
      }
      return CRutinas.FechaMinima();
    }

  }
}
