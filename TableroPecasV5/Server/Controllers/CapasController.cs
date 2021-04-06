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
	public class CapasController : Controller
	{
		// GET: CapasController
		[HttpGet("ListarCapasWSS")]
		public RespuestaCapasWSS ListarCapasWSS(string URL, string Ticket, Int32 ClaseElemento, Int32 CodigoElemento)
		{
			RespuestaCapasWSS Retorno = new RespuestaCapasWSS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapasWSS> Tarea = Cliente.ListarCapasWSSAsync(Ticket,
						(WCFBPI.ClaseElemento)ClaseElemento, CodigoElemento);
				Tarea.Wait();
				WCFBPI.CRespuestaCapasWSS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.Capas = (from C in Respuesta.Capas
												 select new CCapaWSSCN()
												 {
													 Agrupacion = (ModoAgruparDependiente)((Int32)C.Agrupacion),
													 CapaWFS = C.CapaWFS,
													 Clase = (ClaseElemento)((Int32)C.Clase),
													 Codigo = C.Codigo,
													 CodigoElemento = C.CodigoElemento,
													 ColorCompuestoA = C.ColorCompuestoA,
													 ColorCompuestoB = C.ColorCompuestoB,
													 ColorCompuestoG = C.ColorCompuestoG,
													 ColorCompuestoR = C.ColorCompuestoR,
													 ColumnaGeoreferencia = C.ColumnaGeoreferencia,
													 ColumnaLatitud = C.ColumnaLatitud,
													 ColumnaLongitud = C.ColumnaLongitud,
													 ColumnaValor = C.ColumnaValor,
													 Formula = C.Formula,
													 Intervalos = (ClaseIntervalo)((Int32)C.Intervalos),
													 Minimo = C.Minimo,
													 Modo = (ModoGeoreferenciar)((Int32)C.Modo),
													 Nombre = C.Nombre,
													 Rango = C.Rango,
													 Referencias = (from R in C.Referencias
																					select R).ToList(),
													 Satisfactorio = C.Satisfactorio,
													 Segmentos = C.Segmentos,
													 Sobresaliente = C.Sobresaliente,
													 Vinculo = C.Vinculo
												 }).ToList();

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

		// GET: CapasController
		[HttpGet("LeerCapaWFS")]
		public RespuestaCapaWFS LeerCapaWFS(string URL, string Ticket, Int32 Codigo, string ForzarWeb)
		{
			RespuestaCapaWFS Retorno = new RespuestaCapaWFS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapaWFS> Tarea = Cliente.LeerCapaWFSAsync(Ticket,
						Codigo, ForzarWeb=="Y");
				Tarea.Wait();
				WCFBPI.CRespuestaCapaWFS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.Capa = ProyectosController.CopiarCapaWFS(Respuesta.Capa);

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
