using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaMimicos : Respuesta
	{
		//[JsonProperty("Mimicos")]
		public List<CElementoMimicoCN> Mimicos { get; set; }

		public RespuestaMimicos()
		{
			Mimicos = new List<CElementoMimicoCN>();
		}

	}
}
