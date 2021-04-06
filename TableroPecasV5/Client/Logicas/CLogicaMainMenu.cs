using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Blazorise;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Rutinas;

namespace TableroPecasV5.Client.Logicas
{

	/// <summary>
	/// Pendientes:
	/// Subconsultas.
	/// Grillas.
	/// Proveer datos a mapas desde filtros.
	/// Configurar mapas.
	/// Configurar mímicos.
	/// Crear gráficos sobre GIS en indicadores.
	/// Agregar tortas sobre GIS.
	/// OLAP.
	/// Acelerador para grandes datasets.
	/// </summary>

	public class CLogicaMainMenu : LayoutComponentBase
	{

		public CLogicaMainMenu()
		{
			gPuntero = this;
		}

		public static void RefrescarPantalla()
		{
			if (gPuntero != null)
			{
				gPuntero.HayMsg = (Contenedores.CContenedorDatos.MsgUsuario.Length > 0);
				gPuntero.StateHasChanged();
			}
		}

		public static CLogicaMainMenu gPuntero {get;set;}

	[Inject]
	IJSRuntime JSRuntime { get; set; }

		[Inject]
		NavigationManager NavigationManager { get; set; }

		private const string ESTILO_MENU = "font-size: 12px; font-weight: bold; padding: 2px 4px; cursor: pointer; position: absolute; left: 42px; top:";

		public Dropdown MenuSalasReunion { get; set; }

		public string EstiloSalas
		{
			get
			{
				if (Contenedores.CContenedorDatos.EstructuraIndicadores == null)
				{
					return "";
				}
				else {
					Int32 Alto = Contenedores.CContenedorDatos.EstructuraIndicadores.Salas.Count * 28 + 2;
					Int32 Ordenada = 60;
					if (Rutinas.CRutinas.AltoPantalla < (Ordenada + Alto))
					{
						Ordenada = Math.Max(0, Rutinas.CRutinas.AltoPantalla - Alto);
					}
					return ESTILO_MENU + Ordenada.ToString() + ";";
				}
			}
		}

		public Dropdown MenuMapasBing { get; set; }

		public string EstiloMapasBing
		{
			get
			{
				if (Contenedores.CContenedorDatos.ListaMapas == null)
				{
					return "";
				}
				else
				{
					Int32 Alto = Contenedores.CContenedorDatos.ListaMapas.Count * 28 + 2;
					Int32 Ordenada = 60;
					if (Rutinas.CRutinas.AltoPantalla < (Ordenada + Alto))
					{
						Ordenada = Math.Max(0, Rutinas.CRutinas.AltoPantalla - Alto);
					}
					return ESTILO_MENU + Ordenada.ToString() + ";";
				}
			}
		}

		public Dropdown MenuMimicos { get; set; }

		public string EstiloMimicos
		{
			get
			{
				if (Contenedores.CContenedorDatos.ListaMimicos == null)
				{
					return "";
				}
				else
				{
					Int32 Alto = Contenedores.CContenedorDatos.ListaMimicos.Count * 28 + 2;
					Int32 Ordenada = 100;
					if (Rutinas.CRutinas.AltoPantalla < (Ordenada + Alto))
					{
						Ordenada = Math.Max(0, Rutinas.CRutinas.AltoPantalla - Alto);
					}
					return ESTILO_MENU + Ordenada.ToString() + ";";
				}
			}
		}

		public Dropdown MenuSubconsultas { get; set; }
		public string EstiloSubconsulta
		{
			get
			{
				if (Contenedores.CContenedorDatos.gSubconsultas == null)
				{
					return "";
				}
				else
				{
					Int32 Alto = Contenedores.CContenedorDatos.gSubconsultas.Count * 28 + 2;
					Int32 Ordenada = 100;
					if (Rutinas.CRutinas.AltoPantalla < (Ordenada + Alto))
					{
						Ordenada = Math.Max(0, Rutinas.CRutinas.AltoPantalla - Alto);
					}
					return ESTILO_MENU + Ordenada.ToString() + ";";
				}
			}
		}

		public static void AjustarMenu()
		{
			if (gPuntero != null)
			{
				gPuntero.StateHasChanged();
			}
		}

		private static double gDimensionCaracter = -1;

		public async Task DeterminarAnchoOpcionesAsync(Int32 CodSala)
		{
			List<string> gOpciones = (from CSolapaCN Solapa in Contenedores.CContenedorDatos.SolapasEnSala(CodSala)
																select Solapa.Nombre).ToList();
			object[] Args = new object[3];
			Args[0] = 12;
			Args[1] = "serif";
			string Aa = "";
			foreach (string Opcion in gOpciones)
			{
				if (Opcion.Length > Aa.Length)
				{
					Aa = Opcion;
				}
			}
			Args[2] = Aa.Length;

			double R = (Aa.Length > 0 ? await JSRuntime.InvokeAsync<double>("FuncionesJS.ObtenerDimensiones", Args) : 10);
			Contenedores.CContenedorDatos.AnchoOpcionSolapa = (long)Math.Floor(R + 0.5);


			if (gDimensionCaracter < 0)
			{
				Args[0] = "H";
				Args[2] = 12;
				gDimensionCaracter = await JSRuntime.InvokeAsync<double>("FuncionesJS.ObtenerDimensionTexto", Args);
			}

			Contenedores.CContenedorDatos.AltoOpcionSolapa = (long)(Math.Floor(gDimensionCaracter + 0.5) + 18);

		}

