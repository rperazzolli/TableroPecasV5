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
	public class ComitesController : Controller
	{
		// GET: CapasController
		[HttpGet("ListarComites")]
		public RespuestaComites ListarComites(string URL, string Ticket, Int32 Usuario)
		{
			RespuestaComites Retorno = new RespuestaComites();
			WCFEstructura.WcfEstructuraClient Cliente = CRutinas.ObtenerClienteWCFEstructura(URL);
			try
			{
				Task<WCFEstructura.CRespuestaGrupoPuestos> Tarea = Cliente.GruposDePuestosDeUnaPersonaAsync(Ticket, Usuario);
				Tarea.Wait();
				WCFEstructura.CRespuestaGrupoPuestos Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				foreach (WCFEstructura.CGrupoPuestosCN Grupo in Respuesta.GrupoPuestos)
				{
					Retorno.GrupoPuestos.Add(new CGrupoPuestosCN()
					{
						Codigo = Grupo.Codigo,
						CodigoExterno = Grupo.CodigoExterno,
						Descripcion = Grupo.Descripcion,
						FechaRegistro = Grupo.FechaRegistro,
						Hijos = CRutinas.CopiarVectorEnteros(Grupo.Hijos),
						IDOrganigrama = Grupo.IDOrganigrama,
						Objetivo = Grupo.Objetivo,
						Registrador = Grupo.Registrador,
						Status = (StatusEstructura)((Int32)Grupo.Status),
						ValidezDesde = Grupo.ValidezDesde,
						ValidezHasta = Grupo.ValidezHasta
					});
				}

				for (Int32 i = 1; i < 20; i++)
				{
					if ((from C in Respuesta.GrupoPuestos
							 where C.Codigo == i
							 select C).FirstOrDefault() == null)
					{
						Retorno.GrupoPuestos.Add(new CGrupoPuestosCN()
						{
							Codigo = i,
							Descripcion = "Comité " + i.ToString()
						});
					}
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

		// POST: HomeController/Edit/5
		[HttpPost("InsertarSala")]
		public RespuestaEnteros InsertarSala(string URL, string Ticket, [FromBody] CSalaCN Sala)
		{
			RespuestaEnteros Retorno = new RespuestaEnteros();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				//Task<WCFBPI.CRespuestaEntero> Tarea = Cliente.RegistrarSalaAsync(Ticket,
				//	new WCFBPI.CSalaCN()
				//	{
				//		Codigo = Sala.Codigo,
				//		Comite = Sala.Comite,
				//		Dimension = Sala.Dimension,
				//		EdicionRestringida = Sala.EdicionRestringida,
				//		ElementoDimension = Sala.ElementoDimension,
				//		Nombre = Sala.Nombre,
				//		Registrador = Sala.Registrador
				//	});
				//Tarea.Wait();
				//WCFBPI.CRespuestaEntero Respuesta = Tarea.Result;
				//if (!Respuesta.RespuestaOK)
				//{
				//	throw new Exception(Respuesta.MensajeError);
				//}

				//Retorno.Codigos.Add(Respuesta.CodigoAsociado);

				Retorno.Codigos.Add(99);

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

		// POST: HomeController/Edit/5
		[HttpPost("InsertarSolapa")]
		public RespuestaEnteros InsertarSolapa(string URL, string Ticket, [FromBody] CSolapaCN Solapa)
		{
			RespuestaEnteros Retorno = new RespuestaEnteros();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				//Task<WCFBPI.CRespuestaEntero> Tarea = Cliente.RegistrarSolapaAsync(Ticket,
				//	new WCFBPI.CSolapaCN()
				//	{
				//		Codigo = Solapa.Codigo,
				//		Azul = Solapa.Azul,
				//		Block = Solapa.Block,
				//		Orden = Solapa.Orden,
				//		Rojo = Solapa.Rojo,
				//		Sala = Solapa.Sala,
				//		Verde = Solapa.Verde
				//	});
				//Tarea.Wait();
				//WCFBPI.CRespuestaEntero Respuesta = Tarea.Result;
				//if (!Respuesta.RespuestaOK)
				//{
				//	throw new Exception(Respuesta.MensajeError);
				//}

				//Retorno.Codigos.Add(Respuesta.CodigoAsociado);

				Retorno.Codigos.Add(99);

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
