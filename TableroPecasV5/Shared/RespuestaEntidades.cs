using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaEntidades	 : Respuesta
	{
		//[JsonProperty("Entidades")]
		public List<CEntidadCN> Entidades { get; set; }
	}

}
