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

		private bool mbDibujarTortas = false;
		private List<CLineaValorColor> mListaColores;

		private const string REFERENCIA_OTROS = "$$%%";

		private void AgregarValoresGajoTorta(CGajoTorta Gajo)
		{
			if (Gajo.Referencia != REFERENCIA_OTROS)
			{
				CLineaValorColor Linea = (from L in mListaColores
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
					mListaColores.Add(Linea);
				}

				Linea.Valor += Math.Abs(Gajo.Valor);

			}
		}

		private void ImponerColor(CGajoTorta Gajo)
		{
			if (Gajo.Referencia != REFERENCIA_OTROS)
			{
				CLineaValorColor Linea = (from L in mListaColores
																	where L.Referencia == Gajo.Referencia
																	select L).FirstOrDefault();
				if (Linea != null)
				{
					Gajo.Color = Linea.Color;
				}
			}
		}

		private async Task DeterminarTortasAsync()
		{
			if (mTortas != null)
			{
				await CRutinas.LimpiarContenidoMapaAsync(JSRuntime, PosicionMapa);
			}
			if (mVinculador != null && mVinculador.Detalles.Count > 0)
			{
				double Acumulado = ObtenerAcumulado(Lineas);
				bool TodosCeros = Acumulado == 0;
				mTortas = new List<CTortaBing>();
				foreach (CVinculoDetalleCN Detalle in mVinculador.Detalles)
				{
					List<CGajoTorta> Gajos = ExtraerGajos(Detalle.ValorAsociado, TodosCeros, out double AcumTorta);
					mTortas.Add(new CTortaBing()
					{
						CodigoTorta = Detalle.ValorAsociado,
						Centro = ExtraerCentro(Detalle.Posicion),
						Gajos = Gajos,
						Lado = 0,
						Acumulado = Acumulado
					});
				}

				mTortas = (from T in mTortas
									 where T.Gajos.Count > 0 && T.Centro.X > -999
									 select T).ToList();

				if (mTortas.Count > 0)
				{
					double Maximo = (from T in mTortas
													 select T.Acumulado).Max();
					double LadoMaximo = Math.Sqrt((double)(CRutinas.AnchoPantalla - 40) * (double)(CRutinas.AltoPantalla - 40) /
							(1.5 * mTortas.Count));
					DeterminarRangoCoordenadas(out double LngMin, out double LngMax,
							out double LatMin, out double LatMax, out double LadoMax);

					foreach (CTortaBing Torta in mTortas)
					{
						Torta.Lado = Torta.Acumulado * LadoMax / Maximo;
					}

					mAbscisaCentro = (LngMax + LngMin) / 2;
					mOrdenadaCentro = (LatMax + LatMin) / 2;
					mNivelZoom = CRutinas.UbicarNivelZoom(CRutinas.AnchoPantalla - 40, CRutinas.AltoPantalla - 40,
							LngMax - LngMin, LatMax - LatMin);

				}
			}

			// Imponer colores.
			mListaColores = new List<CLineaValorColor>();
			foreach (CTortaBing Torta in mTortas)
			{
				foreach (CGajoTorta Gajo in Torta.Gajos)
				{
					AgregarValoresGajoTorta(Gajo);
				}
			}

			mListaColores.Sort(delegate (CLineaValorColor L1, CLineaValorColor L2)
			{
				return L2.Valor.CompareTo(L1.Valor);
			});

			Int32 Pos = 0;
			foreach (CLineaValorColor Linea in mListaColores)
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

		public async void RecibirVinculos(CVinculoIndicadorCompletoCN Vinculo)
		{
			if (VinculadorCoordenadas != null)
			{
				VinculadorCoordenadas.LiberarMapa();
			}

			if (Vinculo != null && Vinculo.Detalles.Count > 0)
			{
				mVinculador = Vinculo;
				MostrarVinculadorManual = false;
				await DeterminarTortasAsync();
				StateHasChanged();
			}
			else
			{
				Navegador.NavigateTo("Indicadores");
			}
		}

		private CVinculoIndicadorCompletoCN mVinculador = null;

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

			}

		}

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

		private async Task DibujarTortasAsync()
		{
			if (mTortas != null)
			{
				Rectangulo RectPant = await CRutinas.ObtenerRectanguloElementoAsync(JSRuntime, Direccion);
				Rectangulo RectMapa = await CRutinas.ObtenerExtremosMapaAsync(JSRuntime, PosicionMapa);
				double FactorAbsc = (RectPant.height * RectMapa.width) / (RectPant.width * RectMapa.height);
				mbDibujarTortas = false;
				foreach (CTortaBing Torta in mTortas)
				{
					await Torta.GraficarSobreMapaAsync(JSRuntime, PosicionMapa, FactorAbsc);
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
					if (!MostrarVinculadorManual)
					{
						MostrarVinculadorManual = true;
						StateHasChanged();
					}
				}
				else
				{
					return;
				}
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
