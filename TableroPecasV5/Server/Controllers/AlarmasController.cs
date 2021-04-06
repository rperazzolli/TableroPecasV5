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
	public class AlarmasController : ControllerBase
	{
		// GET: api/<AlarmasController>
		[HttpGet("GetAlarmas")]
		public RespuestaInformacionAlarmaVarias GetAlarmas(string URL, string Ticket, Int32 Indicador, Int32 Dimension,
				Int32 Elemento, string Fecha)
		{
			RespuestaInformacionAlarmaVarias Respuesta = new RespuestaInformacionAlarmaVarias();
			try
			{
				WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
				try
				{
					Task<WCFBPI.CRespuestaInformacionAlarmaVarias> Tarea = Cliente.HistoriaAlarmaConDimensionAsync(
							Ticket, Indicador, Dimension, Elemento, Rutinas.CRutinas.FechaDesdeTexto(Fecha));
					Tarea.Wait();
					WCFBPI.CRespuestaInformacionAlarmaVarias Datos = Tarea.Result;
					Respuesta.RespuestaOK = Datos.RespuestaOK;
					Respuesta.MsgErr = Datos.MensajeError;
					Respuesta.Instancias = new List<CInformacionAlarmaCN>();
					if (Datos.Instancias != null) {
						foreach (WCFBPI.CInformacionAlarmaCN Alrm in Datos.Instancias)
						{
							Respuesta.Instancias.Add(Rutinas.CRutinas.ConvertirInformacionAlarma(Alrm));
						}
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

		// GET: api/<AlarmasController>
		[HttpGet("GetAlarmasEntreFechas")]
		public RespuestaInformacionAlarmaVarias GetAlarmasEntreFechas(string URL, string Ticket, Int32 Indicador, Int32 Dimension,
				Int32 Elemento, string Desde, string Hasta)
		{
			RespuestaInformacionAlarmaVarias Respuesta = new RespuestaInformacionAlarmaVarias();
			try
			{
				WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
				try
				{
					Task<WCFBPI.CRespuestaInformacionAlarmaVarias> Tarea = Cliente.AlarmasDeUnPeriodoConDimensionAsync(
							Ticket, Indicador, Dimension, Elemento, Rutinas.CRutinas.FechaDesdeTexto(Desde),
							Rutinas.CRutinas.FechaDesdeTexto(Hasta));
					Tarea.Wait();
					WCFBPI.CRespuestaInformacionAlarmaVarias Datos = Tarea.Result;
					Respuesta.RespuestaOK = Datos.RespuestaOK;
					Respuesta.MsgErr = Datos.MensajeError;
					if (Datos.Instancias != null)
					{
						foreach (WCFBPI.CInformacionAlarmaCN Alrm in Datos.Instancias)
						{
							Respuesta.Instancias.Add(Rutinas.CRutinas.ConvertirInformacionAlarma(Alrm));
						}
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

		[HttpGet("GetAlarmasIndicadores")]
		// GET: IndicadoresController
		public RespuestaInformacionAlarmaVarias GetAlarmasIndicadores(string URL, string Ticket, string Lista)
		{
			RespuestaInformacionAlarmaVarias Respuesta = new RespuestaInformacionAlarmaVarias();
			WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(URL);
			try
			{

				List<Int32> Valores = Rutinas.CRutinas.ExtraerListaEnteros(Lista);

				List<WCFBPI.TIndicadorConDimension> DatosIndi = new List<WCFBPI.TIndicadorConDimension>();
				Int32 Pos = 0;
				while (Pos < Valores.Count) {
					DatosIndi.Add(new WCFBPI.TIndicadorConDimension()
					{
						codigoIndicadorField = Valores[Pos],
						codigoDimensionField = Valores[Pos + 1],
						codigoElementoDimensionField = Valores[Pos + 2]
					});
					Pos += 3;
				}

				Task<WCFBPI.CRespuestaInformacionAlarmaVarias> Tarea = Cliente.ValoresIndicadoresAFechaConDimensionAsync(Ticket,
						DatosIndi, DateTime.Now);
				Tarea.Wait();
				WCFBPI.CRespuestaInformacionAlarmaVarias RespWCF = Tarea.Result;
				if (!RespWCF.RespuestaOK)
				{
					throw new Exception(RespWCF.MensajeError);
				}
				Respuesta.Instancias = (from I in RespWCF.Instancias
																select new CInformacionAlarmaCN()
																{
																	FechaDesde = I.FechaDesde,
																	FechaFinal = I.FechaFinal,
																	Instancias = Rutinas.CRutinas.CopiarVectorEnteros(I.Instancias),
																	CodigoIndicador = I.CodigoIndicador,
																	Color = I.Color,
																	Comentarios = (from C in I.Comentarios
																								 select new CComentarioCN()
																								 {
																									 Archivos = Rutinas.CRutinas.CopiarVectorTextos(C.Archivos),
																									 Clase = (ClaseComentario)((Int32)C.Clase),
																									 ClaseOrigen = (ClaseElemento)((Int32)C.ClaseOrigen),
																									 CodigoOrigen = C.CodigoOrigen,
																									 Contenido = C.Contenido,
																									 Fecha = C.Fecha,
																									 Links = Rutinas.CRutinas.CopiarVectorTextos(C.Links),
																									 Orden = C.Orden,
																									 Propietario = C.Propietario,
																									 SubCodigoOrigen = C.SubCodigoOrigen,
																									 Vinculo = C.Vinculo
																								 }).ToList(),
																	DatosParaFecha = I.DatosParaFecha,
																	Dimension = I.Dimension,
																	ElementoDimension = I.ElementoDimension,
																	FechaHasta = I.FechaHasta,
																	FechaInicial = I.FechaInicial,
																	Minimo = I.Minimo,
																	Periodo = I.Periodo,
																	Satisfactorio = I.Satisfactorio,
																	Sentido = I.Sentido,
																	Sobresaliente = I.Sobresaliente,
																	Tendencia = I.Tendencia,
																	Valor = I.Valor,
																	ValorAnterior = I.ValorAnterior
																}).ToList();
			}
			catch (Exception ex)
			{
				Respuesta.RespuestaOK = false;
				Respuesta.MsgErr = Rutinas.CRutinas.TextoMsg(ex);
			}
			finally
			{
				Cliente.Close();
			}
			return Respuesta;
		}

	}
}
