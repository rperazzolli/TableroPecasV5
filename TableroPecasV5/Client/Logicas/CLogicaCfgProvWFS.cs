using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaCfgProvWFS : ComponentBase
	{

		private List<CProveedorWFSCN> mProveedores = null;

		public List<CProveedorWFSCN> Proveedores
		{
			get { return mProveedores; }
		}

		[Inject]
		public HttpClient Http { get; set; }

		protected override async Task OnInitializedAsync()
		{
			// leer los proveedores.
			mProveedores = await Contenedores.CContenedorDatos.ListarProveedoresWFSAsync(Http);
			StateHasChanged();
			await base.OnInitializedAsync();
		}

		private string mszDescripcion = "";
		public string Descripcion
		{
			get { return mszDescripcion; }
			set
			{
				value = value.Trim();
				if (value != mszDescripcion)
				{
					mszDescripcion = value;
				}
			}
		}

		private string mszURL = "";
		public string URL
		{
			get { return mszURL; }
			set
			{
				value = value.Trim();
				if (value != mszURL)
				{
					mszURL = value;
				}
			}
		}

		private string mszFA = "";
		public string FA
		{
			get { return mszFA; }
			set
			{
				value = value.Trim();
				if (value != mszFA)
				{
					mszFA = value;
				}
			}
		}

		private CProveedorWFSCN mProveedorEnEdicion = null;

		public void PosicionarProveedor(CProveedorWFSCN Proveedor)
		{
			mProveedorEnEdicion = Proveedor;
			if (Proveedor != null)
			{
				Descripcion = Proveedor.Descripcion;
				URL = Proveedor.DireccionURL;
				FA = Proveedor.DireccionFA;
				StateHasChanged();
			}
		}
	}
}
