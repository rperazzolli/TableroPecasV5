using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaTextos : Respuesta
	{
		//[JsonProperty("Mimico")]
		public List<string> Contenidos { get; set; }

	}
}
