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
	public class MimicosController : Controller
	{

		// GET: MimicosController/Details/5
		[HttpGet("ListarMimicos")]
		public RespuestaMimicos ListarMimicos(string URL, string Ticket, string Comites)
		{
			RespuestaMimicos Respuesta = new RespuestaMimicos();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaMimicos> Tarea = Cliente.ListarMimicosAsync(Ticket,
					  CRutinas.ExtraerListaEnteros(Comites));
				Tarea.Wait();
				WCFBPI.CRespuestaMimicos RespMimico = Tarea.Result;
				if (!RespMimico.RespuestaOK)
				{
					throw new Exception(RespMimico.MensajeError);
				}

				Respuesta.Mimicos = (from M in RespMimico.Mimicos
														 select new CElementoMimicoCN()
														 {
															 Abscisa = M.Abscisa,
															 Alto = M.Alto,
															 Ancho = M.Ancho,
															 Codigo = M.Codigo,
															 MimicoBase = M.MimicoBase,
															 Comite = M.Comite,
															 Nombre = M.Nombre,
															 Ordenada = M.Ordenada,
															 Vinculo = M.Vinculo
														 }).ToList();
			}
			catch (Exception ex)
			{
				Respuesta.RespuestaOK = false;
				Respuesta.MsgErr = CRutinas.TextoMsg(ex);
			}
			return Respuesta;
		}

		// GET: MimicosController/Details/5
		[HttpGet("LeerMimico")]
		public RespuestaMimico LeerMimico(string URL, string Ticket, Int32 CodigoUnico)
		{
			RespuestaMimico Respuesta = new RespuestaMimico();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaMimico> Tarea = Cliente.LeerProyectoMimicoAsync(Ticket, CodigoUnico);
				Tarea.Wait();
				WCFBPI.CRespuestaMimico RespMimico = Tarea.Result;
				if (!RespMimico.RespuestaOK)
				{
					throw new Exception(RespMimico.MensajeError);
				}
				Respuesta.Mimico = CrearMimico(RespMimico.Mimico);
				Respuesta.Proceso = CrearProceso(RespMimico.Proceso);
				Respuesta.Proceso.Pools.Add(new CPoolGraficoCN()
				{
					Codigo = "Pool 1",
					Descripcion = "Pool1",
					Lineas = new List<CLineaGraficaCN>()
				});
				Respuesta.Proceso.Pools[0].Lineas.Add(new CLineaGraficaCN()
				{
					Abscisa = 25,
					Alto = 1200,
					Ancho = 800,
					Codigo = "Linea1",
					Descripcion = "Linea 1",
					Ordenada = 25
				});
				Respuesta.Proceso.Tareas.Add(new CTareaGraficaCN()
				{
					Abscisa = 100,
					Alto = 200,
					Ancho = 400,
					Clase = ClaseElemento.Tarea,
					Codigo = "aaaaaaaa",
					Descripcion = "Tarea A",
					EsTransaccion = false,
					Ordenada = 100,
					TareaOrigen = ""
				});
				Respuesta.Proceso.Tareas.Add(new CTareaGraficaCN()
				{
					Abscisa = 300,
					Alto = 200,
					Ancho = 400,
					Clase = ClaseElemento.Tarea,
					Codigo = "qwqwqwqw",
					Descripcion = "Tarea QW",
					EsTransaccion = false,
					Ordenada = 900,
					TareaOrigen = ""
				});
				Respuesta.Proceso.Flechas.Add(new CFlechaGraficaCN()
				{
					Clase = ClaseFlecha.Flecha,
					ClaseDesde = ClaseElemento.Tarea,
					ClaseHasta = ClaseElemento.Tarea,
					Codigo = "ffffffff",
					CodigoDesde = "aaaaaaaa",
					CodigoHasta = "qwqwqwqw",
					Descripcion = "Flecha A-QW",
					PrefijoDesde = "",
					PrefijoHasta = "",
					Puntos = new List<CPuntoGraficoCN>()
				});
				Respuesta.Proceso.Flechas[0].Puntos.Add(new CPuntoGraficoCN()
				{
					Abscisa = 300,
					Ordenada = 300
				});
				Respuesta.Proceso.Flechas[0].Puntos.Add(new CPuntoGraficoCN()
				{
					Abscisa = 500,
					Ordenada = 900
				});
				Respuesta.Imagen = CrearImagen(RespMimico.Imagen, Respuesta.Proceso.Tareas.Count != 0);
			}
			catch (Exception ex)
			{
				Respuesta.RespuestaOK = false;
				Respuesta.MsgErr = CRutinas.TextoMsg(ex);
			}
			return Respuesta;
		}

		private CMimicoCN CrearMimico(WCFBPI.CMimicoCN Origen)
		{
			return new CMimicoCN()
			{
				MimicoPropio = new CElementoMimicoCN()
				{
					Abscisa = Origen.MimicoPropio.Abscisa,
					Alto = Origen.MimicoPropio.Alto,
					Ancho = Origen.MimicoPropio.Ancho,
					Codigo = Origen.MimicoPropio.Codigo,
					Comite = Origen.MimicoPropio.Comite,
					MimicoBase = Origen.MimicoPropio.MimicoBase,
					Nombre = Origen.MimicoPropio.Nombre,
					Ordenada = Origen.MimicoPropio.Ordenada,
					Vinculo = Origen.MimicoPropio.Vinculo
				},
				GruposDePreguntasDelMimico = (from P in Origen.GruposDePreguntasDelMimico
																			select new CElementoPreguntasCN()
																			{
																				Ancho = P.Ancho,
																				Alto = P.Alto,
																				Abscisa = P.Abscisa,
																				ClaseEntidad = P.ClaseEntidad,
																				Codigo = P.Codigo,
																				CodigoEntidad = P.CodigoEntidad,
																				MimicoBase = P.MimicoBase,
																				Nombre = P.Nombre,
																				Ordenada = P.Ordenada,
																				Vinculo = P.Vinculo,
																				PreguntasAsociadas = (from PA in P.PreguntasAsociadas
																															select new CDetallePreguntaCN()
																															{
																																ClaseDeDetalle = (ClaseDetalle)((Int32)PA.ClaseDeDetalle),
																																ClaseEntidad = PA.ClaseEntidad,
																																Codigo = PA.Codigo,
																																CodigoEntidad = PA.CodigoEntidad,
																																CodigoPregunta = PA.CodigoPregunta
																															}).ToList()
																			}).ToList(),
				VinculosComentarios = (from V in Origen.VinculosComentarios
															 select V).ToList()
			};
		}

		private CProcesoGraficoCN CrearProceso(WCFBPI.CProcesoGraficoCN Origen)
		{
			return new CProcesoGraficoCN()
			{
				Codigo = Origen.Codigo,

				Flechas = (from F in Origen.Flechas
									 select new CFlechaGraficaCN()
									 {
										 Clase = (ClaseFlecha)((Int32)F.Clase),
										 ClaseDesde = (ClaseElemento)((Int32)F.ClaseDesde),
										 ClaseHasta = (ClaseElemento)((Int32)F.ClaseHasta),
										 Codigo = F.Codigo,
										 CodigoDesde = F.CodigoDesde,
										 CodigoHasta = F.CodigoHasta,
										 Descripcion = F.Descripcion,
										 PrefijoDesde = F.PrefijoDesde,
										 PrefijoHasta = F.PrefijoHasta,
										 Puntos = (from P in F.Puntos
															 select new CPuntoGraficoCN()
															 {
																 Abscisa = P.Abscisa,
																 Ordenada = P.Ordenada
															 }).ToList()
									 }).ToList(),

				Pools = (from P in Origen.Pools
								 select new CPoolGraficoCN()
								 {
									 Codigo = P.Codigo,
									 Descripcion = P.Descripcion,
									 Lineas = (from L in P.Lineas
														 select new CLineaGraficaCN()
														 {
															 Abscisa = L.Abscisa,
															 Alto = L.Alto,
															 Ancho = L.Ancho,
															 Codigo = L.Codigo,
															 Ordenada = L.Ordenada,
															 Descripcion = L.Descripcion
														 }).ToList()
								 }).ToList(),

				Rombos = (from R in Origen.Rombos
									select new CRomboGraficoCN()
									{
										Abscisa = R.Abscisa,
										Alto = R.Alto,
										Ancho = R.Ancho,
										Clase = (ClaseRombo)((Int32)R.Clase),
										Codigo = R.Codigo,
										Ordenada = R.Ordenada,
										Prefijo = R.Prefijo
									}).ToList(),

				Tareas = (from T in Origen.Tareas
									select new CTareaGraficaCN()
									{
										Abscisa = T.Abscisa,
										Alto = T.Alto,
										Ancho = T.Ancho,
										Clase = (ClaseElemento)((Int32)T.Clase),
										Codigo = T.Codigo,
										EsTransaccion = T.EsTransaccion,
										Descripcion = T.Descripcion,
										Ordenada = T.Ordenada,
										TareaOrigen = T.TareaOrigen
									}).ToList()
			};
		}

		private CImagenCN CrearImagen(WCFBPI.CImagenCN Origen, bool HayProceso)
		{
			Int32 Ancho = 0;
			Int32 Alto = 0;

			string UrlImagen = (HayProceso ? "" :
					(Origen.Imagen != null && Origen.Imagen.Length > 0 ?
					CRutinas.CrearImagen("M", Origen.Codigo, Origen.Imagen, out Ancho, out Alto) : ""));

			return new CImagenCN()
			{
				Codigo = Origen.Codigo,
				MimicoBase = Origen.MimicoBase,
				UrlImagen = UrlImagen,
				Ancho = Ancho,
				Alto = Alto
			};
		}

		private List<WCFBPI.CDetallePreguntaCN> ExtraerPreguntasAsociadasBPI(CElementoPreguntasCN Pregunta)
		{
			List<WCFBPI.CDetallePreguntaCN> Respuesta = new List<WCFBPI.CDetallePreguntaCN>();
			foreach (CDetallePreguntaCN Detalle in Pregunta.PreguntasAsociadas)
			{
				Respuesta.Add(new WCFBPI.CDetallePreguntaCN()
				{
					ClaseDeDetalle = (WCFBPI.ClaseDetalle)(Int32)Detalle.ClaseDeDetalle,
					ClaseEntidad = Detalle.ClaseEntidad,
					Codigo = Detalle.Codigo,
					CodigoEntidad = Detalle.CodigoEntidad,
					CodigoPregunta = Detalle.CodigoPregunta
				});
			}
			return Respuesta;
		}

		private WCFBPI.CMimicoCN ExtraerMimicoPropioBPI(CMimicoCN Mimico)
		{
			WCFBPI.CMimicoCN Respuesta = new WCFBPI.CMimicoCN();
			Respuesta.MimicoPropio = new WCFBPI.CElementoMimicoCN()
			{
				Abscisa = Mimico.MimicoPropio.Abscisa,
				Alto = Mimico.MimicoPropio.Alto,
				Ancho = Mimico.MimicoPropio.Ancho,
				Codigo = Mimico.MimicoPropio.Codigo,
				Comite = Mimico.MimicoPropio.Comite,
				MimicoBase = Mimico.MimicoPropio.MimicoBase,
				Nombre = Mimico.MimicoPropio.Nombre,
				Ordenada = Mimico.MimicoPropio.Ordenada,
				Vinculo = Mimico.MimicoPropio.Vinculo
			};

			Respuesta.GruposDePreguntasDelMimico = new List<WCFBPI.CElementoPreguntasCN>();

			foreach (CElementoPreguntasCN Elemento in Mimico.GruposDePreguntasDelMimico)
			{
				Respuesta.GruposDePreguntasDelMimico.Add(new WCFBPI.CElementoPreguntasCN()
				{
					Abscisa = Elemento.Abscisa,
					Alto = Elemento.Alto,
					Ancho = Elemento.Ancho,
					ClaseEntidad = (Int32)Elemento.ClaseEntidad,
					Codigo = Elemento.Codigo,
					CodigoEntidad = Elemento.CodigoEntidad,
					MimicoBase = Mimico.MimicoPropio.Codigo,
					Nombre = Elemento.Nombre,
					Ordenada = Elemento.Ordenada,
					Vinculo = Elemento.Vinculo,
					PreguntasAsociadas = ExtraerPreguntasAsociadasBPI(Elemento)
				});
			}

			Respuesta.VinculosComentarios = CRutinas.CopiarVectorTextos(Mimico.VinculosComentarios);

			return Respuesta;

		}

		private List<WCFBPI.CImagenCN> ExtraerImagenesBinarias(CMimicoCN Mimico)
		{
			List<WCFBPI.CImagenCN> Respuesta = new List<WCFBPI.CImagenCN>();
			foreach (CImagenBinariaCN Img in Mimico.ImagenesBinarias)
			{
				Respuesta.Add(new WCFBPI.CImagenCN()
				{
					Codigo = Img.Codigo,
					DatosSucios = Img.DatosSucios,
					Imagen = Img.Imagen,
					MimicoBase = Img.Mimico
				});
			}
			return Respuesta;
		}

		private List<ParCodigos> CrearListaPares(List<WCFBPI.ParCodigos> Lista)
		{
			List<ParCodigos> Respuesta = new List<ParCodigos>();
			foreach (WCFBPI.ParCodigos Par in Lista)
			{
				Respuesta.Add(new ParCodigos()
				{
					Actual=Par.Actual,
					Anterior=Par.Anterior
				});
			}
			return Respuesta;
		}

		[HttpPost("InsertarMimico")]
		public RespuestaCodigos InsertarMimico(string URL, string Ticket, [FromBody] CMimicoCN Mimico)
		{
			RespuestaCodigos Retorno = new RespuestaCodigos();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				WCFBPI.CMimicoCN MimicoPropio = ExtraerMimicoPropioBPI(Mimico);
				List<WCFBPI.CImagenCN> Imagenes = ExtraerImagenesBinarias(Mimico);
				Task<WCFBPI.CRespuestaCodigos> Tarea = Cliente.RegistrarMimicoAsync(Ticket, MimicoPropio,
						Imagenes);
				Tarea.Wait();
				WCFBPI.CRespuestaCodigos Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.ParesDeCodigosMimicos.AddRange(CrearListaPares(Respuesta.ParesDeCodigosMimicos));
				Retorno.ParesDeCodigosPreguntas.AddRange(CrearListaPares(Respuesta.ParesDeCodigosPreguntas));
				Retorno.ParesDeCodigosWFS.AddRange(CrearListaPares(Respuesta.ParesDeCodigosWFS));

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

		[HttpDelete("BorrarMimico")]
		public Respuesta BorrarMimico(string URL, string Ticket, Int32 CodigoMimico)
		{
			Respuesta Retorno = new Respuesta();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuesta> Tarea = Cliente.BorrarMimicoAsync(Ticket, CodigoMimico);
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
