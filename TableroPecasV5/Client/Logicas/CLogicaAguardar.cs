using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaAguardar : ComponentBase
	{
		[Parameter]
		public Int32 Abscisa { get; set; }

		[Parameter]
		public Int32 Ordenada { get; set; }

		[Parameter]
		public double Ancho { get; set; }

		[Parameter]
		public double Alto { get; set; }

		public string Estilo()
		{
			return "width: 100%; height: 100%; margin-left: " + Abscisa.ToString() + "px; margin-top: " + Ordenada.ToString() +
				"px; background: #BEC9E7; text-align: center; position: absolute; opacity: 0.5;";
		}

	}
}
