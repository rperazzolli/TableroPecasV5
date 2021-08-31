using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaDefinicionCapasWFS : ComponentBase
	{
		[Parameter]
		public Int32 CodigoProveedor { get; set; }

		public bool NoRegistrado { get; set; } = true;

		public bool DatosIncompletos { get; set; } = true;

		private CCapaWFSCN mCapa = null;

		public string EstiloCapa(CCapaWFSCN Capa)
		{
			return "height: 25px; width: 100 %; cursor: pointer; padding: 0px; margin: 0px; background: " +
				(Capa == mCapa ? "yellow;" : "white;");
		}

		public string Mensaje { get; set; } = "";
		public bool HayMensaje { get; set; } = false;

		public void LimpiarMsg()
		{
			Mensaje = "";
			HayMensaje = false;
			StateHasChanged();
		}

		public void SeleccionarCapa(CCapaWFSCN Capa)
		{
			if (mCapa != Capa)
			{
				mCapa = Capa;
				PonerEnPantalla();
			}
			StateHasChanged();
		}

		private void AjustarHabilitaciones()
		{
			NoRegistrado = (mCapa == null || mCapa.Codigo < 0);
			DatosIncompletos = ListaCapas == null || NombreCapa.Length == 0 ||
					URLCapa.Length == 0 || CapaCapa.Length == 0 || VersionCapa.Length == 0 ||
					SegmentosCapa <= 0 || ElementoCapa == ElementoWFS.NoDefinido ||
					CodigoCapa.Length == 0 || NombreDatosCapa.Length == 0 || CoordenadasCapa.Length == 0;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (ListaCapas == null)
			{
			  RespuestaCapasGIS Respuesta = await Contenedores.CContenedorDatos.LeerCapasWFSAsync(Http, true, true);
				if (!Respuesta.RespuestaOK)
				{
					Mensaje = Respuesta.MsgErr;
					HayMensaje = true;
					ListaCapas = new List<CCapaWFSCN>();
				}
				else
				{
					ListaCapas = Respuesta.CapasWFS;
				}
			  StateHasChanged();
				return;
			}
			await base.OnAfterRenderAsync(firstRender);
		}

		public List<CCapaWFSCN> ListaCapas { get; set; } = null;

		private string mszNombre = "";
		public string NombreCapa
		{
			get { return mszNombre; }
			set
			{
				if (mszNombre != value)
				{
					mszNombre = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private string mszURL = "";
		public string URLCapa
		{
			get { return mszURL; }
			set
			{
				if (mszURL != value)
				{
					mszURL = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private string mszCapa = "";
		public string CapaCapa
		{
			get { return mszCapa; }
			set
			{
				if (mszCapa != value)
				{
					mszCapa = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private string mszVersion = "";
		public string VersionCapa
		{
			get { return mszVersion; }
			set
			{
				if (mszVersion != value)
				{
					mszVersion = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		[CascadingParameter]
		public CLogicaBingMaps CapaProveedor { get; set; }

		public Int32 CodigoElementosCapa
		{
			get
			{
				return (Int32)ElementoCapa;
			}
			set
			{
				ElementoCapa = (ElementoWFS)value;
			}
		}

		private Int32 mSegmentos = 0;
		public Int32 SegmentosCapa
		{
			get { return mSegmentos; }
			set
			{
				if (mSegmentos != value)
				{
					mSegmentos = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private ElementoWFS mElemento = ElementoWFS.NoDefinido;
		public ElementoWFS ElementoCapa
		{
			get { return mElemento;}
			set
			{
				if (mElemento != value)
				{
					mElemento = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private string mszCodigo = "";
		public string CodigoCapa
		{
			get { return mszCodigo; }
			set
			{
				if (mszCodigo != value)
				{
					mszCodigo = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private string mszNombreDatos = "";
		public string NombreDatosCapa
		{
			get { return mszNombreDatos; }
			set
			{
				if (mszNombreDatos != value)
				{
					mszNombreDatos = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private string mszCoordenadas = "";
		public string CoordenadasCapa
		{
			get { return mszCoordenadas; }
			set
			{
				if (mszCoordenadas != value)
				{
					mszCoordenadas = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		private bool mbGuardarCopia = false;
		public bool GuardarCopia
		{
			get { return mbGuardarCopia; }
			set
			{
				if (mbGuardarCopia != value)
				{
					mbGuardarCopia = value;
				}
			}
		}

		public async void ProbarCapa()
		{
			try
			{

				Mensaje = "Probando....";
				HayMensaje = true;
				StateHasChanged();

				ExtraerDePantalla();

				Respuesta RespValid = await Contenedores.CContenedorDatos.ValidarCapaWFSAsync(Http, mCapa);

				if (!RespValid.RespuestaOK)
				{
					throw new Exception(RespValid.MsgErr);
				}

				Mensaje = "Capa verificada con éxito";

			}
			catch (Exception ex)
			{
				Mensaje = CRutinas.TextoMsg(ex);
			}

			HayMensaje = true;
			StateHasChanged();

		}

		[Inject]
		public HttpClient Http { get; set; }

		private void OrdenarListaCapas()
		{
			ListaCapas.Sort(delegate (CCapaWFSCN C1, CCapaWFSCN C2)
			{
				return C1.Descripcion.CompareTo(C2.Descripcion);
			});
		}

		private void ExtraerDePantalla()
		{
			if (mCapa == null)
			{
				mCapa = CrearCapaLimpia();
			}
			mCapa.Capa = CapaCapa;
			mCapa.CodigoProveedor = CodigoProveedor;
			mCapa.Descripcion = NombreCapa;
			mCapa.DireccionURL = URLCapa;
			mCapa.Elemento = ElementoCapa;
			mCapa.GuardaCompactada = GuardarCopia;
			mCapa.NombreCampoCodigo = CodigoCapa;
			mCapa.NombreElemento = NombreDatosCapa;
			mCapa.NombreCampoDatos = CoordenadasCapa;
			mCapa.PuntosMaximosContorno = SegmentosCapa;
			mCapa.Version = VersionCapa;
		}

		public async void Registrar()
		{
			ExtraerDePantalla();
			Int32 Codigo = await Contenedores.CContenedorDatos.RegistrarCapaWFSAsync(Http, mCapa);
			if (mCapa.Codigo != Codigo)
			{
				mCapa.Codigo = Codigo;
				ListaCapas.Add(mCapa);
			}
			OrdenarListaCapas();
			mCapa = CrearCapaLimpia();
			PonerEnPantalla();
		}

		private CCapaWFSCN CrearCapaLimpia()
		{
			return new CCapaWFSCN()
			{
				Codigo = -1,
				Capa = "",
				CamposInformacion = "",
				CodigoProveedor = CodigoProveedor,
				Descripcion = "",
				Detalle = "",
				DireccionURL = "",
				Elemento = ElementoWFS.NoDefinido,
				FechaRefresco = DateTime.Now,
				GuardaCompactada = false,
				NombreCampoCodigo = "",
				NombreElemento = "",
				NombreCampoDatos = "",
				PuntosMaximosContorno = 0,
				Version = ""
			};
		}

		public string EstiloInput
		{
			get
			{
				return "height: 25px; margin-top: 0px;";
			}
		}

		private void PonerEnPantalla()
		{
			if (mCapa == null)
			{
				mCapa = CrearCapaLimpia();
			}
			NombreCapa = mCapa.Descripcion;
			URLCapa = mCapa.DireccionURL;
			CapaCapa = mCapa.Capa;
			VersionCapa = mCapa.Version;
			ElementoCapa = mCapa.Elemento;
			SegmentosCapa = mCapa.PuntosMaximosContorno;
			CodigoCapa = mCapa.NombreCampoCodigo;
			NombreDatosCapa = mCapa.NombreElemento;
			CoordenadasCapa = mCapa.NombreCampoDatos;
			GuardarCopia = mCapa.GuardaCompactada;
		}

		public void Nuevo()
		{
			mCapa = CrearCapaLimpia();
			PonerEnPantalla();
		}

		public async void Borrar()
		{
			ExtraerDePantalla();
			if (mCapa.Codigo < 0)
			{
				Nuevo();
			}
			else
			{
			  Respuesta Respuesta =	await Contenedores.CContenedorDatos.BorrarCapaWFSAsync(Http, mCapa.Codigo);
				if (Respuesta.RespuestaOK)
				{
					ListaCapas = (from C in ListaCapas
												where C.Codigo != mCapa.Codigo
												select C).ToList();
					Nuevo();
				}
				else
				{
					Mensaje = Respuesta.MsgErr;
					HayMensaje = true;
					StateHasChanged();
				}
			}
		}

		public void Cerrar()
		{
			if (CapaProveedor != null)
			{
				CapaProveedor.CerrarDefinicionCapas();
			}
		}

	}
}
