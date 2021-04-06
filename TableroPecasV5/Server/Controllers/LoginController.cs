using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LoginController : Controller
	{
		[HttpGet("GetTicket")]
		public CDatosLogin Get(string URL, string Usuario, string Clave)
		{
			Task<CDatosLogin> Resp0 = ObtenerRespuestaLogin(URL, Usuario, Clave);
			Resp0.Wait();
			return Resp0.Result;
		}

		private async Task<CDatosLogin> ObtenerRespuestaLogin(string URL, string Usuario, string Clave)
		{
			CDatosLogin Respuesta = new CDatosLogin();
			try
			{
				WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
				try
				{
					WCFBPI.CRespuestaTicket RespLogin = await Cliente.LoginUsuarioAsync(Usuario, Clave);
					if (!RespLogin.RespuestaOK)
					{
						throw new Exception("En WCF " + RespLogin.MensajeError);
					}
					Respuesta.Ticket = RespLogin.Ticket;
					Respuesta.Usuario = RespLogin.Usuario;
					Respuesta.Administrador = RespLogin.Administrador;
					Respuesta.DesciendeEnRojo = RespLogin.DesciendeEnRojo;
					Respuesta.ImprimirPDF = RespLogin.ImprimirPDF;
					Respuesta.MaximizaTendencias = RespLogin.MaximizaTendencias;
					Respuesta.Minutos = RespLogin.Minutos;
					Respuesta.PoneEtiquetas = RespLogin.PoneEtiquetas;
					RespLogin.RespetaSentido = RespLogin.RespetaSentido;
					Respuesta.SiempreTendencia = RespLogin.RespetaSentido;
					Respuesta.TendenciasEnTarjeta = RespLogin.TendenciasEnTarjeta;

					WCFBPI.CRespuestaCodigo RespCodUsu = await Cliente.CodigoUsuarioAsync(Respuesta.Ticket);
					if (!RespCodUsu.RespuestaOK)
					{
						throw new Exception("Al buscar código usuario " + RespCodUsu.MensajeError);
					}

					Respuesta.CodigoUsuario = RespCodUsu.Codigo;

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

		public async static Task<string> ObtenerTicketEstructuraAsync(string URL, string Ticket)
		{
			WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
			try
			{
				WCFBPI.CRespuestaTexto RespTicket = await Cliente.ObtenerTicketEstructuraAsync(Ticket);
				if (!RespTicket.RespuestaOK)
				{
					throw new Exception("Al obtener ticket estructura " + RespTicket.MensajeError);
				}
				return RespTicket.Contenido;
			}
			finally
			{
				Cliente.Close();
			}
		}

		[HttpGet("ListarComites")]
		public RespuestaEnteros ListarComites(string URL, string URLEst, string Ticket, Int32 CodigoPersona)
		{
			RespuestaEnteros Respuesta = new RespuestaEnteros();
			try
			{
				Task<string> TareaTicket = ObtenerTicketEstructuraAsync(URL, Ticket);
				TareaTicket.Wait();
				string TicketEstructura = TareaTicket.Result;
				if (TicketEstructura==null || TicketEstructura.Length == 0)
				{
					throw new Exception("No puede logonearse en estructura");
				}

				WCFEstructura.WcfEstructuraClient Cliente = Rutinas.CRutinas.ObtenerClienteWCFEstructura(URLEst);
				try
				{
					Task<WCFEstructura.CRespuestaGrupoPuestos> Tarea = Cliente.GruposDePuestosDeUnaPersonaAsync(
						  TicketEstructura, CodigoPersona);
					Tarea.Wait();
					WCFEstructura.CRespuestaGrupoPuestos RespComites = Tarea.Result; 
					if (!RespComites.RespuestaOK)
					{
						throw new Exception("Al listar comités " + RespComites.MensajeError);
					}
					Respuesta.Codigos = (from C in RespComites.GrupoPuestos
															 select C.Codigo).Distinct().ToList();
					//for (Int32 i = 1; i < 20; i++)
					//{
					//	if (!Respuesta.Codigos.Contains(i))
					//	{
					//		Respuesta.Codigos.Add(i);
					//	}
					//}
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

	}
}
