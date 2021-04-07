using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaVinculos : Respuesta
	{
		//[JsonProperty("Vinculos")]
		public List<CVinculoIndicadorCompletoCN> Vinculos { get; set; } = new List<CVinculoIndicadorCompletoCN>();
	}
}
