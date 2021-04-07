using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Server.Rutinas;
using TableroPecasV5.Shared;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TableroPecasV5.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VinculosController : ControllerBase
	{
		// GET: api/<VinculosController>
		[HttpGet("LeerVinculo")]
		public RespuestaDetalleVinculo LeerVinculo(string URL, string Ticket, Int32 ClaseIndicador,
			    Int32 Codigo, string Columna)
		{
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			RespuestaDetalleVinculo Retorno = new RespuestaDetalleVinculo();
			try
			{
				Task<WCFBPI.CRespuestaDetalleVinculo> Tarea = Cliente.LeerVinculoDeUnIndicadorAsync(Ticket,
					  (WCFBPI.ClaseElemento)ClaseIndicador, Codigo, Columna);
				Tarea.Wait();
				WCFBPI.CRespuestaDetalleVinculo Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al leer vínculo " + Respuesta.MensajeError);
				}

				Retorno.Vinculo = IndicadoresController.CopiarVinculoCompleto(Respuesta.Vinculo);

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

		[HttpGet("ListarVinculos")]
		public RespuestaVinculos ListarVinculos(string URL, string Ticket, Int32 ClaseIndicador,
					Int32 Codigo, string Columna)
		{
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			RespuestaVinculos Retorno = new RespuestaVinculos();
			try
			{
				Task<WCFBPI.CRespuestaVinculos> Tarea = Cliente.VinculosDeUnIndicadorAsync(Ticket,
						(WCFBPI.ClaseElemento)ClaseIndicador, Codigo);
				Tarea.Wait();
				WCFBPI.CRespuestaVinculos Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al listar vínculos " + Respuesta.MensajeError);
				}

				Retorno.Vinculos = (from WCFBPI.CVinculoIndicadorCompletoCN V in Respuesta.Vinculos
														select IndicadoresController.CopiarVinculoCompleto(V)).ToList();

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

		// POST api/<VinculosController>
		[HttpPost("RegistrarVinculo")]
		public RespuestaEnteros RegistrarVinculo(string URL, string Ticket,
			    [FromBody] CVinculoIndicadorCompletoCN Vinculo)
		{
			RespuestaEnteros Retorno = new RespuestaEnteros();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				WCFBPI.CVinculoIndicadorCompletoCN VinculoBPI = IndicadoresController.CopiarVinculoCompletoBPI(Vinculo);
				Task<WCFBPI.CRespuestaCodigo> Tarea = Cliente.RegistrarVinculoDeUnIndicadorAsync(Ticket, VinculoBPI);
				Tarea.Wait();
				WCFBPI.CRespuestaCodigo Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al registrar vínculo " + Respuesta.MensajeError);
				}

				Retorno.Codigos.Add(Respuesta.Codigo);

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
