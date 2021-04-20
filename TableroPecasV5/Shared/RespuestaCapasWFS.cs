using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaCapasWFS : Respuesta
	{
		//[JsonProperty("Capas")]
		public List<CCapaWFSCN> Capas { get; set; } = new List<CCapaWFSCN>();
	}
}
