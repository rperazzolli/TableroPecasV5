using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaTortasGIS : ComponentBase, IDisposable
	{

		[Parameter]
		public List<CLineaComprimida> Lineas { get; set; }

		//// Columna que define las tortas (cuando no es por coordenadas).
		//[Parameter]
		//public CColumnaBase ColumnaTortas { get; set; }

		// Columna donde están los datos.
		[Parameter]
		public CColumnaBase ColumnaDatos { get; set; }

		/// <summary>
		/// Columna que agrupa los gajos.
		/// </summary>
		[Parameter]
		public CColumnaBase ColumnaAgrupadora { get; set; }

		/// <summary>
		/// Columna que posiciona los datos.
		/// </summary>
		[Parameter]
		public CColumnaBase ColumnaPosicion { get; set; }

		// Columnas que definen las coordenadas (cuando es por coordenadas).	}
		[Parameter]
		public CColumnaBase ColumnaLat { get; set; }

		[Parameter]
		public CColumnaBase ColumnaLng { get; set; }

		[Parameter]
		public DatosSolicitados Solicitud { get; set; }

		[Parameter]
		public ClaseElemento ClaseIndicador { get; set; }

		[Parameter]
		public Int32 CodigoIndicador { get; set; }

		[Parameter]
		public Int32 ElementoDimension { get; set; } = -1;

		private bool mbVinculosCompletos = false;

		private static Int32 gCodigoUnico = 0;
		public Int32 CodigoPropio = -1;

		public async void Dispose()
		{
			if (PosicionMapa>=0)
			{
				await CRutinas.LiberarMapaAsync(JSRuntime, PosicionMapa);
			}
		}

		public string Direccion
		{
			get
			{
				return "CLog_Torta_GIS" + CodigoPropio.ToString();
			}
		}

		[Inject]
		public HttpClient Http { get; set; }

		public List<CListaPosicion> PosicionesManuales { get; set; } = null;

		public bool MostrarVinculadorManual { get; set; } = false;

		public bool MostrarVinculadorWFS { get; set; } = false;

		protected override Task OnInitializedAsync()
		{
			CodigoPropio = gCodigoUnico++;
			return base.OnInitializedAsync();
		}

		private void ArmarListaPosiciones (CVinculoIndicadorCompletoCN Vinculador)
		{
			PosicionesManuales = new List<CListaPosicion>();
			foreach (string Valor in ColumnaPosicion.ListaValores)
			{

				CVinculoDetalleCN Vinculo = (Vinculador == null ? null :
					(from D in Vinculador.Detalles
					 where D.ValorAsociado == Valor
					 select D).FirstOrDefault());

				CListaPosicion Posicion = new CListaPosicion()
				{
					Codigo = (Vinculo == null ? -1 : Vinculo.Codigo),
					Color = (Vinculo == null ? "lightred" : "white"),
					Descripcion = Valor,
					Referencia = Valor
				};
				if (Vinculo == null)
				{
					Posicion.Lat = -1000;
					Posicion.Lng = -1000;
				}
				else
				{
					CRutinas.ExtraerCoordenadasPosicion(Vinculo.Posicion, out double Lng, out double Lat);
					Posicion.Lng = Lng;
					Posicion.Lat = Lat;
				}
				PosicionesManuales.Add(Posicion);
			}
		}

		[Inject]
		public NavigationManager Navegador { get; set; }

		private List<CTortaBing> mTortas = null;

		private List<CLineaComprimida> ExtraerLineas(string Referencia)
		{
			Int32 PosBuscada = -1;
			switch (ColumnaPosicion.Clase)
			{
				case ClaseVariable.Booleano:
				case ClaseVariable.Fecha:
				case ClaseVariable.Texto:
					PosBuscada = ColumnaPosicion.PosicionValorIgual(Referencia);
					break;
				case ClaseVariable.Entero:
					PosBuscada = ColumnaPosicion.PosicionValorIgual(Int32.Parse(Referencia));
					break;
			}
			return (from L in Lineas
							where L.Codigos[ColumnaPosicion.Orden] == PosBuscada
							select L).ToList();
		}

		private CGajoTorta ObtenerGajo(Int32 Orden, List<CLineaComprimida> Lineas, bool TodosCeros,
			  double AcumuladoTorta)
		{
			List<CLineaComprimida> LineasPropias = (from L in Lineas
																							where L.Codigos[ColumnaAgrupadora.Orden] == Orden
																							select L).ToList();
			double Acumulado = (TodosCeros ? LineasPropias.Count : ObtenerAcumulado(LineasPropias));
			return new CGajoTorta()
			{
				Color = "",
				Resaltado = false,
				Referencia = ColumnaAgrupadora.Valores[Orden].ToString(),
				Texto = ColumnaAgrupadora.ListaValores[Orden],
				Valor = Acumulado
			};
		}

		private double ObtenerAcumulado(List<CLineaComprimida> Lineas)
		{
			double Respuesta = 0;
			foreach (CLineaComprimida Linea in Lineas)
			{
				Respuesta += Math.Abs(ColumnaDatos.ValorReal(Linea.Codigos[ColumnaDatos.Orden]));
			}
			if (Math.Abs(Respuesta) < 1e-100){
				Respuesta = 0;
			}
			return Respuesta;
		}

		private List<CGajoTorta> ExtraerGajos(string Referencia, bool TodosCeros, out double Acumulado)
		{
			// primer paso: extraer las líneas.
			List<CLineaComprimida> LineasLocales = ExtraerLineas(Referencia);

			// verificar si todos son 0.
			Acumulado = (TodosCeros ? LineasLocales.Count : ObtenerAcumulado(LineasLocales));

			// buscar los posibles gajos.
			List<Int32> OrdenesDiversos = (from L in LineasLocales
																		 select L.Codigos[ColumnaAgrupadora.Orden]).Distinct().ToList();

			// Obtener los gajos.
			List<CGajoTorta> Respuesta = new List<CGajoTorta>();
			foreach (Int32 Codigo in OrdenesDiversos)
			{
				Respuesta.Add(ObtenerGajo(Codigo, LineasLocales, TodosCeros, Acumulado));
			}

			// Compactar gajos por encima de 10.
			double Angulo = 0;
			for (Int32 i = 0; i < 10 && i < Respuesta.Count; i++)
			{
				Respuesta[i].Angulo = Angulo;
				Angulo += 2 * Math.PI * Respuesta[i].Valor / Acumulado;
			}

			if (Respuesta.Count > 10)
			{
				double Valor = 0;
				for (Int32 i = Respuesta.Count - 1; i >= 10; i--)
				{
					Valor += Math.Abs(Respuesta[i].Valor);
					Respuesta.RemoveAt(i);
				}
				Respuesta.Add(new CGajoTorta()
				{
					Angulo = Angulo,
					Texto = "Resto",
					Referencia = REFERENCIA_OTROS,
					Color = "gray",
					Resaltado = false,
					Valor = Valor
				});
			}

			return Respuesta;
		}

		private void DeterminarRangoCoordenadas(out double LngMin, out double LngMax,
							out double LatMin, out double LatMax, out double LadoMax)
		{
			LngMin = double.MaxValue;
			LngMax = double.MinValue;
			LatMin = double.MaxValue;
			LatMax = double.MinValue;

			foreach (CTortaBing Torta in mTortas)
			{
				LngMin = Math.Min(Torta.Centro.X, LngMin);
				LngMax = Math.Max(Torta.Centro.X, LngMax);
				LatMin = Math.Min(Torta.Centro.Y, LatMin);
				LatMax = Math.Max(Torta.Centro.Y, LatMax);
			}

			if (LngMin == LngMax)
			{
				LngMin -= 0.5;
				LngMax += 0.5;
			}

			if (LatMin == LatMax)
			{
				LatMin -= 0.5;
				LatMax += 0.5;
			}

			double ElementosLinea = Math.Sqrt((double)mTortas.Count);

			LadoMax = Math.Sqrt((LngMax - LngMin) * (LatMax - LatMin)) / (ElementosLinea + 1);

			double Extra = (LngMax - LngMin) / ElementosLinea;
			LngMin -= Extra;
			LngMax += Extra;

			Extra = (LatMax - LatMin) / ElementosLinea;
			LatMin -= Extra;
			LatMax += Extra;

		}

		private const string NO_REFERENCIA = "%%%%%%%%";
		private string mszReferenciaAnterior = NO_REFERENCIA;
		private double mFactorEscala = 1;

		public void ModificarFactor(double Factor)
		{
			mFactorEscala *= Factor;
			mbDibujarTortas = true;
			StateHasChanged();
		}

		public void SeleccionarGajo(string Referencia)
		{
			if (mTortas != null)
			{
				if (Referencia == mszReferenciaAnterior)
				{
					Referencia = NO_REFERENCIA;
				}
				mszReferenciaAnterior = Referencia;
				foreach (CTortaBing Torta in mTortas)
				{
					Torta.SeleccionarGajo(Referencia);
				}
				mbDibujarTortas = true;
				StateHasChanged();
			}
		}

		private bool mbDibujarTortas = false;
		public List<CLineaValorColor> ListaColores = null;

		//public List<CLineaValorColor> ListaColores
		//{
		//	get { return mListaColores; }
		//	set { mListaColores = value; }
		//}

		private const string REFERENCIA_OTROS = "$$%%";

		private void AgregarValoresGajoTorta(CGajoTorta Gajo)
		{
			if (Gajo.Referencia != REFERENCIA_OTROS)
			{
				CLineaValorColor Linea = (from L in ListaColores
																	where L.Referencia == Gajo.Referencia
																	select L).FirstOrDefault();
				if (Linea == null)
				{
					Linea = new CLineaValorColor()
					{
						Valor = 0,
						Texto = Gajo.Texto,
						Referencia = Gajo.Referencia
					};
					ListaColores.Add(Linea);
				}

				Linea.Valor += Math.Abs(Gajo.Valor);

			}
		}

		private CLogicaReferencias mReferenciaDrag = null;

		public void IniciarDragReferencias()
		{
			mReferenciaDrag = Referencias;
		}

		public void RecibirDrop(Microsoft.AspNetCore.Components.Web.DragEventArgs e)
		{
			if (mReferenciaDrag != null)
			{
					Int32 Diferencia = (int)e.ClientX - Referencias.AbscisaAbajo;
					mAbscisaReferencias += Diferencia;
					Diferencia = (int)e.ClientY - Referencias.OrdenadaAbajo;
					mOrdenadaReferencias += Diferencia;
					StateHasChanged();
				}
			}

			private void ImponerColor(CGajoTorta Gajo)
		{
			if (Gajo.Referencia != REFERENCIA_OTROS)
			{
				CLineaValorColor Linea = (from L in ListaColores
																	where L.Referencia == Gajo.Referencia
																	select L).FirstOrDefault();
				if (Linea != null)
				{
					Gajo.Color = Linea.Color;
				}
			}
		}

		private double DeterminarLadoMaximo(out double Maximo)
		{
			Maximo = (from T in mTortas
											 select T.Acumulado).Max();
			return Math.Sqrt((double)(CRutinas.AnchoPantalla - 40) * (double)(CRutinas.AltoPantalla - 40) /
					(1.5 * mTortas.Count));

		}

		private void AjustarColoresGajos()
		{
			ListaColores = new List<CLineaValorColor>();
			foreach (CTortaBing Torta in mTortas)
			{
				foreach (CGajoTorta Gajo in Torta.Gajos)
				{
					AgregarValoresGajoTorta(Gajo);
				}
			}

			ListaColores.Sort(delegate (CLineaValorColor L1, CLineaValorColor L2)
			{
				return L2.Valor.CompareTo(L1.Valor);
			});

			ListaColores = (from L in ListaColores
											where L.Valor > 0
											select L).ToList();

			Int32 Pos = 0;
			foreach (CLineaValorColor Linea in ListaColores)
			{
				Linea.Color = CRutinas.ColorSecuencia(Pos++);
			}

			foreach (CTortaBing Torta in mTortas)
			{
				foreach (CGajoTorta Gajo in Torta.Gajos)
				{
					ImponerColor(Gajo);
				}
			}

		}

		public bool HayColores
		{
			get
			{
				return ListaColores != null && ListaColores.Count > 0;
			}
		}

		private async Task DeterminarTortasAsync()
		{
			if (mTortas != null)
			{
				await CRutinas.LimpiarContenidoMapaAsync(JSRuntime, PosicionMapa);
			}
			
			mTortas = new List<CTortaBing>();

			if (mVinculador != null && mVinculador.Detalles.Count > 0)
			{
				double Acumulado = ObtenerAcumulado(Lineas);
				bool TodosCeros = Acumulado == 0;
				foreach (CVinculoDetalleCN Detalle in mVinculador.Detalles)
				{
					if (Detalle.Posicion.Length > 0)
					{
						List<CGajoTorta> Gajos = ExtraerGajos(Detalle.ValorAsociado, TodosCeros, out double AcumTorta);
						mTortas.Add(new CTortaBing()
						{
							CodigoTorta = Detalle.ValorAsociado,
							Centro = ExtraerCentro(Detalle.Posicion),
							Gajos = Gajos,
							Lado = 0,
							Acumulado = AcumTorta
						});
					}
				}

				mTortas = (from T in mTortas
									 where T.Gajos.Count > 0 && T.Centro.X > -999
									 select T).ToList();

				if (!TodosCeros)
				{
					AjustarAcumuladosTortas();
				}

				if (mTortas.Count > 0)
				{
					double Maximo = (from T in mTortas
													 select T.Acumulado).Max();
					DeterminarRangoCoordenadas(out double LngMin, out double LngMax,
							out double LatMin, out double LatMax, out double LadoMax);

					foreach (CTortaBing Torta in mTortas)
					{
						Torta.Lado = Torta.Acumulado * Math.Sqrt(LadoMax / Maximo);
					}

					mAbscisaCentro = (LngMax + LngMin) / 2;
					mOrdenadaCentro = (LatMax + LatMin) / 2;
					mNivelZoom = CRutinas.UbicarNivelZoom(CRutinas.AnchoPantalla - 40, CRutinas.AltoPantalla - 40,
							LngMax - LngMin, LatMax - LatMin);

				}
			}

			AjustarColoresGajos();

			// Imponer colores.
			StateHasChanged();

		}

		private void AjustarAcumuladosTortas() {
			double AcumLocal = (from T in mTortas
													select T.Acumulado).Sum();
			if (AcumLocal == 0)
			{
				foreach (CTortaBing Torta in mTortas)
				{
					Torta.Gajos = ExtraerGajos(Torta.CodigoTorta, true, out double AcumTorta);
					Torta.Acumulado = AcumTorta;
				}
			}
		}

		private CPosicionWFSCN ExtraerCentroWFS(string Posicion)
		{
			CCapaWFSCN Capa = VinculadorWFS.Capa;
			foreach (CPuntoWFSCN Punto in Capa.Puntos)
			{
				if (CLogicaVinculadorWFS.TruncarTexto(Punto.Codigo) == Posicion)
				{
					return Punto.Punto;
				}
			}
			foreach (CLineaWFSCN Linea in Capa.Lineas)
			{
				if (CLogicaVinculadorWFS.TruncarTexto(Linea.Codigo) == Posicion)
				{
					return Linea.Centro;
				}
			}
			foreach (CAreaWFSCN Area in Capa.Areas)
			{
				if (CLogicaVinculadorWFS.TruncarTexto(Area.Codigo) == Posicion)
				{
					return Area.Centro;
				}
			}
			return new CPosicionWFSCN
			{
				X = -1000,
				Y = -1000
			};
		}

		private async Task DeterminarPosicionElementosWFSAsync()
		{
			if (mTortas != null && mTortas.Count > 0)
			{
				await CRutinas.LimpiarContenidoMapaAsync(JSRuntime, PosicionMapa);
			}

			mTortas = new List<CTortaBing>();

			if (mVinculador != null && mVinculador.Detalles.Count > 0 &&
				  VinculadorWFS!=null && VinculadorWFS.Capa!=null)
			{
				double Acumulado = ObtenerAcumulado(Lineas);
				bool TodosCeros = Acumulado == 0;
				foreach (CVinculoDetalleCN Detalle in mVinculador.Detalles)
				{
					if (Detalle.Posicion.Length > 0)
					{
						List<CGajoTorta> Gajos = ExtraerGajos(Detalle.ValorAsociado, TodosCeros, out double AcumTorta);
						mTortas.Add(new CTortaBing()
						{
							CodigoTorta = Detalle.ValorAsociado,
							Centro = ExtraerCentroWFS(Detalle.Posicion),
							Gajos = Gajos,
							Lado = 0,
							Acumulado = AcumTorta
						});
					}
				}

				mTortas = (from T in mTortas
									 where T.Gajos.Count > 0 && T.Centro.X > -999
									 select T).ToList();

				if (!TodosCeros)
				{
					AjustarAcumuladosTortas();
				}

				if (mTortas.Count > 0)
				{
					double Maximo = (from T in mTortas
													 select T.Acumulado).Max();
					DeterminarRangoCoordenadas(out double LngMin, out double LngMax,
							out double LatMin, out double LatMax, out double LadoMax);

					foreach (CTortaBing Torta in mTortas)
					{
						Torta.Lado = LadoMax * Math.Sqrt(Torta.Acumulado / Maximo);
					}

					mAbscisaCentro = (LngMax + LngMin) / 2;
					mOrdenadaCentro = (LatMax + LatMin) / 2;
					mNivelZoom = CRutinas.UbicarNivelZoom(CRutinas.AnchoPantalla - 40, CRutinas.AltoPantalla - 40,
							LngMax - LngMin, LatMax - LatMin);

				}
			}

			AjustarColoresGajos();

			// Imponer colores.
			StateHasChanged();

		}

		private CPosicionWFSCN ExtraerCentro(string Posicion)
		{
			switch (Solicitud)
			{
				case DatosSolicitados.TortasManual:
					CRutinas.ExtraerCoordenadasPosicion(Posicion, out double Lng, out double Lat);
					return new CPosicionWFSCN()
					{
						X = Lng,
						Y = Lat
					};
				default:
					return new CPosicionWFSCN()
					{
						X = -1000,
						Y = -1000
					};
			}
		}

		public CLogicaVinculadorCoordenadas VinculadorCoordenadas { get; set; }
		public CLogicaVinculadorWFS VinculadorWFS { get; set; }

		public async void RecibirVinculos(CVinculoIndicadorCompletoCN Vinculo)
		{
			try
			{
				if (VinculadorCoordenadas != null && MostrarVinculadorManual)
				{
					VinculadorCoordenadas.LiberarMapa();
				}

				MostrarVinculadorManual = false;
				MostrarVinculadorWFS = false;

				if (Vinculo != null && Vinculo.Detalles.Count > 0)
				{
					mVinculador = Vinculo;
					switch (Solicitud)
					{
						case DatosSolicitados.TortasManual:
							await DeterminarTortasAsync();
							break;
						case DatosSolicitados.TortasGIS:
							await DeterminarPosicionElementosWFSAsync();
							break;
					}
				}
				else
				{
					Navegador.NavigateTo("Indicadores");
				}
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}

		private CTortaBing TortaEnPosicion(double Lat, double Lng, double Rango2)
		{
			double Distancia = double.MaxValue;
			CTortaBing TortaRefe = null;
			foreach (CTortaBing Torta in mTortas)
			{
				double Dist = (Torta.Centro.X - Lng) * (Torta.Centro.X - Lng) +
						(Torta.Centro.Y - Lat) * (Torta.Centro.Y - Lat);
				if (Dist < Distancia)
				{
					Distancia = Dist;
					TortaRefe = Torta;
				}
			}
			if (TortaRefe!=null && Distancia < Rango2)
			{
				return TortaRefe;
			}
			else
			{
				return null;
			}
		}

		private void ArmarTortasDesdeCoordenadas()
		{
			// determinar el rango de coordenadas.
			if (Lineas.Count == 0)
			{
				Navegador.NavigateTo("Indicadores");
			}
			else
			{
				try
				{
					double LatMin = double.MaxValue;
					double LatMax = double.MinValue;
					double LngMin = double.MaxValue;
					double LngMax = double.MinValue;
					double Acumulado = 0;
					foreach (CLineaComprimida Linea in Lineas)
					{
						double Lat = ColumnaLat.ValorReal(Linea.Codigos[ColumnaLat.Orden]);
						double Lng = ColumnaLng.ValorReal(Linea.Codigos[ColumnaLng.Orden]);
						if (Math.Abs(Lat) <= 180 && Math.Abs(Lng) <= 180)
						{
							LatMin = Math.Min(LatMin, Lat);
							LatMax = Math.Max(LatMax, Lat);

							LngMin = Math.Min(LngMin, Lng);
							LngMax = Math.Max(LngMax, Lng);
							Acumulado += Math.Abs(ColumnaDatos.ValorReal(Linea.Codigos[ColumnaDatos.Orden]));
						}
					}

					bool TodosCeros = (Acumulado == 0);

					double Rango;
					if (LatMin == LatMax && LngMin == LngMax)
					{
						Rango = 0.1;
					}
					else
					{
						Rango = Math.Max(LatMax - LatMin, LngMax - LngMin) / 90;
					}

					// determina los grupos de datos.
					mTortas = new List<CTortaBing>();
					double Rango2 = Rango * Rango;
					foreach (CLineaComprimida Linea in Lineas)
					{
						double Lat = ColumnaLat.ValorReal(Linea.Codigos[ColumnaLat.Orden]);
						double Lng = ColumnaLng.ValorReal(Linea.Codigos[ColumnaLng.Orden]);
						CTortaBing Torta = TortaEnPosicion(Lat, Lng, Rango2);
						if (Torta == null)
						{
							Torta = new CTortaBing()
							{
								Gajos = new List<CGajoTorta>(),
								Centro = new CPosicionWFSCN()
								{
									X = Lng,
									Y = Lat
								}
							};
						  mTortas.Add(Torta);
						}
						Torta.SumarValor(ColumnaAgrupadora.ListaValores[Linea.Codigos[ColumnaAgrupadora.Orden]],
									ColumnaAgrupadora.Valores[Linea.Codigos[ColumnaAgrupadora.Orden]].ToString(),
									(TodosCeros ? 1 : ColumnaDatos.ValorReal(Linea.Codigos[ColumnaDatos.Orden])));
					}

					foreach (CTortaBing Torta in mTortas)
					{
						Torta.DeterminarAcumulado();
					}

					mTortas = (from T in mTortas
										 where T.Acumulado > 0
										 select T).ToList();

					DeterminarRangoCoordenadas(out LngMin, out LngMax,
							out LatMin, out LatMax, out double LadoMax);

					mAbscisaCentro = (LngMax + LngMin) / 2;
					mOrdenadaCentro = (LatMax + LatMin) / 2;
					mNivelZoom = CRutinas.UbicarNivelZoom(CRutinas.AnchoPantalla - 40, CRutinas.AltoPantalla - 40,
							LngMax - LngMin, LatMax - LatMin);


					double MaximoTorta = (from T in mTortas
													 select T.Acumulado).Max();

					foreach (CTortaBing Torta in mTortas)
					{
						Torta.Lado = LadoMax * Math.Sqrt(Torta.Acumulado / MaximoTorta);
					}

					AjustarColoresGajos();

					StateHasChanged();
				}
				catch (Exception ex)
				{
					CRutinas.DesplegarMsg(ex);
				}

			}
			//
		}

		private CVinculoIndicadorCompletoCN mVinculador = null;

		public CVinculoIndicadorCompletoCN Vinculador { get { return mVinculador; } }

		private async Task PedirDatosVinculadorAsync()
		{
			mVinculador = await CContenedorDatos.LeerVinculoAsync(
					Http, ClaseIndicador,
					CodigoIndicador, ColumnaPosicion.Nombre);
			bool bDatosCompletos = (mVinculador != null);
			if (mVinculador != null)
			{
				// Verificar si están los datos completos.
				foreach (string Valor in ColumnaPosicion.ListaValores)
				{
					if ((from D in mVinculador.Detalles
							 where D.ValorAsociado == Valor
							 select D).FirstOrDefault() == null)
					{
						bDatosCompletos = false;
						break;
					}
				}
				if (!bDatosCompletos)
				{
					mbVinculosCompletos = true;
					StateHasChanged();
					return;
				}
			}

			switch (Solicitud)
			{
				case DatosSolicitados.TortasManual:
					ArmarListaPosiciones(mVinculador);
					MostrarVinculadorManual = true;
					StateHasChanged();
					break;
				case DatosSolicitados.TortasGIS:
					MostrarVinculadorWFS = true;
					StateHasChanged();
					break;
			}

		}

		private Int32 mAbscisaReferencias = 5;
		private Int32 mOrdenadaReferencias = 5;

		public string EstiloContenedorReferencias
		{
			get
			{
				return "width: 250px; height: 250px; position: absolute; margin-left: " + mAbscisaReferencias.ToString() +
						"px; margin-top: " + mOrdenadaReferencias.ToString() + "px; background: white; z-index: 21;";
			}
		}

		public CLogicaReferencias Referencias { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		public Int32 PosicionMapa { get; set; } = -1;
		private double mAbscisaCentro = -1000;
		private double mOrdenadaCentro = -1000;
		private Int32 mNivelZoom = -1;

		//private async Task UbicarCentroAsync()
		//{
		//	double LatMin = double.MaxValue;
		//	double LatMax = double.MinValue;
		//	double LngMin = double.MaxValue;
		//	double LngMax = double.MinValue;
		//	switch (Solicitud)
		//	{
		//		case DatosSolicitados.TortasManual:
		//			if (PosicionesManuales!=null && PosicionesManuales.Count > 0)
		//			{
		//				foreach (CListaPosicion Elemento in PosicionesManuales)
		//				{
		//					LatMin = Math.Min(Elemento.Lat, LatMin);
		//					LatMax = Math.Max(Elemento.Lat, LatMax);
		//					LngMin = Math.Min(Elemento.Lng, LngMin);
		//					LngMax = Math.Max(Elemento.Lng, LngMax);
		//				}
		//			}
		//			break;
		//	}
		//	if (LatMin > LatMax)
		//	{
		//		mAbscisaCentro = -68;
		//		mOrdenadaCentro = -39;
		//		mNivelZoom = 7;
		//	}
		//	else
		//	{
		//		mAbscisaCentro = (LngMax + LngMin) / 2;
		//		mOrdenadaCentro = (LatMax + LatMin) / 2;
		//		CPosicionWFSCN Dimensiones = await CRutinas.ObtenerDimensionesPantallaAsync(JSRuntime, Direccion);
		//		mNivelZoom = CRutinas.UbicarNivelZoom(Dimensiones.X, Dimensiones.Y, LngMax - LngMin, LatMax - LatMin);
		//	}
		//}

		private bool mbRedibujando = false;

		private async Task DibujarTortasAsync()
		{
			if (mbRedibujando)
			{
				await CRutinas.LimpiarContenidoMapaAsync(JSRuntime, PosicionMapa);
			}
			else
			{
				mbRedibujando = true;
			}

			if (mTortas != null)
			{
				Rectangulo RectPant = await CRutinas.ObtenerRectanguloElementoAsync(JSRuntime, Direccion);
				Rectangulo RectMapa = await CRutinas.ObtenerExtremosMapaAsync(JSRuntime, PosicionMapa);
				if (RectPant.width<0 || RectMapa.width < 0)
				{
					mbRedibujando = true;
					StateHasChanged();
					return;
				}
				double FactorAbsc = (RectPant.height * RectMapa.width) / (RectPant.width * RectMapa.height);
				mbDibujarTortas = false;
				foreach (CTortaBing Torta in mTortas)
				{
					await Torta.GraficarSobreMapaAsync(JSRuntime, PosicionMapa, FactorAbsc, mFactorEscala);
				}
			}
		}

		private bool mbPrimerIntento = true;

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if ((Solicitud == DatosSolicitados.TortasGIS || Solicitud == DatosSolicitados.TortasManual) &&
					mVinculador == null)
			{
				if (mbPrimerIntento)
				{
					mbPrimerIntento = false;
					MostrarVinculadorManual = (Solicitud == DatosSolicitados.TortasManual);
					MostrarVinculadorWFS = (Solicitud == DatosSolicitados.TortasGIS);
				  StateHasChanged();
				}
				else
				{
					return;
				}
			}
			else
			{
				if (Solicitud == DatosSolicitados.TortasLL && mTortas == null)
				{
					if (mbPrimerIntento)
					{
						ArmarTortasDesdeCoordenadas();
					}
					return;
				}
				else
				{
					if (PosicionMapa < 0)
					{
						//					await UbicarCentroAsync();
						object[] Args = new object[7];
						Args[0] = PosicionMapa;
						Args[1] = '#' + Direccion; // mProyecto.LatCentro;
						Args[2] = mOrdenadaCentro;
						Args[3] = mAbscisaCentro;
						Args[4] = mNivelZoom;
						Args[5] = false;
						Args[6] = true;
						try
						{
							string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
							PosicionMapa = Int32.Parse(PosLocal);
							mbDibujarTortas = true;
							StateHasChanged();
							return;
						}
						catch (Exception ex)
						{
							CRutinas.DesplegarMsg(ex);
						}
					}
				}
				if (mbDibujarTortas)
				{
					mbDibujarTortas = false;
					await DibujarTortasAsync();
				}
			}
//			return base.OnAfterRenderAsync(firstRender);
		}

	}

	public class CLineaValorColor
	{
		public double Valor { get; set; }
		public string Texto { get; set; }
		public string Referencia { get; set; }
		public string Color { get; set; }
	}

}
