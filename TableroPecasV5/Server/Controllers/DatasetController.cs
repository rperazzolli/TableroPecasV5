using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TableroPecasV5.Shared;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TableroPecasV5.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DatasetController : ControllerBase
	{
		// GET: api/<DatasetController>
		[HttpGet("GetDataset")]
		public RespuestaDatasetBin GetDataset(string URL, string Ticket, Int32 Indicador, Int32 Dimension,
			    Int32 ElementoDimension,Int32 Periodo, bool UnicamenteColumnas, string GUID)
		{
			RespuestaDatasetBin Respuesta = new RespuestaDatasetBin();
			try
			{
				WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
				try
				{
				  Task<WCFBPI.CRespuestaDatasetBin> Tarea =	Cliente.ObtenerDetalleIndicadorBinAsync(Ticket, GUID, Indicador,
						  Periodo, Dimension, ElementoDimension, UnicamenteColumnas);
					Tarea.Wait();
					WCFBPI.CRespuestaDatasetBin Datos = Tarea.Result;
					if (!Datos.RespuestaOK)
					{
						throw new Exception("Al intentar leer dataset " + Datos.MensajeError);
					}
					Respuesta.Situacion = (SituacionPedido)(Int32)Datos.Situacion;
					Respuesta.GUID = GUID;
					if (Datos.Situacion == WCFBPI.SituacionPedido.Completado)
					{
						Respuesta.ClaseOrigen = (ClaseElemento)((Int32)Datos.ClaseOrigen);
						Respuesta.CodigoOrigen = Datos.CodigoOrigen;
						Respuesta.Datos = Datos.Datos;
						Respuesta.Periodo = Datos.Periodo;
						Respuesta.Zipeado = Datos.Zipeado;
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

		[HttpGet("RefrescarPedido")]
		public RespuestaDatasetBin RefrescarPedido(string URL, string Ticket, string GUID, string ContinuarSN)
		{
			RespuestaDatasetBin Respuesta = new RespuestaDatasetBin();
			try
			{
				WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
				try
				{
					Task<WCFBPI.CRespuestaDatasetBin> Tarea = Cliente.RefrescarPedidoDetalleIndicadorBinAsync(
										Ticket, GUID, ContinuarSN != "N");
					Tarea.Wait();
				  WCFBPI.CRespuestaDatasetBin	Datos = Tarea.Result;
					if (!Datos.RespuestaOK)
					{
						throw new Exception(Datos.MensajeError);
					}
					Respuesta.Situacion = (SituacionPedido)(Int32)Datos.Situacion;
					Respuesta.GUID = GUID;
					if (Datos.Situacion == WCFBPI.SituacionPedido.Completado)
					{
						Respuesta.ClaseOrigen = (ClaseElemento)((Int32)Datos.ClaseOrigen);
						Respuesta.CodigoOrigen = Datos.CodigoOrigen;
						Respuesta.Datos = Datos.Datos;
						Respuesta.Periodo = Datos.Periodo;
						Respuesta.Zipeado = Datos.Zipeado;
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

		// GET: DatasetController/Details/5
		[HttpGet("GetProveedor")]
		public RespuestaDatasetBin GetProveedor(string URL, string Ticket, Int32 Indicador,
					Int32 Dimension, Int32 Elemento)
		{
			RespuestaDatasetBin RespuestaRutina = new RespuestaDatasetBin();
			string GUIDPedido = Guid.NewGuid().ToString();
			WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaInformacionAlarmaVarias> Tarea = Cliente.HistoriaAlarmaConDimensionAsync(
							Ticket, Indicador, Dimension, Elemento, DateTime.Now);
				Tarea.Wait();
				WCFBPI.CRespuestaInformacionAlarmaVarias RespTendencia = Tarea.Result;
				if (!RespTendencia.RespuestaOK)
				{
					throw new Exception("Al buscar alarmas " + RespTendencia.MensajeError);
				}
				if (RespTendencia.Instancias.Count > 0)
				{
					Int32 Periodo = RespTendencia.Instancias.Last().Periodo;
					Task<WCFBPI.CRespuestaDatasetBin> TareaDS = Cliente.ObtenerDetalleIndicadorBinAsync(Ticket,
						GUIDPedido, Indicador, Periodo, Dimension, Elemento, false);
					TareaDS.Wait();
					WCFBPI.CRespuestaDatasetBin Respuesta = TareaDS.Result;
					if (!Respuesta.RespuestaOK)
					{
						throw new Exception("Al obtener detalle " + Respuesta.MensajeError);
					}
					while (Respuesta.Situacion == WCFBPI.SituacionPedido.EnMarcha)
					{
						TareaDS = Cliente.RefrescarPedidoDetalleIndicadorBinAsync(Ticket,
							GUIDPedido, true);
						TareaDS.Wait();
						Respuesta = TareaDS.Result;
						if (!Respuesta.RespuestaOK)
						{
							throw new Exception("Al refrescar pedido " + Respuesta.MensajeError);
						}
					}
					if (Respuesta.Situacion == WCFBPI.SituacionPedido.Completado)
					{
						Rutinas.CRutinas.CargarDatosDataset(Respuesta, RespuestaRutina);
					}
				}
			}
			catch (Exception ex)
			{
				RespuestaRutina.RespuestaOK = false;
				RespuestaRutina.MsgErr = Rutinas.CRutinas.TextoMsg(ex);
			}
			finally
			{
				Cliente.Close();
			}
			return RespuestaRutina;
		}

	}
}
