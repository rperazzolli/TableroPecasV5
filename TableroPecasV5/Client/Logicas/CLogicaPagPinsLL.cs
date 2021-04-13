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
		public static List<CColumnaBase> gColumnas { get; set; }
		public static List<CLineaComprimida> gLineas { get; set; }
		public static string gColumnaDatos { get; set; }
		public static string gColumnaLat { get; set; }
		public static string gColumnaLng { get; set; }
		public static bool gAgrupados { get; set; }

		public List<CColumnaBase> Columnas { get; set; } = null;
		public List<CLineaComprimida> Lineas { get; set; } = null;
		public string ColumnaDatos { get; set; } = "";
		public string ColumnaLat { get; set; } = "";
		public string ColumnaLng { get; set; } = "";
		public bool Agrupados { get; set; }

		protected override Task OnInitializedAsync()
		{
			Columnas = gColumnas;
			Lineas = gLineas;
			ColumnaDatos = gColumnaDatos;
			ColumnaLat = gColumnaLat;
			ColumnaLng = gColumnaLng;
			Agrupados = gAgrupados;
//			StateHasChanged();
			return base.OnInitializedAsync();
		}

	}
}
