using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;
using TableroPecasV5.Server.Rutinas;

namespace TableroPecasV5.Server.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class SubConsultasController : Controller
  {
    // GET: SubConsultasController/Create
    [HttpGet("LeerDataset")]
    public RespuestaDatasetBin LeerDataset(string URL, string Ticket, Int32 Codigo, string Parametros)
    {
      RespuestaDatasetBin Retorno = new RespuestaDatasetBin();
      WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
      try
      {
        string szGUID = Guid.NewGuid().ToString();
        Task<WCFBPI.CRespuestaDatasetBinSC> Tarea = Cliente.DetalleSubconsultaAsync(Ticket,
            Codigo, ExtraerPrms(Parametros), "", -1, szGUID, false);
        Tarea.Wait();
        WCFBPI.CRespuestaDatasetBinSC Respuesta = Tarea.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception("Al obtener detalle SC " + Respuesta.MensajeError);
        }
        Retorno.GUID = szGUID;
        Retorno.Situacion = (SituacionPedido)((Int32)Respuesta.Situacion);
        // Si hay datos arma un dataset y lo retorna.
        if (Respuesta.Situacion == WCFBPI.SituacionPedido.Completado)
        {
          Retorno.Datos = Respuesta.Datos;
        }
        else
        {
          Retorno.Datos = new byte[0];
        }

      }
      catch (Exception ex)
      {
        Retorno.RespuestaOK = false;
        Retorno.MsgErr = CRutinas.TextoMsg(ex);
      }
      finally
      {
        Cliente.Close();
      }

      return Retorno;

    }

    // GET: SubConsultasController/Create
    [HttpGet("RefrescarPedido")]
    public RespuestaDatasetBin RefrescarPedido(string URL, string Ticket, string GUID, string ContinuarSN)
    {
      RespuestaDatasetBin Respuesta = new RespuestaDatasetBin();
      try
      {
        WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
        try
        {
          Task<WCFBPI.CRespuestaDatasetBinSC> Tarea = Cliente.RefrescarPedidoDetalleSubconsultaBinAsync(
                    Ticket, GUID, ContinuarSN != "N");
          Tarea.Wait();
          WCFBPI.CRespuestaDatasetBinSC Datos = Tarea.Result;
          if (!Datos.RespuestaOK)
          {
            throw new Exception(Datos.MensajeError);
          }
          Respuesta.Situacion = (SituacionPedido)(Int32)Datos.Situacion;
          Respuesta.GUID = GUID;
          if (Datos.Situacion == WCFBPI.SituacionPedido.Completado)
          {
            Respuesta.ClaseOrigen = ClaseElemento.SubConsulta;
            Respuesta.CodigoOrigen = -1;
            Respuesta.Datos = Datos.Datos;
            Respuesta.Periodo = -1;
            Respuesta.Zipeado = false;
            Respuesta.RespuestaOK = Datos.RespuestaOK;
            Respuesta.MsgErr = Datos.MensajeError;
          }
        }
        finally
        {
          Cliente.Close();
        }
      }
      catch (Exception ex)
      {
        Respuesta.RespuestaOK = false;
        Respuesta.MsgErr = Rutinas.CRutinas.TextoMsg(ex);
      }
      return Respuesta;
    }

    [HttpGet("ListarValoresQuerySC")]
    public RespuestaQuerySubconsultas ListarValoresQuerySC(string URL, string Ticket, Int32 Codigo, string NombreCampo,
        string Prms)
    {
      RespuestaQuerySubconsultas Retorno = new RespuestaQuerySubconsultas();
      WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
      try
      {
        string szGUID = new Guid().ToString();
        Task<WCFBPI.CRespuestaQuerySubconsultas> Tarea = Cliente.ListarValoresQuerySCAsync(Ticket,
            Codigo, NombreCampo, ExtraerPrms(Prms));
        Tarea.Wait();
        WCFBPI.CRespuestaQuerySubconsultas Respuesta = Tarea.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception("Al obtener detalle SC " + Respuesta.MensajeError);
        }
        Retorno.Campo = NombreCampo;
        Retorno.Valores = Respuesta.Valores;
        Retorno.Textos = Respuesta.Textos;
      }
      catch (Exception ex)
      {
        Retorno.RespuestaOK = false;
        Retorno.MsgErr = CRutinas.TextoMsg(ex);
      }
      finally
      {
        Cliente.Close();
      }

      return Retorno;

    }

    private CSubconsultaExt CrearSubconsulta(WCFBPI.CSubconsultaExt Origen)
		{
      return new CSubconsultaExt()
      {
        Codigo = Origen.Codigo,
        Descripcion = Origen.Descripcion,
        Nombre = Origen.Nombre,
        Parametros = (from P in Origen.Parametros
                      select new CParametroExt()
                      {
                        CodigoSubconsulta = P.CodigoSubconsulta,
                        Nombre = P.Nombre,
                        TieneQuery = P.TieneQuery,
                        Tipo = P.Tipo,
                        ValorDateTime = P.ValorDateTime,
                        ValorFloat = P.ValorFloat,
                        ValorInteger = P.ValorInteger,
                        ValorString = P.ValorString
                      }).ToList()
      };
		}

    // GET: SubConsultasController/Create
    [HttpGet("ListarSubconsultas")]
    public RespuestaSubconsultas ListarSubconsultas(string URL, string Ticket, string Filtro)
    {
      RespuestaSubconsultas Retorno = new RespuestaSubconsultas();
      WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
      try
      {
        string szGUID = new Guid().ToString();
        Task<WCFBPI.CRespuestaSubconsultas> Tarea = Cliente.ListarSubconsultasAsync(Ticket,Filtro);
        Tarea.Wait();
        WCFBPI.CRespuestaSubconsultas Respuesta = Tarea.Result;
          if (!Respuesta.RespuestaOK)
          {
            throw new Exception("Al obtener detalle SC " + Respuesta.MensajeError);
          }
        Retorno.Subconsultas = (from S in Respuesta.Subconsultas
                                select CrearSubconsulta(S)).ToList();
      }
      catch (Exception ex)
      {
        Retorno.RespuestaOK = false;
        Retorno.MsgErr = CRutinas.TextoMsg(ex);
      }
      finally
      {
        Cliente.Close();
      }

      return Retorno;

    }

    // GET: SubConsultasController/Create
    [HttpGet("ObtenerParametrosConSC")]
    public RespuestaSubconsultas ObtenerParametrosConSC(string URL, string Ticket, Int32 Codigo)
    {
      RespuestaSubconsultas Retorno = new RespuestaSubconsultas();
      WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
      try
      {
        string szGUID = new Guid().ToString();
        Task<WCFBPI.CRespuestaSubconsultas> Tarea = Cliente.ObtenerParametrosConSCAsync(Ticket, Codigo);
        Tarea.Wait();
        WCFBPI.CRespuestaSubconsultas Respuesta = Tarea.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception("Al obtener detalle SC " + Respuesta.MensajeError);
        }
        Retorno.Subconsultas = (from S in Respuesta.Subconsultas
                                select CrearSubconsulta(S)).ToList();
      }
      catch (Exception ex)
      {
        Retorno.RespuestaOK = false;
        Retorno.MsgErr = CRutinas.TextoMsg(ex);
      }
      finally
      {
        Cliente.Close();
      }

      return Retorno;

    }

    private static string LimpiarParentesis(string Valor)
		{
      return Valor.Substring(1, Valor.Length - 2);
		}

    private static List<WCFBPI.CParametroExt> ExtraerPrms(string Contenido)
    {
      List<WCFBPI.CParametroExt> Respuesta = new List<WCFBPI.CParametroExt>();
      string[] Datos = Contenido.Split(new char[] { '$', '$' }, StringSplitOptions.RemoveEmptyEntries);
      Int32 Cantidad = Int32.Parse(LimpiarParentesis(Datos[0]));
      Int32 Pos = 1;
      for (Int32 i = 0; i < Cantidad; i++)
      {
        WCFBPI.CParametroExt Prm = new WCFBPI.CParametroExt();
        Prm.CodigoSubconsulta = Int32.Parse(LimpiarParentesis(Datos[Pos++]));
        Prm.Nombre = LimpiarParentesis(Datos[Pos++]);
        Prm.TieneQuery = (LimpiarParentesis(Datos[Pos++]) == "Y");
        Prm.Tipo = LimpiarParentesis(Datos[Pos++]);
        Prm.ValorDateTime = CRutinas.FechaDesdeTexto(LimpiarParentesis(Datos[Pos++]));
        Prm.ValorFloat = CRutinas.StrVFloat(LimpiarParentesis(Datos[Pos++]));
        Prm.ValorInteger = Int32.Parse(LimpiarParentesis(Datos[Pos++]));
        Prm.ValorString = LimpiarParentesis(Datos[Pos++]);
        Respuesta.Add(Prm);
      }
      return Respuesta;
    }

  }

}
