using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaDetalleVinculo : Respuesta
	{
		//[JsonProperty("Vinculo")]
		public CVinculoIndicadorCompletoCN Vinculo { get; set; }
	}
}
