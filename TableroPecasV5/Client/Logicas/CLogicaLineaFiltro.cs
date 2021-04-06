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
		Logicas.CDetalleIndicador Pagina { get; set; }

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

		public void IniciarDragLinea(LineaFiltro Linea)
		{
			if (Pagina != null)
			{
				Pagina.IniciarDragLinea(Linea);
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
