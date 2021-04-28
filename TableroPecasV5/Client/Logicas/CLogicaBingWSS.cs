using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Componentes;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaBingWSS : ComponentBase
	{
		public static ClaseElemento gClaseElemento;
		public static Int32 gCodigoElemento;
		public static List<CColumnaBase> gColumnas;
		public static List<CLineaComprimida> gLineas;
		private static Int32 gCodigoPantalla = 0;

		public ClaseElemento ClaseIndicador { get; set; }
		public Int32 CodigoIndicador { get; set; }
		public List<CColumnaBase> Columnas { get; set; }
		public List<CLineaComprimida> Lineas { get; set; }
		public bool Editando { get; set; } = false;
		public List<CCapaWSSCN> CapasWSS { get; set; }
		public CLogicaDefinirCapaWSS DefinidorCapas { get; set; }

		private Int32 mCodigoPantalla;

		public string Direccion
		{
			get { return "BING_CAPA_WSS_" + mCodigoPantalla.ToString(); }
		}

		private Int32 mCodigoCapaElegida = -1;
		private CCapaWSSCN mCapa = null;
		private CCapaWSSCN mCapaDibujada = null;

		public Int32 CapaElegida
		{
			get { return mCodigoCapaElegida; }
			set
			{
				if (mCodigoCapaElegida != value)
				{
					mCodigoCapaElegida = value;
					mCapa = (from C in CapasWSS
									 where C.Codigo == mCodigoCapaElegida
									 select C).FirstOrDefault();
					StateHasChanged();
				}
			}
		}

		public void EditarCapas()
		{
			Editando = true;
			StateHasChanged();
		}

		protected override Task OnInitializedAsync()
		{
			mCodigoPantalla = gCodigoPantalla++;
			ClaseIndicador = gClaseElemento;
			CodigoIndicador = gCodigoElemento;
			Columnas = gColumnas;
			Lineas = gLineas;
			return base.OnInitializedAsync();
		}

		private Int32 mPosicionBingMaps = -1;

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Inject]
		public HttpClient Http { get; set; }

		private CCapaWFSCN mCapaWFS = null;
		private CVinculoIndicadorCompletoCN mVinculo = null;

		private async Task CargarDatosAsociadosCapaAsync()
		{
			if (mPosicionBingMaps >= 0)
			{
				await CRutinas.LimpiarContenidoMapaAsync(JSRuntime, mPosicionBingMaps);
			}

			if (mCapaWFS == null || mCapaWFS.Codigo != mCapa.CapaWFS)
			{
				RespuestaCapaWFS RespCapa = await CContenedorDatos.LeerCapaWFSAsync(Http, mCapa.CapaWFS, false);
				if (RespCapa != null && RespCapa.RespuestaOK)
				{
					mCapaWFS = RespCapa.Capa;
				}
			}

			if (mCapa.Modo == ModoGeoreferenciar.Vinculo &&
				(mVinculo == null || mVinculo.Vinculo.Codigo != mCapa.Vinculo))
			{
				mVinculo = await CContenedorDatos.LeerVinculoAsync(Http, ClaseIndicador, CodigoIndicador, mCapa.ColumnaGeoreferencia);
			}

			switch (mVinculo.Vinculo.ClaseVinculada)
			{
				case ClaseVinculo.ColumnasGIS:
					DeterminarValoresPorCoordenadas();
					break;
				default:
					await DeterminarValoresPorVinculoAsync();
					break;
			}

			UbicarEnLosElementos();

			CentrarElMapa();

			DibujarElMapa();

			StateHasChanged();

		}

		private Posicion UbicarPosicionEnCapaVinculo(string Codigo)
		{
			foreach (CPuntoWFSCN Punto in mCapaVinculo.Puntos)
			{
				if (Punto.Codigo == Codigo)
				{
					return new Posicion(Punto.Punto);
				}
			}

			foreach (CLineaWFSCN Linea in mCapaVinculo.Lineas)
			{
				if (Linea.Codigo == Codigo)
				{
					return new Posicion(Linea.Centro.);
				}
			}

			foreach (CAreaWFSCN Area in mCapaVinculo.Areas)
			{
				if (Area.Codigo == Codigo)
				{
					return new Posicion(Area.Centro.);
				}
			}

			return null;

		}

		private Dictionary<Posicion,List<double>> mValores;

		private CCapaWFSCN mCapaVinculo = null;

		private async Task DeterminarValoresPorVinculoAsync()
		{

			if (mVinculo.Vinculo.ClaseVinculada != ClaseVinculo.Coordenadas)
			{
				if (mCapaWFS != null && mCapaWFS.Codigo == mVinculo.Vinculo.CodigoVinculado)
				{
					mCapaVinculo = mCapaWFS;
				}
				else
				{
					if (mCapaVinculo == null || mCapaVinculo.Codigo != mVinculo.Vinculo.CodigoVinculado)
					{
						RespuestaCapaWFS RespCapa = await CContenedorDatos.LeerCapaWFSAsync(
								Http, mVinculo.Vinculo.CodigoVinculado, false);
						if (RespCapa == null)
						{
							throw new Exception("No puede leer capa WFS");
						}
						else
						{
							mCapaVinculo = RespCapa.Capa;
						}
					}
				}
			}

			CColumnaBase ColumnaVinculo = (CColumnaReal)(from C in Columnas
																								where C.Nombre == mCapa.ColumnaGeoreferencia
																								select C).FirstOrDefault();
			CColumnaBase ColumnaValor = (CColumnaReal)(from C in Columnas
																								 where C.Nombre == mCapa.ColumnaValor
																								 select C).FirstOrDefault();
			foreach (CLineaComprimida Linea in Lineas)
			{
				Posicion Posicion=null;
				string Texto = ColumnaVinculo.ListaValores[Linea.Codigos[ColumnaVinculo.Orden]];
				CVinculoDetalleCN Detalle = (from D in mVinculo.Detalles
																		 where D.ValorAsociado == Texto
																		 select D).FirstOrDefault();
				if (Detalle != null)
				{
					if (mVinculo.Vinculo.ClaseVinculada == ClaseVinculo.Coordenadas)
					{
						Int32 Pos = Detalle.Posicion.IndexOf(" ");
						Posicion = new Posicion()
						{
							Abscisa = CRutinas.StrVFloat(Detalle.Posicion.Substring(0, Pos)),
							Ordenada = CRutinas.StrVFloat(Detalle.Posicion.Substring(Pos + 1))
						};
					}
					else
					{
						Posicion = UbicarPosicionEnCapaVinculo(Detalle.Posicion);
					}
				}
				if (Posicion != null)
				{
					SumarPunto(Posicion, ColumnaValor.ValorReal(Linea.Codigos[ColumnaValor.Orden], true));
				}
			}
		}

		private void SumarPunto(Posicion Posicion, double Valor)
		{
			List<double> ValorPunto;

			if (mValores.TryGetValue(Posicion, out ValorPunto))
			{
				ValorPunto[0] += Valor;
				ValorPunto[1]++;
			}
			else
			{
				ValorPunto.Add(Valor);
				ValorPunto.Add(1);
				mValores.Add(Posicion, ValorPunto);
			}

		}

		private void DeterminarValoresPorCoordenadas()
		{
			CColumnaReal ColumnaAbsc = (CColumnaReal)(from C in Columnas
																								where C.Nombre == mCapa.ColumnaLongitud
																								select C).FirstOrDefault();
			CColumnaReal ColumnaOrd = (CColumnaReal)(from C in Columnas
																							 where C.Nombre == mCapa.ColumnaLatitud
																							 select C).FirstOrDefault();
			CColumnaBase ColumnaValor = (CColumnaReal)(from C in Columnas
																								 where C.Nombre == mCapa.ColumnaValor
																								 select C).FirstOrDefault();

			mValores = new Dictionary<Posicion, List<double>>();

			if (ColumnaAbsc != null && ColumnaOrd != null && ColumnaValor != null) {
				foreach (CLineaComprimida Linea in Lineas)
				{
					Posicion Posicion = new Posicion()
					{
						Abscisa = ColumnaAbsc.ValorReal(Linea.Codigos[ColumnaAbsc.Orden]),
						Ordenada = ColumnaOrd.ValorReal(Linea.Codigos[ColumnaOrd.Orden])
					};

					SumarPunto(Posicion, ColumnaValor.ValorReal(Linea.Codigos[ColumnaValor.Orden], true));

				}
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			try
			{
				if (mCapa != mCapaDibujada)
				{
					mCapaDibujada = mCapa;
					_ = CargarDatosAsociadosCapaAsync();
				}

				if (mPosicionBingMaps < 0)
				{

					object[] Args = new object[7];
					Args[0] = mPosicionBingMaps;
					Args[1] = '#' + Direccion; // mProyecto.LatCentro;
					Args[2] = mProyecto.LatCentro;
					Args[3] = mProyecto.LngCentro;
					Args[4] = mProyecto.NivelZoom;
					Args[5] = false;
					Args[6] = false;
					try
					{
						string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
						//gAlHacerViewChange = FncProcesarViewChange;
						//gAlHacerClick = FncProcesarClick;
						mPosicionBingMaps = Int32.Parse(PosLocal);
						await mProyecto.DibujarAsync(JSRuntime, mPosicionBingMap);
					}
					catch (Exception ex)
					{
						CRutinas.DesplegarMsg(ex);
					}
				}
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
			await base.OnAfterRenderAsync(firstRender);
		}
	}

	public class Posicion : IComparable , IEquatable<Posicion>
	{
		public double Abscisa { get; set; }
		public double Ordenada { get; set; }
		public Posicion()
		{
			//
		}

		public Posicion(CPosicionWFSCN PosRefe)
		{
			Abscisa = PosRefe.X;
			Ordenada = PosRefe.Y;
		}

		int IComparable.CompareTo(object obj)
		{
			if (obj is Posicion Otro)
			{
				return (Abscisa != Otro.Abscisa ? Abscisa.CompareTo(Otro.Abscisa) : Ordenada.CompareTo(Otro.Ordenada));
			}
			else
			{
				throw new Exception("No es ValorPosicion");
			}
		}

		bool IEquatable<Posicion>.Equals(Posicion Otro)
		{
			return (Otro.Abscisa == Abscisa && Otro.Ordenada == Ordenada);
		}
	}
}
