using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Listas;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaLineaAsgCapaWFS	: ComponentBase
	{
		[Parameter]
		public CVinculoDetalleCN Vinculo { get; set; }

		[Parameter]
		public List<CTextoTexto> Elementos { get; set; }

		public string ElementoAsociado
		{
			get { return Vinculo.Posicion; }
			set
			{
				if (value != Vinculo.Posicion)
				{
					Vinculo.Posicion = value;
					StateHasChanged();
				}
			}
		}

		public string EstiloElemento
		{
			get
			{
				return "width: 50%; height: 35px; background: " + (Vinculo.Posicion.Length == 0 ? "yellow;" : "white;");
			}
		}
	}

	public class CElementoVinculador
	{
		public CVinculoDetalleCN Vinculo { get; set; }
		public CLogicaLineaAsgCapaWFS Linea { get; set; }

	}
}
