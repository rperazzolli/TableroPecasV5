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
using TableroPecasV5.Client.Clases;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaBingWSS : ComponentBase
	{
		public static ClaseElemento gClaseElemento;
		public static Int32 gCodigoElemento;
		public static Int32 gCodigoElementoDimension = -1;
		public static List<CColumnaBase> gColumnas;
		public static List<CLineaComprimida> gLineas;
		private static Int32 gCodigoPantalla = 0;

		public ClaseElemento ClaseIndicador { get; set; }
		public Int32 CodigoIndicador { get; set; }
		public Int32 CodigoElementoDimension { get; set; }
		public List<CColumnaBase> Columnas { get; set; }
		public List<CLineaComprimida> Lineas { get; set; }
		public bool Editando { get; set; }
		public List<CCapaWSSCN> CapasWSS { get; set; }
		public CLogicaDefinirCapaWSS DefinidorCapas { get; set; }
		public bool EstaLeyendo { get; set; } = false;

		private Int32 mCodigoPantalla;

		private CCapaComodin mCapaComodin = null;

		private string mNombre0;
		private Int32 mCodigo0;
		private bool mbPorNombre;
		private string mszPrmFecha;
		private List<CDatosPrmWFS> mListaPrm = new List<CDatosPrmWFS>();

		private async Task CargarSCSiCorrespondeAsync()
		{
			if (mCapa.Formula != null)
			{
				string[] Frm = mCapa.Formula.Split('@');
				for (Int32 i = 1; i < Frm.Length; i += 4)
				{
					if (Frm[i].Length > 0)
					{
						string Texto = "[" + Frm[i] + "]";
						if (Int32.TryParse(Frm[i + 1], out mCodigo0))
						{
							if (Frm[0].IndexOf(Texto) >= 0 && SubconsultaCodigo(mCodigo0) == null)
							{
								mNombre0 = Frm[i];
								mbPorNombre = (Frm[i + 2] == "Y");
								mszPrmFecha = (Frm[i + 3].Trim());
								await LeerSubconsultaAsync(mCodigo0);
								return;
							}
						}
					}
				}
			}

			mListaPrm.Clear();
			LeerParametrosWFSSiCorresponde();

		}

		private string UbicarProximoParametro(string Frm, ref Int32 PosIni)
		{
			Int32 Pos = Frm.IndexOf("[Prm-", PosIni);
			if (Pos > 0)
			{
				Pos += 5;
				Int32 Pos2 = Frm.IndexOf("]", Pos);
				if (Pos2 > 0)
				{
					PosIni = Pos2 + 1;
					return Frm.Substring(Pos, Pos2 - Pos);
				}
			}
			PosIni = Frm.Length + 1;
			return "";
		}

		public CColumnaBase ColumnaGeoreferencia { get; set; }
		public CVinculoIndicadorCompletoCN Vinculador { get; set; }
		public bool MostrarDialogoVinculador { get; set; } = false;
		public CLogicaMapaGradiente MapaGradiente { get; set; }

		public Int32 AnchoDisponible
		{
			get { return CContenedorDatos.AnchoPantalla - 45; }
		}

		public Int32 AltoDisponible
		{
			get { return CContenedorDatos.AltoPantalla - 45; }
		}

		private async Task VerificarVinculacionCompletaAsync()
		{
			ColumnaGeoreferencia = mProveedor.ColumnaNombre(mCapa.ColumnaGeoreferencia);
			if (ColumnaGeoreferencia == null)
			{
				throw new Exception("Falta columna " + mCapa.ColumnaGeoreferencia);
			}
			Vinculador = await CContenedorDatos.LeerVinculoAsync(Http, ClaseIndicador, CodigoIndicador,
					mCapa.ColumnaGeoreferencia);
			if (Vinculador == null)
			{
				throw new Exception("No encuentra vinculador");
			}

			MostrarDialogoVinculador = true;
			StateHasChanged();

		}

		public void FncRespuesta (CVinculoIndicadorCompletoCN VinculoDeterminado)
		{
			MostrarDialogoVinculador = false;
			StateHasChanged();
		}

		private bool ParametroYaLeido(string Nombre)
		{
			return (from P in mListaPrm
							where P.Parametro == Nombre
							select P).FirstOrDefault() != null;
		}

		private async Task LeerParametroWFSAsync(string NombrePrm)
		{
			RespuestaTextos Resp = await CContenedorDatos.LeerParametroWFSAsync(Http, mCapaWFS.Codigo, NombrePrm);
			if (!Resp.RespuestaOK)
			{
				CRutinas.DesplegarMsg(Resp.MsgErr);
			}
			else
			{
				CDatosPrmWFS Datos = new CDatosPrmWFS(NombrePrm);
				Datos.ParesValores = Resp.Contenidos;
				mListaPrm.Add(Datos);

				LeerParametrosWFSSiCorresponde();
			}
		}

		private async void LeerParametrosWFSSiCorresponde()
		{
			if (mCapa.Formula != null)
			{
				Int32 Pos = 0;
				string PrmEncontrado;
				while ((PrmEncontrado = UbicarProximoParametro(mCapa.Formula, ref Pos)) != "")
				{
					if (!ParametroYaLeido(PrmEncontrado))
					{
						await LeerParametroWFSAsync(PrmEncontrado);
						return;
					}
				}
			}
		}


			private List<CParametroExt> ArmarParametrosSC()
		{
			List<CParametroExt> Respuesta = new List<CParametroExt>();
			if (mszPrmFecha.Length > 0)
			{
				CParametroExt Prm = new CParametroExt();
				Prm.Nombre = mszPrmFecha;
				Prm.Tipo = "ftDateTime";
				Prm.TieneQuery = false;
				Prm.ValorDateTime = DateTime.Now;
				Respuesta.Add(Prm);
			}
			return Respuesta;
		}

		private async Task LeerSubconsultaAsync(Int32 Codigo, string GUID = "")
		{

			try
			{

				RespuestaDatasetBin Respuesta = await CContenedorDatos.LeerDetalleSubconsultaAsync(Http, Codigo,
						ArmarParametrosSC());

				if (!Respuesta.RespuestaOK)
				{
					throw new Exception(Respuesta.MsgErr);
				}
					
				CDatosSC Elemento = new CDatosSC(mNombre0, mCodigo0, Respuesta.Datos);
				if (mbPorNombre)
				{
					Elemento.CorregirReferencias(mCapaWFSVinculo);
				}

				mDatosSC.Add(Elemento);
				await CargarSCSiCorrespondeAsync();
			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}

		private CDatosSC SubconsultaCodigo(Int32 Codigo)
		{
			return (from SC in mDatosSC
							where SC.Codigo == Codigo
							select SC).FirstOrDefault();
		}

		private void ModificarFormula(CCapaWSSCN Capa)
		{
			string[] Frm = Capa.Formula.Split('@');
			if (Frm.Length > 0 && Frm[0].Length > 0)
			{
				mszFormula = Frm[0];
				foreach (CDatosSC Dato in mDatosSC)
				{
					string Str = "[" + Dato.Nombre + "]";
					mszFormula = mszFormula.Replace("[" + Dato.Nombre + "]",
							"[" + Dato.Codigo.ToString() + "]");
				}
			}
		}

		private CProveedorComprimido mProveedor = null;

		public CProveedorComprimido ProveedorDatos
		{
			get
			{
				if (mProveedor == null)
				{
					mProveedor = new CProveedorComprimido(ClaseIndicador, CodigoIndicador)
					{
						Columnas = Columnas,
						Datos = Lineas
					};
				}
				return mProveedor;
			}
		}

		private List<CDatosSC> mDatosSC;
		private string mszFormula = "";

		private void CrearCapaComodin()
		{
			mCapaComodin = new CCapaComodin();
			mCapaComodin.Opacidad = (mCapa.Intervalos == ClaseIntervalo.Indicador ? 0.5 : 1);
			mCapaComodin.CapaWFS = mCapaWFS;
			mCapaComodin.Clase = ClaseCapa.WFS;
			// Crea un valor por cada area. Hasta aca no considera la formula.
			mCapaComodin.Pares = mPosiciones.Values.ToList();

			mCapaComodin.AgregarFormulaWSS(mCapa, mDatosSC, mListaPrm);

		}

		public CDatoIndicador Indicador
		{
			get
			{
				return (CodigoIndicador < 0 ? null : CContenedorDatos.IndicadorDesdeCodigo(CodigoIndicador));
			}
		}

		public string Direccion
		{
			get { return "BING_CAPA_WSS_" + mCodigoPantalla.ToString(); }
		}

		private Int32 mCodigoCapaElegida = -1;
		private CCapaWSSCN mCapa = null;
		private CCapaWSSCN mCapaDibujada = null;

		public CCapaWSSCN CapaWSS
		{
			get { return mCapa; }
		}

		public bool HayCapa { get; set; } = false;

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
					HayCapa = (mCapa != null);
					StateHasChanged();
				}
			}
		}

		public void FncCerrarEditor ()
		{
			Editando = false;
			CapasWSS = DefinidorCapas.ListaCapas;
			StateHasChanged();
		}

		public void EditarCapas()
		{
			Editando = true;
			StateHasChanged();
		}

		protected override Task OnInitializedAsync()
		{
			Editando = false;
			mCodigoPantalla = gCodigoPantalla++;
			ClaseIndicador = gClaseElemento;
			CodigoIndicador = gCodigoElemento;
			CodigoElementoDimension = gCodigoElementoDimension;
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

		private double DeterminarRangoCoordenadas()
		{
			double SaltoMin = double.MaxValue;
			if (mCapaWFSVinculo.Lineas.Count != 0)
			{
				for (Int32 i = 0; i < mCapaWFS.Lineas.Count; i++)
				{
					for (Int32 ii = i + 1; ii < mCapaWFS.Lineas.Count; ii++)
					{
						SaltoMin = Math.Min(SaltoMin,
								CRutinas.DistanciaEntrePuntos2(mCapaWFS.Lineas[i].Centro, mCapaWFS.Lineas[ii].Centro));
					}
				}
			}

			if (mCapaWFS.Puntos.Count > 0)
			{
				for (Int32 i = 0; i < mCapaWFS.Puntos.Count; i++)
				{
					for (Int32 ii = i + 1; ii < mCapaWFS.Puntos.Count; ii++)
					{
						SaltoMin = Math.Min(SaltoMin,
								CRutinas.DistanciaEntrePuntos2(mCapaWFS.Puntos[i].Punto, mCapaWFS.Puntos[ii].Punto));
					}
				}
			}

			if (mCapaWFS.Areas.Count > 0)
			{
				for (Int32 i = 0; i < mCapaWFS.Areas.Count; i++)
				{
					for (Int32 ii = i + 1; ii < mCapaWFS.Puntos.Count; ii++)
					{
						SaltoMin = Math.Min(SaltoMin,
								CRutinas.DistanciaEntrePuntos2(mCapaWFS.Areas[i].Centro, mCapaWFS.Areas[ii].Centro));
					}
				}
			}

			SaltoMin = Math.Sqrt(SaltoMin) / 2;

			return Math.Min(SaltoMin, 0.0001);

		}

		private bool CumpleCondicionDistancia(CPosicionWFSCN Punto, Posicion Posicion)
		{
			double Distancia = (Punto.X - Posicion.Abscisa) * (Punto.X - Posicion.Abscisa) +
					(Punto.Y - Posicion.Ordenada) * (Punto.Y - Posicion.Ordenada);
			return Distancia <= mSaltoCoordenadas2;
		}

		private string UbicarElementoDesdePunto(Posicion Posicion)
		{
			if (mbPuntos)
			{
				foreach (CPuntoWFSCN Punto in mCapaWFS.Puntos)
				{
					if (CumpleCondicionDistancia(Punto.Punto, Posicion))
					{
						return Punto.Codigo;
					}
				}
				return "";
			}

			if (mbLineas) {
				foreach (CLineaWFSCN Linea in mCapaWFS.Lineas)
				{
					if (CumpleCondicionDistancia(Linea.Centro, Posicion))
					{
						return Linea.Codigo;
					}
				}
				return "";
			}

			foreach (CAreaWFSCN Area in mCapaWFS.Areas)
			{
				if (CRutinas.AreaContienePunto(Area, Posicion.APoint()))
				{
					return Area.Codigo;
				}
			}

			return "";

		}

		private double mSaltoCoordenadas2;
		private bool mbPuntos = false;
		private bool mbLineas = false;
		private Dictionary<string, CParValores> mPosiciones;
		private double mLatCentro;
		private double mLngCentro;
		private Int32 mNivelZoom = -1;
		private bool mbRedibujarDatos = false;

		private Int32 PosicionEnLista(double Valor, List<double> Lista)
		{
			for (Int32 i = 0; i < Lista.Count; i++)
			{
				if (Valor > Lista[i])
				{
					return i;
				}
			}
			return Lista.Count;
		}

		private bool mbValoresEnteros = false;
		private List<double> mReferencias;

		private void CrearReferenciasLineales()
		{
			if (mPosiciones.Values.Count > 0)
			{
				double Minimo = (from P in mPosiciones.Values
												 where !double.IsNaN(P.ValorElemento)
												 select P.ValorElemento).Min();
				double Maximo = (from P in mPosiciones.Values
												 where !double.IsNaN(P.ValorElemento)
												 select P.ValorElemento).Max();
				CRutinas.RedondearExtremos(ref Minimo, ref Maximo);

				mReferencias = new List<double>();
				for (Int32 i = 1; i < mCapa.Segmentos; i++)
				{
					mReferencias.Add(Minimo + i * (Maximo - Minimo) / mCapa.Segmentos);
				}

			}
		}

		private void CrearReferenciasCuantiles()
		{
			if (mPosiciones.Values.Count > 0)
			{
				List<double> Valores = (from P in mPosiciones.Values
																where !double.IsNaN(P.ValorElemento)
																orderby P.ValorElemento
																select P.ValorElemento).ToList();

				Int32 PosAnterior = 0;
				for (Int32 i = 1; i < mCapa.Segmentos; i++)
				{
					while (PosAnterior < (Valores.Count * i / mCapa.Segmentos))
					{
						PosAnterior++;
					}
					if (mCapa.Referencias.Count == 0 ||
							Valores[PosAnterior] != mCapa.Referencias[mCapa.Referencias.Count - 1])
					{
						mCapa.Referencias.Add(Valores[PosAnterior]);
					}
				}
			}
		}

		private void CrearReferenciasCortes()
		{
			if (mPosiciones.Values.Count > 0)
			{
				List<double> Valores = (from P in mPosiciones.Values
																where !double.IsNaN(P.ValorElemento)
																orderby P.ValorElemento
																select P.ValorElemento).ToList();
				mCapa.Referencias = Rutinas.CCortes.DeterminarRangosMinimizandoEMCGlobal(Valores, mCapa.Segmentos);
			}
		}

		private void CrearReferencias()
		{
				mbValoresEnteros = (from P in mPosiciones.Values
														where !double.IsNaN(P.ValorElemento) &&
																Math.Abs(P.ValorElemento - Math.Floor(P.ValorElemento)) > 0.00001
														select P).FirstOrDefault() == null;

			switch (mCapa.Intervalos)
			{
				case ClaseIntervalo.Lineal:
					CrearReferenciasLineales();
					break;
				case ClaseIntervalo.Cuantiles:
					CrearReferenciasCuantiles();
					break;
				case ClaseIntervalo.Cortes:
					CrearReferenciasCortes();
					break;
				case ClaseIntervalo.Manual:
					mReferencias = new List<double>();
					mReferencias.AddRange(mCapa.Referencias);
					break;
			}
		}

		private byte ModificarComponenteColor(byte Componente, double Fraccion)
		{
			return (byte)(Componente + Math.Floor((double)(255 - Componente) * (1 - Fraccion)));
		}

		private string ColorEscalonGradiente(Int32 Escalon)
		{
			double Fraccion = Math.Min(1, (double)Escalon / (double)(mCapa.Referencias.Count + 1));
			return ((Int32)mCapa.ColorCompuestoA*255).ToString() + ";" +
					ModificarComponenteColor(mCapa.ColorCompuestoR, Fraccion).ToString() + ";" +
					ModificarComponenteColor(mCapa.ColorCompuestoG, Fraccion).ToString() + ";" +
					ModificarComponenteColor(mCapa.ColorCompuestoB, Fraccion).ToString();
		}

		private void AjustarColoresPares()
		{
			if (mCapa.Intervalos == ClaseIntervalo.Indicador)
			{
				foreach (CParValores Par in mPosiciones.Values)
				{

					if (double.IsNaN(Par.ValorElemento))
					{
						Par.ColorElemento = ColorBandera.SinDatos;
					}
					else
					{
						if (mCapa.Sobresaliente > mCapa.Minimo)
						{
							if (Par.ValorElemento >= mCapa.Sobresaliente)
							{
								Par.ColorElemento = ColorBandera.Azul;
							}
							else
							{
								if (Par.ValorElemento >= mCapa.Satisfactorio)
								{
									Par.ColorElemento = ColorBandera.Verde;
								}
								else
								{
									if (Par.ValorElemento >= mCapa.Minimo)
									{
										Par.ColorElemento = ColorBandera.Amarillo;
									}
									else
									{
										Par.ColorElemento = ColorBandera.Rojo;
									}
								}
							}
						}
						else
						{
							if (Par.ValorElemento <= mCapa.Sobresaliente)
							{
								Par.ColorElemento = ColorBandera.Azul;
							}
							else
							{
								if (Par.ValorElemento <= mCapa.Satisfactorio)
								{
									Par.ColorElemento = ColorBandera.Verde;
								}
								else
								{
									if (Par.ValorElemento <= mCapa.Minimo)
									{
										Par.ColorElemento = ColorBandera.Amarillo;
									}
									else
									{
										Par.ColorElemento = ColorBandera.Rojo;
									}
								}
							}
						}
					}
				}
			}
			else
			{
				foreach (CParValores Par in mPosiciones.Values)
				{
					Int32 Pos = 0;
					foreach (double Valor in mCapa.Referencias)
					{
						if (Par.ValorElemento > Valor)
						{
							Pos++;
						}
						else
						{
							break;
						}
					}
					Par.ColorImpuesto = ColorEscalonGradiente(Pos);
				}
			}
		}

		private void UbicarEnLosElementos()
		{
			mPosiciones = new Dictionary<string, CParValores>();
			mbPuntos = mCapaWFS.Puntos.Count > 0;
			mbLineas = mCapaWFS.Lineas.Count > 0;
			double Salto = DeterminarRangoCoordenadas();
			mSaltoCoordenadas2 = Salto * Salto;
			foreach (Posicion Posicion in mValores.Keys)
			{
				List<double> Valores;
				if (mValores.TryGetValue(Posicion, out Valores))
				{
					string Elemento = UbicarElementoDesdePunto(Posicion);
					if (Elemento.Length > 0)
					{
						CParValores PosicionWSS;
						if (mPosiciones.TryGetValue(Elemento, out PosicionWSS))
						{
							PosicionWSS.ValorElemento += Valores[0];
							PosicionWSS.Cantidad += (Int32)Valores[1];
						}
						else
						{
							mPosiciones.Add(Elemento, new CParValores(Elemento)
							{
								Cantidad = (Int32)Valores[1],
								ColorElemento = ColorBandera.NoCorresponde,
								ColorImpuesto = "",
								ValorElemento = Valores[0]
							});
						}
					}
				}
			}

			// Ajusta cuando es media o cantidad.
			switch (mCapa.Agrupacion)
			{
				case ModoAgruparDependiente.Cantidad:
					foreach (CParValores ValDic in mPosiciones.Values)
					{
						ValDic.ValorElemento = ValDic.Cantidad;
					}
					break;
				case ModoAgruparDependiente.Media:
					foreach (CParValores ValDic in mPosiciones.Values)
					{
						ValDic.ValorElemento = (ValDic.Cantidad <= 0 ? double.NaN : (ValDic.ValorElemento / ValDic.Cantidad));
					}
					break;
			}
		}

		private void CentrarElMapa()
		{
			double LatMax = double.MinValue;
			double LatMin = double.MaxValue;
			double LngMax = double.MinValue;
			double LngMin = double.MaxValue;
			foreach (Posicion Posicion in mValores.Keys)
			{
				LatMin = Math.Min(LatMin, Posicion.Ordenada);
				LatMax = Math.Max(LatMax, Posicion.Ordenada);
				LngMin = Math.Min(LngMin, Posicion.Abscisa);
				LngMax = Math.Max(LngMax, Posicion.Abscisa);
			}

			if (LatMin > LatMax)
			{
				mLatCentro = 0;
				mLngCentro = 0;
				mNivelZoom = 1;
			}
			else
			{
				CRutinas.UbicarCentro(CContenedorDatos.AnchoPantalla - 40, CContenedorDatos.AltoPantalla - 60,
						LatMin, LatMax, LngMin, LngMax, out mLatCentro, out mLngCentro, out mNivelZoom);
			}
		}

		private async Task CargarDatosAsociadosCapaAsync()
		{
			try
			{
				if (mPosicionBingMaps >= 0)
				{
					await CRutinas.LimpiarContenidoMapaAsync(JSRuntime, mPosicionBingMaps);
				}

				if (mCapa == null)
				{
					StateHasChanged();
					return;
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

				mValores = new Dictionary<Posicion, List<double>>();

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

				CrearReferencias();

				AjustarColoresPares();

				CentrarElMapa();

				CrearCapaComodin();

				mbRedibujarDatos = true;

			  StateHasChanged();

			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}

		private Posicion UbicarPosicionEnCapaVinculo(string Codigo)
		{
			foreach (CPuntoWFSCN Punto in mCapaWFSVinculo.Puntos)
			{
				if (Punto.Codigo == Codigo)
				{
					return new Posicion(Punto.Punto);
				}
			}

			foreach (CLineaWFSCN Linea in mCapaWFSVinculo.Lineas)
			{
				if (Linea.Codigo == Codigo)
				{
					return new Posicion(Linea.Centro);
				}
			}

			foreach (CAreaWFSCN Area in mCapaWFSVinculo.Areas)
			{
				if (Area.Codigo == Codigo)
				{
					return new Posicion(Area.Centro);
				}
			}

			return null;

		}

		private Dictionary<Posicion, List<double>> mValores;

		private CCapaWFSCN mCapaWFSVinculo = null;

		private async Task DeterminarValoresPorVinculoAsync()
		{

			if (mVinculo.Vinculo.ClaseVinculada != ClaseVinculo.Coordenadas)
			{
				if (mCapaWFS != null && mCapaWFS.Codigo == mVinculo.Vinculo.CodigoVinculado)
				{
					mCapaWFSVinculo = mCapaWFS;
				}
				else
				{
					if (mCapaWFSVinculo == null || mCapaWFSVinculo.Codigo != mVinculo.Vinculo.CodigoVinculado)
					{
						RespuestaCapaWFS RespCapa = await CContenedorDatos.LeerCapaWFSAsync(
								Http, mVinculo.Vinculo.CodigoVinculado, false);
						if (RespCapa == null)
						{
							throw new Exception("No puede leer capa WFS");
						}
						else
						{
							mCapaWFSVinculo = RespCapa.Capa;
						}
					}
				}
			}

			CColumnaBase ColumnaVinculo = (from C in Columnas
																		 where C.Nombre.Equals(mCapa.ColumnaGeoreferencia, StringComparison.InvariantCultureIgnoreCase)
																		 select C).FirstOrDefault();
			CColumnaBase ColumnaValor = (from C in Columnas
																	 where C.Nombre == mCapa.ColumnaValor
																	 select C).FirstOrDefault();
			foreach (CLineaComprimida Linea in Lineas)
			{
				Posicion Posicion = null;
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
				ValorPunto = new List<double>();
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

		private CPosicionWFSCN UbicarPosicionCodigo(string Codigo)
		{
			if (mbPuntos)
			{
				return (from P in mCapaWFS.Puntos
								where P.Codigo == Codigo
								select P.Punto).FirstOrDefault();
			}
			if (mbLineas)
			{
				return (from P in mCapaWFS.Lineas
								where P.Codigo == Codigo
								select P.Centro).FirstOrDefault();
			}
			return null;
		}

		private string ColorImpuestoATexto(string Color)
		{
			List<Int32> Valores = CRutinas.ListaAEnteros(Color);
			return "#" + Valores[1].ToString("X2") + Valores[2].ToString("X2") +
				Valores[3].ToString("X2");
		}

		private string ColorParaPar(CParValores Par)
		{
			switch (mCapa.Intervalos)
			{
				case ClaseIntervalo.Indicador:
					return CRutinas.ColorBanderaATexto(Par.ColorElemento, true);
				default:
					return ColorImpuestoATexto(Par.ColorImpuesto);
			}
		}

		private async Task AgregarPushpinAsync(CPosicionWFSCN Posicion, CParValores Par)
		{
			object[] Args = new object[7];
			Args[0] = mPosicionBingMaps;
			Args[1] = Posicion.X;
			Args[2] = Posicion.Y;
			Args[3] = ColorParaPar(Par);
			Args[4] = Par.CodigoElemento;
			Args[5] = "";
			Args[6] = "";
			try
			{
				await JSRuntime.InvokeAsync<Task>("AgregarPushpin", Args);
			}
			catch (Exception ex)
			{
				Rutinas.CRutinas.DesplegarMsg(ex);
			}
		}

		private async Task RedibujarPushpinsAsync()
		{
			foreach (string Codigo in mPosiciones.Keys)
			{
				CPosicionWFSCN Posicion = UbicarPosicionCodigo(Codigo);
				if (Posicion != null)
				{
					CParValores Par;
					if (mPosiciones.TryGetValue(Codigo, out Par))
					{
						await AgregarPushpinAsync(Posicion, Par);
					}
				}
			}
		}

		private async Task AgregarAreaAsync(CAreaWFSCN Area, CParValores Par)
		{
			if (Area.Contorno != null && Area.Contorno.Count > 1)
			{
				double[] Abscisas = new double[Area.Contorno.Count + 1];
				double[] Ordenadas = new double[Area.Contorno.Count + 1];
				for (Int32 i = 0; i < Area.Contorno.Count; i++)
				{
					Abscisas[i] = Area.Contorno[i].X;
					Ordenadas[i] = Area.Contorno[i].Y;
				}
				Abscisas[Area.Contorno.Count] = Area.Contorno[0].X;
				Ordenadas[Area.Contorno.Count] = Area.Contorno[0].Y;
				object[] Args = new object[10];
				Args[0] = mPosicionBingMaps;
				Args[1] = Abscisas;
				Args[2] = Ordenadas;
				Args[3] = (Area.Centro == null ? -1000 : Area.Centro.X);
				Args[4] = (Area.Centro == null ? -1000 : Area.Centro.Y);
				Args[5] = CRutinas.ColorAclarado(Par.ColorElemento);
				Args[6] = Area.Nombre;
				Args[7] = Par.ValorElemento;
				Args[8] = "";
				Args[9] = 1;
				await JSRuntime.InvokeAsync<Task>("DibujarPoligono", Args);
			}
		}

		private async Task AgregarAreaOpacaAsync(CAreaWFSCN Area, CParValores Par)
		{
			if (Area.Contorno != null && Area.Contorno.Count > 1)
			{
				double[] Abscisas = new double[Area.Contorno.Count + 1];
				double[] Ordenadas = new double[Area.Contorno.Count + 1];
				for (Int32 i = 0; i < Area.Contorno.Count; i++)
				{
					Abscisas[i] = Area.Contorno[i].X;
					Ordenadas[i] = Area.Contorno[i].Y;
				}
				Abscisas[Area.Contorno.Count] = Area.Contorno[0].X;
				Ordenadas[Area.Contorno.Count] = Area.Contorno[0].Y;
				List<Int32> Colores = CRutinas.ListaAEnteros(Par.ColorImpuesto);
				object[] Args = new object[13];
				Args[0] = mPosicionBingMaps;
				Args[1] = Abscisas;
				Args[2] = Ordenadas;
				Args[3] = (Area.Centro == null ? -1000 : Area.Centro.X);
				Args[4] = (Area.Centro == null ? -1000 : Area.Centro.Y);
				Args[5] = Colores[0];
				Args[6] = Colores[1];
				Args[7] = Colores[2];
				Args[8] = Colores[3];
				Args[9] = Area.Nombre;
				Args[10] = Par.ValorElemento;
				Args[11] = "";
				Args[12] = 1;
				await JSRuntime.InvokeAsync<Task>("DibujarPoligonoOpaco", Args);
			}
		}

		private async Task RedibujarAreasAsync()
		{
			foreach (string Codigo in mPosiciones.Keys)
			{
				CAreaWFSCN Area = (from A in mCapaWFS.Areas
													 where A.Codigo == Codigo
													 select A).FirstOrDefault();
				if (Area != null)
				{
					CParValores Par;
					if (mPosiciones.TryGetValue(Codigo, out Par))
					{
						if (mCapa.Intervalos == ClaseIntervalo.Indicador)
						{
							await AgregarAreaAsync(Area, Par);
						}
						else
						{
							await AgregarAreaOpacaAsync(Area, Par);
						}
					}
				}
			}
		}

		private async Task RedibujarMapaAsync()
		{
			if (mbPuntos || mbLineas)
			{
				await RedibujarPushpinsAsync();
			}
			else
			{
				await RedibujarAreasAsync();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			try
			{
//				if (mCapa != mCapaDibujada)
//				{
//					mCapaDibujada = mCapa;
//					_ = CargarDatosAsociadosCapaAsync();
//				}

//				if (mPosicionBingMaps < 0 && mNivelZoom > 0)
//				{

//					object[] Args = new object[7];
//					Args[0] = mPosicionBingMaps;
//					Args[1] = '#' + Direccion; // mProyecto.LatCentro;
//					Args[2] = mLatCentro;
//					Args[3] = mLngCentro;
//					Args[4] = mNivelZoom;
//					Args[5] = false;
//					Args[6] = false;
//					try
//					{
//						string PosLocal = await JSRuntime.InvokeAsync<string>("loadMapRetPos", Args);
//						//gAlHacerViewChange = FncProcesarViewChange;
//						//gAlHacerClick = FncProcesarClick;
//						mPosicionBingMaps = Int32.Parse(PosLocal);
//					}
//					catch (Exception ex)
//					{
//						CRutinas.DesplegarMsg(ex);
//					}
//				}

//				if (mPosicionBingMaps >= 0 && mbRedibujarDatos)
//				{
//					mbRedibujarDatos = false;
//					HayCapa = true;
//					StateHasChanged();
////					await RedibujarMapaAsync();
//				}

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

		public Plantillas.Point APoint()
		{
			return new Plantillas.Point(Abscisa, Ordenada);
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

		public override int GetHashCode()
		{
			return (Int32)Math.Floor(Abscisa + Ordenada);
		}

		bool IEquatable<Posicion>.Equals(Posicion Otro)
		{
			return (Otro.Abscisa == Abscisa && Otro.Ordenada == Ordenada);
		}
	}

	public class PosicionWSS
	{
		public Posicion Posicion { get; set; }
		public double Valor { get; set; }
		public double Auxiliar { get; set; }

	}
}
