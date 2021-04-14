using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using TableroPecasV5.Shared;
using TableroPecasV5.Client.Datos;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaPagTortasGIS : ComponentBase
	{
		public static List<CLineaComprimida> gLineas { get; set; }
		public static CColumnaBase gColumnaDatos { get; set; }
		public static CColumnaBase gColumnaAgrupadora { get; set; }
		public static CColumnaBase gColumnaPosicion { get; set; }
		public static CColumnaBase gColumnaLat { get; set; }
		public static CColumnaBase gColumnaLng { get; set; }
		public static DatosSolicitados gSolicitados { get; set; }
		public static ClaseElemento gClaseIndicador { get; set; }
		public static Int32 gIndicador { get; set; }
		public List<CLineaComprimida> Lineas { get; set; } = null;
		public CColumnaBase ColumnaDatos { get; set; } = null;
		public CColumnaBase ColumnaAgrupadora { get; set; } = null;
		public CColumnaBase ColumnaPosicion { get; set; } = null;
		public CColumnaBase ColumnaLat { get; set; } = null;
		public CColumnaBase ColumnaLng { get; set; } = null;
		public DatosSolicitados Solicitados { get; set; }
		public ClaseElemento ClaseIndicador { get; set; }
		public Int32 Indicador { get; set; }

		protected override Task OnInitializedAsync()
		{
			ColumnaDatos = gColumnaDatos;
			ColumnaAgrupadora = gColumnaAgrupadora;
			ColumnaPosicion = gColumnaPosicion;
			ColumnaLat = gColumnaLat;
			ColumnaLng = gColumnaLng;
			Lineas = gLineas;
			Solicitados = gSolicitados;
			ClaseIndicador = gClaseIndicador;
			Indicador = gIndicador;
			return base.OnInitializedAsync();
		}

	}
}
