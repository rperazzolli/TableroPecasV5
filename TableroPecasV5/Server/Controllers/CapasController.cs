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

		private static List<CPosicionWFSCN> CopiarListaPuntosWFS(List<WCFBPI.CPosicionWFSCN> Puntos)
		{
			return (from P in Puntos
							select new CPosicionWFSCN()
							{
								X = P.X,
								Y = P.Y
							}).ToList();
		}

		private static List<WCFBPI.CPosicionWFSCN> CopiarListaPuntosWFSBPI(List<CPosicionWFSCN> Puntos)
		{
			return (from P in Puntos
							select new WCFBPI.CPosicionWFSCN()
							{
								X = P.X,
								Y = P.Y
							}).ToList();
		}

		private static List<CValorDimensionCN> CopiarDimensiones(List<WCFBPI.CValorDimensionCN> Valores)
		{
			return (from D in Valores
							select new CValorDimensionCN()
							{
								Dimension = D.Dimension,
								Valor = D.Valor
							}).ToList();
		}

		private static List<WCFBPI.CValorDimensionCN> CopiarDimensionesBPI(List<CValorDimensionCN> Valores)
		{
			return (from D in Valores
							select new WCFBPI.CValorDimensionCN()
							{
								Dimension = D.Dimension,
								Valor = D.Valor
							}).ToList();
		}

		private static List<CAreaWFSCN> CopiarAreasWFS(List<WCFBPI.CAreaWFSCN> Areas)
		{
			if (Areas == null)
			{
				return new List<CAreaWFSCN>();
			}
			else
			{
				return (from A in Areas
								select new CAreaWFSCN()
								{
									Area = A.Area,
									Centro = new CPosicionWFSCN()
									{
										X = A.Centro.X,
										Y = A.Centro.Y
									},
									Codigo = A.Codigo,
									Contorno = CopiarListaPuntosWFS(A.Contorno),
									Dimensiones = CopiarDimensiones(A.Dimensiones),
									Nombre = A.Nombre
								}).ToList();
			}
		}

		private static List<WCFBPI.CAreaWFSCN> CopiarAreasWFSBPI(List<CAreaWFSCN> Areas)
		{
			if (Areas == null)
			{
				return new List<WCFBPI.CAreaWFSCN>();
			}
			else
			{
				return (from A in Areas
								select new WCFBPI.CAreaWFSCN()
								{
									Area = A.Area,
									Centro = new WCFBPI.CPosicionWFSCN()
									{
										X = A.Centro.X,
										Y = A.Centro.Y
									},
									Codigo = A.Codigo,
									Contorno = CopiarListaPuntosWFSBPI(A.Contorno),
									Dimensiones = CopiarDimensionesBPI(A.Dimensiones),
									Nombre = A.Nombre
								}).ToList();
			}
		}

		private static List<CLineaWFSCN> CopiarLineasWFS(List<WCFBPI.CLineaWFSCN> Lineas)
		{
			if (Lineas == null)
			{
				return new List<CLineaWFSCN>();
			}
			else
			{
				return (from L in Lineas
								select new CLineaWFSCN()
								{
									Centro = new CPosicionWFSCN()
									{
										X = L.Centro.X,
										Y = L.Centro.Y
									},
									Codigo = L.Codigo,
									Contorno = CopiarListaPuntosWFS(L.Contorno),
									Nombre = L.Nombre
								}).ToList();
			}
		}

		private static List<WCFBPI.CLineaWFSCN> CopiarLineasWFSBPI(List<CLineaWFSCN> Lineas)
		{
			if (Lineas == null)
			{
				return new List<WCFBPI.CLineaWFSCN>();
			}
			else
			{
				return (from L in Lineas
								select new WCFBPI.CLineaWFSCN()
								{
									Centro = new WCFBPI.CPosicionWFSCN()
									{
										X = L.Centro.X,
										Y = L.Centro.Y
									},
									Codigo = L.Codigo,
									Contorno = CopiarListaPuntosWFSBPI(L.Contorno),
									Nombre = L.Nombre
								}).ToList();
			}
		}

		private static List<CPuntoWFSCN> CopiarPuntosWFS(List<WCFBPI.CPuntoWFSCN> Puntos)
		{
			if (Puntos == null)
			{
				return new List<CPuntoWFSCN>();
			}
			else
			{
				return (from P in Puntos
								select new CPuntoWFSCN()
								{
									Punto = new CPosicionWFSCN()
									{
										X = P.Punto.X,
										Y = P.Punto.Y
									},
									Codigo = P.Codigo,
									Nombre = P.Nombre
								}).ToList();
			}
		}

		private static List<WCFBPI.CPuntoWFSCN> CopiarPuntosWFSBPI(List<CPuntoWFSCN> Puntos)
		{
			if (Puntos == null)
			{
				return new List<WCFBPI.CPuntoWFSCN>();
			}
			else
			{
				return (from P in Puntos
								select new WCFBPI.CPuntoWFSCN()
								{
									Punto = new WCFBPI.CPosicionWFSCN()
									{
										X = P.Punto.X,
										Y = P.Punto.Y
									},
									Codigo = P.Codigo,
									Nombre = P.Nombre
								}).ToList();
			}
		}
		private static CCapaWFSCN CopiarCapaWFS(WCFBPI.CCapaWFSCN Capa)
		{
			return new CCapaWFSCN()
			{
				Areas = CopiarAreasWFS(Capa.Areas),
				CamposInformacion = Capa.CamposInformacion,
				Capa = Capa.Capa,
				Codigo = Capa.Codigo,
				CodigoProveedor = Capa.CodigoProveedor,
				Descripcion = Capa.Descripcion,
				Detalle = Capa.Detalle,
				DireccionURL = Capa.DireccionURL,
				Elemento = (ElementoWFS)(Int32)Capa.Elemento,
				FechaRefresco = Capa.FechaRefresco,
				GuardaCompactada = Capa.GuardaCompactada,
				Lineas = CopiarLineasWFS(Capa.Lineas),
				NombreCampoCodigo = Capa.NombreCampoCodigo,
				NombreCampoDatos = Capa.NombreCampoDatos,
				NombreElemento = Capa.NombreElemento,
				Puntos = CopiarPuntosWFS(Capa.Puntos),
				PuntosMaximosContorno = Capa.PuntosMaximosContorno,
				Version = Capa.Version
			};
		}

		private static WCFBPI.CCapaWFSCN CopiarCapaWFSBPI(CCapaWFSCN Capa)
		{
			return new WCFBPI.CCapaWFSCN()
			{
				Areas = CopiarAreasWFSBPI(Capa.Areas),
				CamposInformacion = Capa.CamposInformacion,
				Capa = Capa.Capa,
				Codigo = Capa.Codigo,
				CodigoProveedor = Capa.CodigoProveedor,
				Descripcion = Capa.Descripcion,
				Detalle = Capa.Detalle,
				DireccionURL = Capa.DireccionURL,
				Elemento = (WCFBPI.ElementoWFS)(Int32)Capa.Elemento,
				FechaRefresco = Capa.FechaRefresco,
				GuardaCompactada = Capa.GuardaCompactada,
				Lineas = CopiarLineasWFSBPI(Capa.Lineas),
				NombreCampoCodigo = Capa.NombreCampoCodigo,
				NombreCampoDatos = Capa.NombreCampoDatos,
				NombreElemento = Capa.NombreElemento,
				Puntos = CopiarPuntosWFSBPI(Capa.Puntos),
				PuntosMaximosContorno = Capa.PuntosMaximosContorno,
				Version = Capa.Version
			};
		}

		private static CElementoPreguntasWISCN CopiarElementoPreguntaWISCN(WCFBPI.CElementoPreguntasWISCN Elemento)
		{
			return new CElementoPreguntasWISCN()
			{
				Abscisa = Elemento.Abscisa,
				ClaseWIS = (ClaseCapa)(Int32)Elemento.ClaseWIS,
				Codigo = Elemento.Codigo,
				CodigoArea = Elemento.CodigoArea,
				CodigoWIS = Elemento.CodigoWIS,
				Contenidos = (from P in Elemento.Contenidos
											select ProyectosController.CopiarPreguntaWISCN(P)).ToList(),
				Dimension = Elemento.Dimension,
				ElementoDimension = Elemento.ElementoDimension,
				Nombre = Elemento.Nombre,
				Ordenada = Elemento.Ordenada
			};
		}

		private static CCapaWISCompletaCN CopiarCapaWISCompleta(WCFBPI.CCapaWISCompletaCN Capa)
		{
			return new CCapaWISCompletaCN()
			{
				Capa = new CCapaWISCN()
				{
					Codigo = Capa.Capa.Codigo,
					CodigoWFS = Capa.Capa.CodigoWFS,
					Descripcion = Capa.Capa.Descripcion
				},
				Vinculos = (from V in Capa.Vinculos
										select CopiarElementoPreguntaWISCN(V)).ToList()
			};
		}

		private static CCapaWMSCN CopiarCapaWMS(WCFBPI.CCapaWMSCN Capa)
		{
			return new CCapaWMSCN()
			{
				Codigo = Capa.Codigo,
				Descripcion = Capa.Descripcion,
				Capa = Capa.Capa,
				CodigoProveedor = Capa.CodigoProveedor,
				EPGS = Capa.EPGS,
				LatMaxima = Capa.LatMaxima,
				LatMinima = Capa.LatMinima,
				LongMaxima = Capa.LongMaxima,
				LongMinima = Capa.LongMinima,
				Query = Capa.Query,
				URLProveedor = Capa.URLProveedor,
				VersionProveedor = Capa.VersionProveedor
			};
		}

		// GET: CapasController
		[HttpGet("ListarTodasLasCapas")]
		public RespuestaCapasGIS ListarTodasLasCapas(string URL, string Ticket, string WFS, string SinDetalle)
		{
			RespuestaCapasGIS Retorno = new RespuestaCapasGIS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapasGIS> Tarea = Cliente.ListarTodasLasCapasAsync(Ticket,
							WFS == "Y", SinDetalle == "Y");
				Tarea.Wait();
				WCFBPI.CRespuestaCapasGIS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.CapasWFS = (from C in Respuesta.CapasWFS
														select CopiarCapaWFS(C)).ToList();

				Retorno.CapasWIS = (from C in Respuesta.CapasWIS
														select CopiarCapaWISCompleta(C)).ToList();

				Retorno.CapasWMS = (from C in Respuesta.CapasWMS
														select CopiarCapaWMS(C)).ToList();

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

		[HttpGet("LeerCapaWFS")]
		public RespuestaCapaWFS LeerCapaWFS(string URL, string Ticket, Int32 Codigo, string ForzarWeb)
		{
			RespuestaCapaWFS Retorno = new RespuestaCapaWFS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapaWFS> Tarea = Cliente.LeerCapaWFSAsync(Ticket,
						Codigo, ForzarWeb == "Y");
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

		[HttpPost("ValidarCapaWFS")]
		public Respuesta ValidarCapaWFS(string URL, string Ticket, [FromBody] CCapaWFSCN Capa)
		{
			RespuestaCapaWFS Retorno = new RespuestaCapaWFS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{

				WCFBPI.CCapaWFSCN CapaBPI = CopiarCapaWFSBPI(Capa);

				Task<WCFBPI.CRespuestaTexto> TareaVal = Cliente.ValidarCapaWFSAsync(Ticket, CapaBPI);
				TareaVal.Wait();
				WCFBPI.CRespuestaTexto RespuestaVal = TareaVal.Result;
				if (!RespuestaVal.RespuestaOK)
				{
					throw new Exception(RespuestaVal.MensajeError);
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

		[HttpGet("ListarCapasWFS")]
		public RespuestaCapasGIS ListarCapasWFS(string URL, string Ticket)
		{
			RespuestaCapasGIS Retorno = new RespuestaCapasGIS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapasGIS> Tarea = Cliente.ListarTodasLasCapasAsync(Ticket, true, true);
				Tarea.Wait();
				WCFBPI.CRespuestaCapasGIS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.CapasWFS = (from WCFBPI.CCapaWFSCN Capa in Respuesta.CapasWFS
												 select ProyectosController.CopiarCapaWFS(Capa)).ToList();

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

		[HttpGet("ListarCapasProveedorWFS")]
		public RespuestaCapasWFS ListarCapasProveedorWFS(string URL, string Ticket, Int32 Codigo)
		{
			RespuestaCapasWFS Retorno = new RespuestaCapasWFS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapasWFS> Tarea = Cliente.ListarCapasProveedorWFSAsync(Ticket, Codigo);
				Tarea.Wait();
				WCFBPI.CRespuestaCapasWFS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.Capas = (from WCFBPI.CCapaWFSCN Capa in Respuesta.Capas
												 select ProyectosController.CopiarCapaWFS(Capa)).ToList();

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

		[HttpGet("ValidarCapasProveedorWFS")]
		public Respuesta ValidarCapasProveedorWFS(string URL, string Ticket, Int32 Codigo)
		{
			Respuesta Retorno = new Respuesta();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapasWFS> Tarea = Cliente.ListarCapasProveedorWFSAsync(Ticket, Codigo);
				Tarea.Wait();
				WCFBPI.CRespuestaCapasWFS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				foreach (WCFBPI.CCapaWFSCN Capa in Respuesta.Capas)
				{
					Task<WCFBPI.CRespuestaTexto> TareaValid = Cliente.ValidarCapaWFSAsync(Ticket, Capa);
					TareaValid.Wait();
					WCFBPI.CRespuestaTexto RespValid = TareaValid.Result;
					if (!RespValid.RespuestaOK)
					{
						throw new Exception("Capa " + Capa.Capa + " <" + Capa.Descripcion + "> " + RespValid.MensajeError);
					}
					else
					{
						if (RespValid.Contenido.Length > 0)
						{
							throw new Exception("Capa " + Capa.Capa + " <" + Capa.Descripcion + "> " + RespValid.Contenido);
						}
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

		// GET: CapasController
		[HttpGet("ListarProveedoresWFS")]
		public RespuestaProveedoresWFS ListarProveedoresWFS(string URL, string Ticket)
		{
			RespuestaProveedoresWFS Retorno = new RespuestaProveedoresWFS();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaProveedoresWFS> Tarea = Cliente.ProveedoresWFSAsync(Ticket);
				Tarea.Wait();
				WCFBPI.CRespuestaProveedoresWFS Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}

				Retorno.Proveedores = (from C in Respuesta.Proveedores
															 select new CProveedorWFSCN()
															 {
																 Codigo = C.Codigo,
																 Descripcion = C.Descripcion,
																 DireccionFA = C.DireccionFA,
																 DireccionURL = C.DireccionURL,
																 FechaRefresco = C.FechaRefresco
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

		private WCFBPI.CProveedorWFSCN ExtraerProveedorWFSBPI(CProveedorWFSCN Proveedor)
		{
			return new WCFBPI.CProveedorWFSCN()
			{
				Codigo = Proveedor.Codigo,
				Descripcion = Proveedor.Descripcion,
				DireccionFA = Proveedor.DireccionFA,
				DireccionURL = Proveedor.DireccionURL,
				FechaRefresco = Proveedor.FechaRefresco
			};
		}

		[HttpPost("InsertarProveedorWFS")]
		public RespuestaEnteros InsertarProveedorWFS(string URL, string Ticket, [FromBody] CProveedorWFSCN Proveedor)
		{
			RespuestaEnteros Retorno = new RespuestaEnteros();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				WCFBPI.CProveedorWFSCN ProveedorBPI = ExtraerProveedorWFSBPI(Proveedor);
				Task<WCFBPI.CRespuestaCodigo> Tarea = Cliente.RegistrarProveedorWFSAsync(Ticket, ProveedorBPI);
				Tarea.Wait();
				WCFBPI.CRespuestaCodigo Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}
				Retorno.Codigos.Add(Respuesta.Codigo);
			}
			catch (Exception ex)
			{
				Retorno.RespuestaOK = false;
				Retorno.MsgErr = CRutinas.TextoMsg(ex);
			}
			return Retorno;
		}

		[HttpPost("InsertarCapaWFS")]
		public RespuestaEnteros InsertarCapaWFS(string URL, string Ticket, [FromBody] CCapaWFSCN Capa)
		{
			RespuestaEnteros Retorno = new RespuestaEnteros();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				WCFBPI.CCapaWFSCN CapaBPI = CopiarCapaWFSBPI(Capa);
				Task<WCFBPI.CRespuestaCodigo> Tarea = Cliente.RegistrarCapaWFSAsync(Ticket, CapaBPI);
				Tarea.Wait();
				WCFBPI.CRespuestaCodigo Respuesta = Tarea.Result;
				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MensajeError);
				}
				Retorno.Codigos.Add(Respuesta.Codigo);
			}
			catch (Exception ex)
			{
				Retorno.RespuestaOK = false;
				Retorno.MsgErr = CRutinas.TextoMsg(ex);
			}
			return Retorno;
		}

		[HttpDelete("BorrarCapaWFS")]
		public Respuesta BorrarCapaWFS(string URL, string Ticket, Int32 CodigoCapa)
		{
			RespuestaEnteros Retorno = new RespuestaEnteros();
			WCFBPI.WCFBPIClient Cliente = CRutinas.ObtenerClienteWCF(URL);
			try
			{
				Task<WCFBPI.CRespuestaCapaWFS> TareaLect = Cliente.LeerCapaWFSAsync(Ticket, CodigoCapa, false);
				TareaLect.Wait();
				WCFBPI.CRespuestaCapaWFS RespuestaLect = TareaLect.Result;
				if (!RespuestaLect.RespuestaOK)
				{
					throw new Exception(RespuestaLect.MensajeError);
				}


				Task<WCFBPI.CRespuesta> Tarea = Cliente.BorrarCapaWFSAsync(Ticket, RespuestaLect.Capa);
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
			return Retorno;
		}

	}
}
