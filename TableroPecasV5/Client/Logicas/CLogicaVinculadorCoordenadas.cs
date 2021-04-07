using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaVinculadorCoordenadas	: ComponentBase
	{

		[Parameter]
		public Int32 Indicador { get; set; }

		[Parameter]
		public Datos.CColumnaBase ColumnaVinculo { get; set; }

		[Parameter]
		public ClaseElemento ClaseIndicador { get; set; } = ClaseElemento.Indicador;

		public string Direccion { get { return "Cont_Vinc_Coord;"; } }

		public CVinculoIndicadorCompletoCN Vinculador { get; set; } = null;
		public List<CListaPosicion> ListaElementos { get; set; } = null;

		private CListaPosicion mElementoSeleccionado = null;

		public string EstiloLinea(CListaPosicion Elemento)
		{
			return "width: 100%; height: 25px; background: " +
					(mElementoSeleccionado == Elemento ? "yellow" : "white") + "; color: " +
					(Elemento.Lat > -999 ? "blue;" : "red;");
		}

		private Int32 mPosicionBingMap = -1;

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		private const string COLOR_SELECCIONADO = "red";
		private const string COLOR_NO = "lightgray";

		private async Task AgregarPushpinAsync(CListaPosicion Elemento)
		{
			object[] Args = new object[7];
			Args[0] = mPosicionBingMap;
			Args[1] = mElementoSeleccionado.Lng;
			Args[2] = mElementoSeleccionado.Lat;
			Args[3] = mElementoSeleccionado.Color;
			Args[4] = mElementoSeleccionado.Descripcion;
			Args[5] = "";
			Args[6] = mElementoSeleccionado.Referencia;
			await JSRuntime.InvokeAsync<string>("AgregarPushpin", Args);
		}

		private async Task<bool> EliminarPushpinAsync(CListaPosicion Elemento)
		{
			object[] Args = new object[2];
			Args[0] = mPosicionBingMap;
			Args[1] = mElementoSeleccionado.Referencia;
			try
			{
				string PosLocal = await JSRuntime.InvokeAsync<string>("EliminarPushpin", Args);
				return (PosLocal.Length == 0);
			}
			catch (Exception)
			{
				return false;
			}
		}

		private async Task AjustarColoresPushpinsAsync(bool PonerEnRojo)
		{
			if (mPosicionBingMap >= 0 && mElementoSeleccionado != null)
			{
				if (await EliminarPushpinAsync(mElementoSeleccionado))
				{
					{
						// borró, entonces agrega con color correspondiente.
						mElementoSeleccionado.Color = (PonerEnRojo ? COLOR_SELECCIONADO : COLOR_NO);
						await AgregarPushpinAsync(mElementoSeleccionado);
					}
				}
			}
		}

		public async void SeleccionarVinculo(CListaPosicion Elemento)
		{
			await AjustarColoresPushpinsAsync(false);
			Elemento.Color = COLOR_SELECCIONADO;
			mElementoSeleccionado = Elemento;
			await AjustarColoresPushpinsAsync(true);
			StateHasChanged();
		}

		public async void AlHacerClick(double Lat, double Lng)
		{
			if (mElementoSeleccionado != null)
			{
				await EliminarPushpinAsync(mElementoSeleccionado);
				mElementoSeleccionado.Color = COLOR_SELECCIONADO;
				await AgregarPushpinAsync(mElementoSeleccionado);
			}
		}

		protected override Task OnInitializedAsync()
		{
			CLogicaBingMaps.gAlHacerClick = AlHacerClick;
			return base.OnInitializedAsync();
		}

		[Inject]
		public HttpClient Http { get; set; }

		private async Task LeerVinculosAsync()
		{
			Vinculador = await CContenedorDatos.LeerVinculosIndicadorAsync(Http,
				  ClaseIndicador, Indicador, ColumnaVinculo.Nombre);
			if (Vinculador != null)
			{
				StateHasChanged();
			}
		}

		private bool mbReubicar = true;

		private Int32 UbicarCentro(double Ancho, double Alto, out double LatCentro, out double LngCentro)
		{

			if (Vinculador.Detalles.Count == 0)
			{
				LatCentro = -38.92;
				LngCentro = -68.05;
				return 10;
			}
			double LatMin = double.MaxValue;
			double LatMax = double.MinValue;
			double LngMin = double.MaxValue;
			double LngMax = double.MinValue;

			foreach (CVinculoDetalleCN Punto in Vinculador.Detalles)
			{
				CRutinas.ExtraerCoordenadasPosicion(Punto.Posicion, out double Lng, out double Lat);
				if (Lat>-999 && Lng > -999)
				{
					LatMin = Math.Min(LatMin, Lat);
					LatMax = Math.Max(LatMax, Lat);
					LngMin = Math.Min(LngMin, Lng);
					LngMax = Math.Max(LngMax, Lng);
				}
			}

			LatCentro = (LatMin + LatMax) / 2;
			LngCentro = (LngMin + LngMax) / 2;

			double Relacion1 = (LatMax - LatMin) * 650 / Alto;
			double Relacion2 = (LngMax - LngMin) * 1280 / Ancho;
			double Salto = Math.Max(Relacion1, Relacion2);
			if (Salto == 0)
			{
				return 10;
			}
			else
			{
				Salto *= Math.Pow(2, 7);
				for (Int32 i = 15; i > 1; i--)
				{
					if (Salto < 1.5)
					{
						return i;
					}
					Salto /= 2;
				}
				return 1;
			}

		}

		private Int32 mNivelZoom;
		private double mLatCentro;
		private double mLngCentro;

		private void CrearListaElementos()
		{
			ListaElementos = new List<CListaPosicion>();
			foreach (string Valor in ColumnaVinculo.ListaValores)
			{
				CListaPosicion Elemento = new CListaPosicion()
				{
					Codigo = -1,
					Color = COLOR_NO,
					Descripcion = Valor,
					Lat = -1000,
					Lng = -1000,
					Referencia = Valor
				};
				CVinculoDetalleCN Vinculo = (from V in Vinculador.Detalles
																		 where V.ValorAsociado == Valor
																		 select V).FirstOrDefault();
				if (Vinculo != null)
				{
					Elemento.Codigo = Vinculo.Codigo;
					CRutinas.ExtraerCoordenadasPosicion(Vinculo.Posicion, out double Lng, out double Lat);
					Elemento.Lat = Lat;
					Elemento.Lng = Lng;
				}
				ListaElementos.Add(Elemento);
			}
		}

		private async Task DibujarPushpinsAsync()
		{
			foreach (CListaPosicion Elemento in ListaElementos)
			{
				if (Elemento.Lat > -999)
				{
					await AgregarPushpinAsync(Elemento);
				}
			}
			StateHasChanged();
		}

		public void Registrar()
		{
			//
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			try
			{
				if (Vinculador == null)
				{
					_ = LeerVinculosAsync();
				}
				else
				{
					if (mbReubicar)
					{
						mNivelZoom = UbicarCentro(0.8 * Contenedores.CContenedorDatos.AnchoPantalla,
							0.5 * Contenedores.CContenedorDatos.AltoPantalla, out mLatCentro, out mLngCentro);
						mbReubicar = false;
					}
				}

				if (mPosicionBingMap < 0)
				{
					object[] Args = new object[7];
					Args[0] = mPosicionBingMap;
					Args[1] = '#' + Direccion; // mProyecto.LatCentro;
					Args[2] = mLatCentro;
					Args[3] = mLngCentro;
					Args[4] = mNivelZoom;
					Args[5] = false;
					Args[6] = true;
					try
					{
						string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
						mPosicionBingMap = Int32.Parse(PosLocal);
						CrearListaElementos();
						_ = DibujarPushpinsAsync();
						//					await mProyecto.DibujarAsync(JSRuntime, mPosicionBingMap);
					}
					catch (Exception ex)
					{
						CRutinas.DesplegarMsg(ex);
					}
				}
				//foreach (CPuntoTextoColor Punto in mPuntos)
				//{
				//	await AgregarPushPinAsync(Punto);
				//}
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}


	}
}
