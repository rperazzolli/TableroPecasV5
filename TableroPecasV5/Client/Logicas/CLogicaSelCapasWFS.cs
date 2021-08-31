using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using Blazorise;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaSelCapasWFS : ComponentBase
	{

		[CascadingParameter]
		public CLogicaBingMaps Contenedor { get; set; }

		private string mszFiltro = "";
		public string FiltroCapas
		{
			get { return mszFiltro; }
			set
			{
				if (mszFiltro != value)
				{
					mszFiltro = value;
					RefrescarListaCapas();
				}
			}
		}

		private bool mbTodasLasCapas = false;
		public bool TodasLasCapas
		{
			get { return mbTodasLasCapas; }
			set
			{
				if (mbTodasLasCapas != value)
				{
					mbTodasLasCapas = value;
					RefrescarListaCapas();
				}
			}
		}

		private bool mbWMS = false;
		public bool CapasWMS
		{
			get { return mbWMS; }
			set
			{
				if (mbWMS != value)
				{
					mbWMS = value;
					RefrescarListaCapas();
				}
			}
		}

		private bool mbWFS = true;
		public bool CapasWFS
		{
			get { return mbWFS; }
			set
			{
				if (mbWFS != value)
				{
					mbWFS = value;
					RefrescarListaCapas();
				}
			}
		}

		private bool mbWIS = false;
		public bool CapasWIS
		{
			get { return mbWIS; }
			set
			{
				if (mbWIS != value)
				{
					mbWIS = value;
					RefrescarListaCapas();
				}
			}
		}

		public void SeleccionarCapaWMS(CCapaWMSCN Capa)
		{

		}

		public void SeleccionarCapaWFS(CCapaWFSCN Capa)
		{

		}

		public void SeleccionarCapaWIS(CCapaWISCompletaCN Capa)
		{

		}

		public List<CCapaWMSCN> ListaCapasWMS { get; set; } = new List<CCapaWMSCN>();
		public List<CCapaWISCompletaCN> ListaCapasWIS { get; set; } = new List<CCapaWISCompletaCN>();
		public List<CCapaWFSCN> ListaCapasWFS { get; set; } = null;

		public List<CCapaWMSCN> ListaCapasWMSFiltradas { get; set; } = new List<CCapaWMSCN>();
		public List<CCapaWISCompletaCN> ListaCapasWISFiltradas { get; set; } = new List<CCapaWISCompletaCN>();
		public List<CCapaWFSCN> ListaCapasWFSFiltradas { get; set; } = null;

		[Inject]
		public HttpClient Http { get; set; }

		public async void CerrarEditarProveedoresWFS()
		{
			AgregandoCapas = false;
			await LeerCapasAsync();
		}

		public async Task LeerCapasAsync(bool UnicamenteWFS = false)
		{
			RespuestaCapasGIS Respuesta = await Contenedores.CContenedorDatos.LeerCapasWFSAsync(Http, UnicamenteWFS, true);
			if (Respuesta != null)
			{
				ListaCapasWFS = Respuesta.CapasWFS;
				if (!UnicamenteWFS)
				{
					ListaCapasWMS = Respuesta.CapasWMS;
					ListaCapasWIS = Respuesta.CapasWIS;
				}
			  RefrescarListaCapas();
			}
			else
			{
				StateHasChanged();
			}
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (ListaCapasWFS == null)
			{
				await LeerCapasAsync();
			}
			await base.OnAfterRenderAsync(firstRender);
		}

		public bool AgregandoCapas { get; set; } = false;

		private bool CumpleFiltroTexto(string Texto)
		{
			return (mszFiltro.Length == 0 || Texto.IndexOf(mszFiltro, StringComparison.InvariantCultureIgnoreCase) >= 0);
		}

		private void RefrescarListaCapas()
		{
			ListaCapasWMSFiltradas = (from C in ListaCapasWMS
																where (mbTodasLasCapas || mbWMS) &&
																CumpleFiltroTexto(C.Descripcion)
																select C).ToList();
			ListaCapasWFSFiltradas = (from C in ListaCapasWFS
																where (mbTodasLasCapas || mbWFS) &&
																CumpleFiltroTexto(C.Descripcion)
																select C).ToList();
			ListaCapasWISFiltradas = (from C in ListaCapasWIS
																where (mbTodasLasCapas || mbWIS) &&
																CumpleFiltroTexto(C.Capa.Descripcion)
																select C).ToList();
			StateHasChanged();
		}

		public void AgregarProveedoresGIS()
		{
			if (Contenedor is CLogicaBingMaps Mapa)
			{
				Mapa.AbrirEdicionProveedores();
			}
		}

		public void AgregarCapasWIS()
		{
			if (Contenedor is CLogicaBingMaps Mapa)
			{
				Mapa.AbrirEdicionWIS();
			}
		}

		public void Registrar()
		{

		}

	}
}
