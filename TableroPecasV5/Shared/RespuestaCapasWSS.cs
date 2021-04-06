using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaCapasWSS : Respuesta
	{
		//[JsonProperty("Capas")]
		public List<CCapaWSSCN> Capas { get; set; }

		public RespuestaCapasWSS()
		{
			Capas = new List<CCapaWSSCN>();
		}

	}
}
