using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaEnteros	: Respuesta
	{
		//[JsonProperty("Codigos")]
		public List<Int32> Codigos { get; set; } = new List<int>();

		public RespuestaEnteros()
		{
			Codigos = new List<int>();
		}
	}
}
