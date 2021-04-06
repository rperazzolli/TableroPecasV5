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
	public class ParametrosWFSController : Controller
	{
    // GET: SubConsultasController/Create
    [HttpGet("LeerParametroWFS")]
    public RespuestaTextos LeerParametroWFS(string URL, string Ticket, Int32 CodigoCapa, string Parametro)
    {
      RespuestaTextos Retorno = new RespuestaTextos();
      WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
      try
      {
        Task<WCFBPI.CRespuestaTextos> Tarea = Cliente.ListarValoresParametroWFSAsync(Ticket,
            CodigoCapa, Parametro);
        Tarea.Wait();
        WCFBPI.CRespuestaTextos Respuesta = Tarea.Result;
        if (!Respuesta.RespuestaOK)
        {
          throw new Exception(Respuesta.MensajeError);
        }

        Retorno.Contenidos = Respuesta.Contenidos;

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

  }
}
