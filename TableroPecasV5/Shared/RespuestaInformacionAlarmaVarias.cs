using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableroPecasV5.Shared
{
	public class RespuestaInformacionAlarmaVarias : Respuesta
	{
		//[JsonProperty("Alarmas")]
		public List<CInformacionAlarmaCN> Instancias { get; set; }
	}
}
