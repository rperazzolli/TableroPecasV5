using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaCapaWFS : Respuesta
	{
		//[JsonProperty("Capas")]
		public CCapaWFSCN Capa { get; set; }
	}
}
