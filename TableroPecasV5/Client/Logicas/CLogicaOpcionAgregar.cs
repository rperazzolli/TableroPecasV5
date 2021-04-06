using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaOpcionAgregar	: ComponentBase
	{

		[Parameter]
		public int Opcion { get; set; }

		public delegate void FncEjecutarOpcion(Int32 Opc);
		public static FncEjecutarOpcion AlEjecutar { get; set; }

		public void HacerClick()
		{
			if (AlEjecutar != null)
			{
				AlEjecutar(Opcion);
			}
		}
	}
}
