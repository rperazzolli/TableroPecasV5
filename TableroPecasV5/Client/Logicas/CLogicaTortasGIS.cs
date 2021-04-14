using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TableroPecasV5.Shared;
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

		public CLogicaVinculadorCoordenadas VinculadorCoordenadas { get; set; }

		public void RecibirVinculos(CVinculoIndicadorCompletoCN Vinculo)
		{
			if (VinculadorCoordenadas != null)
			{
				VinculadorCoordenadas.LiberarMapa();
			}

			if (Vinculo != null && Vinculo.Detalles.Count > 0)
			{
				mVinculador = Vinculo;
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

		private async Task UbicarCentroAsync()
		{
			double LatMin = double.MaxValue;
			double LatMax = double.MinValue;
			double LngMin = double.MaxValue;
			double LngMax = double.MinValue;
			switch (Solicitud)
			{
				case DatosSolicitados.TortasManual:
					if (PosicionesManuales!=null && PosicionesManuales.Count > 0)
					{
						foreach (CListaPosicion Elemento in PosicionesManuales)
						{
							LatMin = Math.Min(Elemento.Lat, LatMin);
							LatMax = Math.Max(Elemento.Lat, LatMax);
							LngMin = Math.Min(Elemento.Lng, LngMin);
							LngMax = Math.Max(Elemento.Lng, LngMax);
						}
					}
					break;
			}
			if (LatMin > LatMax)
			{
				mAbscisaCentro = -68;
				mOrdenadaCentro = -39;
				mNivelZoom = 7;
			}
			else
			{
				mAbscisaCentro = (LngMax + LngMin) / 2;
				mOrdenadaCentro = (LatMax + LatMin) / 2;
				CPosicionWFSCN Dimensiones = await CRutinas.ObtenerDimensionesPantallaAsync(JSRuntime, Direccion);
				mNivelZoom = CRutinas.UbicarNivelZoom(Dimensiones.X, Dimensiones.Y, LngMax - LngMin, LatMax - LatMin);
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
					await UbicarCentroAsync();
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
					}
					catch (Exception ex)
					{
						CRutinas.DesplegarMsg(ex);
					}
				}
			}
//			return base.OnAfterRenderAsync(firstRender);
		}

	}

}
