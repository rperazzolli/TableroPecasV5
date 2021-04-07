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
	public class IndicadoresController : Controller
	{
		[HttpGet("GetIndicadores")]
		// GET: IndicadoresController
		public RespuestaEstIndicadores Get(string URL, string Ticket)
		{
			Task<RespuestaEstIndicadores> Resp0 = ObtenerRespuestaIndicadores(URL, Ticket);
			Resp0.Wait();
			return Resp0.Result;
		}

		private async Task<RespuestaEstIndicadores> ObtenerRespuestaIndicadores(string URL, string Ticket)
		{

			RespuestaEstIndicadores Respuesta = new RespuestaEstIndicadores();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				WCFBPI.CRespuestaEstIndicadores Salas = await Cliente.LeerEstructuraIndicadoresAsync(Ticket);
				if (!Salas.RespuestaOK)
				{
					throw new Exception(Salas.MensajeError);
				}

				foreach (WCFBPI.CDatoIndicador Indicador in Salas.Estructura.Indicadores)
				{
					Respuesta.Estructura.Indicadores.Add(CrearIndicador(Indicador));
				}

				foreach (WCFBPI.CPuntoSala Sala in Salas.Estructura.Salas)
				{
					Respuesta.Estructura.Salas.Add(CrearPuntoSala(Sala));
				}

				foreach (WCFBPI.CPuntoPregunta Pregunta in Salas.Estructura.PreguntasSueltas)
				{
					Respuesta.Estructura.PreguntasSueltas.Add(CrearPuntoPregunta(Pregunta));
				}

			}
			catch (Exception ex)
			{
				Respuesta.RespuestaOK = false;
				Respuesta.MsgErr = CRutinas.TextoMsg(ex);
			}

			return Respuesta;

		}

		private static Shared.CDatoIndicador CrearIndicador(WCFBPI.CDatoIndicador Origen)
		{
			return new Shared.CDatoIndicador()
			{
				Acumulado = Origen.Acumulado,
				Area = Origen.Area,
				Codigo = Origen.Codigo,
				CodigoArea = Origen.CodigoArea,
				CodigoPuesto = Origen.CodigoPuesto,
				CodigoUsuarioMetas = Origen.CodigoUsuarioMetas,
				CodigoUsuarioValores = Origen.CodigoUsuarioValores,
				Comentario = Origen.Comentario,
				Comite = Origen.Comite,
				DatasetComprimido = Origen.DatasetComprimido,
				Decimales = Origen.Decimales,
				Descripcion = Origen.Descripcion,
				Dimension = Origen.Dimension,
				EscalaCreciente = Origen.EscalaCreciente,
				EsCalculado = Origen.EsCalculado,
				FechaInicio = Origen.FechaInicio,
				Formula = Origen.Formula,
				Frecuencia = Origen.Frecuencia,
				NombreArea = Origen.NombreArea,
				PeriodosPorCiclo = Origen.PeriodosPorCiclo,
				Puesto = Origen.Puesto,
				SoloDiasLaborables = Origen.SoloDiasLaborables,
				TiemposRobot = Origen.TiemposRobot,
				TieneBono = Origen.TieneBono,
				TieneComentarios = Origen.TieneComentarios,
				TieneDetalle = Origen.TieneDetalle,
				TipoCiclo = Origen.TipoCiclo,
				Unidades = Origen.Unidades,
				UsuarioCreador = Origen.UsuarioCreador,
				UsuarioMetas = Origen.UsuarioMetas,
				UsuarioResponsable = Origen.UsuarioResponsable,
				UsuarioValores = Origen.UsuarioValores,
				Usuarios = CRutinas.CopiarVectorEnteros(Origen.Usuarios)
			};
		}

		private static Shared.CPuntoSala CrearPuntoSala(WCFBPI.CPuntoSala Origen)
		{
			Shared.CPuntoSala Respuesta = new Shared.CPuntoSala();
			Respuesta.Sala = new Shared.CSalaCN()
			{
				Codigo = Origen.Sala.Codigo,
				Comite = Origen.Sala.Comite,
				Dimension = Origen.Sala.Dimension,
				EdicionRestringida = Origen.Sala.EdicionRestringida,
				ElementoDimension = Origen.Sala.ElementoDimension,
				Nombre = Origen.Sala.Nombre,
				Registrador = Origen.Sala.Registrador
			};

			foreach (WCFBPI.CPuntoSolapa Solapa in Origen.Solapas)
			{
				Shared.CPuntoSolapa Punto = new Shared.CPuntoSolapa();
				Punto.Solapa = new Shared.CSolapaCN()
				{
					Azul = Solapa.Solapa.Azul,
					Block = Solapa.Solapa.Block,
					Codigo = Solapa.Solapa.Codigo,
					Dimension = Solapa.Solapa.Dimension,
					ElementoDimension = Solapa.Solapa.ElementoDimension,
					Nombre = Solapa.Solapa.Nombre,
					Orden = Solapa.Solapa.Orden,
					Rojo = Solapa.Solapa.Rojo,
					Sala = Solapa.Solapa.Sala,
					Verde = Solapa.Solapa.Verde
				};
				foreach (WCFBPI.CPuntoPregunta Pregunta in Solapa.Preguntas)
				{
					Punto.Preguntas.Add(CrearPuntoPregunta(Pregunta));
				}
				Respuesta.Solapas.Add(Punto);
			}

			return Respuesta;

		}

		private static Shared.CPuntoPregunta CrearPuntoPregunta(WCFBPI.CPuntoPregunta Pregunta)
		{
			Shared.CPuntoPregunta Respuesta = new Shared.CPuntoPregunta();
			foreach (WCFBPI.CPreguntaIndicadorCN Tarjeta in Pregunta.Indicadores)
			{
				Respuesta.Indicadores.Add(new Shared.CPreguntaIndicadorCN()
				{
					Dimension = Tarjeta.Dimension,
					ElementoDimension = Tarjeta.ElementoDimension,
					Indicador = Tarjeta.Indicador,
					Orden = Tarjeta.Orden,
					Pregunta = Tarjeta.Pregunta
				});
			}
			Respuesta.Pregunta = new Shared.CPreguntaCN()
			{
				Block = Pregunta.Pregunta.Block,
				Codigo = Pregunta.Pregunta.Codigo,
				Dimension = Pregunta.Pregunta.Dimension,
				ElementoDimension = Pregunta.Pregunta.ElementoDimension,
				Orden = Pregunta.Pregunta.Orden,
				Pregunta = Pregunta.Pregunta.Pregunta,
				Solapa = Pregunta.Pregunta.Solapa
			};
			return Respuesta;
		}

		public static CVinculoIndicadorCompletoCN CopiarVinculoCompleto(WCFBPI.CVinculoIndicadorCompletoCN Vinculo)
		{
			return new Shared.CVinculoIndicadorCompletoCN()
			{
				Detalles = (from D in Vinculo.Detalles
										select new CVinculoDetalleCN()
										{
											Codigo = D.Codigo,
											Posicion = D.Posicion,
											ValorAsociado = D.ValorAsociado
										}).ToList(),
				Vinculo = new CVinculoIndicadorCN()
				{
					ClaseIndicador = (ClaseElemento)((Int32)Vinculo.Vinculo.ClaseIndicador),
					ClaseVinculada = (ClaseVinculo)((Int32)Vinculo.Vinculo.ClaseVinculada),
					Codigo = Vinculo.Vinculo.Codigo,
					CodigoIndicador = Vinculo.Vinculo.CodigoIndicador,
					CodigoVinculado = Vinculo.Vinculo.CodigoVinculado,
					ColumnaLat = Vinculo.Vinculo.ColumnaLat,
					ColumnaLng = Vinculo.Vinculo.ColumnaLng,
					NombreColumna = Vinculo.Vinculo.NombreColumna,
					Rango = Vinculo.Vinculo.Rango,
					TipoColumna = (ClaseVariable)((Int32)Vinculo.Vinculo.TipoColumna)
				}
			};
		}

		public static WCFBPI.CVinculoIndicadorCompletoCN CopiarVinculoCompletoBPI(CVinculoIndicadorCompletoCN Vinculo)
		{
			return new WCFBPI.CVinculoIndicadorCompletoCN()
			{
				Detalles = (from D in Vinculo.Detalles
										select new WCFBPI.CVinculoDetalleCN()
										{
											Codigo = D.Codigo,
											Posicion = D.Posicion,
											ValorAsociado = D.ValorAsociado
										}).ToList(),
				Vinculo = new WCFBPI.CVinculoIndicadorCN()
				{
					ClaseIndicador = (WCFBPI.ClaseElemento)((Int32)Vinculo.Vinculo.ClaseIndicador),
					ClaseVinculada = (WCFBPI.ClaseVinculo)((Int32)Vinculo.Vinculo.ClaseVinculada),
					Codigo = Vinculo.Vinculo.Codigo,
					CodigoIndicador = Vinculo.Vinculo.CodigoIndicador,
					CodigoVinculado = Vinculo.Vinculo.CodigoVinculado,
					ColumnaLat = Vinculo.Vinculo.ColumnaLat,
					ColumnaLng = Vinculo.Vinculo.ColumnaLng,
					NombreColumna = Vinculo.Vinculo.NombreColumna,
					Rango = Vinculo.Vinculo.Rango,
					TipoColumna = (WCFBPI.ClaseVariable)((Int32)Vinculo.Vinculo.TipoColumna)
				}
			};
		}

		[HttpGet("LeerVinculoDeUnIndicadorCodigo")]
		// GET: IndicadoresController
		public RespuestaDetalleVinculo LeerVinculoDeUnIndicadorCodigo(string URL, string Ticket, Int32 Vinculo)
		{
			RespuestaDetalleVinculo Retorno = new RespuestaDetalleVinculo();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaDetalleVinculo> Tarea = Cliente.LeerVinculoDeUnIndicadorCodigoAsync(Ticket,
						Vinculo);
				Tarea.Wait();
				WCFBPI.CRespuestaDetalleVinculo Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.Vinculo = CopiarVinculoCompleto(Respuesta.Vinculo);

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

		[HttpGet("GetEntidades")]
		// GET: IndicadoresController
		public RespuestaEntidades GetEntidades(string Url, string Ticket)
		{
			RespuestaEntidades Respuesta = new RespuestaEntidades();
			WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(Url);
			try
			{
				Task<WCFBPI.CRespuestaEntidades> Tarea = Cliente.ListarAgrupamientosAsync(Ticket);
				Tarea.Wait();
				WCFBPI.CRespuestaEntidades RespWCF = Tarea.Result;
				if (!RespWCF.RespuestaOK)
				{
					throw new Exception(RespWCF.MensajeError);
				}

				Respuesta.Entidades = (from E in RespWCF.Entidades
															 select new CEntidadCN()
															 {
																 Codigo = E.Codigo,
																 Descripcion = E.Descripcion,
																 Version = E.Version
															 }).ToList();
			}
			finally
			{
				Cliente.CloseAsync();
			}

			return Respuesta;

		}

		[HttpGet("GetSujetosEntidad")]
		// GET: IndicadoresController
		public RespuestaEntidades GetSujetosEntidad(string Url, string Ticket, Int32 Entidad)
		{
			RespuestaEntidades Respuesta = new RespuestaEntidades();
			WCFBPI.WCFBPIClient Cliente = Rutinas.CRutinas.ObtenerClienteWCF(Url);
			try
			{
				Task<WCFBPI.CRespuestaEntidades> Tarea = Cliente.ListarSujetosAgrupamientoAsync(Ticket, Entidad);
				Tarea.Wait();
				WCFBPI.CRespuestaEntidades RespWCF = Tarea.Result;
				if (!RespWCF.RespuestaOK)
				{
					throw new Exception(RespWCF.MensajeError);
				}

				Respuesta.Entidades = (from E in RespWCF.Entidades
															 select new CEntidadCN()
															 {
																 Codigo = E.Codigo,
																 Descripcion = E.Descripcion,
																 Version = E.Version
															 }).ToList();
			}
			finally
			{
				Cliente.CloseAsync();
			}

			return Respuesta;

		}

	}
}
