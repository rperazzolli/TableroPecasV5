using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Datos;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaDefinirCapaWSS : ComponentBase
	{

		public CLogicaDefinirCapaWSS()
		{
			if (Columnas != null)
			{
				return;
			}
		}

		public delegate void FncCerrar();

		[Parameter]
		public FncCerrar AlCerrar { get; set; } = null;

		[Parameter]
		public List<CColumnaBase> Columnas { get; set; } = null;

		[Parameter]
		public ClaseElemento ClaseIndicador { get; set; } = ClaseElemento.NoDefinida;

		[Parameter]
		public Int32 Indicador { get; set; } = -1;

		[Parameter]
		public List<CCapaWSSCN> ListaCapas { get; set; } = null;

		public bool HayMensaje { get; set; } = false;
		public bool HayBoton { get; set; } = false;
		public string Mensaje { get; set; } = "";

		public void CerrarMsg()
		{
			HayMensaje = false;
			if (AlCerrar != null)
			{
				AlCerrar();
			}
		}

		public Int32 ModoCalculo
		{
			get { return (Int32)mCapaSeleccionada.Agrupacion; }
			set
			{
				mCapaSeleccionada.Agrupacion = (ModoAgruparDependiente)value;
			}
		}

		public void SeleccionarWSS(CCapaWSSCN Capa)
		{
			CapaSeleccionada = Capa;
			StateHasChanged();
		}

		public void Cerrar()
		{
			if (AlCerrar != null)
			{
				AlCerrar();
			}
		}

		public string EstiloCapa(CCapaWSSCN Capa)
		{
			return "width: 90%; height: 25px; padding: 0px; background: " +
				  (Capa == mCapaSeleccionada ? "yellow;" : "white;");
		}

		private CCapaWSSCN mCapaSeleccionada = new CCapaWSSCN();
		public CCapaWSSCN CapaSeleccionada
		{
			get { return mCapaSeleccionada; }
			set
			{
				if (mCapaSeleccionada != value)
				{
					mCapaSeleccionada = value;
					if (mCapaSeleccionada == null)
					{
						ColorSeleccionado = "";
					}
					else
					{
						ColorSeleccionado =  "#" + mCapaSeleccionada.ColorCompuestoR.ToString("X2") +
							mCapaSeleccionada.ColorCompuestoG.ToString("X2") +
				      mCapaSeleccionada.ColorCompuestoB.ToString("X2");

					}
					NoSeleccionado = (mCapaSeleccionada == null);
				}
			}
		}

		public Int32 CodigoCapa
		{
			get { return (CapaSeleccionada == null ? -1 : CapaSeleccionada.Codigo); }
		}

		public List<CListaTexto> ListaCalculo { get; set; } = new List<CListaTexto>();
		public List<CListaTexto> ListaIntervalos { get; set; } = new List<CListaTexto>();
		public List<CListaTexto> ListaVinculos { get; set; } = null;
		public List<CCapaWFSCN> ListaCapasWFS { get; set; } = null;

		private void CrearLineasCalculo()
		{
			ListaCalculo.Add(new CListaTexto()
			{
				Codigo = (Int32)ModoAgruparDependiente.NoDefinido,
				Descripcion = "No definido"
			});
			ListaCalculo.Add(new CListaTexto()
			{
				Codigo = (Int32)ModoAgruparDependiente.Acumulado,
				Descripcion = "Acumulado"
			});
			ListaCalculo.Add(new CListaTexto()
			{
				Codigo = (Int32)ModoAgruparDependiente.Cantidad,
				Descripcion = "Cantidad"
			});
			ListaCalculo.Add(new CListaTexto()
			{
				Codigo = (Int32)ModoAgruparDependiente.Media,
				Descripcion = "Media"
			});

			ListaIntervalos.Add(new CListaTexto()
			{
				Codigo = -1,
				Descripcion = "No definido"
			});
			ListaIntervalos.Add(new CListaTexto()
			{
				Codigo = (Int32)ClaseIntervalo.Indicador,
				Descripcion = "Indicador"
			});
			ListaIntervalos.Add(new CListaTexto()
			{
				Codigo = (Int32)ClaseIntervalo.Lineal,
				Descripcion = "Lineal"
			});
			ListaIntervalos.Add(new CListaTexto()
			{
				Codigo = (Int32)ClaseIntervalo.Cuantiles,
				Descripcion = "Cuantiles"
			});
			ListaIntervalos.Add(new CListaTexto()
			{
				Codigo = (Int32)ClaseIntervalo.Manual,
				Descripcion = "Manual"
			});

		}

		public double Minimo
		{
			get { return mCapaSeleccionada.Minimo; }
			set
			{
				mCapaSeleccionada.Minimo = value;
			}
		}

		public double Satisfactorio
		{
			get { return mCapaSeleccionada.Satisfactorio; }
			set
			{
				mCapaSeleccionada.Satisfactorio = value;
			}
		}

    public double Sobresaliente
		{
			get { return mCapaSeleccionada.Sobresaliente; }
			set
			{
				mCapaSeleccionada.Sobresaliente = value;
			}
		}
		public double Rango
		{
			get { return mCapaSeleccionada.Rango; }
			set
			{
				mCapaSeleccionada.Rango = value;
			}
		}

		public string Segmentos { get; set; } = "";

		public string ColorSeleccionado { get; set; } = "";
		public bool UsaVinculo { get; set; } = false;
		public bool UsaCoordenadas { get; set; } = false;
		public Int32 CodigoVinculo
		{
			get { return mCapaSeleccionada.Vinculo; }
			set
			{
				mCapaSeleccionada.Vinculo = value;
			}
		}

		public string ColumnaValor
		{
			get { return mCapaSeleccionada.ColumnaValor; }
			set
			{
				mCapaSeleccionada.ColumnaValor = value;
			}
		}

		public string ColumnaLat
		{
			get { return mCapaSeleccionada.ColumnaLatitud; }
			set
			{
				mCapaSeleccionada.ColumnaLatitud = value;
			}
		}

		public string ColumnaLng
		{
			get { return mCapaSeleccionada.ColumnaLongitud; }
			set
			{
				mCapaSeleccionada.ColumnaLongitud = value;
			}
		}

		public string EstiloInput
		{
			get
			{
				return "height: 25px; margin-top: 0px;";
			}
		}

		public string NombreCapa
		{
			get { return mCapaSeleccionada.Nombre; }
			set
			{
				mCapaSeleccionada.Nombre = value;
			}
		}

		public Int32 CapaWFS
		{
			get { return mCapaSeleccionada.CapaWFS; }
			set
			{
				mCapaSeleccionada.CapaWFS = value;
			}
		}

		public void Georeferencias(Int32 Valor)
		{
			UsaVinculo = (Valor == 1);
			UsaCoordenadas = (Valor == 2);
			if (Valor==1 && ListaVinculos == null)
			{
				_ = LeerVinculosAsync();
			}
		}

		public string EstiloBotones
		{
			get
			{
				return "width: calc(100% - 10px); height: 40px; text-align: right; position: relative; margin-top: 5px; margin-left: 5px;";
			}
		}

		[Inject]
		public HttpClient Http { get; set; }

		private async Task HacerCargaInicialAsync()
		{
			if (CodigoCapa > 0)
			{

			}
			RespuestaCapasGIS Respuesta = await CContenedorDatos.LeerCapasWFSAsync(Http, true, true);
			if (!Respuesta.RespuestaOK)
			{
				CRutinas.DesplegarMsg(Respuesta.MsgErr);
			}
			else
			{
				ListaCapasWFS = Respuesta.CapasWFS;
				ListaCapasWFS.Insert(0, new CCapaWFSCN()
				{
					Codigo = -1,
					Descripcion = CRutinas.NO_DEFINIDA
				});
				StateHasChanged();
			}
		}

		private List<CVinculoIndicadorCompletoCN> mVinculos = null;

		private async Task LeerVinculosAsync()
		{
			mVinculos = await CContenedorDatos.ListarVinculosAsync(Http, ClaseIndicador, Indicador);
			if (mVinculos != null)
			{
				ListaVinculos = (from V in mVinculos
												 where V.Vinculo.ClaseVinculada != ClaseVinculo.ColumnasGIS
												 orderby V.Vinculo.NombreColumna
												 select new CListaTexto()
												 {
													 Codigo = V.Vinculo.Codigo,
													 Descripcion = V.Vinculo.NombreColumna
												 }).ToList();
				ListaVinculos.Insert(0, new CListaTexto()
				{
					Codigo = -1,
					Descripcion = "No definido"
				});
				StateHasChanged();
			}
		}

		public Int32 IntervaloImpuesto
		{
			get { return (Int32)mCapaSeleccionada.Intervalos; }
			set
			{
				mCapaSeleccionada.Intervalos = (ClaseIntervalo)value;
			}
		}

		private async Task CargarListaCapasAsync()
		{
			CRutinas.DesplegarMsg("Prueba del mensaje");
			ListaCapas = await CContenedorDatos.ListarCapasWSSAsync(Http, ClaseIndicador, Indicador);
			OrdenarListaCapas();
			StateHasChanged();
		}

		public List<CColumnaBase> ColumnasAjustadas { get; set; }

		protected override Task OnInitializedAsync()
		{
			CrearLineasCalculo();
			if (ListaCapas == null)
			{
				_ = CargarListaCapasAsync();
			}
			ColumnasAjustadas = (from C in Columnas
													 orderby C.Nombre
													 select C).ToList();
			ColumnasAjustadas.Insert(0, new CColumnaBase()
			{
				Clase = ClaseVariable.NoDefinida,
				Nombre = CRutinas.NO_DEFINIDA
			});
			return base.OnInitializedAsync();
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			if (ListaCapas != null)
			{
				if (ListaCapasWFS == null)
				{
					_ = HacerCargaInicialAsync();
				}
			}
			return base.OnAfterRenderAsync(firstRender);
		}

		public bool NoSeleccionado { get; set; } = true;

		private void CrearCapaNueva()
		{
			mCapaSeleccionada = new CCapaWSSCN();
			NoSeleccionado = true;
			mCapaSeleccionada.Clase = ClaseIndicador;
			mCapaSeleccionada.CodigoElemento = Indicador;
		}

		private void ExtraerColor()
		{
			if (mCapaSeleccionada != null)
			{
			  string Color = Coloreador.Color;
			  mCapaSeleccionada.ColorCompuestoA = 255;
				if (Color == "")
				{
					mCapaSeleccionada.ColorCompuestoB = 255;
					mCapaSeleccionada.ColorCompuestoG = 255;
					mCapaSeleccionada.ColorCompuestoR = 255;
				}
				else
				{
					Color = (Color.StartsWith("#") ? Color.Substring(1) : Color);
					Int32 Paso = Color.Length / 3;
					mCapaSeleccionada.ColorCompuestoR = (byte)Int32.Parse(Color.Substring(0, Paso), System.Globalization.NumberStyles.HexNumber);
					mCapaSeleccionada.ColorCompuestoG = (byte)Int32.Parse(Color.Substring(Paso, Paso), System.Globalization.NumberStyles.HexNumber);
					mCapaSeleccionada.ColorCompuestoB = (byte)Int32.Parse(Color.Substring(2*Paso, Paso), System.Globalization.NumberStyles.HexNumber);
				}
			}
		}

		private bool ColumnaIncorrecta(string Nombre)
		{
			return Nombre.Length == 0 || Nombre == CRutinas.NO_DEFINIDA;
		}

		private bool ExtraerDatos()
		{
			try
			{
				if (mCapaSeleccionada.Intervalos == ClaseIntervalo.NoDefinido)
				{
					return false;
				}

				if (!UsaCoordenadas && !UsaVinculo)
				{
					return false;
				}

				if (mCapaSeleccionada.Intervalos == ClaseIntervalo.NoDefinido || mCapaSeleccionada.Nombre.Length == 0 ||
						ColumnaIncorrecta(mCapaSeleccionada.ColumnaValor))
				{
					return false;
				}

				mCapaSeleccionada.Clase = ClaseIndicador;
				mCapaSeleccionada.CodigoElemento = Indicador;

				ExtraerColor();

				if (mCapaSeleccionada == null)
				{
					mCapaSeleccionada = new CCapaWSSCN();
				}

				mCapaSeleccionada.Modo = (UsaVinculo ? ModoGeoreferenciar.Vinculo : ModoGeoreferenciar.Coordenadas);

				switch (mCapaSeleccionada.Modo)
				{
					case ModoGeoreferenciar.Coordenadas:
						if (ColumnaIncorrecta(mCapaSeleccionada.ColumnaLatitud) ||
							  ColumnaIncorrecta(mCapaSeleccionada.ColumnaLongitud))
						{
							return false;
						}
						break;
					case ModoGeoreferenciar.Vinculo:
						if (mCapaSeleccionada.Vinculo < 0)
						{
							return false;
						}
						CVinculoIndicadorCompletoCN Vinculo = (from V in mVinculos
																				 where V.Vinculo.Codigo == mCapaSeleccionada.Vinculo
																				 select V).FirstOrDefault();
						mCapaSeleccionada.ColumnaGeoreferencia = (Vinculo == null ? "" : Vinculo.Vinculo.NombreColumna);
						break;
				}

				switch (mCapaSeleccionada.Intervalos)
				{
					case ClaseIntervalo.Indicador:
						if (mCapaSeleccionada.Minimo == mCapaSeleccionada.Satisfactorio ||
								mCapaSeleccionada.Satisfactorio == mCapaSeleccionada.Sobresaliente)
						{
							return false;
						}
						break;
					case ClaseIntervalo.Manual:
						mCapaSeleccionada.Referencias = CRutinas.ListaAReales(Segmentos);
						mCapaSeleccionada.Segmentos = mCapaSeleccionada.Referencias.Count;
						if (mCapaSeleccionada.Segmentos < 1)
						{
							return false;
						}
						break;
					case ClaseIntervalo.Cuantiles:
					case ClaseIntervalo.Lineal:
						mCapaSeleccionada.Segmentos = Int32.Parse(Segmentos);
						if (mCapaSeleccionada.Segmentos < 1)
						{
							return false;
						}
						break;
					default:
						return false;
				}
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public Blazorise.ColorEdit Coloreador { get; set; }

		private void OrdenarListaCapas()
		{
			ListaCapas.Sort(delegate (CCapaWSSCN C1, CCapaWSSCN C2)
			{
				return C1.Nombre.CompareTo(C2.Nombre);
			});
		}

		public async void Registrar()
		{
			if (!ExtraerDatos())
			{
				HayMensaje = true;
				HayBoton = true;
				Mensaje = "Datos incompletos o incorrectos";
				StateHasChanged();
			}
			else
			{
				HayMensaje = false;
				StateHasChanged();
				//string RespVal = await CContenedorDatos.VerificarBaseDatosAsync(Http);
				Int32 Codigo = await CContenedorDatos.RegistrarCapaWSSAsync(Http, mCapaSeleccionada);
				if (Codigo > 0)
				{
					if (mCapaSeleccionada.Codigo < 0)
					{
						mCapaSeleccionada.Codigo = Codigo;
						ListaCapas.Add(mCapaSeleccionada);
						Nuevo();
					}
					else
					{
						ListaCapas = (from C in ListaCapas
													where C.Codigo != Codigo
													select C).ToList();
						ListaCapas.Add(mCapaSeleccionada);
						OrdenarListaCapas();
						HayMensaje = false;
						StateHasChanged();
					}
				}
				else
				{
					HayBoton = true;
					HayMensaje = true;
					Mensaje = "No pudo registrar";
					StateHasChanged();
				}

			}
		}

		public void Nuevo()
		{
			CrearCapaNueva();
			StateHasChanged();
		}

		public void Borrar()
		{
			//
		}
	}
}
