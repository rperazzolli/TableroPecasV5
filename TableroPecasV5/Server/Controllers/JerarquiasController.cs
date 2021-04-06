using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Server.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JerarquiasController : Controller
	{
		[HttpGet("LeerJerarquias")]
		public RespuestaJerarquias LeerJerarquias(string URL, string Ticket, Int32 Mimico)
		{
			RespuestaJerarquias Respuesta = new RespuestaJerarquias();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaJerarquias> RespJerarquias = Cliente.ListarJerarquiasAsync(Ticket, Mimico);
				RespJerarquias.Wait();
				WCFBPI.CRespuestaJerarquias Resp = RespJerarquias.Result;
				if (!Resp.RespuestaOK)
				{
					throw new Exception(Resp.MensajeError);
				}

				foreach (WCFBPI.CJerarquiaCN Jerarquia in Resp.Jerarquias)
				{
					Respuesta.Jerarquias.Add(new CJerarquiaCN()
					{
						Inferior = Jerarquia.Inferior,
						Mimico = Jerarquia.Mimico,
						Orden = Jerarquia.Orden,
						Superior = Jerarquia.Superior
					});
				}

			}
			catch (Exception ex)
			{
				Respuesta.RespuestaOK = false;
				Respuesta.MsgErr = CRutinas.TextoMsg(ex);
			}
			finally
			{
				Cliente.Close();
			}
			return Respuesta;
		}

		[HttpPost("RegistrarJerarquias")]
		public Respuesta RegistrarJerarquias(string URL, string Ticket, Int32 Mimico,
			  [FromBody] List<CJerarquiaCN> Jerarquias)
		{
			Respuesta Retorno = new Respuesta();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				List<WCFBPI.CJerarquiaCN> JerarquiasBPI = (from J in Jerarquias
																									 select new WCFBPI.CJerarquiaCN()
																									 {
																										 Inferior = J.Inferior,
																										 Mimico = J.Mimico,
																										 Orden = J.Orden,
																										 Superior = J.Superior
																									 }).ToList();
				Task<WCFBPI.CRespuesta> Tarea = Cliente.RegistrarJerarquiasAsync(Ticket, Mimico, JerarquiasBPI);
				Tarea.Wait();
				WCFBPI.CRespuesta Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
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

	}
}
