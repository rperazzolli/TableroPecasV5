using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace TableroPecasV5.Client.Logicas
{
	public class CLogicaReferencias	: ComponentBase
	{

		[Parameter]
		public List<CLineaValorColor> ListaElementos { get; set; }

		[CascadingParameter]
		public CLogicaTortasGIS Tortas { get; set; }

		public void SeleccionarElemento(CLineaValorColor Elemento)
		{
			if (Tortas != null)
			{
				Tortas.SeleccionarGajo(Elemento.Referencia);
			}
		}

		public Int32 AbscisaAbajo = 0;
		public Int32 OrdenadaAbajo = 0;

		public void EventoMouseAbajo(Microsoft.AspNetCore.Components.Web.MouseEventArgs e)
		{
			AbscisaAbajo = (int)e.ClientX + 5;
			OrdenadaAbajo = (int)e.ClientY + 5;
		}

		public string EstiloLinea(CLineaValorColor Elemento)
		{
			return "width: 20px; height: 20px; background: " + Elemento.Color + "; margin-left: 5px; margin-top: 4px;";
		}

		public string EstiloBlock
		{
			get
			{
				return "height: 28px; margin-left: 5px; width: calc(100% - 40px);";
			}
		}

		public void Incrementar()
		{
			Tortas.ModificarFactor(1.25);
		}

		public void Reducir()
		{
			Tortas.ModificarFactor(0.8);
		}

	}
}
