using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Contenedores
{
  public class CContenedorDatos : ComponentBase
  {

    public static Int32 AnchoPantalla { get; set; }
    public static Int32 AltoPantalla { get; set; }

    public static long AnchoOpcionSolapa { get; set; }

    public static long AltoOpcionSolapa { get; set; }

    public static List<Clases.CElementoXML> gElementosXML = new List<CElementoXML>();

    public static Int32 DefasajeAbscisasPantallaIndicadores { get; set; } = -1;
    public static Int32 DefasajeOrdenadasPantallaIndicadores { get; set; } = -1;
    public static Int32 AnchoPantallaIndicadores { get; set; } = -1;
    public static List<Int32> gComitesEnterosUsuario = null;
    public static List<CGrupoPuestosCN> gComitesUsuario = null;

    public static bool EsAdministrador { get; set; }
    public static bool TendenciasEnTarjeta { get; set; }
    public static bool DesciendeEnRojo { get; set; }
    public static bool SiempreTendencia { get; set; }
    public static bool PoneEtiquetas { get; set; }
    public static bool RespetaSentido { get; set; }
    public static string ImprimirPDF { get; set; }
    public static Int32 AnchoPantallaAmpliada { get { return (AnchoPantallaIndicadores > 0 ? AnchoPantallaIndicadores : AnchoPantalla); } }
    public static Int32 AltoPantallaAmpliada { get { return (AltoPantallaIndicadores > 0 ? AltoPantallaIndicadores : AltoPantalla - 120); } }
    public static Int32 AltoPantallaIndicadores { get; set; } = -1;

    //public static List<WCFEstructura.CGrupoPuestosCN> Comites { get; set; } = null;
    //public static List<CElementoMimicoCN> Mimicos { get; set; } = null;

    public static List<CSubconsultaExt> gSubconsultas = null;
    //public async static Task<List<CSubconsultaExt>> ObtenerSubconsultasAsync()
    //{
    //  if (gSubconsultas == null)
    //  {
    //    WCFBPIClient Cliente = CRutinas.ObtenerClienteBPI();
    //    try
    //    {
    //      CRespuestaSubconsultas Respuesta = await Cliente.ListarSubconsultasAsync(Ticket, "");
    //      if (!Respuesta.RespuestaOK)
    //      {
    //        throw new Exception("Al listar SC " + Respuesta.MensajeError);
    //      }
    //      gSubconsultas = Respuesta.Subconsultas;
    //    }
    //    catch (Exception ex)
    //    {
    //      gSubconsultas = new List<CSubconsultaExt>();
    //      CRutinas.DesplegarMsg(ex);
    //    }
    //    finally
    //    {
    //      await Cliente.CloseAsync();
    //    }
    //  }
    //  return gSubconsultas;
    //}

    public static string MsgUsuario { get; set; } = "";
    public static bool VerBotonMsg { get; set; }
    public static string EstiloBotonMsg { get; set; } = "width: 100%; text-align: center; position: relative; margin-top: -50px; visible: " +
          (VerBotonMsg ? "visible; " : "collapse; ");

    public static void MostrarMensaje(string Msg)
    {
      MsgUsuario = Msg;
      VerBotonMsg = true;
      Logicas.CLogicaMainMenu.RefrescarPantalla();
    }

    public static double MinutosTicket { get; set; } = 10;
    public static DateTime gFechaLimite { get; set; }

    public static void AjustarHoraHasta()
    {
      if (MinutosTicket > 0)
      {
        gFechaLimite = DateTime.Now.AddMinutes(MinutosTicket);
      }
    }

    public async static Task<CVinculoIndicadorCompletoCN> LeerVinculosIndicadorAsync(HttpClient Http,
          ClaseElemento Clase, Int32 Indicador, string Columna)
    {
      try
      {

        RespuestaDetalleVinculo Respuesta = await Http.GetFromJsonAsync<RespuestaDetalleVinculo>(
            "api/Vinculos/LeerVinculo?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&ClaseIndicador="+((Int32)Clase).ToString()+
            "&Codigo=" + Indicador.ToString() +
            "&Columna=" + Columna);
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }

        return Respuesta.Vinculo;

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return null;
      }
    }

    public async static Task<RespuestaDatasetBin> LeerDetalleDatasetAsync(HttpClient Http,
          Int32 Indicador, Int32 Dimension,
          Int32 Elemento, Int32 Periodo)
    {
      try
      {

        if (Periodo < 0)
        {
          List<CInformacionAlarmaCN> Info = await ObtenerAlarmasIndicadorAsync(Http, Indicador, Elemento);
          if (Info == null || Info.Count == 0)
          {
            throw new Exception("No hay períodos definidos");
          }
          Periodo = Info.Last().Periodo;
        }

        string szGUID = Guid.NewGuid().ToString();

        RespuestaDatasetBin Respuesta = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
            "api/Dataset/GetDataset?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&Indicador=" + Indicador.ToString() +
            "&Dimension=" + Dimension.ToString() +
            "&ElementoDimension=" + Elemento.ToString() +
            "&Periodo=" + Periodo.ToString() +
            "&UnicamenteColumnas=false" +
            "&GUID=" + szGUID);
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }
        while (Respuesta.Situacion != SituacionPedido.Completado)
        {
          switch (Respuesta.Situacion)
          {
            case SituacionPedido.EnMarcha:
              Respuesta = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
                "api/Dataset/RefrescarPedido?URL=" + Contenedores.CContenedorDatos.UrlBPI +
                "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
                "&GUID=" + szGUID +
                "&ContinuarSN=S");
              break;
            case SituacionPedido.Abortado:
              throw new Exception("Pedido abortado");
            default:
              break;
          }
          if (!Respuesta.RespuestaOK)
          {
            throw new Exception(Respuesta.MsgErr);
          }
        }
        return Respuesta;
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return null;
      }
    }

    public async static Task<RespuestaCapasGIS> LeerCapasWFSAsync(HttpClient Http, bool UnicamenteWFS,
        bool SinDetalle)
    {
      try
      {

        RespuestaCapasGIS Respuesta = await Http.GetFromJsonAsync<RespuestaCapasGIS>(
            "api/Capas/ListarTodasLasCapas?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&WFS=" + CRutinas.BoolToStr(UnicamenteWFS) +
            "&SinDetalle=" + CRutinas.BoolToStr(SinDetalle));
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }

        return Respuesta;

      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return null;
      }
    }

    public async static Task<RespuestaDatasetBin> LeerDetalleSubconsultaAsync(HttpClient Http,
          Int32 Subconsulta, List<CParametroExt> Parametros)
		{
      return await LeerDetalleSubconsultaAsync(Http, Subconsulta, CRutinas.ListaPrmsATexto(Parametros));
		}

    public async static Task<RespuestaDatasetBin> LeerDetalleSubconsultaAsync(HttpClient Http,
        Int32 Subconsulta, string Parametros)
    {
      try
      {

        RespuestaDatasetBin Respuesta = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
            "api/SubConsultas/LeerDataset?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
            "&Codigo=" + Subconsulta.ToString() +
            "&Parametros=" + Parametros);
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }
        while (Respuesta.Situacion != SituacionPedido.Completado)
        {
          switch (Respuesta.Situacion)
          {
            case SituacionPedido.EnMarcha:
              Respuesta = await Http.GetFromJsonAsync<RespuestaDatasetBin>(
                "api/SubConsultas/RefrescarPedido?URL=" + Contenedores.CContenedorDatos.UrlBPI +
                "&Ticket=" + Contenedores.CContenedorDatos.Ticket +
                "&GUID=" + Respuesta.GUID +
                "&ContinuarSN=S");
              break;
            case SituacionPedido.Abortado:
              throw new Exception("Pedido abortado");
            default:
              break;
          }
          if (!Respuesta.RespuestaOK)
          {
            throw new Exception(Respuesta.MsgErr);
          }
        }
        return Respuesta;
      }
      catch (Exception ex)
      {
        CRutinas.DesplegarMsg(ex);
        return null;
      }
    }

    public static string NombreOrigen(ClaseElemento Clase, Int32 Codigo)
		{
      switch (Clase)
			{
        case ClaseElemento.Indicador:
          CDatoIndicador Indi = IndicadorDesdeCodigo(Codigo);
          return "Ind " + (Indi == null ? "??" : Indi.Descripcion);
        case ClaseElemento.SubConsulta:
          CSubconsultaExt SubC = SubconsultaCodigo(Codigo);
          return "SubC " + (SubC == null ? "??" : SubC.Descripcion);
        default: return "??";
      }
    }

    public static Int32 ObtenerOrdenProximaSolapa(Int32 Sala)
		{
      foreach (CPuntoSala Punto in gEstIndicadores.Salas)
			{
        if (Punto.Sala.Codigo == Sala)
				{
          if (Punto.Solapas.Count == 0)
					{
            return 0;
					}
          else
					{
            return (from S in Punto.Solapas
                    select S.Solapa.Orden).Max() + 1;
					}
				}
			}
      return 0;
		}

    public static bool EventosBloqueados { get; set; } = false;

    public static Dictionary<IdentificadorIndicador, List<CInformacionAlarmaCN>> gDatosAlarma =
        new Dictionary<IdentificadorIndicador, List<CInformacionAlarmaCN>>();

    static CContenedorDatos()
    {
      Usuario = ""; // "fcuello";
      Clave = ""; // "fc1234!";
      Ticket = "";
      UrlBPI = "";
      UrlEstructura = "";
      AnchoPantalla = -1;
      AltoPantalla = -1;
      HabilitarIndicadores = false;
      HabilitarSalasReunion = false;
      HabilitarMimicos = false;
      ListaIndicadores = new List<CDatoIndicador>();
      ListaSalasReunion = new List<CListaTexto>();
      ListaSalasReunion.Add(new CListaTexto(1, "Sala nro 1"));
      ListaSalasReunion.Add(new CListaTexto(2, "Sala nro 2"));
    }

    public static async Task<List<CInformacionAlarmaCN>> ObtenerAlarmasIndicadorAsync(HttpClient Cliente,
          Int32 Indicador,
          Int32 ElementoDimension = -1,
          bool Forzada = false)
		{
      return await ObtenerAlarmasIndicadorAsync(Cliente, IndicadorDesdeCodigo(Indicador), ElementoDimension, Forzada);
		}

    public static CInformacionAlarmaCN AlarmaIndicadorDesdeGlobal(Int32 Indicador, Int32 Dimension)
    {
      List<CInformacionAlarmaCN> DatosLocales = (from A in gAlarmasIndicador
                                                 where A.CodigoIndicador == Indicador &&
                                                   A.ElementoDimension == Dimension
                                                 orderby A.Periodo
                                                 select A).ToList();
      if (DatosLocales != null && DatosLocales.Count > 0)
      {
        return DatosLocales.Last();
      }
      else
      {
        return null;
      }
    }

    public static async Task<List<CInformacionAlarmaCN>> ObtenerAlarmasIndicadorAsync(HttpClient Cliente,
        CDatoIndicador Indicador,
        Int32 ElementoDimension = -1,
        bool Forzada = false)
    {
      if (Indicador == null)
      {
        return new List<CInformacionAlarmaCN>();
      }

      if (gAlarmasIndicador != null && !Forzada)
      {
        List<CInformacionAlarmaCN> DatosLocales = (from A in gAlarmasIndicador
                                                   where A.CodigoIndicador == Indicador.Codigo &&
                                                     A.ElementoDimension == ElementoDimension
                                                   orderby A.Periodo
                                                   select A).ToList();
        if (DatosLocales.Count > 0)
        {
          return DatosLocales;
        }
      }

      IdentificadorIndicador Referencia = new IdentificadorIndicador(Indicador.Codigo, ElementoDimension);
      if (gDatosAlarma != null && !Forzada)
      {
        lock (gDatosAlarma)
        {
          List<CInformacionAlarmaCN> Datos;
          if (gDatosAlarma.TryGetValue(Referencia, out Datos))
          {
            return Datos;
          }
        }
      }
      try
      {
        RespuestaInformacionAlarmaVarias Respuesta = await Cliente.GetFromJsonAsync<RespuestaInformacionAlarmaVarias>(
            "api/Alarmas/GetAlarmas?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Ticket +
            "&Indicador=" + Indicador.Codigo.ToString() +
            "&Dimension=" + Indicador.Dimension.ToString() +
            "&Elemento=" + ElementoDimension.ToString() +
            "&Fecha=" + CRutinas.FechaATexto(DateTime.Now));

        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MsgErr);
        }
        lock (gDatosAlarma)
        {
          if (gDatosAlarma.ContainsKey(Referencia))
          {
            gDatosAlarma.Remove(Referencia);
          }
          gDatosAlarma.Add(Referencia, Respuesta.Instancias);
          return Respuesta.Instancias;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Al intentar acceder a información de alarmas" + Environment.NewLine + ex.Message);
      }
    }

		public static async Task<List<CInformacionAlarmaCN>> ObtenerAlarmasIndicadorEntreFechasAsync(
        HttpClient Cliente,
				CDatoIndicador Indicador, DateTime Desde, DateTime Hasta, Int32 CodigoElemento = -1)
		{
			IdentificadorIndicador Identificador = new IdentificadorIndicador(Indicador.Codigo, CodigoElemento);
      if (gAlarmasIndicador != null)
			{
        List<CInformacionAlarmaCN> DatosLocales = (from A in gAlarmasIndicador
                                                   where A.CodigoIndicador == Indicador.Codigo &&
                                                     A.ElementoDimension == CodigoElemento
                                                   orderby A.Periodo
                                                   select A).ToList();
        if (DatosLocales.Count > 0)
				{
          return DatosLocales;
				}
			}

			lock (gDatosAlarma)
			{
				List<CInformacionAlarmaCN> Datos;
				if (gDatosAlarma.TryGetValue(Identificador, out Datos))
				{
					return Datos;
				}
			}

			try
			{
        RespuestaInformacionAlarmaVarias Respuesta = await Cliente.GetFromJsonAsync<RespuestaInformacionAlarmaVarias>(
            "api/Alarmas/GetAlarmasEntreFechas?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Ticket +
            "&Indicador=" + Indicador.Codigo.ToString() +
            "&Dimension=" + Indicador.Dimension.ToString() +
            "&Elemento=" + CodigoElemento.ToString() +
            "&Desde=" + CRutinas.FechaATexto(DateTime.Now) +
            "&Hasta=" + CRutinas.FechaATexto(Hasta));
        if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MsgErr);
				}
				lock (gDatosAlarma)
				{
					if (gDatosAlarma.ContainsKey(Identificador))
					{
						gDatosAlarma.Remove(Identificador);
					}
					gDatosAlarma.Add(Identificador, Respuesta.Instancias);
					return Respuesta.Instancias;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Al intentar acceder a información de alarmas fechas" + Environment.NewLine + ex.Message);
			}
		}

		public static async Task<bool> InicializarDimensionesAsync(Microsoft.JSInterop.IJSRuntime Runtime)
    {
      if (AnchoPantalla < 0)
      {
        AnchoPantalla = await Runtime.InvokeAsync<Int32>("FuncionesJS.getInnerWidth", null) - 40;
        AltoPantalla = await Runtime.InvokeAsync<Int32>("FuncionesJS.getInnerHeight", null);
      }
      return true;
    }

    public static CEstIndicadoresCN EstructuraIndicadores
		{
      get
			{
        return gEstIndicadores;
			}
		}

    private static CEstIndicadoresCN gEstIndicadores;

    public static string Usuario { get; set; }
    public static string Clave { get; set; }
    public static string Ticket { get; set; }
    public static string UrlBPI { get; set; }
    public static string UrlEstructura { get; set; }
    public static Int32 CodigoUsuario { get; set; } = -1;
    public static List<CListaTexto> ListaSalasReunion { get; set; }
    public static List<CDatoIndicador> ListaIndicadores { get; set; }
    public static List<CMapaBingCN> ListaMapas { get; set; }
    public static List<CElementoMimicoCN> ListaMimicos { get; set; }
    public static bool HabilitarIndicadores { get; set; }
    public static bool HabilitarSalasReunion { get; set; }

    public static bool HabilitarMimicos { get; set; }

    public static bool HabilitarMapas { get; set; } = false;

    private static bool mbHabilitarLogin = true;
    public static bool HabilitarLogin
    {
      get
      {
        return mbHabilitarLogin;
      }
      set
      {
        if (value != mbHabilitarLogin)
        {
          mbHabilitarLogin = value;
        }
      }
    }

    public static CElementoMimicoCN MimicoDesdeCodigo(Int32 Codigo)
		{
      return (from M in ListaMimicos
              where M.Codigo == Codigo
              select M).FirstOrDefault();
		}

    public static List<CPreguntaCN> ListaPreguntas
    {
      get
      {
        List<CPreguntaCN> Respuesta = new List<CPreguntaCN>();
        foreach (CPuntoSala Punto in gEstIndicadores.Salas)
        {
          foreach (CPuntoSolapa Solapa in Punto.Solapas)
          {
            Respuesta.AddRange((from P in Solapa.Preguntas
                                select P.Pregunta).ToList());
          }
        }
        Respuesta.AddRange((from P in gEstIndicadores.PreguntasSueltas
                            select P.Pregunta).ToList());
        return Respuesta;
      }
    }

    //private static List<CInformacionAlarmaCN> gAlarmasAFecha = null;

    //public static async Task<List<CInformacionAlarmaCN>> AlarmasAFechaAsync()
    //{
    //  if (gAlarmasAFecha == null)
    //  {
    //    WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteBPI();
    //    try
    //    {
    //      CRespuestaInformacionAlarmaVarias Respuesta = await Cliente.AlarmasAFechaAsync(Ticket, DateTime.Now);
    //      if (!Respuesta.RespuestaOK)
    //      {
    //        throw new Exception(Respuesta.MensajeError);
    //      }

    //      gAlarmasAFecha = Respuesta.Instancias;
    //    }
    //    catch (Exception ex)
    //    {
    //      Rutinas.CRutinas.DesplegarMsg(ex);
    //    }
    //    finally
    //    {
    //      await Cliente.CloseAsync();
    //    }
    //  }
    //  return gAlarmasAFecha;
    //}


    //[Inject]
    //public static HttpClient Http { get; set; }


    public static async Task<string> InicializarDatosAsync(HttpClient Http)
    {

      try
      {
        RespuestaEstIndicadores Datos = await Http.GetFromJsonAsync<RespuestaEstIndicadores>(
            "api/Indicadores/GetIndicadores?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Ticket);
        if (!Datos.RespuestaOK)
        {
          throw new Exception(Datos.MsgErr);
        }
        else
        {

          gEstIndicadores = Datos.Estructura;

					ListaSalasReunion = (from S in gEstIndicadores.Salas
															 orderby S.Sala.Nombre
															 select new Listas.CListaTexto(S.Sala.Codigo, S.Sala.Nombre)).ToList();

					ListaIndicadores = (from I in gEstIndicadores.Indicadores
															orderby I.Descripcion
															select I).ToList();

					HabilitarIndicadores = (ListaIndicadores.Count > 0);

					HabilitarSalasReunion = (ListaSalasReunion.Count > 0);

					HabilitarLogin = false;

					_ = AsegurarAlarmasAsync(Http);

          _ = CargarProyectosBingAsync(Http);

          _ = CargarComitesAsync(Http);

				}
      }
      catch (Exception ex)
      {
        return Client.Rutinas.CRutinas.TextoMsg(ex);
      }
      //  CRespuestaBings RespMapas = await Cliente.ListarProyectosBingAsync(Ticket);
      //  if (!RespMapas.RespuestaOK)
      //  {
      //    throw new Exception("Al listar mapas " + RespMapas.MensajeError);
      //  }

      //  HabilitarMapas = (RespMapas.Proyectos.Count > 0);
      //  ListaMapas = (from M in RespMapas.Proyectos
      //                orderby M.Descripcion
      //                select M).ToList();

      //  CRespuestaTexto RespTicket = await Cliente.ObtenerTicketEstructuraAsync(Ticket);
      //  if (!RespTicket.RespuestaOK)
      //  {
      //    throw new Exception(RespTicket.MensajeError);
      //  }

      //  CRespuestaCodigo RespUsu = await Cliente.CodigoUsuarioAsync(Ticket);
      //  if (!RespUsu.RespuestaOK)
      //  {
      //    throw new Exception(RespUsu.MensajeError);
      //  }
      //  CodigoUsuario = RespUsu.Codigo;

      //  WCFEstructura.WcfEstructuraClient CliEst = Rutinas.CRutinas.ObtenerClienteEstructura();
      //  try
      //  {
      //    WCFEstructura.CRespuestaGrupoPuestos RespComites = await CliEst.GruposDePuestosDeUnaPersonaAsync(
      //        RespTicket.Contenido, CodigoUsuario);
      //    if (!RespComites.RespuestaOK)
      //    {
      //      throw new Exception(RespComites.MensajeError);
      //    }
      //    Comites = RespComites.GrupoPuestos;
      //  }
      //  catch (Exception ex)
      //  {
      //    Rutinas.CRutinas.DesplegarMsg(ex);
      //  }
      //  finally
      //  {
      //    await CliEst.CloseAsync();
      //  }

      //  // Provisorio.
      //  for (Int32 i = 1; i < 100; i++)
      //  {
      //    Comites.Add(new WCFEstructura.CGrupoPuestosCN() { Codigo = i });
      //  }
      //  // Hasta aca.

      //  CRespuestaMimicos RespMimicos = await Cliente.ListarMimicosAsync(Ticket, (from C in Comites
      //                                                                            select C.Codigo).ToList());
      //  if (!RespMimicos.RespuestaOK)
      //  {
      //    throw new Exception("Al listar mímicos " + RespMimicos.MensajeError);
      //  }
      //  Mimicos = RespMimicos.Mimicos;
      //  HabilitarMimicos = true;

      //  HabilitarMapas = (RespMapas.Proyectos.Count > 0);
      //  ListaMapas = (from M in RespMapas.Proyectos
      //                orderby M.Descripcion
      //                select M).ToList();

      //  return "Proceso completo";
      //}
      //catch (Exception ex)
      //{
      //  return ex.Message;
      //}
      //finally
      //{
      //  await Cliente.CloseAsync();
      //}
      return "";
    }

    public static CDatoIndicador IndicadorDesdeCodigo(Int32 CodIndicador)
    {
      return (from I in ListaIndicadores
              where I.Codigo == CodIndicador
              select I).FirstOrDefault();
    }

  //  public static string ColorPregunta(Int32 CodigoPregunta)
		//{
  //    List<CPreguntaIndicadorCN> Indicadores = IndicadoresEnPregunta(CodigoPregunta);
  //    string Respuesta = CRutinas.COLOR_GRIS;
  //    foreach (CPreguntaIndicadorCN Dato in Indicadores)
		//	{
  //      Respuesta = CRutinas.ColorMasCritico(Respuesta, ColorIndicador(Dato.Indicador));
		//	}
  //    return Respuesta;
		//}

  //  public static string ColorSalaReunion(Int32 CodigoSR)
  //  {
  //    CPuntoSala Punto = (from CPuntoSala S in gEstIndicadores.Salas
  //                        where S.Sala.Codigo == CodigoSR
  //                        select S).FirstOrDefault();
  //    string Respuesta = CRutinas.COLOR_GRIS;
  //    if (Punto != null)
  //    {
  //      foreach (CPuntoSolapa PS in Punto.Solapas)
  //      {
  //        foreach (CPuntoPregunta Preg in PS.Preguntas)
  //        {
  //          Respuesta = CRutinas.ColorMasCritico(Respuesta, ColorPregunta(Preg.Pregunta.Codigo));
  //        }
  //      }
  //    }
  //    return Respuesta;
  //  }

  //  public static string ColorIndicador(Int32 CodIndicador)
		//{
  //    CDatoIndicador Indicador = IndicadorDesdeCodigo(CodIndicador);
  //    if (Indicador == null)
		//	{
  //      return "GRIS";
		//	}
  //    List<CInformacionAlarmaCN> InfAl = (from I in gAlarmasIndicador
  //                                        where I.CodigoIndicador == CodIndicador
  //                                        orderby I.FechaInicial
  //                                        select I).ToList();
  //    if (InfAl.Count == 0)
		//	{
  //      return CRutinas.COLOR_GRIS;
		//	}
  //    else
		//	{
  //      CInformacionAlarmaCN Info = InfAl.Last();
  //      if (Info.Sobresaliente == Info.Minimo)
		//		{
  //        return CRutinas.COLOR_GRIS;
		//		}
  //      else
		//		{
  //        if (Info.Sobresaliente > Info.Minimo)
		//			{
  //          if (Info.Valor < Info.Minimo)
		//				{
  //            return CRutinas.COLOR_ROJO;
		//				}
  //          else
		//				{
  //            if (Info.Valor < Info.Satisfactorio)
		//					{
  //              return CRutinas.COLOR_AMARILLO;
		//					}
  //            else
		//					{
  //              return (Info.Valor < Info.Sobresaliente ? CRutinas.COLOR_VERDE : CRutinas.COLOR_AZUL);
		//					}
		//				}
		//			}
  //        else
		//			{
  //          if (Info.Valor > Info.Minimo)
  //          {
  //            return CRutinas.COLOR_ROJO;
  //          }
  //          else
  //          {
  //            if (Info.Valor > Info.Satisfactorio)
  //            {
  //              return CRutinas.COLOR_AMARILLO;
  //            }
  //            else
  //            {
  //              return (Info.Valor > Info.Sobresaliente ? CRutinas.COLOR_VERDE : CRutinas.COLOR_AZUL);
  //            }
  //          }
  //        }
  //      }
		//	}
		//}

    public static List<CInformacionAlarmaCN> gAlarmasIndicador = null;



    public static List<CPreguntaIndicadorCN> IndicadoresEnPregunta(Int32 CodigoPregunta)
    {
      if (gEstIndicadores != null)
      {
        foreach (CPuntoSala Sala in gEstIndicadores.Salas)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
            {
              if (Pregunta.Pregunta.Codigo == CodigoPregunta)
              {
                return Pregunta.Indicadores;
              }
            }
          }
        }
      }
      return new List<CPreguntaIndicadorCN>();
    }

    public static bool IndicadorEnPregunta(Int32 CodigoIndicador, Int32 CodigoPregunta,
          Int32 CodigoEntidad)
    {
      if (gEstIndicadores != null)
      {
        foreach (CPuntoSala Sala in gEstIndicadores.Salas)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
            {
              if (Pregunta.Pregunta.Codigo == CodigoPregunta)
              {
                foreach (CPreguntaIndicadorCN Indicador in Pregunta.Indicadores)
                {
                  if (Indicador.Indicador == CodigoIndicador && Indicador.ElementoDimension == CodigoEntidad)
                  {
                    return true;
                  }
                }
                return false;
              }
            }
          }
        }
      }
      return false;
    }

    public static Int32 DimensionIndicador(Int32 Codigo)
    {
      CDatoIndicador Indicador = (from I in ListaIndicadores
                                  where I.Codigo == Codigo
                                  select I).FirstOrDefault();
      return (Indicador == null ? -1 : Indicador.Dimension);
    }

    public static List<CListaElementosDimension> gDimensiones = null;

    private static object OBJ_LOCK = new object();
    public async static Task CargarElementosDimensionesAsync(HttpClient Http)
    {

      RespuestaEntidades RespWCF = await Http.GetFromJsonAsync<RespuestaEntidades>(
        "api/Indicadores/GetEntidades?URL=" + Contenedores.CContenedorDatos.UrlBPI +
        "&Ticket=" + Ticket);
      if (!RespWCF.RespuestaOK)
      {
        throw new Exception(RespWCF.MsgErr);
      }

//      gDimensiones = new List<CListaElementosDimension>();
      gDimensiones = (from E in RespWCF.Entidades
                      where E.Version == -1
                      orderby E.Descripcion
                      select new CListaElementosDimension()
                      {
                        CodigoDimension = E.Codigo,
                        Descripcion = E.Descripcion
                      }).ToList();
    }

    public async static Task<List<CEntradaIndicador>> CodigosIndicadoresPreguntaAsync(HttpClient Http, Int32 CodigoPregunta)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      if (gEstIndicadores != null)
      {
        foreach (CPuntoSala Sala in gEstIndicadores.Salas)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            if (Solapa.Preguntas != null)
            {
              foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
              {
                if (Pregunta.Pregunta.Codigo == CodigoPregunta)
                {
                  foreach (CPreguntaIndicadorCN Indicador in Pregunta.Indicadores)
                  {
                    if (Indicador.Dimension < 0)
                    {
                      Respuesta.Add(new CEntradaIndicador(Indicador.Indicador,
                        (Indicador.Dimension >= 0 ? Indicador.ElementoDimension : -1)));
                    }
                    else
                    {
                      Respuesta.AddRange(await Client.Rutinas.CRutinas.ListaIndicadoresDimensionAsync(Http,
                          Indicador.Indicador, Indicador.Dimension,
                          Indicador.ElementoDimension));
                    }
                  }
                  return Respuesta;
                }
              }
            }
          }
        }
      }
      return Respuesta;
    }

    public async static Task<List<CEntradaIndicador>> CodigosIndicadoresSalaReunionAsync(HttpClient Http, Int32 CodigoSR)
    {
      List<CEntradaIndicador> Respuesta = new List<CEntradaIndicador>();
      if (gEstIndicadores != null)
      {
        foreach (CPuntoSala Sala in gEstIndicadores.Salas)
        {
          if (Sala.Sala.Codigo == CodigoSR)
          {
            foreach (CPuntoSolapa Solapa in Sala.Solapas)
            {
              if (Solapa.Preguntas != null) {
                foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
                {
                  if (Pregunta.Indicadores != null)
                  {
                    foreach (CPreguntaIndicadorCN Indicador in Pregunta.Indicadores)
                    {
                      if (Indicador.Dimension < 0)
                      {
                        Respuesta.Add(new CEntradaIndicador(Indicador.Indicador,
                          (Indicador.Dimension >= 0 ? Indicador.ElementoDimension : -1)));
                      }
                      else
                      {
                        Respuesta.AddRange(await Client.Rutinas.CRutinas.ListaIndicadoresDimensionAsync(Http, Indicador.Indicador, Indicador.Dimension,
                            Indicador.ElementoDimension));
                      }
                    }
                  }
                }
              }
            }
            return Respuesta;
          }
        }
      }
      return Respuesta;
    }

    public async static Task<List<CListaElementosDimension>> ObtenerListaDimensionesAsync(HttpClient Http)
		{
      if (gDimensiones == null)
      {
        await CargarElementosDimensionesAsync(Http);
      }
      return gDimensiones;
    }

    public async static Task<List<CEntidadCN>> ObtenerElementosDimensionAsync(HttpClient Http,
          Int32 DimensionIndicador, Int32 ElementoIndicador,
          Int32 DimensionTarjeta, Int32 ElementoTarjeta)
    {
      //lock (OBJ_LOCK)
      //{
        if (DimensionIndicador < 0)
        {
          return new List<CEntidadCN>();
        }
        else
        {
          if (gDimensiones == null)
          {
            await CargarElementosDimensionesAsync(Http);
          }

          CListaElementosDimension Dimension = (from D in gDimensiones
                                                where D.CodigoDimension == DimensionIndicador
                                                select D).FirstOrDefault();
          if (Dimension == null)
          {
            return new List<CEntidadCN>();
          }
          if (ElementoIndicador >= 0)
          {
            return (from E in Dimension.Elementos
                    where E.Codigo == ElementoIndicador
                    select E).ToList();
          }
          else
          {
            if (ElementoTarjeta >= 0 && DimensionIndicador == DimensionTarjeta)
            {
              return (from E in Dimension.Elementos
                      where E.Codigo == ElementoTarjeta
                      select E).ToList();
            }
            else
            {
              return (from E in Dimension.Elementos
                      select E).ToList();
            }
          }
        }
      //}
    }

    private static bool ColorPeor(string ColorAntes, string ColorAhora)
    {
      if (ColorAhora == ColorAntes)
      {
        return false;
      }
      switch (ColorAntes)
      {
        case CRutinas.COLOR_AZUL:
          return true;
        case CRutinas.COLOR_VERDE:
          return (ColorAhora != CRutinas.COLOR_AZUL);
        case CRutinas.COLOR_AMARILLO:
          return (ColorAhora == CRutinas.COLOR_ROJO);
        default:
          return false;
      }
    }

    public static CPuntoPregunta PuntoPreguntaDesdeCodigo(Int32 Codigo)
    {
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        foreach (CPuntoSolapa Solapa in Sala.Solapas)
        {
          foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
          {
            if (Pregunta.Pregunta.Codigo == Codigo)
            {
              return Pregunta;
            }
          }
        }
      }

      return (from P in gEstIndicadores.PreguntasSueltas
              where P.Pregunta.Codigo == Codigo
              select P).FirstOrDefault();

    }

    public static CPreguntaCN PreguntaDesdeCodigo(Int32 Codigo)
    {
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        foreach (CPuntoSolapa Solapa in Sala.Solapas)
        {
          foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
          {
            if (Pregunta.Pregunta.Codigo == Codigo)
            {
              return Pregunta.Pregunta;
            }
          }
        }
      }

      return (from P in gEstIndicadores.PreguntasSueltas
              where P.Pregunta.Codigo == Codigo
              select P.Pregunta).FirstOrDefault();

    }

    public static CSubconsultaExt SubconsultaCodigo(Int32 Codigo)
    {
      return (from S in gSubconsultas
              where S.Codigo == Codigo
              select S).FirstOrDefault();
    }

    public async static Task<CInformacionAlarmaCN> DatosAlarmaIndicadorAsync(HttpClient Cliente,
        Int32 CodigoIndicador, Int32 Dimension, Int32 Elemento)
    {
      await AsegurarAlarmasAsync(Cliente);
      List<Int32> Dimensiones = new List<int>();
      if (Elemento == CRutinas.ELEMENTO_TODOS)
      {
        foreach (CEntidadCN Entidad in await ObtenerElementosDimensionAsync(Cliente, Dimension, Elemento, -1, -1))
        {
          Dimensiones.Add(Entidad.Codigo);
        }
      }
      else
      {
        if (Elemento >= 0 || Dimension < 0)
        {
          Dimensiones.Add(Elemento);
        }
      }

      CInformacionAlarmaCN Info = null;
      foreach (Int32 ElLocal in Dimensiones)
      {
        foreach (CInformacionAlarmaCN Alarma in gAlarmasIndicador)
        {
          if (Alarma.CodigoIndicador == CodigoIndicador &&
              (Dimension < 0 ||
                (Alarma.Dimension == Dimension && Alarma.ElementoDimension == ElLocal)))
          {
            if (Info == null || Alarma.FechaInicial >= Info.FechaInicial)
            {
              if (Info == null || Alarma.FechaInicial > Info.FechaInicial || ColorPeor(Info.Color, Alarma.Color))
              {
                Info = Alarma;
              }
            }
          }
        }
      }
      return Info;
    }

		public static double ValorGIS(Int32 CodigoIndicador, CElementoPreguntasWISCN Pregunta)
		{
			if (Pregunta != null && Pregunta.Contenidos != null)
			{
				foreach (CPreguntaPreguntaWISCN Contenido in Pregunta.Contenidos)
				{
					if (Contenido.Clase == ClaseDetalle.Indicador && Contenido.Codigo == CodigoIndicador)
					{
						foreach (CInformacionAlarmaCN Dato in gAlarmasIndicador)
						{
							if (Dato.CodigoIndicador == CodigoIndicador &&
									(Contenido.CodigoDimension < 0 || Contenido.CodigoElementoDimension == Dato.ElementoDimension))
							{
								return Dato.Valor;
							}
						}
					}
				}
			}
			return double.NaN;
		}

    public async static Task AsegurarAlarmasAsync(HttpClient Cliente)
    {
      if (gAlarmasIndicador == null)
      {
        string Lista = "";
        foreach (CDatoIndicador Indi in ListaIndicadores)
        {
          Lista += Indi.Codigo.ToString() + ";" + Indi.Dimension.ToString() + ";" + (Indi.Dimension < 0 ? "-1" : "-1000") + ";";
        }
        RespuestaInformacionAlarmaVarias RespWCF = await Cliente.GetFromJsonAsync<RespuestaInformacionAlarmaVarias>(
            "api/Alarmas/GetAlarmasIndicadores?URL=" + Contenedores.CContenedorDatos.UrlBPI +
            "&Ticket=" + Ticket +
            "&Lista=" + Lista);
        if (!RespWCF.RespuestaOK)
        {
          throw new Exception(RespWCF.MsgErr);
        }
        gAlarmasIndicador = RespWCF.Instancias;
        Logicas.CLogicaMainMenu.AjustarMenu();
      }

    }

    public async static Task CargarProyectosBingAsync(HttpClient Cliente)
    {
      if (ListaMapas == null)
      {
        try
        {
          RespuestaProyectosBing RespWCF = await Cliente.GetFromJsonAsync<RespuestaProyectosBing>(
              "api/Proyectos/ListarProyectosBing?URL=" + Contenedores.CContenedorDatos.UrlBPI +
              "&Ticket=" + Ticket);
          if (!RespWCF.RespuestaOK)
          {
            throw new Exception(RespWCF.MsgErr);
          }
          else
          {
            ListaMapas = RespWCF.Proyectos;
            Logicas.CLogicaMainMenu.AjustarMenu();
          }
        }
        catch (Exception ex)
				{
          CRutinas.DesplegarMsg(ex);
				}
      }
    }

    public async static Task CargarComitesAsync(HttpClient Cliente)
    {
      if (gComitesUsuario == null)
      {
        try
        {
          //RespuestaEnteros Respuesta = await Cliente.GetFromJsonAsync<RespuestaEnteros>(
          //    "api/Login/ListarComites?URL="+CContenedorDatos.UrlBPI+
          //    "&URLEst="+CContenedorDatos.UrlEstructura+
          //    "&Ticket=" + Ticket+
          //    "&CodigoPersona="+CodigoUsuario.ToString());
          RespuestaComites Respuesta = await Cliente.GetFromJsonAsync<RespuestaComites>(
              "api/Comites/ListarComites?URL=" + CContenedorDatos.UrlEstructura +
              "&Ticket=" + Ticket +
              "&Usuario=" + CodigoUsuario.ToString());
          if (!Respuesta.RespuestaOK)
          {
            throw new Exception(Respuesta.MsgErr);
          }
          else
          {
            gComitesUsuario = Respuesta.GrupoPuestos;
            gComitesEnterosUsuario = (from C in gComitesUsuario
                                      select C.Codigo).ToList();
            _ = CargarListaMimicosAsync(Cliente);
            _ = CargarListaSubconsultasAsync(Cliente);
          }
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
      }
    }

    public async static Task CargarListaMimicosAsync(HttpClient Cliente)
    {
      if (ListaMimicos == null)
      {
        try
        {
          RespuestaMimicos RespWCF = await Cliente.GetFromJsonAsync<RespuestaMimicos>(
              "api/Mimicos/ListarMimicos?URL=" + Contenedores.CContenedorDatos.UrlBPI +
              "&Ticket=" + Ticket+
              "&Comites="+CRutinas.EnterosALista(gComitesEnterosUsuario));
          if (!RespWCF.RespuestaOK)
          {
            throw new Exception(RespWCF.MsgErr);
          }
          else
          {
            ListaMimicos = RespWCF.Mimicos;
            Logicas.CLogicaMainMenu.AjustarMenu();
          }
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
      }
    }

    public async static Task CargarListaSubconsultasAsync(HttpClient Cliente)
    {
      if (gSubconsultas == null)
      {
        try
        {
          RespuestaSubconsultas RespWCF = await Cliente.GetFromJsonAsync<RespuestaSubconsultas>(
              "api/SubConsultas/ListarSubconsultas?URL=" + Contenedores.CContenedorDatos.UrlBPI +
              "&Ticket=" + Ticket);
          if (!RespWCF.RespuestaOK)
          {
            throw new Exception(RespWCF.MsgErr);
          }
          else
          {
            gSubconsultas = RespWCF.Subconsultas;
            Logicas.CLogicaMainMenu.AjustarMenu();
          }
        }
        catch (Exception ex)
        {
          CRutinas.DesplegarMsg(ex);
        }
      }
    }

    public static List<CPreguntaIndicadorCN> ExtraerIndicadoresPregunta(Int32 CodigoPregunta)
    {
      List<CPreguntaIndicadorCN> Respuesta = new List<CPreguntaIndicadorCN>();
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        foreach (CPuntoSolapa Solapa in Sala.Solapas)
        {
          foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
          {
            if (Pregunta.Pregunta.Codigo == CodigoPregunta)
            {
              foreach (CPreguntaIndicadorCN Indicador in Pregunta.Indicadores)
              {
                Respuesta.Add(Indicador);
              }
              return Respuesta;
            }
          }
        }
      }

      CPuntoPregunta Punto = (from P in gEstIndicadores.PreguntasSueltas
                              where P.Pregunta.Codigo == CodigoPregunta
                              select P).FirstOrDefault();
      if (Punto != null)
      {
        Respuesta.AddRange(Punto.Indicadores);
      }

      return Respuesta;
    }

    public static List<CPreguntaIndicadorCN> ExtraerIndicadoresSalaReunion(Int32 CodigoSala)
    {
      List<CPreguntaIndicadorCN> Respuesta = new List<CPreguntaIndicadorCN>();
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        if (Sala.Sala.Codigo == CodigoSala)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
            {
              foreach (CPreguntaIndicadorCN Indicador in Pregunta.Indicadores)
              {
                Respuesta.Add(Indicador);
              }
              return Respuesta;
            }
          }
        }
      }

      return Respuesta;

    }

    public static CSolapaCN SolapaDesdeCodigo(Int32 CodigoSala, Int32 CodigoSolapa)
    {
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        if (Sala.Sala.Codigo == CodigoSala)
        {
          return (from S in Sala.Solapas
                  where S.Solapa.Codigo == CodigoSolapa
                  select S.Solapa).FirstOrDefault();
        }
      }
      return null;
    }

    public static List<CSolapaCN> SolapasEnSala(Int32 CodigoSala)
    {
      List<CSolapaCN> Respuesta = new List<CSolapaCN>();
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        if (Sala.Sala.Codigo == CodigoSala)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            Respuesta.Add(Solapa.Solapa);
          }
          break;
        }
      }
      return Respuesta;
    }

    public static List<CPreguntaCN> PreguntasEnSolapa(Int32 CodigoSala, Int32 CodigoSolapa)
    {
      List<CPreguntaCN> Respuesta = new List<CPreguntaCN>();
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        if (CodigoSala < 0 || CodigoSala == Sala.Sala.Codigo)
        {
          foreach (CPuntoSolapa Solapa in Sala.Solapas)
          {
            if (Solapa.Solapa.Codigo == CodigoSolapa)
            {
              foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
              {
                Respuesta.Add(Pregunta.Pregunta);
              }
              break;
            }
          }
        }
      }
      return Respuesta;
    }

    public static CPuntoSala UbicarPuntoSala(Int32 Codigo)
    {
      return (from S in gEstIndicadores.Salas
              where S.Sala.Codigo == Codigo
              select S).FirstOrDefault();
    }

    public static CSalaCN UbicarSala(Int32 Codigo)
    {
      return (from S in gEstIndicadores.Salas
              where S.Sala.Codigo == Codigo
              select S.Sala).FirstOrDefault();
    }

    public static CPuntoPregunta UbicarPuntoPregunta(Int32 Codigo)
    {
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        foreach (CPuntoSolapa Solapa in Sala.Solapas)
        {
          foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
          {
            if (Pregunta.Pregunta.Codigo == Codigo)
            {
              return Pregunta;
            }
          }
        }
      }
      return null;
    }

    public static CPreguntaCN UbicarPregunta(Int32 Codigo)
    {
      CPuntoPregunta Punto = UbicarPuntoPregunta(Codigo);
      return (Punto == null ? null : Punto.Pregunta);
    }

    public static List<CPreguntaIndicadorCN> UbicarIndicadoresEnPregunta(Int32 Codigo)
    {
      foreach (CPuntoSala Sala in gEstIndicadores.Salas)
      {
        foreach (CPuntoSolapa Solapa in Sala.Solapas)
        {
          foreach (CPuntoPregunta Pregunta in Solapa.Preguntas)
          {
            if (Pregunta.Pregunta.Codigo == Codigo)
            {
              return Pregunta.Indicadores;
            }
          }
        }
      }
      return new List<CPreguntaIndicadorCN>();
    }

    [CascadingParameter]
    public Logicas.CLogicaSalaReunion PaginaSala { get; set; }

    public async static Task<CInformacionAlarmaCN> DatosDisponiblesIndicadorFechaAsync(
          HttpClient Cliente, Int32 CodIndicador, Int32 Dimension = -1)
    {
      if (Cliente==null && gAlarmasIndicador == null)
			{
        return null;
			}
      if (gAlarmasIndicador == null)
      {
        await AsegurarAlarmasAsync(Cliente);
      }
      return (from A in gAlarmasIndicador
              where A.CodigoIndicador == CodIndicador && A.ElementoDimension == -1
              select A).FirstOrDefault();
    }

    //public static async Task<CMapaBingCN> LeerMapaAsync(Int32 Codigo)
    //{
    //  WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteBPI();
    //  try
    //  {
    //    CRespuestaBing RespMapa = await Cliente.LeerProyectoBingAsync(Ticket, Codigo);
    //    if (!RespMapa.RespuestaOK)
    //    {
    //      throw new Exception(RespMapa.MensajeError);
    //    }

    //    return RespMapa.Proyecto;

    //  }
    //  finally
    //  {
    //    await Cliente.CloseAsync();
    //  }
    //}

  }

  public class IdentificadorIndicador : IEquatable<IdentificadorIndicador>
  {
    public Int32 Indicador { get; set; }
    public Int32 Elemento { get; set; }

    public IdentificadorIndicador(Int32 Indi, Int32 CodDim)
    {
      Indicador = Indi;
      Elemento = CodDim;
    }

    public bool Equals(IdentificadorIndicador Otro)
    {
      if (ReferenceEquals(null, Otro))
      {
        return false;
      }
      if (ReferenceEquals(this, Otro))
      {
        return true;
      }
      return (Indicador == Otro.Indicador && Elemento == Otro.Elemento);
    }
  }
}
