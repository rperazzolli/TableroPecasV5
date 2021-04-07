using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TableroPecasV5.Client.Listas
{
	public class CListaPosicion : CListaTexto
	{
		public string Color { get; set; } = "red";
		public double Lat { get; set; } = -1000;
		public double Lng { get; set; } = -1000;
		public string Referencia { get; set; } = "";
	}
}
