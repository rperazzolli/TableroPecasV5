using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using TableroPecasV5.Client.Listas;
using TableroPecasV5.Shared;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaVinculadorWFS : ComponentBase
	{
		[Inject]
		public HttpClient Http { get; set; }

		public List<CListaTexto> ListaCapas { get; set; } = null;

		private async Task LeerCapasAsync()
		{
			RespuestaCapasGIS Respuesta = await Contenedores.CContenedorDatos.LeerCapasWFSAsync(Http, true, true);
			if (Respuesta != null)
			{
				ListaCapas = (from C in Respuesta.CapasWFS
											select new CListaTexto()
											{
												Codigo = C.Codigo,
												Descripcion = C.Descripcion
											}).ToList();
				StateHasChanged();
			}
		}

		public void AsociarCapaElemento(Int32 Codigo)
		{
			//
		}

		public void Registrar()
		{
			//
		}

		public List<CListaDoble> ListaElementosCapa { get; set; }

		public List<CListaTexto> ListaElementos { get; set; }

		public void SeleccionoCapa()
		{
			//
		}

		protected override Task OnAfterRenderAsync(bool firstRender)
		{
			_ = LeerCapasAsync();
			return base.OnAfterRenderAsync(firstRender);
		}

	}
}
