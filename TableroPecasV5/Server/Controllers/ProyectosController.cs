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
	public class ProyectosController : Controller
	{

		[HttpGet("CargarProyecto")]
		public RespuestaProyectoBing CargarProyecto(string URL, string Ticket, Int32 Codigo)
		{
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			RespuestaProyectoBing Retorno = new RespuestaProyectoBing();
			try
			{
				Task<WCFBPI.CRespuestaBing> Tarea = Cliente.LeerProyectoBingAsync(Ticket, Codigo);
				Tarea.Wait();
				WCFBPI.CRespuestaBing Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al leer mapa " + Respuesta.MensajeError);
				}

				Retorno.Proyecto = CRutinas.CrearMapaShared(Respuesta.Proyecto);

				Retorno.Proyecto.Preguntas = ConvertirPreguntas(Respuesta.Proyecto.Preguntas);

				foreach (WCFBPI.CCapaBingCN Capa in Respuesta.Proyecto.Capas)
				{
					if (Capa != null)
					{
						switch (Capa.Clase)
						{
							case WCFBPI.ClaseCapa.WFS:
								Retorno.CapasCompletas.Add(CargarInformacionWFS(Cliente, Ticket, Capa));
								break;
							case WCFBPI.ClaseCapa.WIS:
								Retorno.CapasCompletas.Add(CargarInformacionWIS(Cliente, Ticket, Capa, Retorno));
								break;

							case WCFBPI.ClaseCapa.WMS:
								Retorno.CapasCompletas.Add(CargarInformacionWMS(Cliente, Ticket, Capa));
								break;

						}
					}
				}

				OrdenarCapas(Retorno.CapasCompletas);

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

		public void OrdenarCapas(List<CDatosCapaComodin> CapasCompletas)
		{
			if (CapasCompletas != null)
			{
				CapasCompletas.Sort(delegate (CDatosCapaComodin C1, CDatosCapaComodin C2)
				{
					if (C1.Clase != C2.Clase)
					{
						return ((Int32)C1.Clase).CompareTo((Int32)C2.Clase);
					}

					if (C1.CapaWFS != null && C2.CapaWFS != null)
					{
						if (C1.CapaWFS.Areas.Count > 0)
						{
							return (C2.CapaWFS.Areas.Count > 0 ? 0 : -1);
						}
						else
						{
							if (C2.CapaWFS.Areas.Count > 0)
							{
								return 1;
							}
						}
						if (C1.CapaWFS.Lineas.Count > 0)
						{
							return (C2.CapaWFS.Lineas.Count > 0 ? 0 : -1);
						}
						else
						{
							if (C2.CapaWFS.Lineas.Count > 0)
							{
								return 1;
							}
						}
					}

					return 0;

				});
			}
		}

		private CDatosCapaComodin CrearCapaComodinDesdeWMS(WCFBPI.CCapaWMSCN Capa, WCFBPI.CCapaBingCN CapaProyecto)
		{

			CDatosCapaComodin CapaC = new CDatosCapaComodin();
			CapaC.Clase = ClaseCapa.WMS;
			CapaC.CodigoCapa = Capa.Codigo;
			CapaC.CapaWMS = new CCapaWMSCN()
			{
				Capa = Capa.Capa,
				Codigo = Capa.Codigo,
				CodigoProveedor = Capa.CodigoProveedor,
				Descripcion = Capa.Descripcion,
				EPGS = Capa.EPGS,
				LatMaxima = Capa.LatMaxima,
				LatMinima = Capa.LatMinima,
				LongMaxima = Capa.LongMaxima,
				LongMinima = Capa.LongMinima,
				URLProveedor = Capa.URLProveedor,
				Query = Capa.Query,
				VersionProveedor = Capa.VersionProveedor
			};
			CapaC.Opacidad = CapaProyecto.Opacidad;
			CapaC.ColorWFS = System.Drawing.Color.FromArgb(255,
					CapaProyecto.Rojo, CapaProyecto.Verde, CapaProyecto.Azul);
			return CapaC;
		}

		private CDatosCapaComodin CargarInformacionWMS(WCFBPI.WCFBPIClient Cliente, string Ticket, WCFBPI.CCapaBingCN Capa)
		{
			Task<WCFBPI.CRespuestaCapaWMS> Tarea = Cliente.LeerDatosCapaWMSAsync(Ticket, Capa.CodigoCapa);
			Tarea.Wait();
			WCFBPI.CRespuestaCapaWMS Respuesta = Tarea.Result;
			if (!Respuesta.RespuestaOK)
			{
				throw new Exception(Respuesta.MensajeError);
			}

			return CrearCapaComodinDesdeWMS(Respuesta.Capa, Capa);

		}

		private static CPosicionWFSCN CopiarPosicion(WCFBPI.CPosicionWFSCN Posicion)
		{
			return new CPosicionWFSCN()
			{
				X = Posicion.X,
				Y = Posicion.Y
			};
		}

		private static List<CPosicionWFSCN> CopiarPosiciones(List<WCFBPI.CPosicionWFSCN> PosicionesWCF)
		{
			List<CPosicionWFSCN> Respuesta = new List<CPosicionWFSCN>();
			foreach (WCFBPI.CPosicionWFSCN Posicion in PosicionesWCF)
			{
				Respuesta.Add(CopiarPosicion(Posicion));
			}
			return Respuesta;
		}

		private static List<CValorDimensionCN> CopiarDimensiones(List<WCFBPI.CValorDimensionCN> DimensionesWCF)
		{
			return (from D in DimensionesWCF
							select new CValorDimensionCN()
							{
								Dimension = D.Dimension,
								Valor = D.Valor
							}).ToList();
		}

		private static List<CAreaWFSCN> CrearCopiaAreas(List<WCFBPI.CAreaWFSCN> AreasWCF)
		{
			List<CAreaWFSCN> Respuesta = new List<CAreaWFSCN>();
			foreach (WCFBPI.CAreaWFSCN Area in AreasWCF)
			{
				Respuesta.Add(new CAreaWFSCN()
				{
					Area = Area.Area,
					Centro = CopiarPosicion(Area.Centro),
					Codigo = Area.Codigo,
					Contorno = CopiarPosiciones(Area.Contorno),
					Dimensiones = CopiarDimensiones(Area.Dimensiones),
					Nombre = Area.Nombre
				});
			}
			return Respuesta;
		}

		private static List<CLineaWFSCN> CrearCopiaLineas(List<WCFBPI.CLineaWFSCN> LineasWCF)
		{
			return (from L in LineasWCF
							select new CLineaWFSCN()
							{
								Centro = CopiarPosicion(L.Centro),
								Codigo = L.Codigo,
								Contorno = CopiarPosiciones(L.Contorno),
								Nombre = L.Nombre
							}).ToList();
		}

		private static List<CPuntoWFSCN> CrearCopiaPuntos(List<WCFBPI.CPuntoWFSCN> PuntosWCF)
		{
			return (from P in PuntosWCF
							select new CPuntoWFSCN()
							{
								Codigo = P.Codigo,
								Nombre = P.Nombre,
								Punto = CopiarPosicion(P.Punto)
							}).ToList();
		}

		public static CCapaWFSCN CopiarCapaWFS(WCFBPI.CCapaWFSCN Capa)
		{
			return new CCapaWFSCN()
			{
				Capa = Capa.Capa,
				Codigo = Capa.Codigo,
				CodigoProveedor = Capa.CodigoProveedor,
				Descripcion = Capa.Descripcion,
				Areas = CrearCopiaAreas(Capa.Areas),
				CamposInformacion = Capa.CamposInformacion,
				Detalle = Capa.Detalle,
				DireccionURL = Capa.DireccionURL,
				Elemento = (ElementoWFS)((Int32)Capa.Elemento),
				FechaRefresco = Capa.FechaRefresco,
				GuardaCompactada = Capa.GuardaCompactada,
				Lineas = CrearCopiaLineas(Capa.Lineas),
				NombreCampoCodigo = Capa.NombreCampoCodigo,
				NombreCampoDatos = Capa.NombreCampoDatos,
				NombreElemento = Capa.NombreElemento,
				Puntos = CrearCopiaPuntos(Capa.Puntos),
				PuntosMaximosContorno = Capa.PuntosMaximosContorno,
				Version = Capa.Version
			};
		}

		public static CDatosCapaComodin CrearCapaComodinDesdeWFS(WCFBPI.CCapaWFSCN Capa, WCFBPI.CCapaBingCN CapaProyecto)
		{

			CDatosCapaComodin CapaC = new CDatosCapaComodin();
			CapaC.Clase = ClaseCapa.WFS;
			CapaC.CodigoCapa = Capa.Codigo;
			CapaC.CapaWFS = CopiarCapaWFS(Capa);
			CapaC.Opacidad = CapaProyecto.Opacidad;
			CapaC.ColorWFS = System.Drawing.Color.FromArgb(255,
					CapaProyecto.Rojo, CapaProyecto.Verde, CapaProyecto.Azul);
			return CapaC;
		}

		public static CDatosCapaComodin CargarInformacionWFS(WCFBPI.WCFBPIClient Cliente, string Ticket,
					WCFBPI.CCapaBingCN Capa)
		{
			Task<WCFBPI.CRespuestaCapaWFS> Tarea = Cliente.LeerCapaWFSAsync(Ticket, Capa.CodigoCapa, false);
			Tarea.Wait();
			WCFBPI.CRespuestaCapaWFS Respuesta = Tarea.Result;
			if (!Respuesta.RespuestaOK)
			{
				throw new Exception(Respuesta.MensajeError);
			}
			return CrearCapaComodinDesdeWFS(Respuesta.Capa, Capa);
		}

		private CDatosCapaComodin CrearCapaComodinDesdeWIS(WCFBPI.CCapaWISCN Capa, WCFBPI.CCapaBingCN CapaProyecto)
		{

			CDatosCapaComodin CapaC = new CDatosCapaComodin();
			CapaC.Clase = ClaseCapa.WIS;
			CapaC.CodigoCapa = Capa.Codigo;
			CapaC.CapaWIS = new CCapaWISCN()
			{
				CodigoWFS = Capa.CodigoWFS,
				Codigo = Capa.Codigo,
				Descripcion = Capa.Descripcion
			};
			CapaC.Opacidad = CapaProyecto.Opacidad;
			CapaC.ColorWFS = System.Drawing.Color.FromArgb(255,
					CapaProyecto.Rojo, CapaProyecto.Verde, CapaProyecto.Azul);
			return CapaC;
		}

		private CPosicionWFSCN CopiarPosicion(CPosicionWFSCN Otra)
		{
			return new CPosicionWFSCN()
			{
				X = Otra.X,
				Y = Otra.Y
			};
		}

		private List<CPosicionWFSCN> CopiarContorno(List<CPosicionWFSCN> Otro)
		{
			return (from P in Otro
							select CopiarPosicion(P)).ToList();
		}

		private List<CValorDimensionCN> CopiarDimensiones(List<CValorDimensionCN> Otras)
		{
			return (from V in Otras
							select new CValorDimensionCN()
							{
								Dimension = V.Dimension,
								Valor = V.Valor
							}).ToList();
		}

		private CCapaWFSCN CrearCopiaCapaWFS(CCapaWFSCN Otra)
		{
			return new CCapaWFSCN()
			{
				Areas = (from A in Otra.Areas
								 select new CAreaWFSCN()
								 {
									 Area = A.Area,
									 Centro = CopiarPosicion(A.Centro),
									 Codigo = A.Codigo,
									 Contorno = CopiarContorno(A.Contorno),
									 Dimensiones = CopiarDimensiones(A.Dimensiones),
									 Nombre = A.Nombre
								 }).ToList(),
				CamposInformacion = Otra.CamposInformacion,
				Capa = Otra.Capa,
				Codigo = Otra.Codigo,
				CodigoProveedor = Otra.CodigoProveedor,
				Descripcion = Otra.Descripcion,
				Detalle = Otra.Detalle,
				DireccionURL = Otra.DireccionURL,
				Elemento = Otra.Elemento,
				FechaRefresco = Otra.FechaRefresco,
				GuardaCompactada = Otra.GuardaCompactada,
				Lineas = (from L in Otra.Lineas
									select new CLineaWFSCN()
									{
										Centro = CopiarPosicion(L.Centro),
										Codigo = L.Codigo,
										Contorno = CopiarContorno(L.Contorno),
										Nombre = L.Nombre
									}).ToList(),
				NombreCampoCodigo = Otra.NombreCampoCodigo,
				NombreCampoDatos = Otra.NombreCampoDatos,
				NombreElemento = Otra.NombreElemento,
				Puntos = (from P in Otra.Puntos
									select new CPuntoWFSCN()
									{
										Codigo = P.Codigo,
										Nombre = P.Nombre,
										Punto = CopiarPosicion(P.Punto)
									}).ToList(),
				PuntosMaximosContorno = Otra.PuntosMaximosContorno,
				Version = Otra.Version
			};
		}

		public static List<CElementoPreguntasWISCN> ConvertirPreguntas(List<WCFBPI.CElementoPreguntasWISCN> PreguntasWCF)
		{
			return (from P in PreguntasWCF
							select new CElementoPreguntasWISCN()
							{
								Abscisa = P.Abscisa,
								Codigo = P.Codigo,
								ClaseWIS = (ClaseCapa)((Int32)P.ClaseWIS),
								CodigoArea = P.CodigoArea,
								CodigoWIS = P.CodigoWIS,
								Contenidos = (from C in P.Contenidos
															select new CPreguntaPreguntaWISCN()
															{
																Clase = (ClaseDetalle)((Int32)C.Clase),
																Codigo = C.Codigo,
																CodigoDimension = C.CodigoDimension,
																CodigoElemento = C.CodigoElemento,
																CodigoElementoDimension = C.CodigoElementoDimension,
																CodigoPregunta = C.CodigoPregunta
															}).ToList(),
								Dimension = P.Dimension,
								ElementoDimension = P.ElementoDimension,
								Nombre = P.Nombre,
								Ordenada = P.Ordenada
							}).ToList();
		}

		private CDatosCapaComodin CargarInformacionWIS(WCFBPI.WCFBPIClient Cliente, string Ticket,
					WCFBPI.CCapaBingCN Capa, RespuestaProyectoBing Retorno)
		{
			Task<WCFBPI.CRespuestaCapaWIS> Tarea = Cliente.LeerCapaWISAsync(Ticket, Capa.CodigoCapa);
			Tarea.Wait();
			WCFBPI.CRespuestaCapaWIS Respuesta = Tarea.Result;
			if (!Respuesta.RespuestaOK)
			{
				throw new Exception(Respuesta.MensajeError);
			}

			CDatosCapaComodin CapaRespuesta = CrearCapaComodinDesdeWIS(Respuesta.Capa.Capa, Capa);

			// Busca la capa WFS.
			CCapaWFSCN CapaWFS = (from CapaL in Retorno.CapasCompletas
														where CapaL.Clase == ClaseCapa.WFS && CapaL.CapaWFS != null
																&& CapaL.CapaWFS.Codigo == Respuesta.Capa.Capa.CodigoWFS
														select CapaL.CapaWFS).FirstOrDefault();

			// Si no la encuentra, la busca en otra capa WIS.
			if (CapaWFS == null)
			{
				CapaWFS = (from CapaL in Retorno.CapasCompletas
									 where CapaL.Clase == ClaseCapa.WIS && CapaL.CapaWFS != null
											 && CapaL.CapaWFS.Codigo == Respuesta.Capa.Capa.CodigoWFS
									 select CapaL.CapaWFS).FirstOrDefault();
			}

			// Si no está, la lee.
			if (CapaWFS == null)
			{
				WCFBPI.CCapaBingCN CapaWFSLeer = new WCFBPI.CCapaBingCN();
				CapaWFSLeer.CodigoCapa = Respuesta.Capa.Capa.CodigoWFS;
				CapaWFSLeer.Opacidad = Capa.Opacidad;
				CapaWFSLeer.Azul = Capa.Azul;
				CapaWFSLeer.Verde = Capa.Verde;
				CapaWFSLeer.Rojo = Capa.Rojo;
				CapaWFS = CargarInformacionWFS(Cliente, Ticket, CapaWFSLeer).CapaWFS;
			}

			CapaRespuesta.CapaWFS = CrearCopiaCapaWFS(CapaWFS);

			CapaRespuesta.Preguntas = ConvertirPreguntas(Respuesta.Capa.Vinculos);

			return CapaRespuesta;

		}

		[HttpGet("ListarProyectosBing")]
		public RespuestaProyectosBing ListarProyectosBing(string URL, string Ticket)
		{
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			RespuestaProyectosBing Retorno = new RespuestaProyectosBing();
			try
			{
				Task<WCFBPI.CRespuestaBings> Tarea = Cliente.ListarProyectosBingAsync(Ticket);
				Tarea.Wait();
				WCFBPI.CRespuestaBings Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al leer mapa " + Respuesta.MensajeError);
				}

				Retorno.Proyectos = (from P in Respuesta.Proyectos
														 select CRutinas.CrearMapaShared(P)).ToList();

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

		[HttpGet("ListarCapasWSS")]
		public RespuestaCapasWSS ListarCapasWSS(string URL, string Ticket, Int32 Clase, Int32 Codigo)
		{
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			RespuestaCapasWSS Retorno = new RespuestaCapasWSS();
			try
			{
				Task<WCFBPI.CRespuestaCapasWSS> Tarea = Cliente.ListarCapasWSSAsync(Ticket,
					    (WCFBPI.ClaseElemento)Clase, Codigo);
				Tarea.Wait();
				WCFBPI.CRespuestaCapasWSS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al leer capas " + Respuesta.MensajeError);
				}

				Retorno.Capas = (Respuesta.Capas == null ? new List<Shared.CCapaWSSCN>() :
					(from P in Respuesta.Capas
					 select CRutinas.CrearCapaWSSShared(P)).ToList());

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

		[HttpGet("ListarGraficosIndicadores")]
		public RespuestaGraficosVarios ListarGraficosIndicador(string URL, string Ticket, string Indicadores)
		{
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			RespuestaGraficosVarios Retorno = new RespuestaGraficosVarios();
			try
			{
				Task<WCFBPI.CRespuestaGraficosVarios> Tarea = Cliente.ListarGraficosIndicadoresAsync(Ticket,
							CRutinas.ExtraerListaEnteros(Indicadores));
				Tarea.Wait();
				WCFBPI.CRespuestaGraficosVarios Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception("Al leer gráficos " + Respuesta.MensajeError);
				}

				Retorno.Graficos = (Respuesta.Graficos == null ? new List<CGraficoCompletoCN>() :
						(from P in Respuesta.Graficos
						 select CRutinas.CrearGraficoCompletoShared(P)).ToList());

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