		public async void AbrirSalaReunion(Int32 Codigo)
		{
		  CerrarMenues();
			if (Codigo < 0)
			{
				NavigationManager.NavigateTo("CrearSalaReunion");
			}
			else
			{
				await DeterminarAnchoOpcionesAsync(Codigo);
				NavigationManager.NavigateTo("SalaReunion/" + Codigo.ToString());
			}
		}

		public bool HayMenu { get; set; } = false;

		public bool HayMenuSalasReunion
		{
			get {
				return Contenedores.CContenedorDatos.EstructuraIndicadores != null &&
					Contenedores.CContenedorDatos.gAlarmasIndicador != null; 
			}
		}

		public bool HayMenuMapasBing
		{
			get
			{
				return Contenedores.CContenedorDatos.ListaMapas != null &&
					Contenedores.CContenedorDatos.gAlarmasIndicador != null;
			}
		}

		public bool HayMenuMimicos
		{
			get
			{
				return (Contenedores.CContenedorDatos.ListaMimicos != null &&
					Contenedores.CContenedorDatos.gAlarmasIndicador != null) ||
					Contenedores.CContenedorDatos.EsAdministrador;
			}
		}

		public bool HaySubconsultas
		{
			get
			{
				return Contenedores.CContenedorDatos.gSubconsultas != null &&
					Contenedores.CContenedorDatos.gSubconsultas.Count > 0;
			}
		}

		public bool HayMsg { get; set; } = false;
		//public string LineaMsg1 { get; set; } = "";
		//public string LineaMsg2 { get; set; } = "";
		public void OcultarMsg ()
		{
			HayMsg = false;
			StateHasChanged();
		}

		public void CerrarMenues()
		{
			HayMenu = false;
			if (MenuSalasReunion != null)
			{
				MenuSalasReunion.Hide();
			}
			if (MenuMapasBing != null)
			{
				MenuMapasBing.Hide();
			}
			if (MenuMimicos != null)
			{
				MenuMimicos.Hide();
			}
			if (MenuSubconsultas != null)
			{
				MenuSubconsultas.Hide();
			}
		}

		public void AbrirBusqueda()
		{
			NavigationManager.NavigateTo("Indicadores");
		}

		public void AbrirMenuReunion()
		{
			CerrarMenues();
			MenuSalasReunion.Show();
			HayMenu = true;
			StateHasChanged();
		}

		public void AbrirMenuMapasBing()
		{
			CerrarMenues();
			MenuMapasBing.Show();
			HayMenu = true;
			StateHasChanged();
		}

		public void AbrirMenuSubconsultas()
		{
			CerrarMenues();
			MenuSubconsultas.Show();
			HayMenu = true;
			StateHasChanged();
		}

		public void AbrirMenuMimicos()
		{
			CerrarMenues();
			MenuMimicos.Show();
			HayMenu = true;
			StateHasChanged();
		}

		public void AbrirMapaBing(Int32 Codigo)
		{
			CerrarMenues();
			NavigationManager.NavigateTo("BingMaps/" + Codigo.ToString());
		}

		public void AbrirMimico(Int32 Codigo)
		{
			CerrarMenues();
			NavigationManager.NavigateTo("Mimicos/" + Codigo.ToString());
		}

		public void AbrirSubconsulta(Int32 Codigo)
		{
			CerrarMenues();
			CSubconsultaExt SubC = Contenedores.CContenedorDatos.SubconsultaCodigo(Codigo);
			if (SubC != null && SubC.Parametros.Count == 0)
			{
				Logicas.CLogicaSubconsulta.gParametros = "(0)";
				NavigationManager.NavigateTo("Subconsulta/" + Codigo.ToString());
			}
			else
			{
				NavigationManager.NavigateTo("PedirPrmsSubconsulta/" + Codigo.ToString());
			}
		}

		protected override async void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
			if (firstRender)
			{
				await Contenedores.CContenedorDatos.InicializarDimensionesAsync(this.JSRuntime);
			}
		}

		[JSInvokable]
		public static string ImponerDimensiones()
		{
			string Aa = "AA";
			return Aa;
		}

		public void OKMsg()
		{
			Contenedores.CContenedorDatos.MsgUsuario = "";
			Contenedores.CContenedorDatos.VerBotonMsg = false;
		}

		[JSInvokable]
		public static void ImponerDimensionesAsync(string Nombre)
		{
			string[] Campos = Nombre.Split(',');
			Rutinas.CRutinas.AnchoPantalla = Int32.Parse(Campos[0]);
			Rutinas.CRutinas.AltoPantalla = Int32.Parse(Campos[1]);
		}
	}
}
