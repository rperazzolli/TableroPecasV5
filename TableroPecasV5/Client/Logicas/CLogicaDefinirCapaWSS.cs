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
			StateHasChanged();
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
			//
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

		public Int32 Segmentos
		{
			get { return mCapaSeleccionada.Segmentos; }
			set
			{
				mCapaSeleccionada.Segmentos = value;
			}
		}

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
				return "width: 100%; height: 40px; text-align: right; position: relative; margin-top: 5px;";
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
				StateHasChanged();
			}
		}

		private async Task LeerVinculosAsync()
		{
			List<CVinculoIndicadorCompletoCN> Vinculos = await CContenedorDatos.ListarVinculosAsync(Http, ClaseIndicador,Indicador);
			if (Vinculos != null)
			{
				ListaVinculos = (from V in Vinculos
												 orderby V.Vinculo.NombreColumna
												 select new CListaTexto()
												 {
													 Codigo = V.Vinculo.Codigo,
													 Descripcion = V.Vinculo.NombreColumna
												 }).ToList();
				StateHasChanged();
			}
		}

		public Int32 IntervaloImpuesto
		{
			get { return (Int32)mCapaSeleccionada.Intervalos; }
			set
			{
				if (value != -1)
				{
					mCapaSeleccionada.Intervalos = (ClaseIntervalo)value;
				}
			}
		}

		public bool PorIndicador { get; set; } = false;
		public bool PorGradiente { get; set; } = false;

		public void Coloracion(Int32 Opcion)
		{
			PorIndicador = (Opcion == 1);
			PorGradiente = (Opcion == 2);
			StateHasChanged();
		}

		protected override Task OnInitializedAsync()
		{
			CrearLineasCalculo();
			return base.OnInitializedAsync();
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			if (ListaCapasWFS == null)
			{
				_ = HacerCargaInicialAsync();
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

		public void Registrar()
		{
			//
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
