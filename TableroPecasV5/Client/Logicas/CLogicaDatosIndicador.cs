using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaDatosIndicador : ComponentBase
	{
		[Parameter]
		public TableroPecasV5.Shared.CDatoIndicador Datos { get; set; }

		[Parameter]
		public Int32 CodigoElemento { get; set; } = -1;

		public TableroPecasV5.Shared.CInformacionAlarmaCN Alarma { get; set; }

		protected override Task OnInitializedAsync()
		{
			Contenedores.CContenedorDatos.AlarmaIndicadorDesdeGlobal(Datos.Codigo, CodigoElemento);
			return base.OnInitializedAsync();
		}
	}
}
