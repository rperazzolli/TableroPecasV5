using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Blazorise;
using System.Net.Http;
using System.Net.Http.Json;
using TableroPecasV5.Client.Contenedores;
using TableroPecasV5.Client.Rutinas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaCrearSolapa : ComponentBase
	{

		[Parameter]
		public Int32 Sala { get; set; }

		public Modal ModalCrearSolapa { get; set; }

		private string mszNombre = "";

		private void AjustarHabilitado()
		{
			DesHabilitado = mszNombre.Trim().Length == 0;
		}

		public void CambiaColor(string Ahora)
		{
			ColorSolapa = Ahora;
			StateHasChanged();
		}

		public string Nombre
		{
			get { return mszNombre; }
			set
			{
				if (mszNombre != value)
				{
					mszNombre = value;
					AjustarHabilitado();
				}
			}
		}


		public string ColorSolapa { get; set; } = "#ffffff";

		public bool MultiInstrumentos { get; set; } = false;

		public bool DesHabilitado { get; set; } = true;

		[Inject]
		private HttpClient Cliente { get; set; }

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			if (ModalCrearSolapa != null && !ModalCrearSolapa.Visible)
			{
				ModalCrearSolapa.Show();
			}
			return base.OnAfterRenderAsync(firstRender);
		}

		[Inject]
		NavigationManager NavigationManager { get; set; }

		public void Salir()
		{
			NavigationManager.NavigateTo("Login");
		}

		public void Cerrando(Blazorise.ModalClosingEventArgs e)
		{
			switch (e.CloseReason)
			{
				case CloseReason.EscapeClosing:
				case CloseReason.FocusLostClosing:
					e.Cancel = true;
					break;
			}
		}

		private void ExtraerComponentesColor(out Int32 Rojo, out Int32 Verde, out Int32 Azul)
		{
			try
			{
				string Aa = ColorSolapa.Substring(1);
				Rojo = Int32.Parse(Aa.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				Verde = Int32.Parse(Aa.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				Azul = Int32.Parse(Aa.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			}
			catch (Exception)
			{
				throw new Exception("Color incorrecto <" + ColorSolapa + ">");
			}
		}

		public async void Registrar()
		{
			try
			{

				ExtraerComponentesColor(out Int32 Rojo, out Int32 Verde, out Int32 Azul);

				CSolapaCN Solapa = new CSolapaCN()
				{
					Codigo = -1,
					Dimension = -1,
					ElementoDimension = -1,
					Nombre = mszNombre.Trim(),
					Orden = CContenedorDatos.ObtenerOrdenProximaSolapa(Sala),
					Sala = Sala,
					Rojo = Rojo,
					Verde = Verde,
					Azul = Azul
				};

				var Respuesta = await Cliente.PostAsJsonAsync<CSolapaCN>("api/Comites/InsertarSolapa?URL=" + Contenedores.CContenedorDatos.UrlBPI +
								"&Ticket=" + Contenedores.CContenedorDatos.Ticket, Solapa);
				if (!Respuesta.IsSuccessStatusCode)
				{
					throw new Exception(Respuesta.ReasonPhrase);
				}

				RespuestaEnteros RespuestaCodigo = await Respuesta.Content.ReadFromJsonAsync<RespuestaEnteros>();
				if (!RespuestaCodigo.RespuestaOK)
				{
					throw new Exception(RespuestaCodigo.MsgErr);
				}

				Solapa.Codigo = RespuestaCodigo.Codigos[0];

				CPuntoSala Punto = CContenedorDatos.UbicarPuntoSala(Sala);
				if (Punto != null)
				{
					Punto.Solapas.Add(new CPuntoSolapa()
					{
						Solapa = Solapa,
						Preguntas = new List<CPuntoPregunta>()
					});
				}

				Salir();

			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}


	}
}
