using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Client.Datos;


namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaPagPinsLL	: ComponentBase
	{

		[Parameter]
		public List<CColumnaBase> Columnas { get; set; } = null;
		[Parameter]
		public List<CLineaComprimida> Lineas { get; set; } = null;
		[Parameter]
		public string ColumnaDatos { get; set; } = "";
		[Parameter]
		public string ColumnaLat { get; set; } = "";
		[Parameter]
		public string ColumnaLng { get; set; } = "";
		[Parameter]
		public bool Agrupados { get; set; }

		[CascadingParameter]
		public CLogicaIndicador Contenedor { get; set; }

		protected override Task OnInitializedAsync()
		{
			return base.OnInitializedAsync();
		}

		public void Retroceder()
		{
			Contenedor.CerrarPinesLL();
		}

	}
}
