using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaLineaFiltro : ComponentBase
	{

		[CascadingParameter]
		Logicas.CLogicaIndicador Pagina { get; set; }

		[CascadingParameter]
		Plantillas.CContenedorBlocks Blocks { get; set; }

		[CascadingParameter]
		Componentes.CContenedorFiltros Filtros { get; set; }

		[Parameter]
		public LineaFiltro Linea { get; set; }

		public string EstiloLinea(LineaFiltro Linea)
		{
			return "background-color: " + Linea.ColorBase + ";";
		}

		public void IniciarDragLinea(Microsoft.AspNetCore.Components.Web.DragEventArgs e, LineaFiltro Linea)
		{
			if (Pagina != null)
			{
				Pagina.IniciarDragLinea(e.OffsetX, e.OffsetY, Linea);
			}
			else
			{
				if (Blocks != null)
				{
					Blocks.IniciarDragLinea(Linea);
				}
			}
		}

	}
}
