using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaComando : ComponentBase
	{
		public delegate void FncEjecutarComando();

		[Parameter]
		public FncEjecutarComando Funcion { get;set; }

		[Parameter]
		public string Imagen { get; set; }

		[Parameter]
		public bool Seleccionado { get; set; }

		public bool BajoMouse { get; set; }

		public void PonerEnGris()
		{
			BajoMouse = true;
			StateHasChanged();
		}

		public void SacarDeGris()
		{
			BajoMouse = false;
			StateHasChanged();
		}

		public void EjecutarFuncion ()
		{
			if (Funcion != null)
			{
				Funcion();
			}
		}

		public string Estilo
		{
			get
			{
				return "width: 40px; height: 40px; background-color: "+(Seleccionado?"#ffffff;":(BajoMouse?"#d7d8df;":"#edf0f8;")); ;
			}
		}
	}
}
