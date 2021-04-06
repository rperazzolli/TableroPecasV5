using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class Respuesta
	{
		//[JsonProperty("RespuestaOK")]
		public bool RespuestaOK { get; set; }

		//[JsonProperty("MsgErr")]
		public string MsgErr { get; set; }

		public Respuesta()
		{
			RespuestaOK = true;
			MsgErr = "";
		}
	}

	public class RespuestaEstIndicadores : Respuesta
	{
		//[JsonProperty("Estructura")]
		public CEstIndicadoresCN Estructura { get; set; } = new CEstIndicadoresCN();

	}
}
