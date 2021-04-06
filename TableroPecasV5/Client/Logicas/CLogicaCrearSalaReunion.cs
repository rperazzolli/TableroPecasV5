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
	public class CLogicaCrearSalaReunion : ComponentBase
	{
		public Modal ModalCrearSala { get; set; }

		private string mszNombre = "";

		private void AjustarHabilitado()
		{
			DesHabilitado = mszNombre.Trim().Length == 0 || Comite < 0;
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

		private Int32 mComite = -1;
		public Int32 Comite
		{
			get { return mComite; }
			set
			{
				if (mComite != value)
				{
					mComite = value;
					AjustarHabilitado();
				}
			}
		}

		public List<CGrupoPuestosCN> GruposPuestos { get; set; }

		public bool Exclusiva { get; set; } = false;

		public bool DesHabilitado { get; set; } = true;

		[Inject]
		private HttpClient Cliente { get; set; }

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			if (ModalCrearSala!=null && !ModalCrearSala.Visible)
			{
				if (Contenedores.CContenedorDatos.gComitesUsuario != null)
				{
					GruposPuestos = (from C in CContenedorDatos.gComitesUsuario
													 orderby C.Descripcion
													 select C).ToList();
					GruposPuestos.Insert(0, new CGrupoPuestosCN()
					{
						Codigo = -1,
						Descripcion = "No definido"
					});
				}
				ModalCrearSala.Show();
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

		public async void Registrar()
		{
			try
			{
				CSalaCN Sala = new CSalaCN()
				{
					Codigo = -1,
					Dimension = -1,
					ElementoDimension = -1,
					Comite = Comite,
					EdicionRestringida = Exclusiva,
					Registrador = Contenedores.CContenedorDatos.CodigoUsuario,
					Nombre = mszNombre.Trim()
				};

				var Respuesta = await Cliente.PostAsJsonAsync<CSalaCN>("api/Comites/InsertarSala?URL=" + Contenedores.CContenedorDatos.UrlBPI +
								"&Ticket=" + Contenedores.CContenedorDatos.Ticket, Sala);
				if (!Respuesta.IsSuccessStatusCode)
				{
					throw new Exception(Respuesta.ReasonPhrase);
				}

				RespuestaEnteros RespuestaCodigo = await Respuesta.Content.ReadFromJsonAsync<RespuestaEnteros>();
				if (!RespuestaCodigo.RespuestaOK)
				{
					throw new Exception(RespuestaCodigo.MsgErr);
				}

				Sala.Codigo = RespuestaCodigo.Codigos[0];
				CContenedorDatos.ListaSalasReunion.Add(new Listas.CListaTexto()
				{
					Codigo = Sala.Codigo,
					Descripcion = Sala.Nombre
				});
				CContenedorDatos.ListaSalasReunion = (from S in CContenedorDatos.ListaSalasReunion
																							orderby S.Descripcion
																							select S).ToList();

				CContenedorDatos.EstructuraIndicadores.Salas.Add(new CPuntoSala()
				{
					Sala = Sala,
					Solapas = new List<CPuntoSolapa>()
				});
				CContenedorDatos.EstructuraIndicadores.Salas = (from S in CContenedorDatos.EstructuraIndicadores.Salas
																							orderby S.Sala.Nombre
																							select S).ToList();

				CLogicaMainMenu.RefrescarPantalla();

				Salir();

			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
		}

	}
}
