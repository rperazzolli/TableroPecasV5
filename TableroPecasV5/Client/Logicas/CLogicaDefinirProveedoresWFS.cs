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
	public class CLogicaDefinirProveedoresWFS : ComponentBase
	{

		public Int32 CodigoProveedor
		{
			get
			{
				return (mProveedor == null ? -1 : mProveedor.Codigo);
			}
		}

		public void CerrarCapas()
		{
			HayCapas = false;
			StateHasChanged();
		}

		public bool NoRegistrado { get; set; } = true;

		public bool DatosIncompletos { get; set; } = true;

		public void SeleccionarProveedor(CProveedorWFSCN Proveedor)
		{
			if (mProveedor != Proveedor)
			{
				mProveedor = Proveedor;
				if (Proveedor == null)
				{
					NombreProveedor = "";
					URLProveedor = "";
					FAProveedor = "";
				}
				else
				{
					NombreProveedor = Proveedor.Descripcion;
					URLProveedor = Proveedor.DireccionURL;
					FAProveedor = Proveedor.DireccionFA;
				}
			}
      StateHasChanged();
		}

		public List<CProveedorWFSCN> ListaProveedores { get; set; } = null;

		public bool HayCapas { get; set; } = false;

		public void EditarCapas()
		{
			HayCapas = true;
			StateHasChanged();
		}

		public string EstiloProveedor(CProveedorWFSCN Prov)
		{
			return "height: 25px; width: 100 %; cursor: pointer; padding: 0px; margin: 0px; background: " +
				(Prov == mProveedor ? "yellow;" : "white;");
		}

		public async void Validar()
		{
			//ValidarCapasProveedorWFS
			HayMensaje = true;
			Mensaje = "Validando ....";
			StateHasChanged();

			Respuesta RespWCF = await Http.GetFromJsonAsync<Respuesta>(
				"api/Capas/ValidarCapasProveedorWFS?URL=" + Contenedores.CContenedorDatos.UrlBPI +
				"&Ticket=" + Contenedores.CContenedorDatos.Ticket +
				"&Codigo=" + (mProveedor == null ? -1 : mProveedor.Codigo));
			if (!RespWCF.RespuestaOK)
			{
				Mensaje = RespWCF.MsgErr;
			}
			else
			{
				Mensaje = "Capas validadas con éxito";
			}
			
			HayMensaje = true;
			StateHasChanged();

		}

		public string Mensaje { get; set; } = "";
		public bool HayMensaje { get; set; } = false;

		public void LimpiarMsg()
		{
			Mensaje = "";
			HayMensaje = false;
			StateHasChanged();
		}

		public void CrearNuevo()
		{
      SeleccionarProveedor(new CProveedorWFSCN()
      {
        Codigo = -1,
        Descripcion = "",
        DireccionFA = "",
        DireccionURL = "",
        FechaRefresco = DateTime.Now
      });
		}

		public async void Registrar()
		{
			try
			{

				if (mProveedor == null)
				{
					mProveedor = new CProveedorWFSCN()
					{
						Codigo = -1,
						FechaRefresco = DateTime.Now
					};
				}

				mProveedor.Descripcion = NombreProveedor;
				mProveedor.DireccionFA = FAProveedor;
				mProveedor.DireccionURL = URLProveedor;

				var Respuesta = await Http.PostAsJsonAsync<CProveedorWFSCN>("api/Capas/InsertarProveedorWFS?URL=" +
								Contenedores.CContenedorDatos.UrlBPI +
								"&Ticket=" + Contenedores.CContenedorDatos.Ticket, mProveedor);
				if (!Respuesta.IsSuccessStatusCode)
				{
					throw new Exception(Respuesta.ReasonPhrase);
				}

				RespuestaEnteros RespuestaCodigo = await Respuesta.Content.ReadFromJsonAsync<RespuestaEnteros>();
				if (!RespuestaCodigo.RespuestaOK)
				{
					throw new Exception(RespuestaCodigo.MsgErr);
				}

				if (mProveedor.Codigo < 0)
				{
					mProveedor.Codigo = RespuestaCodigo.Codigos[0];
					ListaProveedores.Add(mProveedor);
				}

				CrearNuevo();
				StateHasChanged();

			}
			catch (Exception ex)
			{
				CRutinas.DesplegarMsg(ex);
			}
    }

		public void Cerrar()
		{
			//
		}


    private void AjustarHabilitaciones()
		{
			NoRegistrado = (mProveedor == null || mProveedor.Codigo < 0);
			DatosIncompletos = ListaProveedores == null || NombreProveedor.Length == 0 ||
					URLProveedor.Length == 0 || FAProveedor.Length == 0;
		}

		private CProveedorWFSCN mProveedor = null;

		private string mszNombre = "";
		public string NombreProveedor
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
		public string URLProveedor
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

		private string mszFA = "";
		public string FAProveedor
		{
			get { return mszFA; }
			set
			{
				if (mszFA != value)
				{
					mszFA = value;
					AjustarHabilitaciones();
					StateHasChanged();
				}
			}
		}

		[Inject]
		public HttpClient Http { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
      if (ListaProveedores == null)
			{
        ListaProveedores = await Contenedores.CContenedorDatos.ListarProveedoresWFSAsync(Http);
        StateHasChanged();
			}
		  await	base.OnAfterRenderAsync(firstRender);
		}
	}
}
