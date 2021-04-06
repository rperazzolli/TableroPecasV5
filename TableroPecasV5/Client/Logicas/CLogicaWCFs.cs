using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Blazorise;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaWCFs : LayoutComponentBase
	{

    [Inject]
		IJSRuntime JSRuntime { get; set; }

    public Modal Dialogo { get; set; }

		public Modal DialogoEdicion { get; set; }

		public List<CPaqueteWCF> Paquetes { get; set; }

		private string mszPaqueteSeleccionado = "";
		public string PaqueteSeleccionado
		{
			get { return mszPaqueteSeleccionado; }
			set { mszPaqueteSeleccionado = value; }
		}

		public void Cerrando (Blazorise.ModalClosingEventArgs e)
		{
			switch (e.CloseReason)
			{
				case CloseReason.EscapeClosing:
				case CloseReason.FocusLostClosing:
					e.Cancel = true;
					break;
			}
		}

		public bool NoPuedeSeleccionar
		{
			get { return (Paquetes==null || Paquetes.Count!=1) && PaqueteSeleccionado.Length == 0; }
		}

		public string SinNombre
		{
			get { return ""; }
		}

		private const string PRM_WCFS = "WCFS";
		private async Task CargarPaquetesAsync()
		{
			object[] Prms = new object[1];
			Prms[0] = PRM_WCFS;
			string Texto;
			try
			{
				Texto = await JSRuntime.InvokeAsync<string>("FuncionesJS.LeerDatosLocalStorage", Prms);
			}
			catch (Exception)
			{
				Texto = "";
			}
			string Seleccionado;
			Paquetes = CPaqueteWCF.ExtraerListaPaquetes(Texto, out Seleccionado);
			if (Paquetes.Count == 1)
			{
				PaqueteSeleccionado = Paquetes[0].Nombre;
			}
			else
			{
				PaqueteSeleccionado = Seleccionado;
			}
		}

		private string mszPaqueteEdicion = "";
		public string PaqueteEnEdicion
		{
			 get { return mszPaqueteEdicion; }
			set
			{
				mszPaqueteEdicion = value;
				CPaqueteWCF Paquete = (from P in Paquetes
															 where P.Nombre == mszPaqueteEdicion
															 select P).FirstOrDefault();
				if (Paquete == null)
				{
					NombreLocal = "";
					UrlBPILocal = "";
					UrlEstructuraLocal = "";
				}
				else
				{
					NombreLocal = Paquete.Nombre;
					UrlBPILocal = Paquete.UrlBPI;
					UrlEstructuraLocal = Paquete.UrlEstructura;
				}
				StateHasChanged();
			}
		}

		[Inject]
		public NavigationManager Navegador { get; set; }

		public void Seleccionar()
		{
			CPaqueteWCF Paquete = (from P in Paquetes
														 where P.Nombre == PaqueteSeleccionado
														 select P).FirstOrDefault();
			if (Paquete != null)
			{
				Contenedores.CContenedorDatos.UrlBPI = Paquete.UrlBPI;
				Contenedores.CContenedorDatos.UrlEstructura = Paquete.UrlEstructura;
				Navegador.NavigateTo("Login");
			}
		}

		public string NombreLocal { get; set; } = "";
		public string UrlBPILocal { get; set; } = "";
		public string UrlEstructuraLocal { get; set; } = "";

		private async Task RegistrarPaquetesAsync()
		{
			object[] Prms = new object[2];
			Prms[0] = PRM_WCFS;
			Prms[1] = CPaqueteWCF.ArmarTextoLista(PaqueteSeleccionado, Paquetes);
			await JSRuntime.InvokeVoidAsync("FuncionesJS.RegistrarDatosLocalStorage", Prms);
		}

		public void Editar()
		{
			if (DialogoEdicion != null)
			{
				DialogoEdicion.Show();
			}
		}

		protected override async void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
			if (firstRender)
			{
				await CargarPaquetesAsync();
				Dialogo.Show();
			}
		}

		public bool NoPuedeRegistrar
		{
			get
			{
				return NombreLocal == null || NombreLocal.Length == 0 ||
						UrlBPILocal == null || UrlBPILocal.Length == 0 ||
						UrlEstructuraLocal == null || UrlEstructuraLocal.Length == 0;
			}
		}

		public bool NoPuedeBorrar
		{
			get
			{
				return PaqueteEnEdicion == null || PaqueteEnEdicion.Length == 0;
			}
		}

		public void Salir()
		{
			DialogoEdicion.Hide();
		}

		public async void Registrar()
		{
			CPaqueteWCF Paquete = (from P in Paquetes
														 where P.Nombre == NombreLocal
														 select P).FirstOrDefault();
			if (Paquete == null)
			{
				Paquete = new CPaqueteWCF()
				{
					Nombre = NombreLocal,
					UrlBPI = UrlBPILocal,
					UrlEstructura = UrlEstructuraLocal
				};
				Paquetes.Add(Paquete);
			}
			else
			{
				Paquete.UrlBPI = UrlBPILocal;
				Paquete.UrlEstructura = UrlEstructuraLocal;
			}
			await RegistrarPaquetesAsync();
			PaqueteEnEdicion = "";
			StateHasChanged();
		}

		public async void Borrar()
		{
			CPaqueteWCF Paquete = (from P in Paquetes
														 where P.Nombre == NombreLocal
														 select P).FirstOrDefault();
			if (Paquete != null)
			{
				Paquetes.Remove(Paquete);
			  await RegistrarPaquetesAsync();
				PaqueteEnEdicion = "";
				StateHasChanged();
			}
		}

		public void Nuevo()
		{
			PaqueteEnEdicion = "";
			StateHasChanged();
		}

	}

	public class CPaqueteWCF
	{
		private const string SEPARADOR = "%%";
		private const string SEPARADOR_PAQUETES = "$$$$";
		public string Nombre { get; set; }
		public string UrlBPI { get; set; }
		public string UrlEstructura { get; set; }

		public override string ToString()
		{
			return Nombre + SEPARADOR + UrlBPI + SEPARADOR + UrlEstructura;
		}

		public static CPaqueteWCF CrearDesdeTexto(string Texto)
		{
			string[] Elementos = Texto.Split(SEPARADOR, StringSplitOptions.RemoveEmptyEntries);
			if (Elementos.Length == 3)
			{
				return new CPaqueteWCF()
				{
					Nombre = Elementos[0],
					UrlBPI = Elementos[1],
					UrlEstructura = Elementos[2]
				};
			}
			else
			{
				return null;
			}
		}

		public static string ArmarTextoLista(string OpcionSeleccionada, List<CPaqueteWCF> Paquetes)
		{
			string Respuesta = (OpcionSeleccionada.Length==0?"--": OpcionSeleccionada);
			foreach (CPaqueteWCF Paquete in Paquetes)
			{
				if (Paquete.Nombre.Length > 0 && Paquete.UrlBPI.Length > 0 && Paquete.UrlEstructura.Length > 0)
				{
					Respuesta += SEPARADOR_PAQUETES + Paquete.ToString();
				}
			}
			return Respuesta;
		}

		public static List<CPaqueteWCF> ExtraerListaPaquetes(string Texto, out string Opcion)
		{
			List<CPaqueteWCF> Respuesta = new List<CPaqueteWCF>();
			Opcion = "";
			try
			{
				if (Texto != null && Texto.Length > 0)
				{
					string[] Elementos = Texto.Split(SEPARADOR_PAQUETES, StringSplitOptions.RemoveEmptyEntries);
					Opcion = Elementos[0];
					if (Opcion == "--")
					{
						Opcion = "";
					}
					for (Int32 i = 1; i < Elementos.Length; i++)
					{
						CPaqueteWCF Paquete = CPaqueteWCF.CrearDesdeTexto(Elementos[i]);
						if (Paquete != null)
						{
							Respuesta.Add(Paquete);
						}
					}
				}
			}
			catch (Exception)
			{
				// si no funciona, se pierde la configuración.
			}
			return Respuesta;
		}

	}
}
