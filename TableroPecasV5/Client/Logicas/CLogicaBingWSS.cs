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

		[Parameter]
		public ClaseElemento ClaseIndicador { get; set; }
		[Parameter]
		public Int32 CodigoIndicador { get; set; }

		[Parameter]
		public Int32 CodigoElementoDimension { get; set; }

		[Parameter]
		public List<CColumnaBase> Columnas { get; set; }
		[Parameter]
		public List<CLineaComprimida> Lineas { get; set; }
		public bool Editando { get; set; }
		public List<CCapaWSSCN> CapasWSS { get; set; }
		public CLogicaDefinirCapaWSS DefinidorCapas { get; set; }
		public bool EstaLeyendo { get; set; } = false;

		private Int32 mCodigoPantalla;

		public bool VerLabels { get; set; } = true;

		private bool mbVerMapaCalor = false;
		public bool VerComoMapaCalor
		{
			get { return mbVerMapaCalor; }
			set
			{
				if (mbVerMapaCalor != value)
				{
					mbVerMapaCalor = value;
					StateHasChanged();
				}
			}
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

		public void FncRespuesta (CVinculoIndicadorCompletoCN VinculoDeterminado)
		{
			MostrarDialogoVinculador = false;
			StateHasChanged();
		}

		[CascadingParameter]
		public CLogicaIndicador Contenedor { get; set; }

		private CProveedorComprimido mProveedor = null;

		public void Retroceder()
		{
			Contenedor.CerrarWSS();
		}

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

		public CMapaCalor MapaCalor { get; set; }

		public CCapaWSSCN CapaWSS
		{
			get { return mCapa; }
		}

		public string NombreColumnaLat
		{
			get
			{
				return (mCapa == null ? "" : mCapa.ColumnaLatitud);
			}
		}

		public string NombreColumnaLng
		{
			get
			{
				return (mCapa == null ? "" : mCapa.ColumnaLongitud);
			}
		}

		public string NombreColumnaValor
		{
			get
			{
				return (mCapa == null ? "" : mCapa.ColumnaValor);
			}
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

		[Inject]
		public HttpClient Http { get; set; }

		protected override async Task OnInitializedAsync()
		{
			Editando = false;
			mCodigoPantalla = gCodigoPantalla++;
			CapasWSS = await CContenedorDatos.ListarCapasWSSAsync(Http, ClaseElemento.Indicador, Indicador.Codigo);
			//ClaseIndicador = gClaseElemento;
			//CodigoIndicador = gCodigoElemento;
			//CodigoElementoDimension = gCodigoElementoDimension;
			//Columnas = gColumnas;
			//Lineas = gLineas;
			await base.OnInitializedAsync();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
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
