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
	public class CLogicaDefCapaWIS : ComponentBase
	{
		public List<CCapaWISCompletaCN> ListaCapas { get; set; } = null;
		public List<CCapaWFSCN> ListaCapasWFS { get; set; } = null;

		public bool HayMensaje { get; set; } = false;
		public bool HayBoton { get; set; } = false;
		public string Mensaje { get; set; } = "";


		[CascadingParameter]
		public CLogicaBingMaps Contenedor { get; set; }

		public void Cerrar()
		{
			Contenedor.CerrarDefinicionWIS();
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			if (mCapaSeleccionada == null)
			{
				CrearCapaNueva();
			}
			if (ListaCapas == null)
			{
				ListaCapas = Contenedor.ListaCapasWIS;
				ListaCapasWFS = Contenedor.ListaCapasWFS;
				Aguardando = false;
				StateHasChanged();
			}
			return base.OnAfterRenderAsync(firstRender);
		}

		public void CerrarMsg()
		{
			HayMensaje = false;
			Contenedor.CerrarDefinicionWIS();
		}

		public void SeleccionarWIS(CCapaWISCompletaCN Capa)
		{
			CapaSeleccionada = Capa;
			StateHasChanged();
		}

		public string EstiloCapa(CCapaWISCompletaCN Capa)
		{
			return "width: 90%; height: 25px; padding: 0px; background: " +
					(Capa == mCapaSeleccionada ? "yellow;" : "white;");
		}

		private CCapaWISCompletaCN mCapaSeleccionada = null;
		public CCapaWISCompletaCN CapaSeleccionada
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
			get { return (CapaSeleccionada == null ? -1 : CapaSeleccionada.Capa.Codigo); }
		}

		public string Segmentos { get; set; } = "";

		public string EstiloInput
		{
			get
			{
				return "height: 25px; margin-top: 0px;";
			}
		}

		public string NombreCapa
		{
			get { return (mCapaSeleccionada == null ? "" : mCapaSeleccionada.Capa.Descripcion); }
			set
			{
				mCapaSeleccionada.Capa.Descripcion = value;
			}
		}

		public Int32 CapaWFS
		{
			get { return mCapaSeleccionada == null ? -1 : mCapaSeleccionada.Capa.CodigoWFS; }
			set
			{
				mCapaSeleccionada.Capa.CodigoWFS = value;
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

//		private List<CVinculoIndicadorCompletoCN> mVinculos = null;

		public List<CColumnaBase> ColumnasAjustadas { get; set; }

		public bool NoSeleccionado { get; set; } = true;

		private void CrearCapaNueva()
		{
			mCapaSeleccionada = new CCapaWISCompletaCN();
			mCapaSeleccionada.Capa = new CCapaWISCN()
			{
				Codigo = -1,
				CodigoWFS = -1,
				Descripcion = ""
			};
			mCapaSeleccionada.Vinculos = new List<CElementoPreguntasWISCN>();
		}

		private bool ExtraerDatos()
		{
			try
			{

				if (mCapaSeleccionada == null)
				{
					CrearCapaNueva();
				}

				NombreCapa = NombreCapa.Trim();
				return (NombreCapa.Length > 0 && mCapaSeleccionada.Capa.CodigoWFS > 0);
			}
			catch (Exception)
			{
				return false;
			}
		}

		public Blazorise.ColorEdit Coloreador { get; set; }

		private void OrdenarListaCapas()
		{
			ListaCapas.Sort(delegate (CCapaWISCompletaCN C1, CCapaWISCompletaCN C2)
			{
				return C1.Capa.Descripcion.CompareTo(C2.Capa.Descripcion);
			});
		}

		public bool Aguardando { get; set; } = false;

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
				Aguardando = true;
				HayMensaje = false;
				StateHasChanged();
				try
				{
					Int32 Codigo = await CContenedorDatos.RegistrarCapaWISAsync(Http, mCapaSeleccionada);
					if (Codigo > 0)
					{
						if (mCapaSeleccionada.Capa.Codigo < 0)
						{
							mCapaSeleccionada.Capa.Codigo = Codigo;
							ListaCapas.Add(mCapaSeleccionada);
							Nuevo();
						}
						else
						{
							ListaCapas = (from C in ListaCapas
														where C.Capa.Codigo != Codigo
														select C).ToList();
							ListaCapas.Add(mCapaSeleccionada);
							OrdenarListaCapas();
							HayMensaje = false;
						}
					}
					else
					{
						HayBoton = true;
						HayMensaje = true;
						Mensaje = "No pudo registrar";
					}
				}
				finally
				{
					Aguardando = false;
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
