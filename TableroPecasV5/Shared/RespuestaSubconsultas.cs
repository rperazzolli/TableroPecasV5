using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{

	public class RespuestaSubconsultas : Respuesta
	{
		//[JsonProperty("Subconsultas")]
		public List<CSubconsultaExt> Subconsultas { get; set; } = new List<CSubconsultaExt>();
	}
}
